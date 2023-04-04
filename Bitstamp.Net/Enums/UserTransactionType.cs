namespace Bitstamp.Net.Enums
{
    public enum UserTransactionType
    {
        Deposit = 0,
        Withdrawal = 1,
        MarketTrade = 2,
        SubAccountTransfer = 14,
        CreditedWithStakedAssets = 25,
        SentAssetsToStaking = 26,
        StakingReward = 27,
        ReferralReward = 32,
        InterAccountTransfer = 35
    }
}
