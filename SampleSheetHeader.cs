using System.IO;
using System.Text.RegularExpressions;

namespace NGSTransferConsole
{
    class SampleSheetHeader
    {
        string sampleSheetPath, experimentName, investigatorName;

        public SampleSheetHeader(string sampleSheetPath)
        {
            this.sampleSheetPath = sampleSheetPath;
        }

        public void Populate()
        {
            string line;
            string[] fields;
            Regex investigatorNameRgx = new Regex(@"^Investigator Name");
            Regex experimentNameRgx = new Regex(@"^Experiment Name");

            //Pass the file path and file name to the StreamReader constructor
            StreamReader SampleSheetReader = new StreamReader(sampleSheetPath);

            //Continue to read until you reach end of file
            while ((line = SampleSheetReader.ReadLine()) != null)
            {
                //skip empty and comma lines
                if (line == "" || CommaOnlyLine(line) == true)
                {
                    continue;
                }

                if (investigatorNameRgx.IsMatch(line))
                {
                    fields = line.Split(',');

                    if (fields.Length > 0)
                    {
                        investigatorName = fields[1];
                    }

                }

                if (experimentNameRgx.IsMatch(line))
                {
                    fields = line.Split(',');

                    if (fields.Length > 0)
                    {
                        experimentName = fields[1];
                    }

                }

            } //end reading file

            SampleSheetReader.Close();
        }

        private static bool CommaOnlyLine(string SampleSheetLine)
        {
            foreach (char c in SampleSheetLine){
                
                if (c != ',')
                {
                    return false;
                }

            }
            
            return true;
        }

        public string getInvestigatorName { get { return investigatorName; } }
        public string getExperimentName { get { return experimentName; } }
    }
}
