using SQLite;
using System;

namespace CryptoControlCenter.Common.Models
{
    [Table("CachedData")]
    internal class CachedData
    {
        [PrimaryKey]
        public int Id { get; set; }
        public DateTime LastSync { get; set; }
        public string SerializedBinance { get; set; }
        public string SerializedBitstamp { get; set; }
        public CachedData() { }
    }
}
