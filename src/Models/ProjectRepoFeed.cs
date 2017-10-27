using System;
using System.Collections.Generic;

namespace dotnetfoundation.Models
{
    public class ProjectRepoFeed
    {
        public ProjectRepoFeed()
        {
            Summary = new ProjectRepoSummary();
            Projects = new List<ProjectRepo>();
        }

        public DateTime LastUpdated { get; set; }
        public ProjectRepoSummary Summary { get; set; }
        public List<ProjectRepo> Projects { get; set; }

    }
}
