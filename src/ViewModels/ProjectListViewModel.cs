using cloudscribe.Pagination.Models;
using cloudscribe.Web.Pagination;
using dotnetfoundation.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace dotnetfoundation.ViewModels
{
    public class ProjectListViewModel
    {
        public ProjectListViewModel()
        {
            ProjectRepos = new PagedResult<ProjectRepo>();
            Projects = new PagedResult<Project>();
        }


        public string Q { get; set; }
        public string Type { get; set; }
        public ProjectRepoSummary Summary { get; set; }
        public PagedResult<ProjectRepo> ProjectRepos { get; set; }
        public PagedResult<Project> Projects { get; set; }
    }
}
