using Cysharp.Threading.Tasks;
using Feli.JobRewards.API.Models;
using Feli.JobRewards.API.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using OpenMod.API.Ioc;
using OpenMod.API.Plugins;
using OpenMod.Core.Helpers;
using OpenMod.Core.Users;
using OpenMod.Extensions.Economy.Abstractions;
using OpenMod.Unturned.Players;
using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Feli.JobRewards.Services
{
    [PluginServiceImplementation(Lifetime = ServiceLifetime.Singleton)]
    public class RewardsService : IRewardsService
    {
        private IEconomyProvider economyProvider;
        private IConfiguration configuration;
        private IStringLocalizer stringLocalizer;
        private IPluginAccessor<JobRewards> jobRewards;

        private List<RewardSession> rewardSessions;

        public RewardsService(
            IStringLocalizer stringLocalizer,
            IConfiguration configuration,
            IPluginAccessor<JobRewards> jobRewards,
            IEconomyProvider economyProvider)
        {
            this.stringLocalizer = stringLocalizer;
            this.configuration = configuration;
            this.jobRewards = jobRewards;
            this.economyProvider = economyProvider;
            this.rewardSessions = new List<RewardSession>();
            AsyncHelper.Schedule("RewardsService", () => UICleaner().AsTask());
        }

        public async UniTask GiveRewards(UnturnedPlayer player, EPlayerStat stat)
        {
            var session = rewardSessions.FirstOrDefault(x => x.PlayerId == player.SteamId);

            if (session == null)
            {
                rewardSessions.Add( GetNewRewardSession(player, stat) );
                session = rewardSessions.FirstOrDefault(x => x.PlayerId == player.SteamId);
            }
            else if (session.SessionType != stat)
            {
                rewardSessions.Remove(session);

                rewardSessions.Add( GetNewRewardSession(player, stat) );
                session = rewardSessions.FirstOrDefault(x => x.PlayerId == player.SteamId);
            }

            int money = 0;

            switch (stat)
            {
                case EPlayerStat.FOUND_RESOURCES:
                    money = configuration.GetSection("rewards:resources").Get<int>();
                    await economyProvider.UpdateBalanceAsync(player.SteamId.ToString(), KnownActorTypes.Player, money, "Working");
                    break;
                case EPlayerStat.FOUND_FISHES:
                    money = configuration.GetSection("rewards:fishing").Get<int>();
                    await economyProvider.UpdateBalanceAsync(player.SteamId.ToString(), KnownActorTypes.Player, money, "Working");
                    break;
                case EPlayerStat.KILLS_ANIMALS:
                    money = configuration.GetSection("rewards:animals").Get<int>();
                    await economyProvider.UpdateBalanceAsync(player.SteamId.ToString(), KnownActorTypes.Player, money, "Working");
                    break;
                case EPlayerStat.FOUND_PLANTS:
                    money = configuration.GetSection("rewards:plants").Get<int>();
                    await economyProvider.UpdateBalanceAsync(player.SteamId.ToString(), KnownActorTypes.Player, money, "Working");
                    break;
            }

            session.TotalSessionRewards += money;
            session.SessionRewardsCount++;
            session.ClearDate = DateTime.Now.AddSeconds(configuration.GetSection("uiSessionTime").Get<double>());

            await UIUpdate(session);
        }

        internal RewardSession GetNewRewardSession(UnturnedPlayer player, EPlayerStat stat)
        {
            return new RewardSession
            {
                PlayerId = player.SteamId,
                SessionRewardsCount = 0,
                SessionType = stat,
                ClearDate = DateTime.Now
            };
        }

        internal async UniTask UIUpdate(RewardSession rewardSession)
        {
            await UniTask.SwitchToMainThread();

            EffectManager.sendUIEffect(3463, 663, Provider.findTransportConnection(rewardSession.PlayerId), true,
                GetSessionTranslation("ui:title", rewardSession),
                GetSessionTranslation("ui:reward", rewardSession),
                GetSessionTranslation("ui:rewardCount", rewardSession));

            await UniTask.SwitchToThreadPool();
        }

        internal async UniTask UICleaner()
        {
            while (jobRewards.Instance.IsComponentAlive)
            {
                await UniTask.SwitchToMainThread();
                foreach (var session in rewardSessions.ToList())
                {
                    if ((DateTime.Now - session.ClearDate).TotalSeconds > configuration.GetSection("uiSessionTime").Get<double>())
                    {
                        EffectManager.askEffectClearByID(3463, Provider.findTransportConnection(session.PlayerId));
                        rewardSessions.Remove(session);
                    }
                }
                await UniTask.SwitchToThreadPool();
                await UniTask.Delay(1000);
            }
        }
       
        internal string GetSessionTranslation(string key, RewardSession rewardSession)
        {
            string sessionType = string.Empty;

            switch (rewardSession.SessionType)
            {
                case EPlayerStat.FOUND_RESOURCES:
                    sessionType = stringLocalizer["sessionTypes:resources"].Value;
                    break;
                case EPlayerStat.FOUND_FISHES:
                    sessionType = stringLocalizer["sessionTypes:fishing"].Value;
                    break;
                case EPlayerStat.KILLS_ANIMALS:
                    sessionType = stringLocalizer["sessionTypes:animals"].Value;
                    break;
                case EPlayerStat.FOUND_PLANTS:
                    sessionType = stringLocalizer["sessionTypes:plants"].Value;
                    break;
            }

            return stringLocalizer[key, new { RewardSession = rewardSession, SessionType = sessionType }].Value;
        }
    }
}
