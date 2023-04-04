using CryptoControlCenter.Common.Models.Interfaces;
using SQLite;
using System;

namespace CryptoControlCenter.Common.Models
{
    /// <summary>
    /// This object is used in Logs and displays a message, its timestamp and - if the problem is from an specific observer - the Wallet ID
    /// </summary>
    [Table("LogEntry")]
    public class LogEntry : ILogEntry
    {
        public DateTime DateTime { get; set; }
        public string? Trigger { get; set; }
        public string Message { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public LogEntry() { }
        public LogEntry(string message, string? trigger = null)
        {
            DateTime = DateTime.Now;
            Trigger = trigger;
            Message = message;
        }
    }
}
