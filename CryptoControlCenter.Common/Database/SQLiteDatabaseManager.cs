using SQLite;
using System.IO;
using System.Threading.Tasks;
using System.Globalization;
using CryptoControlCenter.Common.Models;

namespace CryptoControlCenter.Common.Database
{
    /// <summary>
    /// This manager is the only class with access to the SQLite Database.
    /// It's solely purpose is to offer this access to other classes
    /// </summary>
    internal static class SQLiteDatabaseManager
    {
        private static CultureInfo culture = new CultureInfo("en-US");

        private static SQLiteAsyncConnection database;
        internal static SQLiteAsyncConnection Database
        {
            get
            {
                if (database == null)
                {
#if DEBUG
                    database = new SQLiteAsyncConnection(Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location),"CryptoControlCenter_Database_DEBUG.db3"), SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create);

#else
                    database = new SQLiteAsyncConnection(Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location), "CryptoControlCenter_Database.db3"), SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create);
#endif
                    database.CreateTableAsync<CachedData>().Wait();
                    database.CreateTableAsync<Transaction>().Wait();
                    database.CreateTableAsync<ExchangeWallet>().Wait();
                    database.CreateTableAsync<SecuredCredentials>().Wait();
                    database.CreateTableAsync<LogEntry>().Wait();
                }
                return database;
            }
        }

        /// <summary>
        /// Standard AppClosureHandling.
        /// </summary>
        internal async static Task AppClosureHandling()
        {
            if (database != null)
            {
                await database.CloseAsync();
            }
        }
    }
}