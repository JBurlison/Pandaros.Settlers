using Pandaros.Settlers.Managers;
using Pandaros.Settlers.Monsters.Bosses;
using Pandaros.Settlers.Seasons;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pandaros.Settlers.Extender.Providers
{
    public class SeasonProvider : ISettlersExtension
    {
        public List<Type> LoadedAssembalies { get; } = new List<Type>();

        public string InterfaceName => nameof(ISeason);

        public void AfterAddingBaseTypes(Dictionary<string, ItemTypesServer.ItemTypeRaw> itemTypes)
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
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("");
            sb.AppendLine("-------------------Seasons Loaded----------------------");
            sb.AppendLine("");

            foreach (var s in LoadedAssembalies)
            {
                if (Activator.CreateInstance(s) is ISeason season &&
                    !string.IsNullOrEmpty(season.Name))
                {
                    sb.Append($"{season.Name}, ");
                    SeasonsFactory.AddSeason(season);
                }
            }

            SeasonsFactory.ResortSeasons();

            sb.AppendLine("");
            sb.AppendLine("---------------------------------------------------------");

            PandaLogger.Log(ChatColor.lime, sb.ToString());
        }
    }
}
