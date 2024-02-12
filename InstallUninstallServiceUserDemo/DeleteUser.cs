using System;
using System.Collections.Generic;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace InstallUninstallServiceUserDemo
{
    internal class DeleteUser
    {
        // LSA_UNICODE_STRING struct
        [StructLayout(LayoutKind.Sequential)]
        public struct LSA_UNICODE_STRING
        {
            public UInt16 Length;
            public UInt16 MaximumLength;
            public IntPtr Buffer;
        }

        // LSA_OBJECT_ATTRIBUTES struct
        [StructLayout(LayoutKind.Sequential)]
        public struct LSA_OBJECT_ATTRIBUTES
        {
            public int Length;
            public IntPtr RootDirectory;
            public IntPtr ObjectName;
            public uint Attributes;
            public IntPtr SecurityDescriptor;
            public IntPtr SecurityQualityOfService;
        }

        // PInvoke signatures
        [DllImport("advapi32.dll", SetLastError = true, PreserveSig = true)]
        private static extern uint LsaOpenPolicy(
            ref LSA_UNICODE_STRING SystemName,
            ref LSA_OBJECT_ATTRIBUTES ObjectAttributes,
            uint DesiredAccess,
            out IntPtr PolicyHandle);

        [DllImport("advapi32.dll", SetLastError = true, PreserveSig = true)]
        private static extern uint LsaRemoveAccountRights(
            IntPtr PolicyHandle,
            IntPtr AccountSid,
            bool AllRights,
            LSA_UNICODE_STRING[] UserRights,
            uint CountOfRights);

        [DllImport("advapi32.dll", SetLastError = true, PreserveSig = true)]
        private static extern uint LsaClose(IntPtr PolicyHandle);

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern bool LookupAccountName(
            string lpSystemName,
            string lpAccountName,
            byte[] Sid,
            ref int cbSid,
            StringBuilder lpReferencedDomainName,
            ref int cchReferencedDomainName,
            out int peUse);

        // Constants
        private const uint POLICY_CREATE_ACCOUNT = 0x00000010;
        private const uint POLICY_LOOKUP_NAMES = 0x00000200;

        public static void DeleteServiceUser(string DeleteUser)
        {
            string username = DeleteUser; //username you want to delete



            try
            {
                using (PrincipalContext pc = new PrincipalContext(ContextType.Machine))
                {
                    // Check if the user exists
                    UserPrincipal existingUser = UserPrincipal.FindByIdentity(pc, IdentityType.SamAccountName, username);

                    if (existingUser != null)
                    {
                        // Delete the user if it exists
                        //RemoveRight will remove the user from the SeServiceLogonRight (Log on as a service) policy    
                        RemoveRight(username, "SeServiceLogonRight");
                        existingUser.Delete();
                        Console.WriteLine($"User {username} deleted.");
                    }
                    else
                    {
                        Console.WriteLine($"User {username} does not exist.");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        private static void RemoveRight(string userName, string privilegeName)
        {
            IntPtr policyHandle = IntPtr.Zero;
            IntPtr userSid = IntPtr.Zero;
            try
            {
                userSid = GetSidForUser(userName);
                if (userSid == IntPtr.Zero)
                    throw new Exception("Unable to obtain SID for user " + userName);

                LSA_OBJECT_ATTRIBUTES loa = new LSA_OBJECT_ATTRIBUTES();
                LSA_UNICODE_STRING systemName = new LSA_UNICODE_STRING();

                uint result = LsaOpenPolicy(ref systemName, ref loa, POLICY_CREATE_ACCOUNT | POLICY_LOOKUP_NAMES, out policyHandle);
                if (result != 0)
                    throw new Exception("OpenPolicy failed: " + result);

                LSA_UNICODE_STRING[] userRights = new LSA_UNICODE_STRING[1];
                userRights[0] = new LSA_UNICODE_STRING
                {
                    Buffer = Marshal.StringToHGlobalUni(privilegeName),
                    Length = (ushort)(privilegeName.Length * UnicodeEncoding.CharSize),
                    MaximumLength = (ushort)((privilegeName.Length + 1) * UnicodeEncoding.CharSize)
                };

                result = LsaRemoveAccountRights(policyHandle, userSid, false, userRights, 1);
                if (result != 0)
                    throw new Exception("LsaRemoveAccountRights failed: " + result);
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
            byte[] sid = new byte[1024];
            int sidLen = sid.Length;
            StringBuilder domainName = new StringBuilder(1024);
            int nameLen = domainName.Capacity;
            int peUse;

            if (!LookupAccountName(null, userName, sid, ref sidLen, domainName, ref nameLen, out peUse))
                throw new System.ComponentModel.Win32Exception();

            IntPtr sidPtr = Marshal.AllocHGlobal(sidLen);
            Marshal.Copy(sid, 0, sidPtr, sidLen);
            return sidPtr;
        }
    }
}
