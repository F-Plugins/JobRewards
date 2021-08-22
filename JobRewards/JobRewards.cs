using System;
using OpenMod.Unturned.Plugins;
using OpenMod.API.Plugins;

[assembly: PluginMetadata("Feli.JobRewards", 
    DisplayName = "JobRewards",
    Author = "Feli",
    Description = "A plugin that allows your users to get paid for working",
    Website = "discord.fplugins.com"
)]

namespace Feli.JobRewards
{
    public class JobRewards : OpenModUnturnedPlugin
    {
        public JobRewards(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }
    }
}
