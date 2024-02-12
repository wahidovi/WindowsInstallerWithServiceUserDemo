using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace InstallUninstallServiceUserDemo
{
    internal class Permission
    {
        public static void setUserPermission(string folder_path)
        {

            string folderPath = folder_path;

            //get current user
            string current_user = Environment.UserName;
            // List of usernames to grant access
            string[] usernames = { "DemoService-User", "Operator", current_user };

            foreach (string username in usernames)
            {
                if (!Directory.Exists(folderPath))
                {
                    Console.WriteLine("The folder does not exist.");
                    return;
                }

                if (!UserExists(username))
                {
                    Console.WriteLine($"The specified user '{username}' does not exist.");
                    continue; // Skip this user and move on to the next one
                }

                DirectorySecurity directorySecurity = Directory.GetAccessControl(folderPath);
                // FileSystemRights rights = username == "FTT-User" ?
                FileSystemRights rights =
                       FileSystemRights.FullControl; //: // Full control for FTT-User
                                                     // FileSystemRights.ReadData | FileSystemRights.WriteData; // Read and Write for others
                FileSystemAccessRule accessRule = new FileSystemAccessRule(
                    username,
                    rights,
                    //FileSystemRights.ReadData | FileSystemRights.WriteData, // Provide the desired permissions here
                    InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit,
                    PropagationFlags.None,
                    AccessControlType.Allow
                );

                directorySecurity.AddAccessRule(accessRule);
                Directory.SetAccessControl(folderPath, directorySecurity);
                Console.WriteLine($"Access granted to '{username}' successfully.");
            }

        }
        private static bool UserExists(string username)
        {
            System.Security.Principal.NTAccount account = new System.Security.Principal.NTAccount(username);
            try
            {
                account.Translate(typeof(System.Security.Principal.SecurityIdentifier));
                return true;
            }
            catch (IdentityNotMappedException)
            {
                return false;
            }
        }
    }
}
