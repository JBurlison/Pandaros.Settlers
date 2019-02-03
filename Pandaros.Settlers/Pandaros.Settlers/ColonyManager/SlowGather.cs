using Happiness;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandaros.Settlers.ColonyManager
{
    public class SlowGather : IHappinessEffect
    {
        public string GetDescription(Colony colony, Players.Player player)
        {
            var localizationHelper = new localization.LocalizationHelper("Happiness");

            // if (colony.HappinessData.CachedHappiness < 0)

            return ""; //localizationHelper.LocalizeOrDefault("SlowGather", player);
        }
    }
}
