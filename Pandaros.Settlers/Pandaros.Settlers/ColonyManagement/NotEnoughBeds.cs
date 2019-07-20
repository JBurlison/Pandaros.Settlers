using Happiness;
using Pandaros.API.localization;

namespace Pandaros.Settlers.ColonyManagement
{
    public class NotEnoughBeds : IHappinessCause
    {
        public static LocalizationHelper LocalizationHelper { get; private set; } = new LocalizationHelper(GameLoader.NAMESPACE, "Happiness");

        public float Evaluate(Colony colony)
        {
            var remainingBeds = colony.BedTracker.CalculateTotalBedCount() - colony.FollowerCount;
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
