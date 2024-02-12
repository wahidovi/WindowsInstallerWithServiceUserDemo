using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstallUninstallServiceUserDemo
{
    internal class Logger
    {
        private readonly string logFilePath;
        private readonly object fileLock = new object();
        private const long MaxLogSize = 2 * 1024 * 1024; // 2MB

        public Logger(string logFilePath)
        {
            //if file does not exist, create it
            if (!File.Exists(logFilePath))
            {
                File.Create(logFilePath).Close();
            }
            this.logFilePath = logFilePath;
        }

        public void Log(string message)
        {
            lock (fileLock)
            {


                File.AppendAllText(logFilePath, $"{DateTime.Now}: {message}" + "\n");

            }
        }
    }
}
