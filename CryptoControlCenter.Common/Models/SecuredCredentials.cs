using CryptoControlCenter.Common.Security;
using SQLite;

namespace CryptoControlCenter.Common.Models
{
    [Table("SecuredCredentials")]
    public class SecuredCredentials
    {
        /// <summary>
        /// Primary Key ID
        /// </summary>
        [PrimaryKey, AutoIncrement]
        public int ID { get; set; }
        /// <summary>
        /// Unique Key
        /// </summary>
        [Unique]
        public string Key { get; set; }
        /// <summary>
        /// Encrypted Secret
        /// </summary>
        public string Secret { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public SecuredCredentials() { }
        public SecuredCredentials(string key, string secret)
        {
            Key = key;
            SetEncryptedSecret(secret);
        }

        /// <summary>
        /// Returns the decrypted Secret
        /// </summary>
        internal string GetDecryptedSecret()
        {
            return StringCipher.Decrypt(Secret);
        }
        /// <summary>
        /// Called by constructor
        /// </summary>
        internal void SetEncryptedSecret(string decrypted)
        {
            Secret = StringCipher.Encrypt(decrypted);
        }
    }
}
