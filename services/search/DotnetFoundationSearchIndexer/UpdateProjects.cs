
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Search;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Azure.Search.Models;
using System.Text.RegularExpressions;
using System;

namespace DotnetFoundationSearchIndexer
{
    public static class UpdateProjects
    {
        private static HttpClient httpClient = new HttpClient();
        [FunctionName("UpdateProjects")]
        public static async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)]HttpRequest req, ILogger log)
        {
            var indexName = Env("SEARCH_INDEX_NAME");
            var searchClient = new SearchServiceClient(
                Env("SEARCH_SERVICE_NAME"), new SearchCredentials(Env("SEARCH_SERVICE_KEY")));
            
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

            ProjectList projectList;
            using (var stream = await httpClient.GetStreamAsync("https://raw.githubusercontent.com/anthonychu/home/fix-project-json-errors/projects/projects.json"))
            using (var streamReader = new StreamReader(stream))
            {
                var jsonTextReader = new JsonTextReader(streamReader);
                projectList = JsonSerializer.CreateDefault().Deserialize<ProjectList>(jsonTextReader);
            }

            var indexClient = searchClient.Indexes.GetClient(indexName);

            var indexActions = projectList
                .Projects
                .Select(async project => {
                    var contributor = projectList.Contributors.FirstOrDefault(c => c.Name == project.Contributor);

                    if (contributor == null)
                    {
                        log.LogError("Contributor '{contributor}' not found for project '{project}'.",
                            project.Contributor, project.Name);
                        return null;
                    }

                    var response = await httpClient.GetAsync($"https://raw.githubusercontent.com/dotnet/home/master/projects/{project.Name}");
                    if (!response.IsSuccessStatusCode)
                    {
                        log.LogError(
                            "Cannot download markdown file '{markdownFilename}'", project.Name);
                        return null;
                    }
                    var markdownDescription = await response.Content.ReadAsStringAsync();

                    var projectNameMatch = Regex.Match(markdownDescription, @"^\s*#(?!#)\s*(.*?)\s*$", RegexOptions.Multiline);
                    var projectName = projectNameMatch.Success ? projectNameMatch.Groups[1].Value : project.Name;

                    var document = new Document
                    {
                        { "id", project.Id },
                        { "name", projectName },
                        { "contributorName", contributor.Name },
                        { "contributorUrl", contributor.Web },
                        { "contributorLogo", contributor.Logo },
                        { "descriptionMarkdownFilename", project.Name },
                        { "descriptionMarkdown", markdownDescription }
                    };
                    
                    log.LogInformation("{name}, {contributorName}", projectName, contributor.Name);

                    return IndexAction.MergeOrUpload(document);
                })
                .Select(t => t.Result)
                .OfType<IndexAction>();

            var result = await indexClient.Documents.IndexAsync(new IndexBatch(indexActions));

            return (ActionResult)new OkObjectResult($"Hello");
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
