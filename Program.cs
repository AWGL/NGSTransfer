using System;
using System.Linq;
using System.IO;

//TODO convert to sys service

namespace NGSTransferConsole
{
    class Program
    {
        public const double programVersion = 0.2;
        private static Config config = new Config();

        static void Main(string[] args)
        {
            onStart();
            onStop();
        }

        private static void onStart()
        {
            //parse parameters
            config.ParseConfig();

            Framework.WriteLog("Starting NGSTransfer v" + programVersion, 0);

            if (config.getIsMiSeqHost)
            {
                Framework.WriteLog(@"Determined host to be Illumina MiSeq", 0);
            }
            else
            {
                Framework.WriteLog(@"Determined host to be Illumina HiSeq", 0);
            }

            timer_Tick();

            /*pretend infinit loop
                timer_Tick();
            */
        }

        private static void onStop()
        {
            Framework.WriteLog("Stoppping NGSTransfer ...", 0);
        }

        private static void timer_Tick()
        {
            bool foundCompletedRun = false;

            //check if runs are complete but not already uploaded
            foreach (string folder in Directory.GetDirectories(config.getAnalysisFolderPath))
            {
                if (File.Exists(Path.Combine(folder, @"RTAComplete.txt")) && !File.Exists(Path.Combine(folder, @"TransferComplete.txt")))
                {
                    Framework.WriteLog(@"Found job: " + Path.Combine(config.getAnalysisFolderPath, folder), 0);
                    foundCompletedRun = true;

                    //transfer data to cluster
                    try
                    {
                        TransferJob job = new TransferJob(folder, config);
                        job.TransferData();

                    } catch (Exception e) {
                        Framework.WriteLog(e.Message, -1);
                        foundCompletedRun = false;
                    }

                }

            }

            //delete old runs
            if (foundCompletedRun && config.getDeleteOldestLocalRun)
            {
                //get dir list and sort by date creation
                var di = new DirectoryInfo(config.getAnalysisFolderPath);
                var directories = di.EnumerateDirectories()
                                    .OrderBy(d => d.CreationTime)
                                    .Select(d => d.Name)
                                    .ToList();

                //delete subfolders; protect the last maxSubDirsToKeep (newest) runs
                for (int n = 0; n < directories.Count - 5; ++n)
                {
                    if (File.Exists(Path.Combine(directories[n], @"TransferComplete.txt")))
                    {
                        try
                        {
                            Framework.WriteLog(@"Deleting folder: " + Path.Combine(config.getAnalysisFolderPath, directories[n]), 0);
                            //Directory.Delete(localRunDir + directories[n], true);
                        }
                        catch (Exception e)
                        {
                            Framework.WriteLog(@"Could not delete folder: " + e.ToString(), -1);
                        }
                    }
                }

                if (config.getIsMiSeqHost)
                {
                    try
                    {
                        Framework.WriteLog(@"Deleting and making folder: " + config.getOutputFolderPath, 0);
                        //Directory.Delete(config.getOutputFolderPath, true);
                        //Directory.CreateDirectory(config.getOutputFolderPath);
                    }
                    catch (Exception e)
                    {
                        Framework.WriteLog(@"Could not delete folder: " + e.ToString(), -1);
                    }

                }

            }

        }
    }
}
