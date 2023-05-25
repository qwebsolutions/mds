using System;
using System.Collections.Generic;

namespace MdsBuildManager
{
    public class InputArguments
    {
        public string ListeningUrl { get; set; }
        public string BinariesFolder { get; set; }
        public string AzureDevopsOrganisation { get; set; }
        public string AzureDevopsToken { get; set; }
        public string ArtifactsFolder { get; set; }
        public string AzureProject { get; set; }
        public List<int> AzurePipeDefinitions { get; set; }
        public int AzurePoolingIntervalSeconds { get; set; }
        public string SmtpHostName { get; set; }
        public string FromMailAddress { get; set; }
        public string SenderMailPassword { get; set; }
        public string ToMailAddresses { get; set; }
        public bool NotifyDuplicateBinaries { get; set; }
        public bool NotifyNewBinaries { get; set; }
        public string BinariesAvailableOutputChannel { get; set; }

        private static InputArguments Read(string inputFileName)
        {
            string inputFilePath = Metapsi.RelativePath.GetFullPath(".", inputFileName);
            string inputFileContent = System.IO.File.ReadAllText(inputFilePath);
            InputArguments arguments = System.Text.Json.JsonSerializer.Deserialize<InputArguments>(inputFileContent);
            return arguments;
        }

        private static InputArguments inputArguments = null;

        public static InputArguments GetInputArguments(string inputFileName)
        {
            if(inputArguments == null)
            {
                inputArguments = Read(inputFileName);
            }

            return inputArguments;
        }
    }
}
