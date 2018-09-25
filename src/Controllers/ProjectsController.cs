using dotnetfoundation.Services;
using dotnetfoundation.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace dotnetfoundation.Controllers
{
    public class ProjectsController : Controller
    {
        public ProjectsController(
            ProjectService projectService,
            ILogger<ProjectsController> logger
            )
        {
            _projectService = projectService;
            _log = logger;
        }

        private readonly ProjectService _projectService;
        private ILogger _log;

        // TODO: would we want a different default page size for projects vs repos since projects shows details?

        public async Task<IActionResult> Index(string searchquery = "", string type = "project", int pn = 1, int ps = 10)
        {
            var maxPageSize = 50; //TODO: make this configurable?
            if (ps > maxPageSize) ps = maxPageSize;

            var model = new ProjectListViewModel();
            model.SearchQuery = searchquery;
            model.Type = type;
            model.Summary = await _projectService.GetRepoSummary();

            if (type == "project")
            {
                model.Projects = await _projectService.SearchProjects(searchquery, pn, ps);
            }
            else
            {
                
                model.ProjectRepos = await _projectService.SearchRepos(searchquery, pn, ps);
            }
            


            return View(model);
        }

    }
}
