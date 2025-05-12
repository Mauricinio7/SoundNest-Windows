using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SoundNest_Windows_Client.Utilities
{
    public static class TokenStorageHelper
    {
        private static readonly string folderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SoundNest");
        private static readonly string filePath = Path.Combine(folderPath, "token.dat");

        public static void SaveToken(string token)
        {
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            byte[] data = Encoding.UTF8.GetBytes(token);
            byte[] encrypted = ProtectedData.Protect(data, null, DataProtectionScope.CurrentUser);
            File.WriteAllBytes(filePath, encrypted);
        }

        public static string? LoadToken()
        {
            if (!File.Exists(filePath))
                return null;

            try
            {
                byte[] encrypted = File.ReadAllBytes(filePath);
                byte[] decrypted = ProtectedData.Unprotect(encrypted, null, DataProtectionScope.CurrentUser);
                return Encoding.UTF8.GetString(decrypted);
            }
            catch
            {
                return null;
            }
        }

        public static void DeleteToken()
        {
            if (File.Exists(filePath))
                File.Delete(filePath);
        }
    }
}
