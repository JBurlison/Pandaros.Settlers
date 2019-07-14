using Happiness;
using Pandaros.Settlers.Entities;
using Pandaros.Settlers.Extender;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandaros.Settlers.ColonyManagement
{
    public class ColonistBoughtTracker : IOnTimedUpdate
    {
        public double NextUpdateTimeMin => 10;

        public double NextUpdateTimeMax => 15;

        public double NextUpdateTime { get; set; }

        public void OnTimedUpdate()
        {
            foreach (var colonyKvp in ColonistsBought.BoughtCount)
            {
                List<double> remove = new List<double>();

                foreach (var time in colonyKvp.Value)
                {
                    if (TimeCycle.TotalHours > time)
                        remove.Add(time);
                }

                foreach (var time in remove)
                    colonyKvp.Value.Remove(time);
            }
        }
    }

    public class ColonistsBought : IHappinessCause
    {
        public static Dictionary<Colony, List<double>> BoughtCount { get; set; } = new Dictionary<Colony, List<double>>();
        public static localization.LocalizationHelper LocalizationHelper { get; private set; } = new localization.LocalizationHelper(GameLoader.NAMESPACE, "Happiness");

        public float Evaluate(Colony colony)
        {
            var cs = ColonyState.GetColonyState(colony);

            if (cs.SettlersEnabled != Models.SettlersState.Disabled && BoughtCount.TryGetValue(colony, out var count))
                return (float)(count.Count * cs.Difficulty.UnhappyColonistsBought);

            return 0;
        }

        public string GetDescription(Colony colony, Players.Player player)
        {
            if (BoughtCount.TryGetValue(colony, out var times))
            {
                var time = times.LastOrDefault();

                return string.Format(LocalizationHelper.LocalizeOrDefault("ColonistsBought", player), System.Math.Round(time - TimeCycle.TotalHours, 1));
            }
            return "";
        }
    }
}
