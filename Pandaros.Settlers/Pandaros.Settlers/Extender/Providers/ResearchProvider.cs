using Pandaros.Settlers.Research;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pandaros.Settlers.Extender.Providers
{
    public class ResearchProvider : IOnAddResearchables
    {
        public List<Type> LoadedAssembalies { get; } = new List<Type>();

        public string InterfaceName => nameof(IPandaResearch);
        public Type ClassType => null;

        public void OnAddResearchables()
        {
            StringBuilder sb = new StringBuilder();
            PandaLogger.LogToFile("-------------------Research Loaded----------------------");
            var i = 0;

            foreach (var s in LoadedAssembalies)
            {
                if (Activator.CreateInstance(s) is IPandaResearch pandaResearch &&
                    !string.IsNullOrEmpty(pandaResearch.name))
                {
                    var research = new PandaResearchable(pandaResearch, 1);
                    research.ResearchComplete += pandaResearch.ResearchComplete;

                    if (pandaResearch.NumberOfLevels > 1)
                        for (var l = 2; l <= pandaResearch.NumberOfLevels; l++)
                        {
                            research = new PandaResearchable(pandaResearch, l);
                            research.ResearchComplete += pandaResearch.ResearchComplete;
                        }

                    sb.Append(pandaResearch.name + ", ");
                    pandaResearch.OnRegister();
                    i++;

                    if (i > 5)
                    {
                        i = 0;
                        sb.AppendLine();
                    }
                }
            }

            PandaLogger.LogToFile(sb.ToString());
            PandaLogger.LogToFile("---------------------------------------------------------");
        }
    }
}
