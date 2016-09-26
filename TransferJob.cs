using System;
using System.Linq;
using System.IO;
using WinSCP;

namespace NGSTransferConsole
{
    class TransferJob
    {
        string localRunDir;
        Config config;

        public TransferJob(string localRunDir, Config config)
        {
            this.localRunDir = localRunDir;
            this.config = config;
        }

        public void TransferData()
        {
            bool nhsRun;
            string[] fields;
            SessionOptions sessionOptions = new SessionOptions();

            //get run parameters
            string runId = Framework.GetRunIdFromRunInfoXml(Path.Combine(localRunDir, @"RunInfo.xml"));

            //Parse samplesheet
            SampleSheetHeader sampleSheetHeader = new SampleSheetHeader(Path.Combine(localRunDir, @"SampleSheet.csv"));
            sampleSheetHeader.Populate();

            //write variables to log
            Framework.WriteLog(@"Local run directory: " + localRunDir, 0);
            Framework.WriteLog(@"Run identifier: " + runId, 0);
            Framework.WriteLog(@"Local SampleSheet path: " + Path.Combine(localRunDir, @"SampleSheet.csv"), 0);
            Framework.WriteLog(@"Experiment name: " + sampleSheetHeader.getExperimentName, 0);

            //determine institution
            if (sampleSheetHeader.getInvestigatorName == null || sampleSheetHeader.getInvestigatorName == "" || !sampleSheetHeader.getInvestigatorName.Contains('-'))
            {
                throw new InvalidDataException(@"Investigator name field not provided or malformed.");
            }

            //split investigtor name field
            fields = sampleSheetHeader.getInvestigatorName.Split('-');

            if (fields[1].ToUpper() == @"CU")
            {
                nhsRun = false;
                Framework.WriteLog(@"Investigator name: " + fields[0], 0);
                Framework.WriteLog(@"Institution name: Cardiff University", 0);

                sessionOptions.SshHostKeyFingerprint = config.getWotanSshHostKeyFingerprint;
                sessionOptions.HostName = config.getWotanHostName;
                sessionOptions.UserName = config.getWotanSshUserName;
                sessionOptions.SshPrivateKeyPath = config.getWotanSshPrivateKeyPath;
            }
            else if (fields[1].ToUpper() == @"NHS")
            {
                nhsRun = true;
                Framework.WriteLog(@"Investigator name: " + fields[0], 0);
                Framework.WriteLog(@"Institution name: NHS", 0);

                sessionOptions.SshHostKeyFingerprint = config.getCvxGenSshHostKeyFingerprint;
                sessionOptions.HostName = config.getCvxGenHostName;
                sessionOptions.UserName = config.getCvxGenSshUserName;
                sessionOptions.SshPrivateKeyPath = config.getCvxGenSshPrivateKeyPath;
            }
            else
            {
                throw new InvalidDataException(@"Institute not recognised. CU an NHS are only available options.");
            }

            //convert metrics into table
            Framework.WriteLog(@"Writing SAV table ...", 0);
            string savFilePath = Path.Combine(Path.GetTempPath(), runId + @"_SAV.txt");
            Framework.CallSequenceAnalysisViewer(savFilePath, localRunDir, config.getSavExePath);

            //transfer data to cluster
            if (nhsRun)
            {
                Framework.WriteLog(@"Starting transfer to cvx-gen ...", 0);

                using (Session session = new Session())
                {
                    string remotePath;
                    if (config.getIsMiSeqHost)
                    {
                        remotePath = config.getCvxGenRemoteArchivePath + @"/miseq/" + runId;
                    }
                    else
                    {
                        remotePath = config.getCvxGenRemoteArchivePath + @"/hiseq/" + runId;
                    }

                    session.Open(sessionOptions);
                    Framework.WriteLog(@"Connected!", 0);

                    TransferOperationResult transferResult;
                    TransferOptions transferOptions = new TransferOptions();
                    transferOptions.TransferMode = TransferMode.Binary;

                    //make remote project directory
                    Framework.WriteLog(@"Creating remote directory: " + remotePath, 0);
                    session.CreateDirectory(remotePath);

                    //transfer run folder to archive
                    Framework.WriteLog(@"Tranfer started ...", 0);
                    transferResult = session.PutFiles(localRunDir, remotePath, false, transferOptions);
                    transferResult.Check(); // Throw on any error

                    //transfer SAV file
                    transferResult = session.PutFiles(savFilePath, config.getCvxGenRemoteSAVPath, false, transferOptions);
                    transferResult.Check(); // Throw on any error

                    //execute remote function
                    Framework.WriteLog(@"Execuing post-run command ...", 0);
                    session.ExecuteCommand(@"bash " + config.getCvxGenRemoteScriptPath + ' ' + remotePath + ' ' + config.getCvxGenRemoteResultsPath + '/' + runId + @" >> ~/runPipeline.log");
                }
            }
            else
            {
                //TODO transfer to Wotan
            }

            //write run transfer completion
            using (StreamWriter file = new StreamWriter(Path.Combine(localRunDir, @"TransferComplete.txt")))
            {
                file.WriteLine(@"NGSTransferComplete," + Program.programVersion);
                file.Close();
            }

        }

    }
}
