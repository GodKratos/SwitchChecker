using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;

namespace SwitchChecker
{
    public static class Functions
    {

        public static string DecryptPassword(string encryptedPassword)
        {
            if (string.IsNullOrEmpty(encryptedPassword.Trim()))
                return "";

            byte[] cyphertext = Convert.FromBase64String(encryptedPassword);
            byte[] b_entropy = Encoding.UTF8.GetBytes(String.Empty);
            byte[] plaintext = ProtectedData.Unprotect(cyphertext, b_entropy, DataProtectionScope.CurrentUser);
            return Encoding.UTF8.GetString(plaintext);
        }

        public static string EncryptPassword(string decryptedPassword)
        {
            if (string.IsNullOrEmpty(decryptedPassword.Trim()))
                return "";

            byte[] plaintext = Encoding.UTF8.GetBytes(decryptedPassword);
            byte[] b_entropy = Encoding.UTF8.GetBytes(String.Empty);
            byte[] cyphertext = ProtectedData.Protect(plaintext, b_entropy, DataProtectionScope.CurrentUser);
            return Convert.ToBase64String(cyphertext);
        }

    }
}
