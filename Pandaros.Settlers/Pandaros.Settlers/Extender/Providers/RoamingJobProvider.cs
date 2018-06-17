using Pandaros.Settlers.Jobs.Roaming;
using Pipliz.Mods.APIProvider.Jobs;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pandaros.Settlers.Extender.Providers
{
    public class RoamingJobProvider : ISettlersExtension
    {
        public List<Type> LoadedAssembalies { get; } = new List<Type>();

        public string InterfaceName => null;
        public string ClassName => nameof(RoamingJob);

        private List<RoamingJob> _jobs = new List<RoamingJob>();

        public void AfterAddingBaseTypes(Dictionary<string, ItemTypesServer.ItemTypeRaw> itemTypes)
        {
            
        }

        public void AfterItemTypesDefined()
        {
            StringBuilder sb = new StringBuilder();
            PandaLogger.Log(ChatColor.lime, "-------------------Roaming Jobs Loaded----------------------");

            foreach (var monster in LoadedAssembalies)
            {
                if (Activator.CreateInstance(monster) is RoamingJob roamingJob &&
                    !string.IsNullOrEmpty(roamingJob.NPCTypeKey))
                {
                    BlockJobManagerTracker.Register<RoamingJob>(roamingJob.JobItemKey);
                    sb.Append($"{roamingJob.NPCTypeKey}, ");
                }
            }

            PandaLogger.Log(ChatColor.lime, sb.ToString());
            PandaLogger.Log(ChatColor.lime, "------------------------------------------------------");
        }

        public void AfterSelectedWorld()
        {
            
        }

        public void AfterWorldLoad()
        {
 
        }
    }
}
