using Microsoft.Extensions.Logging;
using RhDev.Common.Web.Core.Composition;
using System.Text.Json;

namespace RhDev.Common.Web.Core.Utils
{
    public class BuildInfo : IAutoregisteredService
    {
        private const string fileName = "buildinfo.json";
        private readonly ILogger<BuildInfo> logger;

        public string BuildNumber { get; set; } = string.Empty;
        public string BuildId { get; set; } = string.Empty;
        public string SourceVersion { get; set; } = string.Empty;
        public string SourceVersionMessage { get; set; } = string.Empty;
        public string SourceBranch { get; set; } = string.Empty;
        public string SourceBranchName { get; set; } = string.Empty;

        public BuildInfo()
        {

        }

        public BuildInfo(ILogger<BuildInfo> logger)
        {
            FetchInfo();
            this.logger = logger;
        }

        private void FetchInfo()
        {
            try
            {
                var currPath = AppDomain.CurrentDomain.BaseDirectory;

                var path = Path.Combine(currPath, fileName);

                var jsonData = File.ReadAllText(path);

                var bi = JsonSerializer.Deserialize<BuildInfo>(jsonData);

                BuildId = bi.BuildId;
                BuildNumber = bi.BuildNumber;
                SourceVersion = bi.SourceVersion;
                SourceVersionMessage = bi.SourceVersionMessage;
                SourceBranch = bi.SourceBranch;
                SourceBranchName = bi.SourceBranchName;
                
            }
            catch(Exception ex) { logger.LogError(ex, "An error occured when initializing build info"); }
        }
    }
}
