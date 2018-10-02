using dotnetfoundation.Models;
using dotnetfoundation.Services;
using dotnetfoundation.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
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
            ILogger<ProjectsController> logger,
            IOptions<ProjectsConfig> configOptionsAccessor
            )
        {
            _projectService = projectService;
            _log = logger;

            _configuration = configOptionsAccessor.Value;
        }

        private readonly ProjectService _projectService;
        private readonly ILogger _log;
        private readonly ProjectsConfig _configuration;

        // TODO: would we want a different default page size for projects vs repos since projects shows details?

        public async Task<IActionResult> Index(string searchquery = "", string type = "project", int pn = 1, int ps = 10)
        {
            ps = Math.Min(ps, _configuration.MaxPageSize);

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
