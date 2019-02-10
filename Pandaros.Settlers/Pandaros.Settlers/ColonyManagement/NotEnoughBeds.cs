using Happiness;
using Pipliz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static BlockEntities.Implementations.BedTracker;

namespace Pandaros.Settlers.ColonyManagement
{
    public class NotEnoughBeds : IHappinessCause
    {
        public static localization.LocalizationHelper LocalizationHelper { get; private set; } = new localization.LocalizationHelper("Happiness");

        public float Evaluate(Colony colony)
        {
            var remainingBeds = ServerManager.BlockEntityTracker.BedTracker.CalculateBedCount(colony) - colony.FollowerCount;
            if (remainingBeds <= 0)
                return remainingBeds;
            else
                return 0;
        }

        public string GetDescription(Colony colony, Players.Player player)
        {
            return LocalizationHelper.LocalizeOrDefault("NotEnoughBeds", player);
        }
    }
}
