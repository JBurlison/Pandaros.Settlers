using Happiness;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandaros.Settlers.Extender.Providers
{
    public class HappinessEffectProvider : IOnColonyCreated
    {
        public List<Type> LoadedAssembalies { get; } = new List<Type>();

        public string InterfaceName { get; } = nameof(IHappinessEffect);

        public Type ClassType => null;

        public void ColonyCreated(Colony c)
        {
            StringBuilder sb = new StringBuilder();
            PandaLogger.Log(ChatColor.lime, "-------------------Happiness Effects Loaded----------------------");
            var i = 0;

            foreach (var item in LoadedAssembalies)
            {
                if (Activator.CreateInstance(item) is IHappinessEffect effect)
                {
                    c.HappinessData.HappinessEffects.Add(effect);
                    sb.Append($"{effect.GetType().Name}, ");
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
