using Pandaros.Settlers.Monsters;
using Pandaros.Settlers.Monsters.Bosses;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pandaros.Settlers.Extender.Providers
{
    public class BossesProvider : IAfterWorldLoad
    {
        public List<Type> LoadedAssembalies { get; } = new List<Type>();

        public string InterfaceName => nameof(IPandaBoss);

        public Type ClassType => null;

        public void AfterWorldLoad()
        {
            StringBuilder sb = new StringBuilder();
            PandaLogger.LogToFile("-------------------Bosses Loaded----------------------");
            var i = 0;

            foreach (var monster in LoadedAssembalies)
            {
                if (Activator.CreateInstance(monster) is IPandaBoss pandaBoss &&
                    !string.IsNullOrEmpty(pandaBoss.name))
                {
                    sb.Append($"{pandaBoss.name}, ");
                    MonsterManager.AddBoss(pandaBoss);
                    i++;

                    if (i > 5)
                    {
                        i = 0;
                        sb.AppendLine();
                    }
                }
            }

            PandaLogger.LogToFile(sb.ToString());
            PandaLogger.LogToFile("------------------------------------------------------");
        }
    }
}
