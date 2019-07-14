using Happiness;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandaros.Settlers.ColonyManagement
{
    public class SkillChance : IHappinessEffect
    {
        static localization.LocalizationHelper _localization = new localization.LocalizationHelper(GameLoader.NAMESPACE, "Settlers");

        public string GetDescription(Colony colony, Players.Player player)
        {
            float boost = GetSkillChance(colony);

            return string.Format(_localization.LocalizeOrDefault("SkillChance", player), boost * 100);
        }

        public static float GetSkillChance(Colony colony)
        {
            var boost = colony.HappinessData.CachedHappiness * .001f;

            if (colony.HappinessData.CachedHappiness < 0)
                boost = colony.HappinessData.CachedHappiness * .002f;

            if (boost > .25f)
                boost = .25f;


            if (boost < -.25)
                boost = -.25f;

            return (float)System.Math.Round(boost, 2);
        }
    }
}
