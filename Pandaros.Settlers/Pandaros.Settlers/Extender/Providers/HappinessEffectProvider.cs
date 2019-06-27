using Happiness;
using Pipliz.JSON;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandaros.Settlers.Extender.Providers
{
    public class HappinessEffectProvider : IOnColonyCreated, IOnLoadingColony
    {
        public List<Type> LoadedAssembalies { get; } = new List<Type>();

        public string InterfaceName { get; } = nameof(IHappinessEffect);

        public Type ClassType => null;

        public void ColonyCreated(Colony c)
        {
            StringBuilder sb = new StringBuilder();
            PandaLogger.LogToFile("-------------------Happiness Effects Loaded----------------------");
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
                        i = 0;
                        sb.AppendLine();
                    }
                }
            }

            PandaLogger.LogToFile(sb.ToString());
            PandaLogger.LogToFile("---------------------------------------------------------");
        }

        public void OnLoadingColony(Colony c, JSONNode n)
        {
            ColonyCreated(c);
        }
    }
}
