using Happiness;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandaros.Settlers.ColonyManagement
{
    public class NotEnoughJobs : IHappinessCause
    {
        public static localization.LocalizationHelper LocalizationHelper { get; private set; } = new localization.LocalizationHelper("Happiness");

        public float Evaluate(Colony colony)
        {
            if (colony.LaborerCount > 10)
                return colony.LaborerCount - 10;
            else
                return 0;
        }

        public string GetDescription(Colony colony, Players.Player player)
        {
            return LocalizationHelper.LocalizeOrDefault("NotEnoughJobs", player);
        }
    }
}
