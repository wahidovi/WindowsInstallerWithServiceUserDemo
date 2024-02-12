using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace InstallUninstallServiceUserDemo
{
    public partial class DEMO_ServiceUser : ServiceBase
    {
        Logger logger = new Logger("C:\\Program Files (x86)\\DemoServiceApp\\SetupDemo\\log.txt");
        public DEMO_ServiceUser()
        {
            InitializeComponent();
         
        }

        protected override void OnStart(string[] args)
        {

            logger.Log("Service Started  ");



        }

        protected override void OnStop()
        {

            logger.Log("Service Stopped  ");
        }
    }
}
