using Happiness;
using Pandaros.API.localization;
using System.Linq;
using System.Collections.Generic;

namespace Pandaros.Settlers.ColonyManagement
{
    public class DecorHappiness : IHappinessCause
    {
        public static LocalizationHelper LocalizationHelper { get; private set; } = new LocalizationHelper(GameLoader.NAMESPACE, "Happiness");
        public static Dictionary<string, float> DecorBonuses { get; set; } = new Dictionary<string, float>();

        public float Evaluate(Colony colony)
        {
            return DecorBonuses.Sum(kvp => kvp.Value);
        }

        public string GetDescription(Colony colony, Players.Player player)
        {
            return LocalizationHelper.LocalizeOrDefault("Decor", player);
        }
    }
}
