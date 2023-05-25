//using Microsoft.VisualStudio.Services.ServiceHooks.WebApi;

using System;

namespace MdsBuildManager
{
    public class Build
    {
        public string Id { get; set; }
        public string Tag { get; set; }
        public string BuildNumber { get; set; }
        public string CommitSha { get; set; }
        public string ProjectName { get; set; }
        public string Version { get; set; }
        public int BuildId { get; set; }
        public string Base64Hash { get; set; }
        public DateTime Timestamp { get; set; }
        public string Target { get; set; }
    }

    public class Binaries
    {
        public string Id { get; set; }
        public string Base64Hash { get; set; }
        public string BinaryPath { get; set; }
    }
}
