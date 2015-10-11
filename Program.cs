using System.IO;

//TODO convert to sys service

namespace NGSTransferConsole
{
    class Program
    {
        public const double programVersion = 0.1;
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

            if (config.getIsMiSeqHost) Framework.WriteLog(@"Determined host to be Illumina MiSeq", 0);
                else Framework.WriteLog(@"Determined host to be Illumina HiSeq", 0);

            /*pretend loop countdown*/
            timer_Tick();
            /*pretend loop countdown*/
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
                    Framework.WriteLog(@"Calling transfer job: " + Path.Combine(config.getAnalysisFolderPath, folder), 0);
                    foundCompletedRun = true;

                    //tranfer data to cluster
                    try
                    {

                        TransferJob job = new TransferJob(folder, config);
                        job.TransferData();

                    } catch (InvalidDataException e)
                    {
                        Framework.WriteLog(e.Message, -1);
                        foundCompletedRun = false;
                    }

                }

            }

            //TODO send built list of runs for delete
            //only delete data when the instrument is not running
            if (foundCompletedRun && config.getDeleteOldestLocalRun)
            {
                Framework.DeleteOldestSubfoldersRecursively(config.getAnalysisFolderPath, 5);
                if (config.getIsMiSeqHost) Framework.DeleteOldestSubfoldersRecursively(config.getOutputFolderPath, 0);
            }
        }
    }
}
