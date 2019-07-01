using Happiness;
using Pandaros.Settlers.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandaros.Settlers.ColonyManagement
{
    public class ColonistsBought : IHappinessCause
    {
        public static localization.LocalizationHelper LocalizationHelper { get; private set; } = new localization.LocalizationHelper("Happiness");

        public float Evaluate(Colony colony)
        {
            var cs = ColonyState.GetColonyState(colony);
            return cs.ColonistsBought * cs.Difficulty.UnhappyColonistsBought;
        }

        public string GetDescription(Colony colony, Players.Player player)
        {
            return LocalizationHelper.LocalizeOrDefault("ColonistsBought", player);
        }
    }
}
