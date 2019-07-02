using Happiness;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandaros.Settlers.ColonyManagement
{
    public class SlowerGuards : IHappinessEffect
    {
        public string GetDescription(Colony colony, Players.Player player)
        {
            var localizationHelper = new localization.LocalizationHelper("Happiness");
            var name = "";
            var cs = Entities.ColonyState.GetColonyState(colony);

            if (colony.HappinessData.CachedHappiness < 20)
            {
                float percent = 0.05f;

                if (colony.HappinessData.CachedHappiness < 15)
                    percent = 0.10f;

                if (colony.HappinessData.CachedHappiness < 10)
                    percent = 0.15f;

                if (colony.HappinessData.CachedHappiness < 5)
                    percent = 0.20f;

                if (colony.HappinessData.CachedHappiness < 0)
                    percent = 0.25f;

                percent = percent * cs.Difficulty.UnhappyGuardsMultiplyRate;

                foreach (var colonist in colony.Followers)
                {
                    colonist.ApplyJobResearch();

                    if (colonist.Job != null && colonist.Job.IsValid && colonist.TryGetNPCGuardSettings(out var guardJobSettings) && colonist.TryGetNPCGuardDefaultSettings(out var defaultSettings))
                        guardJobSettings.CooldownShot = defaultSettings.CooldownShot + (defaultSettings.CooldownShot * percent);
                }
                
                name = localizationHelper.LocalizeOrDefault("SlowGuards", player) + " " + Math.Round((percent * 100), 2) + "%";
            }
            else
            {
                foreach (var colonist in colony.Followers)
                    colonist.ApplyJobResearch();
            }

            return name;
        }
    }
}
