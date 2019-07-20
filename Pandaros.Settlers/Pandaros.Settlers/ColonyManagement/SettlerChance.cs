using Happiness;
using Pandaros.API.localization;

namespace Pandaros.Settlers.ColonyManagement
{
    public class SettlerChance : IHappinessEffect
    {
        static LocalizationHelper _localization = new LocalizationHelper(GameLoader.NAMESPACE, "Settlers");

        public string GetDescription(Colony colony, Players.Player player)
        {
            float boost = GetSettlerChance(colony);

            return string.Format(_localization.LocalizeOrDefault("SettlerChance", player), boost * 100);
        }

        public static float GetSettlerChance(Colony colony)
        {
            var boost = colony.HappinessData.CachedHappiness * .005f;

            if (colony.HappinessData.CachedHappiness < 0)
                boost = colony.HappinessData.CachedHappiness * .02f;

            if (boost > .4f)
                boost = .4f;

            return (float)System.Math.Round(boost, 2);
        }
    }
}
