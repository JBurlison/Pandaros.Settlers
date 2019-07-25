using Happiness;
using Pandaros.API.localization;
using System.Linq;
using System.Collections.Generic;

namespace Pandaros.Settlers.ColonyManagement
{
    public class DecorHappiness : IHappinessCause
    {
        public static LocalizationHelper LocalizationHelper { get; private set; } = new LocalizationHelper(GameLoader.NAMESPACE, "Happiness");
        public static Dictionary<Colony, Dictionary<string, float>> DecorBonuses { get; set; } = new Dictionary<Colony, Dictionary<string, float>>();

        public float Evaluate(Colony colony)
        {
            if (DecorBonuses.ContainsKey(colony))
                return DecorBonuses[colony].Sum(kvp => kvp.Value);
            else
                return 0;
        }

        public string GetDescription(Colony colony, Players.Player player)
        {
            return LocalizationHelper.LocalizeOrDefault("Decor", player);
        }
    }
}
