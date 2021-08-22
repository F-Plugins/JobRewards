using Cysharp.Threading.Tasks;
using OpenMod.API.Ioc;
using OpenMod.Unturned.Players;
using SDG.Unturned;

namespace Feli.JobRewards.API.Services
{
    [Service]
    public interface IRewardsService
    {
        UniTask GiveRewards(UnturnedPlayer player, EPlayerStat stat);
    }
}
