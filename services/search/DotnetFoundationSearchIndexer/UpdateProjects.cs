using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace DotnetFoundationSearchIndexer
{
    public static class UpdateProjects
    {
        private static HttpClient httpClient = new HttpClient();
        [FunctionName("UpdateProjects")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post")]HttpRequest req,
            ILogger log)
        {
            var indexName = Env("SEARCH_INDEX_NAME");
            var searchClient = new SearchServiceClient(
                Env("SEARCH_SERVICE_NAME"), new SearchCredentials(Env("SEARCH_SERVICE_KEY")));

            await CreateSearchIndexIfMissing(searchClient, indexName, log);
            var projects = await GetProjects(log);
            await AddOrUpdateIndex(searchClient, projects, indexName, log);

            return new OkObjectResult(projects.Select(p => new { p.Name, p.Contributor }));
        }

        private static async Task CreateSearchIndexIfMissing(SearchServiceClient searchClient, string indexName, ILogger log)
        {
            var indexExists = await searchClient.Indexes.ExistsAsync(indexName);

            if (!indexExists)
            {
                var index = new Index
                {
                    Name = indexName,
                    Fields = new Field[]
                    {
                        new Field("id", DataType.String)
                        {
                            IsRetrievable = true,
                            IsKey = true
                        },
                        new Field("name", DataType.String, AnalyzerName.StandardLucene)
                        {
                            IsRetrievable = true,
                            IsSearchable = true,
                            IsSortable = true
                        },
                        new Field("repoUrls", DataType.Collection(DataType.String), AnalyzerName.StandardLucene)
                        {
                            IsRetrievable = true,
                            IsSearchable = true
                        },
                        new Field("contributorName", DataType.String, AnalyzerName.StandardLucene)
                        {
                            IsRetrievable = true,
                            IsSearchable = true,
                            IsSortable = true
                        },
                        new Field("contributorLogo", DataType.String)
                        {
                            IsRetrievable = true
                        },
                        new Field("contributorUrl", DataType.String)
                        {
                            IsRetrievable = true
                        },
                        new Field("descriptionMarkdown", DataType.String, AnalyzerName.StandardLucene)
                        {
                            IsRetrievable = true,
                            IsSearchable = true
                        },
                        new Field("descriptionMarkdownFilename", DataType.String)
                        {
                            IsRetrievable = true
                        }
                    },
                    ScoringProfiles = new ScoringProfile[]
                    {
                        new ScoringProfile("default")
                        {
                            TextWeights = new TextWeights(new Dictionary<string, double>
                            {
                                { "name", 3 },
                                { "contributorName", 2 },
                                { "repoUrls", 1 },
                                { "descriptionMarkdown", 1 }
                            })
                        }
                    }
                };

                log.LogInformation("Index {indexName} missing. Creating index.", indexName);

                await searchClient.Indexes.CreateAsync(index);
            }
        }

        private static async Task<IEnumerable<Project>> GetProjects(ILogger log)
        {
            ProjectList projectList;
            using (var stream = await httpClient.GetStreamAsync("https://raw.githubusercontent.com/anthonychu/home/fix-project-json-errors/projects/projects.json"))
            using (var streamReader = new StreamReader(stream))
            {
                var jsonTextReader = new JsonTextReader(streamReader);
                projectList = JsonSerializer.CreateDefault().Deserialize<ProjectList>(jsonTextReader);
            }

            foreach (var project in projectList.Projects)
            {
                var contributor = projectList.Contributors.FirstOrDefault(c => c.Name == project.Contributor);
                if (contributor == null)
                {
                    log.LogError("Contributor '{contributor}' not found for project '{project}'.",
                        project.Contributor, project.Name);
                    break;
                }
                project.ContributorInfo = contributor;

                var response = await httpClient.GetAsync($"https://raw.githubusercontent.com/dotnet/home/master/projects/{project.Name}");
                if (!response.IsSuccessStatusCode)
                {
                    log.LogError(
                        "Cannot download markdown file '{markdownFilename}'", project.Name);
                    break;
                }

                project.MarkdownDescription = await response.Content.ReadAsStringAsync();
            }

            return projectList.Projects;
        }

        private static async Task AddOrUpdateIndex(SearchServiceClient searchClient, IEnumerable<Project> projects, string indexName, ILogger log)
        {
            var indexClient = searchClient.Indexes.GetClient(indexName);

            var indexActions = projects
                .Select(project =>
                {
                    var projectNameMatch = Regex.Match(project.MarkdownDescription, @"^\s*#(?!#)\s*(.*?)\s*$", RegexOptions.Multiline);
                    var projectName = projectNameMatch.Success ? projectNameMatch.Groups[1].Value : project.Name;
                    var contributor = project.ContributorInfo;
                    var document = new Document
                    {
                        { "id", project.Id },
                        { "name", projectName },
                        { "contributorName", contributor.Name },
                        { "contributorUrl", contributor.Web },
                        { "contributorLogo", contributor.Logo },
                        { "descriptionMarkdownFilename", project.Name },
                        { "descriptionMarkdown", project.MarkdownDescription }
                    };

                    log.LogInformation("{name}, {contributorName}", projectName, contributor.Name);

                    return IndexAction.MergeOrUpload(document);
                })
                .OfType<IndexAction>();

            var result = await indexClient.Documents.IndexAsync(new IndexBatch(indexActions));
        }


        private static string Env(string key)
        {
            return Environment.GetEnvironmentVariable(key, EnvironmentVariableTarget.Process);
        }
    }

    public class Project
    {
        public string Id
        {
            get { return Regex.Replace(Name, @"\W", "-"); }
        }
        public string Name { get; set; }
        public string Contributor { get; set; }
        public Contributor ContributorInfo { get; set; }
        public string MarkdownDescription { get; set; }
    }

    public class Contributor
    {
        public string Name { get; set; }
        public string Logo { get; set; }
        public string Web { get; set; }
    }

    public class ProjectList
    {
        public string Name { get; set; }
        public List<Project> Projects { get; set; }
        public List<Contributor> Contributors { get; set; }
    }
}
