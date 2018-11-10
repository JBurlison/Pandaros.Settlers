using Pandaros.Settlers.Research;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pandaros.Settlers.Extender.Providers
{
    public class ResearchProvider : ISettlersExtension
    {
        public List<Type> LoadedAssembalies { get; } = new List<Type>();

        public string InterfaceName => nameof(IPandaResearch);
        public Type ClassType => null;

        public void AddItemTypes(Dictionary<string, ItemTypesServer.ItemTypeRaw> itemTypes)
        {
            
        }

        public void AfterItemTypesDefined()
        {
           
        }

        public void AfterSelectedWorld()
        {
           
        }

        public void AfterWorldLoad()
        {
            
        }

        public void OnAddResearchables()
        {
            StringBuilder sb = new StringBuilder();
            PandaLogger.Log(ChatColor.lime, "-------------------Research Loaded----------------------");

            foreach (var s in LoadedAssembalies)
            {
                if (Activator.CreateInstance(s) is IPandaResearch pandaResearch &&
                    !string.IsNullOrEmpty(pandaResearch.Name))
                {
                    var research = new PandaResearch(pandaResearch.RequiredItems, 1, pandaResearch.Name, pandaResearch.BaseValue, pandaResearch.Dependancies, pandaResearch.BaseIterationCount, pandaResearch.AddLevelToName);
                    research.ResearchComplete += pandaResearch.ResearchComplete;
                    ServerManager.ScienceManager.RegisterResearchable(research);

                    for (var i = 2; i <= 5; i++)
                    {
                        research = new PandaResearch(pandaResearch.RequiredItems, i, pandaResearch.Name, pandaResearch.BaseValue, pandaResearch.Dependancies, pandaResearch.BaseIterationCount, pandaResearch.AddLevelToName);
                        research.ResearchComplete += pandaResearch.ResearchComplete;
                        ServerManager.ScienceManager.RegisterResearchable(research);
                    }

                    sb.Append(pandaResearch.Name + ", ");
                }
            }

            PandaLogger.Log(ChatColor.lime, "---------------------------------------------------------");
        }
    }
}
