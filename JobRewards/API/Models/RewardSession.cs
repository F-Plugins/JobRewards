using SDG.Unturned;
using Steamworks;
using System;

namespace Feli.JobRewards.API.Models
{
    public class RewardSession
    {
        public CSteamID PlayerId { get; set; }
        public EPlayerStat SessionType { get; set; }
        public int SessionRewardsCount { get; set; }
        public decimal TotalSessionRewards { get; set; }
        public DateTime ClearDate { get; set; }
    }
}
