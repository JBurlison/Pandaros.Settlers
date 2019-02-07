using Happiness;
using Pipliz.JSON;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandaros.Settlers.Extender.Providers
{
    class HappinessCausesProvider : IOnColonyCreated, IOnLoadingColony
    {
        public List<Type> LoadedAssembalies { get; } = new List<Type>();

        public string InterfaceName { get; } = nameof(IHappinessCause);

        public Type ClassType => null;

        public void ColonyCreated(Colony c)
        {
            StringBuilder sb = new StringBuilder();
            PandaLogger.Log(ChatColor.lime, "-------------------Happiness Cause Loaded----------------------");
            var i = 0;

            foreach (var item in LoadedAssembalies)
            {
                if (Activator.CreateInstance(item) is IHappinessCause cause)
                {
                    c.HappinessData.HappinessCauses.Add(cause);
                    sb.Append($"{cause.GetType().Name}, ");
                    i++;

                    if (i > 5)
                    {
                        sb.Append("</color>");
                        i = 0;
                        sb.AppendLine();
                        sb.Append("<color=lime>");
                    }
                }
            }

            PandaLogger.Log(ChatColor.lime, sb.ToString());
            PandaLogger.Log(ChatColor.lime, "---------------------------------------------------------");
        }

        public void OnLoadingColony(Colony c, JSONNode n)
        {
            ColonyCreated(c);
        }
    }
}
