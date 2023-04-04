using Bitstamp.Net.Enums;
using Newtonsoft.Json;
using System;

namespace Bitstamp.Net.Objects
{
    /// <summary>
    /// Class for BitstampUserTransactions
    /// 
    /// As class has a high need for maintenance (Bitstamp adding new coins = new properties), this class gets dynamic generated in Client-Method
    /// </summary>
    public class BitstampUserTransaction
    {
        /// <summary>
        /// Transaction Time
        /// </summary>
        [JsonProperty("datetime")]
        public DateTime TransactionTime { get; set; }
        /// <summary>
        /// Fee Amount
        /// </summary>
        [JsonProperty("fee")]
        public decimal FeeAmount { get; set; }
        /// <summary>
        /// Fee Asset
        /// </summary>
        public string FeeAsset
        {
            get
            {
                if (Type == UserTransactionType.MarketTrade)
                {
                    var split = tradingPair.Split('_');
                    return split[1].ToUpper();
                }
                else if (Type == UserTransactionType.Withdrawal)
                {
                    return FromAsset;
                }
                else return string.Empty;
            }
        }
        /// <summary>
        /// Transaction ID
        /// </summary>
        [JsonProperty("id")]
        public long Id { get; set; }
        /// <summary>
        /// Transaction Type
        /// </summary>
        [JsonProperty("type")]
        public UserTransactionType Type { get; set; }
        /// <summary>
        /// Order ID
        /// </summary>
        [JsonProperty("order_id")]
        public long OrderId { get; set; }
        /// <summary>
        /// Exchange Rate
        /// </summary>
        public decimal ExchangeRate { get; set; }
        /// <summary>
        /// Start Asset
        /// </summary>
        public string FromAsset { get; set; }
        /// <summary>
        /// Destination Asset (if double-sided transaction)
        /// </summary>
        public string ToAsset { get; set; }
        /// <summary>
        /// Start Amount
        /// </summary>
        public decimal FromAmount { get; set; }
        /// <summary>
        /// To Amount (if double-sided transaction)
        /// </summary>
        public decimal ToAmount { get; set; }
        /// <summary>
        /// Indicates, if a market trade was Buy or Sell
        /// </summary>
        public bool IsBuyer
        {
            get
            {
                if (Type == UserTransactionType.MarketTrade)
                {
                    var pair = tradingPair.Split('_');
                    if (pair[0].ToUpper() == FromAsset)
                    {
                        return false;
                    }
                    else return true;
                }
                else return false;
            }
        }

        #region Currencies
        //TODO Bitstamp: Add more Currencies here

        /// <summary>
        /// USD amount
        /// </summary>
        [JsonProperty("usd")]
        private decimal Usd
        {
            set
            {
                if (value < 0.0m)
                {
                    FromAmount = value * -1;
                    FromAsset = "USD";
                }
                else if (value > 0.0m)
                {
                    ToAmount = value;
                    ToAsset = "USD";
                }
            }
        }
        /// <summary>
        /// EUR amount
        /// </summary>
        [JsonProperty("eur")]
        private decimal Eur
        {
            set
            {
                if (value < 0.0m)
                {
                    FromAmount = value * -1;
                    FromAsset = "EUR";
                }
                else if (value > 0.0m)
                {
                    ToAmount = value;
                    ToAsset = "EUR";
                }
            }
        }
        /// <summary>
        /// BTC amount
        /// </summary>
        [JsonProperty("btc")]
        private decimal btc
        {
            set
            {
                if (value < 0.0m)
                {
                    FromAmount = value * -1;
                    FromAsset = "BTC";
                }
                else if (value > 0.0m)
                {
                    ToAmount = value;
                    ToAsset = "BTC";
                }
            }
        }
        /// <summary>
        /// ETH amount
        /// </summary>
        [JsonProperty("eth")]
        private decimal Eth
        {
            set
            {
                if (value < 0.0m)
                {
                    FromAmount = value * -1;
                    FromAsset = "ETH";
                }
                else if (value > 0.0m)
                {
                    ToAmount = value;
                    ToAsset = "ETH";
                }
            }
        }
        /// <summary>
        /// XRP amount
        /// </summary>
        [JsonProperty("xrp")]
        private decimal Xrp
        {
            set
            {
                if (value < 0.0m)
                {
                    FromAmount = value * -1;
                    FromAsset = "XRP";
                }
                else if (value > 0.0m)
                {
                    ToAmount = value;
                    ToAsset = "XRP";
                }
            }
        }
        /// <summary>
        ///GBP amount
        /// </summary>
        [JsonProperty("gbp")]
        private decimal Gbp
        {
            set
            {
                if (value < 0.0m)
                {
                    FromAmount = value * -1;
                    FromAsset = "GBP";
                }
                else if (value > 0.0m)
                {
                    ToAmount = value;
                    ToAsset = "GBP";
                }
            }
        }
        /// <summary>
        ///BCH amount
        /// </summary>
        [JsonProperty("bch")]
        private decimal Bch
        {
            set
            {
                if (value < 0.0m)
                {
                    FromAmount = value * -1;
                    FromAsset = "BCH";
                }
                else if (value > 0.0m)
                {
                    ToAmount = value;
                    ToAsset = "BCH";
                }
            }
        }
        /// <summary>
        ///LTC amount
        /// </summary>
        [JsonProperty("ltc")]
        private decimal Ltc
        {
            set
            {
                if (value < 0.0m)
                {
                    FromAmount = value * -1;
                    FromAsset = "LTC";
                }
                else if (value > 0.0m)
                {
                    ToAmount = value;
                    ToAsset = "LTC";
                }
            }
        }
        /// <summary>
        ///PAX amount
        /// </summary>
        [JsonProperty("pax")]
        private decimal Pax
        {
            set
            {
                if (value < 0.0m)
                {
                    FromAmount = value * -1;
                    FromAsset = "PAX";
                }
                else if (value > 0.0m)
                {
                    ToAmount = value;
                    ToAsset = "PAX";
                }
            }
        }
        /// <summary>
        ///XLM amount
        /// </summary>
        [JsonProperty("xlm")]
        private decimal Xlm
        {
            set
            {
                if (value < 0.0m)
                {
                    FromAmount = value * -1;
                    FromAsset = "XLM";
                }
                else if (value > 0.0m)
                {
                    ToAmount = value;
                    ToAsset = "XLM";
                }
            }
        }
        /// <summary>
        ///ETH2 amount
        /// </summary>
        [JsonProperty("eth2")]
        private decimal Eth2
        {
            set
            {
                if (value < 0.0m)
                {
                    FromAmount = value * -1;
                    FromAsset = "ETH2";
                }
                else if (value > 0.0m)
                {
                    ToAmount = value;
                    ToAsset = "ETH2";
                }
            }
        }
        /// <summary>
        ///ETH2R amount
        /// </summary>
        [JsonProperty("eth2r")]
        private decimal Eth2R
        {
            set
            {
                if (value < 0.0m)
                {
                    FromAmount = value * -1;
                    FromAsset = "ETH2R";
                }
                else if (value > 0.0m)
                {
                    ToAmount = value;
                    ToAsset = "ETH2R";
                }
            }
        }
        /// <summary>
        ///LINK amount
        /// </summary>
        [JsonProperty("link")]
        private decimal Link
        {
            set
            {
                if (value < 0.0m)
                {
                    FromAmount = value * -1;
                    FromAsset = "LINK";
                }
                else if (value > 0.0m)
                {
                    ToAmount = value;
                    ToAsset = "LINK";
                }
            }
        }
        /// <summary>
        ///OMG amount
        /// </summary>
        [JsonProperty("omg")]
        private decimal Omg
        {
            set
            {
                if (value < 0.0m)
                {
                    FromAmount = value * -1;
                    FromAsset = "OMG";
                }
                else if (value > 0.0m)
                {
                    ToAmount = value;
                    ToAsset = "OMG";
                }
            }
        }
        /// <summary>
        ///USDC amount
        /// </summary>
        [JsonProperty("usdc")]
        private decimal Usdc
        {
            set
            {
                if (value < 0.0m)
                {
                    FromAmount = value * -1;
                    FromAsset = "USDC";
                }
                else if (value > 0.0m)
                {
                    ToAmount = value;
                    ToAsset = "USDC";
                }
            }
        }
        /// <summary>
        ///AAVE amount
        /// </summary>
        [JsonProperty("aave")]
        private decimal Aave
        {
            set
            {
                if (value < 0.0m)
                {
                    FromAmount = value * -1;
                    FromAsset = "AAVE";
                }
                else if (value > 0.0m)
                {
                    ToAmount = value;
                    ToAsset = "AAVE";
                }
            }
        }
        /// <summary>
        ///BAT amount
        /// </summary>
        [JsonProperty("bat")]
        private decimal Bat
        {
            set
            {
                if (value < 0.0m)
                {
                    FromAmount = value * -1;
                    FromAsset = "BAT";
                }
                else if (value > 0.0m)
                {
                    ToAmount = value;
                    ToAsset = "BAT";
                }
            }
        }
        /// <summary>
        ///UMA amount
        /// </summary>
        [JsonProperty("uma")]
        private decimal Uma
        {
            set
            {
                if (value < 0.0m)
                {
                    FromAmount = value * -1;
                    FromAsset = "UMA";
                }
                else if (value > 0.0m)
                {
                    ToAmount = value;
                    ToAsset = "UMA";
                }
            }
        }
        /// <summary>
        ///DAI amount
        /// </summary>
        [JsonProperty("dai")]
        private decimal Dai
        {
            set
            {
                if (value < 0.0m)
                {
                    FromAmount = value * -1;
                    FromAsset = "DAI";
                }
                else if (value > 0.0m)
                {
                    ToAmount = value;
                    ToAsset = "DAI";
                }
            }
        }
        /// <summary>
        ///KNC amount
        /// </summary>
        [JsonProperty("knc")]
        private decimal Knc
        {
            set
            {
                if (value < 0.0m)
                {
                    FromAmount = value * -1;
                    FromAsset = "KNC";
                }
                else if (value > 0.0m)
                {
                    ToAmount = value;
                    ToAsset = "KNC";
                }
            }
        }
        /// <summary>
        ///MKR amount
        /// </summary>
        [JsonProperty("mkr")]
        private decimal Mkr
        {
            set
            {
                if (value < 0.0m)
                {
                    FromAmount = value * -1;
                    FromAsset = "MKR";
                }
                else if (value > 0.0m)
                {
                    ToAmount = value;
                    ToAsset = "MKR";
                }
            }
        }
        /// <summary>
        ///ZRX amount
        /// </summary>
        [JsonProperty("zrx")]
        private decimal Zrx
        {
            set
            {
                if (value < 0.0m)
                {
                    FromAmount = value * -1;
                    FromAsset = "ZRX";
                }
                else if (value > 0.0m)
                {
                    ToAmount = value;
                    ToAsset = "ZRX";
                }
            }
        }
        /// <summary>
        ///GUSD amount
        /// </summary>
        [JsonProperty("gusd")]
        private decimal Gusd
        {
            set
            {
                if (value < 0.0m)
                {
                    FromAmount = value * -1;
                    FromAsset = "GUSD";
                }
                else if (value > 0.0m)
                {
                    ToAmount = value;
                    ToAsset = "GUSD";
                }
            }
        }
        /// <summary>
        ///ALGO amount
        /// </summary>
        [JsonProperty("algo")]
        private decimal Algo
        {
            set
            {
                if (value < 0.0m)
                {
                    FromAmount = value * -1;
                    FromAsset = "ALGO";
                }
                else if (value > 0.0m)
                {
                    ToAmount = value;
                    ToAsset = "ALGO";
                }
            }
        }
        /// <summary>
        ///AUDIO amount
        /// </summary>
        [JsonProperty("audio")]
        private decimal Audio
        {
            set
            {
                if (value < 0.0m)
                {
                    FromAmount = value * -1;
                    FromAsset = "AUDIO";
                }
                else if (value > 0.0m)
                {
                    ToAmount = value;
                    ToAsset = "AUDIO";
                }
            }
        }
        /// <summary>
        ///CRV amount
        /// </summary>
        [JsonProperty("crv")]
        private decimal Crv
        {
            set
            {
                if (value < 0.0m)
                {
                    FromAmount = value * -1;
                    FromAsset = "CRV";
                }
                else if (value > 0.0m)
                {
                    ToAmount = value;
                    ToAsset = "CRV";
                }
            }
        }
        /// <summary>
        ///SNX amount
        /// </summary>
        [JsonProperty("snx")]
        private decimal Snx
        {
            set
            {
                if (value < 0.0m)
                {
                    FromAmount = value * -1;
                    FromAsset = "SNX";
                }
                else if (value > 0.0m)
                {
                    ToAmount = value;
                    ToAsset = "SNX";
                }
            }
        }
        /// <summary>
        ///UNI amount
        /// </summary>
        [JsonProperty("uni")]
        private decimal Uni
        {
            set
            {
                if (value < 0.0m)
                {
                    FromAmount = value * -1;
                    FromAsset = "UNI";
                }
                else if (value > 0.0m)
                {
                    ToAmount = value;
                    ToAsset = "UNI";
                }
            }
        }
        /// <summary>
        ///YFI amount
        /// </summary>
        [JsonProperty("yfi")]
        private decimal Yfi
        {
            set
            {
                if (value < 0.0m)
                {
                    FromAmount = value * -1;
                    FromAsset = "YFI";
                }
                else if (value > 0.0m)
                {
                    ToAmount = value;
                    ToAsset = "YFI";
                }
            }
        }
        /// <summary>
        ///COMP amount
        /// </summary>
        [JsonProperty("comp")]
        private decimal Comp
        {
            set
            {
                if (value < 0.0m)
                {
                    FromAmount = value * -1;
                    FromAsset = "COMP";
                }
                else if (value > 0.0m)
                {
                    ToAmount = value;
                    ToAsset = "COMP";
                }
            }
        }
        /// <summary>
        ///GRT amount
        /// </summary>
        [JsonProperty("grt")]
        private decimal Grt
        {
            set
            {
                if (value < 0.0m)
                {
                    FromAmount = value * -1;
                    FromAsset = "GRT";
                }
                else if (value > 0.0m)
                {
                    ToAmount = value;
                    ToAsset = "GRT";
                }
            }
        }
        /// <summary>
        ///USDT amount
        /// </summary>
        [JsonProperty("usdt")]
        private decimal Usdt
        {
            set
            {
                if (value < 0.0m)
                {
                    FromAmount = value * -1;
                    FromAsset = "USDT";
                }
                else if (value > 0.0m)
                {
                    ToAmount = value;
                    ToAsset = "USDT";
                }
            }
        }
        /// <summary>
        ///EURT amount
        /// </summary>
        [JsonProperty("eurt")]
        private decimal Eurt
        {
            set
            {
                if (value < 0.0m)
                {
                    FromAmount = value * -1;
                    FromAsset = "EURT";
                }
                else if (value > 0.0m)
                {
                    ToAmount = value;
                    ToAsset = "EURT";
                }
            }
        }
        /// <summary>
        ///MATIC amount
        /// </summary>
        [JsonProperty("matic")]
        private decimal Matic
        {
            set
            {
                if (value < 0.0m)
                {
                    FromAmount = value * -1;
                    FromAsset = "MATIC";
                }
                else if (value > 0.0m)
                {
                    ToAmount = value;
                    ToAsset = "MATIC";
                }
            }
        }
        /// <summary>
        ///SUSHI amount
        /// </summary>
        [JsonProperty("sushi")]
        private decimal Sushi
        {
            set
            {
                if (value < 0.0m)
                {
                    FromAmount = value * -1;
                    FromAsset = "SUSHI";
                }
                else if (value > 0.0m)
                {
                    ToAmount = value;
                    ToAsset = "SUSHI";
                }
            }
        }
        /// <summary>
        ///CHZ amount
        /// </summary>
        [JsonProperty("chz")]
        private decimal Chz
        {
            set
            {
                if (value < 0.0m)
                {
                    FromAmount = value * -1;
                    FromAsset = "CHZ";
                }
                else if (value > 0.0m)
                {
                    ToAmount = value;
                    ToAsset = "CHZ";
                }
            }
        }
        /// <summary>
        ///ENJ amount
        /// </summary>
        [JsonProperty("enj")]
        private decimal Enj
        {
            set
            {
                if (value < 0.0m)
                {
                    FromAmount = value * -1;
                    FromAsset = "ENJ";
                }
                else if (value > 0.0m)
                {
                    ToAmount = value;
                    ToAsset = "ENJ";
                }
            }
        }
        /// <summary>
        ///HBAR amount
        /// </summary>
        [JsonProperty("hbar")]
        private decimal Hbar
        {
            set
            {
                if (value < 0.0m)
                {
                    FromAmount = value * -1;
                    FromAsset = "HBAR";
                }
                else if (value > 0.0m)
                {
                    ToAmount = value;
                    ToAsset = "HBAR";
                }
            }
        }
        /// <summary>
        ///ALPHA amount
        /// </summary>
        [JsonProperty("alpha")]
        private decimal Alpha
        {
            set
            {
                if (value < 0.0m)
                {
                    FromAmount = value * -1;
                    FromAsset = "ALPHA";
                }
                else if (value > 0.0m)
                {
                    ToAmount = value;
                    ToAsset = "ALPHA";
                }
            }
        }
        /// <summary>
        ///AXS amount
        /// </summary>
        [JsonProperty("axs")]
        private decimal Axs
        {
            set
            {
                if (value < 0.0m)
                {
                    FromAmount = value * -1;
                    FromAsset = "AXS";
                }
                else if (value > 0.0m)
                {
                    ToAmount = value;
                    ToAsset = "AXS";
                }
            }
        }
        /// <summary>
        ///FTT amount
        /// </summary>
        [JsonProperty("ftt")]
        private decimal Ftt
        {
            set
            {
                if (value < 0.0m)
                {
                    FromAmount = value * -1;
                    FromAsset = "FTT";
                }
                else if (value > 0.0m)
                {
                    ToAmount = value;
                    ToAsset = "FTT";
                }
            }
        }
        /// <summary>
        ///SAND amount
        /// </summary>
        [JsonProperty("sand")]
        private decimal Sand
        {
            set
            {
                if (value < 0.0m)
                {
                    FromAmount = value * -1;
                    FromAsset = "SAND";
                }
                else if (value > 0.0m)
                {
                    ToAmount = value;
                    ToAsset = "SAND";
                }
            }
        }
        /// <summary>
        ///STORJ amount
        /// </summary>
        [JsonProperty("storj")]
        private decimal Storj
        {
            set
            {
                if (value < 0.0m)
                {
                    FromAmount = value * -1;
                    FromAsset = "STORJ";
                }
                else if (value > 0.0m)
                {
                    ToAmount = value;
                    ToAsset = "STORJ";
                }
            }
        }
        #endregion

        #region Bitstamp Exchange Rate Properties
        //TODO Bitstamp: Add more currencies here
        internal string tradingPair;
        [JsonProperty("btc_usd")] private decimal btc_usd { set { ExchangeRate = value; tradingPair = "btc_usd"; } }
        [JsonProperty("btc_eur")] private decimal btc_eur { set { ExchangeRate = value; tradingPair = "btc_eur"; } }
        [JsonProperty("btc_gbp")] private decimal btc_gbp { set { ExchangeRate = value; tradingPair = "btc_gbp"; } }
        [JsonProperty("btc_pax")] private decimal btc_pax { set { ExchangeRate = value; tradingPair = "btc_pax"; } }
        [JsonProperty("btc_usdt")] private decimal btc_usdt { set { ExchangeRate = value; tradingPair = "btc_usdt"; } }
        [JsonProperty("btc_usdc")] private decimal btc_usdc { set { ExchangeRate = value; tradingPair = "btc_usdc"; } }
        [JsonProperty("gbp_usd")] private decimal gbp_usd { set { ExchangeRate = value; tradingPair = "gbp_usd"; } }
        [JsonProperty("gbp_eur")] private decimal gbp_eur { set { ExchangeRate = value; tradingPair = "gbp_eur"; } }
        [JsonProperty("eur_usd")] private decimal eur_usd { set { ExchangeRate = value; tradingPair = "eur_usd"; } }
        [JsonProperty("eth_usd")] private decimal eth_usd { set { ExchangeRate = value; tradingPair = "eth_usd"; } }
        [JsonProperty("eth_eur")] private decimal eth_eur { set { ExchangeRate = value; tradingPair = "eth_eur"; } }
        [JsonProperty("eth_btc")] private decimal eth_btc { set { ExchangeRate = value; tradingPair = "eth_btc"; } }
        [JsonProperty("eth_gbp")] private decimal eth_gbp { set { ExchangeRate = value; tradingPair = "eth_gbp"; } }
        [JsonProperty("eth_pax")] private decimal eth_pax { set { ExchangeRate = value; tradingPair = "eth_pax"; } }
        [JsonProperty("eth_usdt")] private decimal eth_usdt { set { ExchangeRate = value; tradingPair = "eth_usdt"; } }
        [JsonProperty("eth_usdc")] private decimal eth_usdc { set { ExchangeRate = value; tradingPair = "eth_usdc"; } }
        [JsonProperty("xrp_usd")] private decimal xrp_usd { set { ExchangeRate = value; tradingPair = "xrp_usd"; } }
        [JsonProperty("xrp_eur")] private decimal xrp_eur { set { ExchangeRate = value; tradingPair = "xrp_eur"; } }
        [JsonProperty("xrp_btc")] private decimal xrp_btc { set { ExchangeRate = value; tradingPair = "xrp_btc"; } }
        [JsonProperty("xrp_gbp")] private decimal xrp_gbp { set { ExchangeRate = value; tradingPair = "xrp_gbp"; } }
        [JsonProperty("xrp_pax")] private decimal xrp_pax { set { ExchangeRate = value; tradingPair = "xrp_pax"; } }
        [JsonProperty("xrp_usdt")] private decimal xrp_usdt { set { ExchangeRate = value; tradingPair = "xrp_usdt"; } }
        [JsonProperty("uni_usd")] private decimal uni_usd { set { ExchangeRate = value; tradingPair = "uni_usd"; } }
        [JsonProperty("uni_eur")] private decimal uni_eur { set { ExchangeRate = value; tradingPair = "uni_eur"; } }
        [JsonProperty("uni_btc")] private decimal uni_btc { set { ExchangeRate = value; tradingPair = "uni_btc"; } }
        [JsonProperty("ltc_usd")] private decimal ltc_usd { set { ExchangeRate = value; tradingPair = "ltc_usd"; } }
        [JsonProperty("ltc_eur")] private decimal ltc_eur { set { ExchangeRate = value; tradingPair = "ltc_eur"; } }
        [JsonProperty("ltc_btc")] private decimal ltc_btc { set { ExchangeRate = value; tradingPair = "ltc_btc"; } }
        [JsonProperty("ltc_gbp")] private decimal ltc_gbp { set { ExchangeRate = value; tradingPair = "ltc_gbp"; } }
        [JsonProperty("link_usd")] private decimal link_usd { set { ExchangeRate = value; tradingPair = "link_usd"; } }
        [JsonProperty("link_eur")] private decimal link_eur { set { ExchangeRate = value; tradingPair = "link_eur"; } }
        [JsonProperty("link_btc")] private decimal link_btc { set { ExchangeRate = value; tradingPair = "link_btc"; } }
        [JsonProperty("link_gbp")] private decimal link_gbp { set { ExchangeRate = value; tradingPair = "link_gbp"; } }
        [JsonProperty("link_eth")] private decimal link_eth { set { ExchangeRate = value; tradingPair = "link_eth"; } }
        [JsonProperty("matic_usd")] private decimal matic_usd { set { ExchangeRate = value; tradingPair = "matic_usd"; } }
        [JsonProperty("matic_eur")] private decimal matic_eur { set { ExchangeRate = value; tradingPair = "matic_eur"; } }
        [JsonProperty("xlm_usd")] private decimal xlm_usd { set { ExchangeRate = value; tradingPair = "xlm_usd"; } }
        [JsonProperty("xlm_eur")] private decimal xlm_eur { set { ExchangeRate = value; tradingPair = "xlm_eur"; } }
        [JsonProperty("xlm_btc")] private decimal xlm_btc { set { ExchangeRate = value; tradingPair = "xlm_btc"; } }
        [JsonProperty("xlm_gbp")] private decimal xlm_gbp { set { ExchangeRate = value; tradingPair = "xlm_gbp"; } }
        [JsonProperty("ftt_usd")] private decimal ftt_usd { set { ExchangeRate = value; tradingPair = "ftt_usd"; } }
        [JsonProperty("ftt_eur")] private decimal ftt_eur { set { ExchangeRate = value; tradingPair = "ftt_eur"; } }
        [JsonProperty("bch_usd")] private decimal bch_usd { set { ExchangeRate = value; tradingPair = "bch_usd"; } }
        [JsonProperty("bch_eur")] private decimal bch_eur { set { ExchangeRate = value; tradingPair = "bch_eur"; } }
        [JsonProperty("bch_btc")] private decimal bch_btc { set { ExchangeRate = value; tradingPair = "bch_btc"; } }
        [JsonProperty("bch_gbp")] private decimal bch_gbp { set { ExchangeRate = value; tradingPair = "bch_gbp"; } }
        [JsonProperty("aave_usd")] private decimal aave_usd { set { ExchangeRate = value; tradingPair = "aave_usd"; } }
        [JsonProperty("aave_eur")] private decimal aave_eur { set { ExchangeRate = value; tradingPair = "aave_eur"; } }
        [JsonProperty("aave_btc")] private decimal aave_btc { set { ExchangeRate = value; tradingPair = "aave_btc"; } }
        [JsonProperty("axs_usd")] private decimal axs_usd { set { ExchangeRate = value; tradingPair = "axs_usd"; } }
        [JsonProperty("axs_eur")] private decimal axs_eur { set { ExchangeRate = value; tradingPair = "axs_eur"; } }
        [JsonProperty("algo_usd")] private decimal algo_usd { set { ExchangeRate = value; tradingPair = "algo_usd"; } }
        [JsonProperty("algo_eur")] private decimal algo_eur { set { ExchangeRate = value; tradingPair = "algo_eur"; } }
        [JsonProperty("algo_btc")] private decimal algo_btc { set { ExchangeRate = value; tradingPair = "algo_btc"; } }
        [JsonProperty("comp_usd")] private decimal comp_usd { set { ExchangeRate = value; tradingPair = "comp_usd"; } }
        [JsonProperty("comp_eur")] private decimal comp_eur { set { ExchangeRate = value; tradingPair = "comp_eur"; } }
        [JsonProperty("comp_btc")] private decimal comp_btc { set { ExchangeRate = value; tradingPair = "comp_btc"; } }
        [JsonProperty("snx_usd")] private decimal snx_usd { set { ExchangeRate = value; tradingPair = "snx_usd"; } }
        [JsonProperty("snx_eur")] private decimal snx_eur { set { ExchangeRate = value; tradingPair = "snx_eur"; } }
        [JsonProperty("snx_btc")] private decimal snx_btc { set { ExchangeRate = value; tradingPair = "snx_btc"; } }
        [JsonProperty("hbar_usd")] private decimal hbar_usd { set { ExchangeRate = value; tradingPair = "hbar_usd"; } }
        [JsonProperty("hbar_eur")] private decimal hbar_eur { set { ExchangeRate = value; tradingPair = "hbar_eur"; } }
        [JsonProperty("chz_usd")] private decimal chz_usd { set { ExchangeRate = value; tradingPair = "chz_usd"; } }
        [JsonProperty("chz_eur")] private decimal chz_eur { set { ExchangeRate = value; tradingPair = "chz_eur"; } }
        [JsonProperty("enj_usd")] private decimal enj_usd { set { ExchangeRate = value; tradingPair = "enj_usd"; } }
        [JsonProperty("enj_eur")] private decimal enj_eur { set { ExchangeRate = value; tradingPair = "enj_eur"; } }
        [JsonProperty("bat_usd")] private decimal bat_usd { set { ExchangeRate = value; tradingPair = "bat_usd"; } }
        [JsonProperty("bat_eur")] private decimal bat_eur { set { ExchangeRate = value; tradingPair = "bat_eur"; } }
        [JsonProperty("bat_btc")] private decimal bat_btc { set { ExchangeRate = value; tradingPair = "bat_btc"; } }
        [JsonProperty("mkr_usd")] private decimal mkr_usd { set { ExchangeRate = value; tradingPair = "mkr_usd"; } }
        [JsonProperty("mkr_eur")] private decimal mkr_eur { set { ExchangeRate = value; tradingPair = "mkr_eur"; } }
        [JsonProperty("mkr_btc")] private decimal mkr_btc { set { ExchangeRate = value; tradingPair = "mkr_btc"; } }
        [JsonProperty("zrx_usd")] private decimal zrx_usd { set { ExchangeRate = value; tradingPair = "zrx_usd"; } }
        [JsonProperty("zrx_eur")] private decimal zrx_eur { set { ExchangeRate = value; tradingPair = "zrx_eur"; } }
        [JsonProperty("zrx_btc")] private decimal zrx_btc { set { ExchangeRate = value; tradingPair = "zrx_btc"; } }
        [JsonProperty("yfi_usd")] private decimal yfi_usd { set { ExchangeRate = value; tradingPair = "yfi_usd"; } }
        [JsonProperty("yfi_eur")] private decimal yfi_eur { set { ExchangeRate = value; tradingPair = "yfi_eur"; } }
        [JsonProperty("yfi_btc")] private decimal yfi_btc { set { ExchangeRate = value; tradingPair = "yfi_btc"; } }
        [JsonProperty("sushi_usd")] private decimal sushi_usd { set { ExchangeRate = value; tradingPair = "sushi_usd"; } }
        [JsonProperty("sushi_eur")] private decimal sushi_eur { set { ExchangeRate = value; tradingPair = "sushi_eur"; } }
        [JsonProperty("alpha_usd")] private decimal alpha_usd { set { ExchangeRate = value; tradingPair = "alpha_usd"; } }
        [JsonProperty("alpha_eur")] private decimal alpha_eur { set { ExchangeRate = value; tradingPair = "alpha_eur"; } }
        [JsonProperty("grt_usd")] private decimal grt_usd { set { ExchangeRate = value; tradingPair = "grt_usd"; } }
        [JsonProperty("grt_eur")] private decimal grt_eur { set { ExchangeRate = value; tradingPair = "grt_eur"; } }
        [JsonProperty("uma_usd")] private decimal uma_usd { set { ExchangeRate = value; tradingPair = "uma_usd"; } }
        [JsonProperty("uma_eur")] private decimal uma_eur { set { ExchangeRate = value; tradingPair = "uma_eur"; } }
        [JsonProperty("uma_btc")] private decimal uma_btc { set { ExchangeRate = value; tradingPair = "uma_btc"; } }
        [JsonProperty("omg_usd")] private decimal omg_usd { set { ExchangeRate = value; tradingPair = "omg_usd"; } }
        [JsonProperty("omg_eur")] private decimal omg_eur { set { ExchangeRate = value; tradingPair = "omg_eur"; } }
        [JsonProperty("omg_btc")] private decimal omg_btc { set { ExchangeRate = value; tradingPair = "omg_btc"; } }
        [JsonProperty("omg_gbp")] private decimal omg_gbp { set { ExchangeRate = value; tradingPair = "omg_gbp"; } }
        [JsonProperty("knc_usd")] private decimal knc_usd { set { ExchangeRate = value; tradingPair = "knc_usd"; } }
        [JsonProperty("knc_eur")] private decimal knc_eur { set { ExchangeRate = value; tradingPair = "knc_eur"; } }
        [JsonProperty("knc_btc")] private decimal knc_btc { set { ExchangeRate = value; tradingPair = "knc_btc"; } }
        [JsonProperty("crv_usd")] private decimal crv_usd { set { ExchangeRate = value; tradingPair = "crv_usd"; } }
        [JsonProperty("crv_eur")] private decimal crv_eur { set { ExchangeRate = value; tradingPair = "crv_eur"; } }
        [JsonProperty("crv_btc")] private decimal crv_btc { set { ExchangeRate = value; tradingPair = "crv_btc"; } }
        [JsonProperty("sand_usd")] private decimal sand_usd { set { ExchangeRate = value; tradingPair = "sand_usd"; } }
        [JsonProperty("sand_eur")] private decimal sand_eur { set { ExchangeRate = value; tradingPair = "sand_eur"; } }
        [JsonProperty("audio_usd")] private decimal audio_usd { set { ExchangeRate = value; tradingPair = "audio_usd"; } }
        [JsonProperty("audio_eur")] private decimal audio_eur { set { ExchangeRate = value; tradingPair = "audio_eur"; } }
        [JsonProperty("audio_btc")] private decimal audio_btc { set { ExchangeRate = value; tradingPair = "audio_btc"; } }
        [JsonProperty("storj_usd")] private decimal storj_usd { set { ExchangeRate = value; tradingPair = "storj_usd"; } }
        [JsonProperty("storj_eur")] private decimal storj_eur { set { ExchangeRate = value; tradingPair = "storj_eur"; } }
        [JsonProperty("usdt_usd")] private decimal usdt_usd { set { ExchangeRate = value; tradingPair = "usdt_usd"; } }
        [JsonProperty("usdt_eur")] private decimal usdt_eur { set { ExchangeRate = value; tradingPair = "usdt_eur"; } }
        [JsonProperty("usdc_usd")] private decimal usdc_usd { set { ExchangeRate = value; tradingPair = "usdc_usd"; } }
        [JsonProperty("usdc_eur")] private decimal usdc_eur { set { ExchangeRate = value; tradingPair = "usdc_eur"; } }
        [JsonProperty("usdc_usdt")] private decimal usdc_usdt { set { ExchangeRate = value; tradingPair = "usdc_usdt"; } }
        [JsonProperty("eurt_usd")] private decimal eurt_usd { set { ExchangeRate = value; tradingPair = "eurt_usd"; } }
        [JsonProperty("eurt_eur")] private decimal eurt_eur { set { ExchangeRate = value; tradingPair = "eurt_eur"; } }
        [JsonProperty("dai_usd")] private decimal dai_usd { set { ExchangeRate = value; tradingPair = "dai_usd"; } }
        [JsonProperty("pax_usd")] private decimal pax_usd { set { ExchangeRate = value; tradingPair = "pax_usd"; } }
        [JsonProperty("pax_eur")] private decimal pax_eur { set { ExchangeRate = value; tradingPair = "pax_eur"; } }
        [JsonProperty("pax_gbp")] private decimal pax_gbp { set { ExchangeRate = value; tradingPair = "pax_gbp"; } }
        [JsonProperty("eth2_eth")] private decimal eth2_eth { set { ExchangeRate = value; tradingPair = "eth2_eth"; } }
        [JsonProperty("gusd_usd")] private decimal gusd_usd { set { ExchangeRate = value; tradingPair = "gusd_usd"; } }
        #endregion

        /// <summary>
        /// Construktor
        /// </summary>
        public BitstampUserTransaction() { }

    }
}