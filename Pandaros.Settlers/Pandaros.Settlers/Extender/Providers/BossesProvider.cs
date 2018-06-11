using Pandaros.Settlers.Managers;
using Pandaros.Settlers.Monsters.Bosses;
using System;
using System.Collections.Generic;

namespace Pandaros.Settlers.Extender.Providers
{
    public class BossesProvider : ISettlersExtension
    {
        public List<Type> LoadedAssembalies { get; } = new List<Type>();

        public string InterfaceName => nameof(IPandaBoss);

        public void ActiveAssemblies()
        {
            foreach (var monster in LoadedAssembalies)
            {
                if (Activator.CreateInstance(monster) is IPandaBoss pandaBoss &&
                    !string.IsNullOrEmpty(pandaBoss.Name))
                {
                    PandaLogger.Log($"Boss {pandaBoss.Name} Loaded!");
                    MonsterManager.AddBoss(pandaBoss);
                }
            }
        }
    }
}
