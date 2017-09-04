using System;

namespace dotnetfoundation.Models
{
    public class Project
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string Language { get; set; } = string.Empty;
        public string Contributor { get; set; } = string.Empty;
        public int Contributors { get; set; }
        public string GithubOrg { get; set; } = string.Empty;
        public string GithubRepo { get; set; } = string.Empty;
        public int OpenIssues { get; set; }
        public int Awesomeness { get; set; }
        public int Rank { get; set; }
        public int Stars { get; set; }
        public int Forks { get; set; }
        public bool Fork { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }
        public DateTime CommitLast { get; set; }
    }
}
