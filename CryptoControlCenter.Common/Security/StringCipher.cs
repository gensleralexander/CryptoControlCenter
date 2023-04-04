using System;
using System.Text;
using System.Security.Cryptography;

namespace CryptoControlCenter.Common.Security
{
    /// <summary>
    /// This class is responsible for decrypting and encrypting data (currently only used by API credentials)
    /// Decryption is limited to the local user scope.
    /// </summary>
    internal static class StringCipher
    {
        // Create byte array for additional entropy when using Protect method.
        private static byte[] s_additionalEntropy = { 9, 5, 3, 7, 6, 1, 2 };

        /// <summary>
        /// Encrypt a string using the current user context scope.
        /// </summary>
        /// <param name="data">Unprotected Data</param>
        /// <returns>Protected Data</returns>
        internal static string Encrypt(string data)
        {
            if (data == null)
            {
                throw new ArgumentNullException("Encryption data was null.");
            }
            else if (string.IsNullOrWhiteSpace(data))
            {
                throw new ArgumentException("Encryption data was empty or white space.");
            }
            else
            {
                return Protect(data);
            }
        }
        /// <summary>
        /// Decrypt protected data using the current user context scope.
        /// </summary>
        /// <param name="encrypted">Protected Data</param>
        /// <returns>Unprotected Data</returns>
        internal static string Decrypt(string encrypted)
        {
            if (encrypted == null)
            {
                throw new ArgumentNullException("Decrpytion data was null.");
            }
            else if (string.IsNullOrWhiteSpace(encrypted))
            {
                throw new ArgumentException("Decryption data was empty or white space.");
            }
            else
            {
                return Unprotect(encrypted);
            }
        }

        private static string Protect(string stringUnprotected)
        {
            try
            {
                // Encrypt the data using DataProtectionScope.CurrentUser. The result can be decrypted
                // only by the same current user.
                return Convert.ToBase64String(ProtectedData.Protect(Encoding.UTF8.GetBytes(stringUnprotected), s_additionalEntropy, DataProtectionScope.CurrentUser));
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        private static string Unprotect(string stringProtected)
        {
            try
            {
                //Decrypt the data using DataProtectionScope.CurrentUser.
                return Encoding.UTF8.GetString(ProtectedData.Unprotect(Convert.FromBase64String(stringProtected), s_additionalEntropy, DataProtectionScope.CurrentUser));
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }
    }
}
