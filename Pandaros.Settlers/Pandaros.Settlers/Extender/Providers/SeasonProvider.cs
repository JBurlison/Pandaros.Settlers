using Pandaros.Settlers.Managers;
using Pandaros.Settlers.Monsters.Bosses;
using Pandaros.Settlers.Seasons;
using System;
using System.Collections.Generic;

namespace Pandaros.Settlers.Extender.Providers
{
    public class SeasonProvider : ISettlersExtension
    {
        public List<Type> LoadedAssembalies { get; } = new List<Type>();

        public string InterfaceName => nameof(ISeason);

        public void ActiveAssemblies()
        {
            foreach (var s in LoadedAssembalies)
            {
                if (Activator.CreateInstance(s) is ISeason season &&
                    !string.IsNullOrEmpty(season.Name))
                {
                    PandaLogger.Log($"Season {season.Name} Loaded.");
                    SeasonsFactory.AddSeason(season);
                }
            }

            SeasonsFactory.ResortSeasons();
        }
    }
}
