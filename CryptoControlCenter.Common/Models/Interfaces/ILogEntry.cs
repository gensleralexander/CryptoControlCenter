using System;

namespace CryptoControlCenter.Common.Models.Interfaces
{
    /// <summary>
    /// This object is used in Logs and displays a message, its timestamp and - if the problem is from an specific observer - the Wallet ID
    /// </summary>
    public interface ILogEntry
    {
        public DateTime DateTime { get; }
        public string? Trigger { get; }
        public string Message { get; }
    }
}
