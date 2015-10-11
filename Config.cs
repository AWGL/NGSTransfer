using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace NGSTransferConsole
{
    class Config
    {
        Dictionary<string, string> configKeyValuePairs = new Dictionary<string, string>();

        //server connections
        private string cvxGenSshHostKeyFingerprint;
        private string cvxGenHostName;
        private string cvxGenSshUserName;
        private string cvxGenSshPrivateKeyPath;
        private string wotanSshHostKeyFingerprint;
        private string wotanHostName;
        private string wotanSshUserName;
        private string wotanSshPrivateKeyPath;

        //Remote file paths
        private string cvxGenRemoteScriptPath;
        private string cvxGenRemoteResultsPath;
        private string cvxGenRemoteArchivePath;
        private string cvxGenRemoteSAVPath;
        private string wotanRemoteScriptPath;
        private string wotanRemoteResultsPath;
        private string wotanRemoteArchivePath;
        private string wotanRemoteSAVPath;

        //rig
        private bool isMiSeqHost;
        private string analysisFolderPath;
        private string outputFolderPath;
        private bool deleteOldestLocalRun;
        private string savExePath;

        public void ParseConfig()
        {
            Regex sectionHeaderRgx = new Regex(@"^\[.*\]$");
            StreamReader inputFile = new StreamReader(Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location) + @"\config.ini");
            string line;

            //parse config file
            while ((line = inputFile.ReadLine()) != null)
            {
                //skip section headers and empty lines
                if (line != @"" && !sectionHeaderRgx.IsMatch(line))
                {
                    string[] fields = line.Split('=');

                    if (fields.Length == 2)
                    {
                        configKeyValuePairs.Add(fields[0], fields[1]);
                    } else
                    {
                        throw new FileLoadException(@"Line: " + line + @" is malformed");
                    }

                }

            }
            inputFile.Close();
            try
            {
                //extract parameters
                cvxGenSshHostKeyFingerprint = configKeyValuePairs[@"cvxGenSshHostKeyFingerprint"];
                cvxGenHostName = configKeyValuePairs[@"cvxGenHostName"];
                cvxGenSshUserName = configKeyValuePairs[@"cvxGenSshUserName"];
                cvxGenSshPrivateKeyPath = configKeyValuePairs[@"cvxGenSshPrivateKeyPath"];
                wotanSshHostKeyFingerprint = configKeyValuePairs[@"wotanSshHostKeyFingerprint"];
                wotanHostName = configKeyValuePairs[@"wotanHostName"];
                wotanSshUserName = configKeyValuePairs[@"wotanSshUserName"];
                wotanSshPrivateKeyPath = configKeyValuePairs[@"wotanSshPrivateKeyPath"];

                cvxGenRemoteScriptPath = configKeyValuePairs[@"cvxGenRemoteScriptPath"];
                cvxGenRemoteResultsPath = configKeyValuePairs[@"cvxGenRemoteResultsPath"];
                cvxGenRemoteArchivePath = configKeyValuePairs[@"cvxGenRemoteArchivePath"];
                cvxGenRemoteSAVPath = configKeyValuePairs[@"cvxGenRemoteSAVPath"];
                wotanRemoteScriptPath = configKeyValuePairs[@"wotanRemoteScriptPath"];
                wotanRemoteResultsPath = configKeyValuePairs[@"wotanRemoteResultsPath"];
                wotanRemoteArchivePath = configKeyValuePairs[@"wotanRemoteArchivePath"];
                wotanRemoteSAVPath = configKeyValuePairs[@"wotanRemoteSAVPath"];

                isMiSeqHost = Convert.ToBoolean(configKeyValuePairs[@"isMiSeqHost"]);
                analysisFolderPath = configKeyValuePairs[@"analysisFolderPath"];
                outputFolderPath = configKeyValuePairs[@"outputFolderPath"];
                deleteOldestLocalRun = Convert.ToBoolean(configKeyValuePairs[@"deleteOldestLocalRun"]);
                savExePath = configKeyValuePairs[@"savExePath"];

            } catch (KeyNotFoundException e)
            {
                Framework.WriteLog("Config file missing key-value: " + e.Message, -1);
            }
        }
         
        public string getCvxGenSshHostKeyFingerprint { get { return cvxGenSshHostKeyFingerprint; } }
        public string getCvxGenHostName { get { return cvxGenHostName; } }
        public string getCvxGenSshUserName { get { return cvxGenSshUserName; } }
        public string getCvxGenSshPrivateKeyPath { get { return cvxGenSshPrivateKeyPath; } }
        public string getWotanSshHostKeyFingerprint { get { return wotanSshHostKeyFingerprint; } }
        public string getWotanHostName { get { return wotanHostName; } }
        public string getWotanSshUserName { get { return wotanSshUserName; } }
        public string getWotanSshPrivateKeyPath { get { return wotanSshPrivateKeyPath; } }
        public string getCvxGenRemoteScriptPath { get { return cvxGenRemoteScriptPath; } }
        public string getCvxGenRemoteResultsPath { get { return cvxGenRemoteResultsPath; } }
        public string getCvxGenRemoteArchivePath { get { return cvxGenRemoteArchivePath; } }
        public string getCvxGenRemoteSAVPath { get { return cvxGenRemoteSAVPath; } }
        public string getWotanRemoteScriptPath { get { return wotanRemoteScriptPath; } }
        public string getWotanRemoteResultsPath { get { return wotanRemoteResultsPath; } }
        public string getWotanRemoteArchivePath { get { return wotanRemoteArchivePath; } }
        public string getWotanRemoteSAVPath { get { return wotanRemoteSAVPath; } }
        public bool getIsMiSeqHost { get { return isMiSeqHost; } }
        public string getAnalysisFolderPath { get { return analysisFolderPath; } }
        public string getOutputFolderPath { get { return outputFolderPath; } }
        public bool getDeleteOldestLocalRun { get { return deleteOldestLocalRun; } }
        public string getSavExePath { get { return savExePath; } }

    }
}
