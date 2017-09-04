using cloudscribe.Core.Models.Generic;
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
            Paging = new PaginationSettings();
        }


        public string Q { get; set; }
        public string Type { get; set; }
        public ProjectSummary Summary { get; set; }
        public PagedResult<Project> Projects { get; set; }

        public PaginationSettings Paging { get; set; }
    }
}
