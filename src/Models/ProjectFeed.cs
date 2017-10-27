using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace dotnetfoundation.Models
{
    public class ProjectFeed
    {
        public ProjectFeed()
        {
            Projects = new List<Project>();
            Contributors = new List<ProjectContributor>();
        }

        public string Name { get; set; }
        public List<Project> Projects { get; set; }
        public List<ProjectContributor> Contributors { get; set; }
    }
}
