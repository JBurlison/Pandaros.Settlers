using Pandaros.Settlers.Items.Machines;
using Pandaros.Settlers.Jobs.Roaming;
using Pandaros.Settlers.Managers;
using Pandaros.Settlers.Monsters.Bosses;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pandaros.Settlers.Extender.Providers
{
    public class RoamingJobObjectiveProvider : ISettlersExtension
    {
        public List<Type> LoadedAssembalies { get; } = new List<Type>();

        public string InterfaceName => nameof(IRoamingJobObjective);
        public Type ClassType => null;

        public void AddItemTypes(Dictionary<string, ItemTypesServer.ItemTypeRaw> itemTypes)
        {
            
        }

        public void AfterItemTypesDefined()
        {
            StringBuilder sb = new StringBuilder();
            PandaLogger.Log(ChatColor.lime, "-------------------Roaming Job Objective Loaded----------------------");

            foreach (var s in LoadedAssembalies)
            {
                if (Activator.CreateInstance(s) is IRoamingJobObjective roamingJobObjective &&
                    !string.IsNullOrEmpty(roamingJobObjective.Name))
                {
                    sb.Append($"{roamingJobObjective.Name}, ");
                    RoamingJobManager.RegisterObjectiveType(roamingJobObjective);
                }
            }

            PandaLogger.Log(ChatColor.lime, sb.ToString());
            PandaLogger.Log(ChatColor.lime, "---------------------------------------------------------");
        }

        public void AfterSelectedWorld()
        {
            
        }

        public void AfterWorldLoad()
        {
            
        }

        public void OnAddResearchables()
        {

        }
    }
}
