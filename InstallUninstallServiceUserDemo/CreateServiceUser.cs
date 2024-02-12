using System;
using System.Collections.Generic;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace InstallUninstallServiceUserDemo
{
    internal class CreateServiceUser
    {
        // LSA imports
        [DllImport("advapi32", SetLastError = true, PreserveSig = true)]
        private static extern uint LsaOpenPolicy(ref LSA_UNICODE_STRING SystemName, ref LSA_OBJECT_ATTRIBUTES ObjectAttributes, uint DesiredAccess, out IntPtr PolicyHandle);

        [DllImport("advapi32", SetLastError = true, PreserveSig = true)]
        private static extern uint LsaAddAccountRights(IntPtr PolicyHandle, IntPtr AccountSid, LSA_UNICODE_STRING[] UserRights, int CountOfRights);

        [DllImport("advapi32")]
        private static extern int LsaClose(IntPtr PolicyHandle);

        [DllImport("advapi32")]
        private static extern int LsaNtStatusToWinError(uint status);



        // LSA structures
        [StructLayout(LayoutKind.Sequential)]
        private struct LSA_UNICODE_STRING
        {
            public ushort Length;
            public ushort MaximumLength;
            public IntPtr Buffer;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct LSA_OBJECT_ATTRIBUTES
        {
            public int Length;
            public IntPtr RootDirectory;
            public IntPtr ObjectName;
            public uint Attributes;
            public IntPtr SecurityDescriptor;
            public IntPtr SecurityQualityOfService;
        }

        // LSA constants
        private const uint POLICY_CREATE_ACCOUNT = 0x00000010;
        private const uint POLICY_LOOKUP_NAMES = 0x00000200;

        public static void CreateUser(string serviceUserName, string serviceUserPassword)
        {
            string username = serviceUserName; //  username
            string password = serviceUserPassword;   //  password

            try
            {
                using (PrincipalContext pc = new PrincipalContext(ContextType.Machine))
                {
                    // Check if the user exists
                    UserPrincipal existingUser = UserPrincipal.FindByIdentity(pc, IdentityType.SamAccountName, username);

                    // If the user exists, delete it
                    if (existingUser != null)
                    {
                        existingUser.Delete();
                        Console.WriteLine($"User {username} deleted.");
                    }

                    // Create a new user
                    UserPrincipal newUser = new UserPrincipal(pc);
                    newUser.SamAccountName = username;
                    newUser.SetPassword(password);
                    newUser.Description = "This service user will run FTT-tool";
                    newUser.Enabled = true;
                    newUser.Save();

                    AddRight(username, "SeServiceLogonRight");
                    Console.WriteLine($"User {username} created successfully.");

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }

        }



        private static void AddRight(string userName, string privilegeName)
        {
            IntPtr policyHandle = IntPtr.Zero;
            IntPtr userSid = IntPtr.Zero;
            try
            {
                // Get SID for the user
                userSid = GetSidForUser(userName);
                if (userSid == IntPtr.Zero)
                    throw new Exception("Unable to obtain SID for user " + userName);

                // Open the local security policy
                LSA_OBJECT_ATTRIBUTES loa = new LSA_OBJECT_ATTRIBUTES();
                LSA_UNICODE_STRING systemName = new LSA_UNICODE_STRING();

                uint result = LsaOpenPolicy(ref systemName, ref loa, POLICY_CREATE_ACCOUNT | POLICY_LOOKUP_NAMES, out policyHandle);
                if (result != 0)
                    throw new Exception("OpenPolicy failed: " + LsaNtStatusToWinError(result));

                // Add rights
                LSA_UNICODE_STRING[] userRights = new LSA_UNICODE_STRING[1];
                userRights[0] = new LSA_UNICODE_STRING();
                userRights[0].Buffer = Marshal.StringToHGlobalUni(privilegeName);
                userRights[0].Length = (ushort)(privilegeName.Length * UnicodeEncoding.CharSize);
                userRights[0].MaximumLength = (ushort)((privilegeName.Length + 1) * UnicodeEncoding.CharSize);

                result = LsaAddAccountRights(policyHandle, userSid, userRights, 1);
                if (result != 0)
                    throw new Exception("LsaAddAccountRights failed: " + LsaNtStatusToWinError(result));
            }
            finally
            {


                if (userSid != IntPtr.Zero)
                    Marshal.FreeHGlobal(userSid);
                if (policyHandle != IntPtr.Zero)
                    LsaClose(policyHandle);
            }
        }

        private static IntPtr GetSidForUser(string userName)
        {
            // Convert username to SID
            byte[] sid = new byte[1024];
            int sidLen = sid.Length;
            StringBuilder domainName = new StringBuilder(1024);
            int nameLen = domainName.Capacity;
            int accountType = 0;

            if (!LookupAccountName(string.Empty, userName, sid, ref sidLen, domainName, ref nameLen, ref accountType))
            {
                throw new System.ComponentModel.Win32Exception();
            }

            IntPtr sidPtr = Marshal.AllocHGlobal(sidLen);
            Marshal.Copy(sid, 0, sidPtr, sidLen);

            return sidPtr;
        }

        // PInvoke for LookupAccountName
        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool LookupAccountName(
            string lpSystemName,
            string lpAccountName,
            [MarshalAs(UnmanagedType.LPArray)] byte[] Sid,
            ref int cbSid,
            StringBuilder lpReferencedDomainName,
            ref int cchReferencedDomainName,
            ref int peUse);

    }
}
