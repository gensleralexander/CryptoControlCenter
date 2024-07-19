using CryptoControlCenter.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CryptoControlCenter.WPF.Helper
{
    public class EnumSourceProvider
    {
        public EnumSourceProvider() { }

        public IEnumerable<TransactionType> TransactionType
        {
            get
            {
                return Enum.GetValues(typeof(TransactionType)).Cast<TransactionType>();
            }
        }
    }
}
