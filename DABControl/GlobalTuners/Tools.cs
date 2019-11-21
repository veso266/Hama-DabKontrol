using System;
using System.IO;
using System.Diagnostics;
using System.Reflection;
using System.Linq;
using URadioServer.Radio;

namespace DABControl.GlobalTuners
{
    partial class GTRadio
    {
        public void say(string message, int delay)
        {
            this.RadioTextChanged(this, new RadioTextChangedEventArgs(message));
            System.Threading.Thread.Sleep(delay);
        }
        public void RecieverLogBackup()
        {
            if (Directory.Exists("ChatBackup") && File.Exists("Log.txt.old"))
            {
                string dirPath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
                string fileName = "Log.txt.old";
                File.Move(fileName, "ChatBackup/" + fileName);
                string[] files = Directory.GetFiles(dirPath + "/ChatBackup");
                Debug.WriteLine(files[0]);
                int countFiles = files.Count(file => { return file.Contains(fileName); });
                Debug.WriteLine(countFiles)
;
                string newFileName = (countFiles == 0) ? "Log.txt.old" : String.Format("{0}.bak{1}", fileName, countFiles + 1);
                File.Move("ChatBackup/" + fileName, "ChatBackup/" + newFileName);
            }
            else
            {
                Directory.CreateDirectory("ChatBackup");
            }
        }
        //File Check function to not loose our log
    }
}
