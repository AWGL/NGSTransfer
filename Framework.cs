using System;
using System.Linq;
using System.Text;
using System.IO;
using System.Security.Cryptography;
using System.Xml;
using System.Diagnostics;

namespace NGSTransferConsole
{
    class Framework
    {
        public static void WriteLog(string logMessage, int errorCode)
        {
            //skip empty messages
            if (logMessage == @"")
            {
                return;
            }

            Console.Write("{0} {1} ", DateTime.Now.ToShortDateString(), DateTime.Now.ToLongTimeString());

            if (errorCode == 0)
            {
                Console.WriteLine("INFO: {0}", logMessage);
            }
            else if (errorCode == 1)
            {
                Console.WriteLine("WARN: {0}", logMessage);
            }
            else if (errorCode == -1)
            {
                Console.WriteLine("ERROR: {0}", logMessage);
            }
        }

        public static void DeleteOldestSubfoldersRecursively(string localRunDir, int maxSubDirsToKeep)
        {
            //get dir list and sort by date creation
            var di = new DirectoryInfo(localRunDir);
            var directories = di.EnumerateDirectories()
                                .OrderBy(d => d.CreationTime)
                                .Select(d => d.Name)
                                .ToList();

            //delete subfolders; protect the last maxSubDirsToKeep (newest) runs
            for (int n = 0; n < directories.Count - maxSubDirsToKeep; ++n)
            {
                try
                {
                    Framework.WriteLog(@"Deleting folder: " + Path.Combine(localRunDir, directories[n]), 0);
                    //Directory.Delete(localRunDir + directories[n], true);
                }
                catch (Exception e)
                {
                    Framework.WriteLog(@"Could not delete folder: " + e.ToString(), -1);
                }
            }

        }

        public static string GetMD5HashFromFile(string fileName)
        {
            FileStream file = new FileStream(fileName, FileMode.Open);
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] retVal = md5.ComputeHash(file);
            file.Close();

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < retVal.Length; i++)
            {
                sb.Append(retVal[i].ToString("x2"));
            }

            return sb.ToString();
        }

        public static void DirectoryCopyFunction(string sourceDirName, string destDirName, bool copySubDirs)
        {
            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);
            DirectoryInfo[] dirs = dir.GetDirectories();

            if (!dir.Exists)
            {
                Framework.WriteLog(@"Source directory does not exist or could not be found " + sourceDirName, -1);
                throw new DirectoryNotFoundException();
            }

            // If the destination directory doesn't exist, create it. 
            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
            }

            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string temppath = Path.Combine(destDirName, file.Name);
                file.CopyTo(temppath, false);
            }

            // If copying subdirectories, copy them and their contents to new location. 
            if (copySubDirs)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    string temppath = Path.Combine(destDirName, subdir.Name);
                    DirectoryCopyFunction(subdir.FullName, temppath, copySubDirs);
                }
            }
        }

        public static string ConvertWindowsPathToCygwin(string path)
        {
            string cygPath = "/cygdrive/";

            foreach (char c in path)
            {
                if(c == '\\')
                {
                    cygPath += '/';
                } else if (c == ':')
                {
                    continue;
                } else 
                {
                    cygPath += c;
                }
            }

            return cygPath;
        }

        public static string GetRunIdFromRunInfoXml(string runInfoFilePath)
        {
            string runId = "";

            XmlReader xmlReader = XmlReader.Create(runInfoFilePath);
            while (xmlReader.Read())
            {
                if ((xmlReader.NodeType == XmlNodeType.Element) && (xmlReader.Name == "Run"))
                {
                    if (xmlReader.HasAttributes)
                        runId = xmlReader.GetAttribute("Id");
                }
            }

            return runId;
        }

        public static void CallSequenceAnalysisViewer(string savFilePath, string localRunDir, string savExePath)
        {
            //write SAV data to table
            Process proc = new Process();
            proc.StartInfo.FileName = savExePath;
            proc.StartInfo.Arguments = localRunDir + ' ' + savFilePath + " -t";
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.RedirectStandardOutput = true;
            proc.StartInfo.RedirectStandardError = true;
            proc.Start();

            Framework.WriteLog(proc.StandardOutput.ReadToEnd(), 0);
            Framework.WriteLog(proc.StandardError.ReadToEnd(), 1);

            proc.WaitForExit();
            proc.Close();
        }

    }
}
