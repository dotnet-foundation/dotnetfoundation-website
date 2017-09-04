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

        public async Task<IActionResult> Index(string q = "", string type = "project", int pn = 1, int ps = 20)
        {
            var maxPageSize = 50; //TODO: make this configurable?
            if (ps > maxPageSize) ps = maxPageSize;

            var model = new ProjectListViewModel();
            model.Q = q;
            model.Type = type;
            model.Summary = await _projectService.GetSummary();
            model.Projects = await _projectService.Search(q, pn, ps);
            model.Paging.CurrentPage = pn;
            model.Paging.ItemsPerPage = ps;
            model.Paging.TotalItems = model.Projects.TotalItems;

            return View(model);
        }

        // TODO: another action for project detail? the current site has this.
        // for example https://dotnetfoundation.org/net-compiler-platform-roslyn
        // seems like more data is shown in the current site than exists in the feed
        // unclear to me how the slugs are determined
        // also even names on th elist like for .NET Compiler Platform ("Roslyn") does not match what is in the feed
        // is there another data source?
        // actually it seems like the feed data corresponds to the side bar on the current site not the main list

    }
}
