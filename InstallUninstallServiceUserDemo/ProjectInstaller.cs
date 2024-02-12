using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;


namespace InstallUninstallServiceUserDemo
{
    [RunInstaller(true)]
    public partial class ProjectInstaller : System.Configuration.Install.Installer
    {
        public ProjectInstaller()
        {
            InitializeComponent();
        }

        private void serviceProcessInstaller1_AfterInstall(object sender, InstallEventArgs e)
        {

            CreateServiceUser.CreateUser("DemoService-User", "Password123");
           
            serviceProcessInstaller1.Account = ServiceAccount.User;
            serviceProcessInstaller1.Username = ".\\DemoService-User";
            serviceProcessInstaller1.Password = "Password123";
            serviceInstaller1.Description = " Demo Service installer description";


            EventLog.WriteEntry("DemoService Install", "Service User created with Log on as Service ");
        }

        private void serviceInstaller1_AfterInstall(object sender, InstallEventArgs e)
        {
            // Permission.setUserPermission(@"C:\Program Files (x86)\DemoServiceApp\SetupDemo\");
            Permission.setUserPermission(@"C:\Program Files (x86)\DemoServiceApp\");
        }

        private void serviceInstaller1_Committed(object sender, InstallEventArgs e)
        {
            using (ServiceController sc = new ServiceController(serviceInstaller1.ServiceName))
            {
                sc.Start();
            }
        }

        public override void Uninstall(IDictionary savedState)
        {
           

            this.AfterUninstall += new InstallEventHandler(ServiceInstaller_AfterUninstall);
            base.Uninstall(savedState);
        }

        private void ServiceInstaller_AfterUninstall(object sender, InstallEventArgs e)
        {
            // Your code here
            // For example, you might perform cleanup tasks or log the uninstallation
            //DeleteUser.removeUserPermission(); //causing issue fix it later

            DeleteUser.DeleteServiceUser("DemoService-User");
            try
            {
               
                Directory.Delete(@"C:\Program Files (x86)\DemoServiceApp\", true);

            }
            catch (Exception ex)
            {
                  EventLog.WriteEntry("DemoService Uninstall", ex.Message);
            }  
            

            //DeleteUser.DeleteUserDirectories();
            EventLog.WriteEntry("DemoService Uninstall", "User Deleted");
            
        }
    }
}
