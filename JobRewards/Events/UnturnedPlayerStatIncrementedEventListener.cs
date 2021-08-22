using Feli.JobRewards.API.Services;
using OpenMod.API.Eventing;
using OpenMod.Unturned.Players.Stats.Events;
using SDG.Unturned;
using System.Threading.Tasks;

namespace Feli.JobRewards.Events
{
    public class UnturnedPlayerStatIncrementedEventListener : IEventListener<UnturnedPlayerStatIncrementedEvent>
    {
        private readonly IRewardsService rewardsService;

        public UnturnedPlayerStatIncrementedEventListener(IRewardsService rewardsService)
        {
            this.rewardsService = rewardsService;
        }

        public async Task HandleEventAsync(object sender, UnturnedPlayerStatIncrementedEvent @event)
        {
            if (@event.Stat == EPlayerStat.FOUND_RESOURCES || @event.Stat == EPlayerStat.FOUND_FISHES || @event.Stat == EPlayerStat.KILLS_ANIMALS || @event.Stat == EPlayerStat.FOUND_PLANTS)
                await rewardsService.GiveRewards(@event.Player, @event.Stat);
        }
    }
}
