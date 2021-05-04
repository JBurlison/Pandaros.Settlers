using Pandaros.API;
using Pandaros.API.localization;
using Pandaros.API.Upgrades;

namespace Pandaros.Settlers.ColonyManagement
{
    public class SkilledSettlerChance : IPandaUpgrade
    {
        static LocalizationHelper _localization = new LocalizationHelper(GameLoader.NAMESPACE, "Settlers");
        public static string KEY => GameLoader.NAMESPACE + ".ColonyManagement.SkilledSettlerChance";

        public int LevelCount => 5;

        public string UniqueKey => KEY;

        public string GetDescription(Colony colony, Players.Player player)
        {
            float boost = GetSettlerChance(colony);

            return string.Format(_localization.LocalizeOrDefault("SkilledSettlerChance", player), boost * 100);
        }

        public static float GetSettlerChance(Colony colony, int level = -1)
        {
            if (level == -1)
                level = colony.GetUpgradeLevel(KEY);

            return level * .05f;
        }

        public void GetLocalizedValues(Players.Player player, Colony colony, int unlockedLevelCount, out string upgradeName, out string currentResults, out string nextResults)
        {
            upgradeName = _localization.LocalizeOrDefault("SkilledSettlerChance", player);
            currentResults = string.Format(_localization.LocalizeOrDefault("SkilledSettlerChancepct", player), GetSettlerChance(colony) * 100);
            nextResults = string.Format(_localization.LocalizeOrDefault("SkilledSettlerChancepct", player), GetSettlerChance(colony, colony.GetUpgradeLevel(KEY) + 1) * 100);
        }

        public long GetUpgradeCost(int unlockedLevels)
        {
            return (unlockedLevels * 500) + 1000;
        }

        public void ApplyLevel(Colony colony, int unlockedLevels, bool isLoading)
        {

        }

        public bool AppliesToColony(Colony colony)
        {
            return true;
        }
    }
}
