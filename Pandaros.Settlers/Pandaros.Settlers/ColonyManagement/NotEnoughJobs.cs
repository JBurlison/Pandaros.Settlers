using Happiness;
using Pandaros.API.localization;

namespace Pandaros.Settlers.ColonyManagement
{
    public class NotEnoughJobs : IHappinessCause
    {
        public static LocalizationHelper LocalizationHelper { get; private set; } = new LocalizationHelper(GameLoader.NAMESPACE, "Happiness");

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
