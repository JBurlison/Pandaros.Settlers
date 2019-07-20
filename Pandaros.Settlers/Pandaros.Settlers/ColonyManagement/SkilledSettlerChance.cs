using Happiness;
using Pandaros.API.localization;

namespace Pandaros.Settlers.ColonyManagement
{
    public class SkilledSettlerChance : IHappinessEffect
    {
        static LocalizationHelper _localization = new LocalizationHelper(GameLoader.NAMESPACE, "Settlers");

        public string GetDescription(Colony colony, Players.Player player)
        {
            float boost = GetSkilledSettlerChance(colony);

            return string.Format(_localization.LocalizeOrDefault("SkilledSettlerChance", player), boost * 100);
        }

        public static float GetSkilledSettlerChance(Colony colony)
        {
            var boost = colony.HappinessData.CachedHappiness * .002f;

            if (boost > .4f)
                boost = .4f;

            return (float)System.Math.Round(boost, 2);
        }
    }
}
