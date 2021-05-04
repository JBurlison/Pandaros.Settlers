using Pandaros.API.Entities;
using Pandaros.API.Extender;
using Pandaros.API.localization;
using Pandaros.API.Models;
using System.Collections.Generic;
using System.Linq;

namespace Pandaros.Settlers.ColonyManagement
{
    public class ColonistBoughtTracker : IOnTimedUpdate
    {

        public int NextUpdateTimeMinMs => 10000;

        public int NextUpdateTimeMaxMs => 15000;

        ServerTimeStamp IOnTimedUpdate.NextUpdateTime { get; set; }

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

    public class ColonistsBought
    {
        public static Dictionary<Colony, List<double>> BoughtCount { get; set; } = new Dictionary<Colony, List<double>>();
    }
}