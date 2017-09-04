using System;
using System.Collections.Generic;

namespace dotnetfoundation.Models
{
    public class ProjectFeed
    {
        public ProjectFeed()
        {
            Summary = new ProjectSummary();
            Projects = new List<Project>();
        }

        public DateTime LastUpdated { get; set; }
        public ProjectSummary Summary { get; set; }
        public List<Project> Projects { get; set; }

    }
}
