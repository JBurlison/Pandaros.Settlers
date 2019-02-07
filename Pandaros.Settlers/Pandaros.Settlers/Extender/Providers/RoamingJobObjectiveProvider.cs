using Pandaros.Settlers.Jobs.Roaming;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pandaros.Settlers.Extender.Providers
{
    public class RoamingJobObjectiveProvider : IAfterItemTypesDefined
    {
        public List<Type> LoadedAssembalies { get; } = new List<Type>();

        public string InterfaceName => nameof(IRoamingJobObjective);
        public Type ClassType => null;

        public void AfterItemTypesDefined()
        {
            StringBuilder sb = new StringBuilder();
            PandaLogger.Log(ChatColor.lime, "-------------------Roaming Job Objective Loaded----------------------");
            var i = 0;

            foreach (var s in LoadedAssembalies)
            {
                if (Activator.CreateInstance(s) is IRoamingJobObjective roamingJobObjective &&
                    !string.IsNullOrEmpty(roamingJobObjective.name))
                {
                    sb.Append($"{roamingJobObjective.name}, ");
                    RoamingJobManager.RegisterObjectiveType(roamingJobObjective);
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
    }
}
