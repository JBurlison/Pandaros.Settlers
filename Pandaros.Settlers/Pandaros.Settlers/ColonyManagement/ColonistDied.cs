using Happiness;
using Newtonsoft.Json;
using Pandaros.Settlers.Items;
using Pipliz.JSON;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ItemTypesServer;
using Pandaros.Settlers.Extender;
using Pandaros.Settlers.Entities;
using Pipliz;

namespace Pandaros.Settlers.ColonyManagement
{
    public class ColonistDiedTracker : IOnTimedUpdate
    {
        public double NextUpdateTimeMin => 10;

        public double NextUpdateTimeMax => 15;

        public double NextUpdateTime { get; set; }

        public void OnTimedUpdate()
        {
            foreach(var colonyKvp in ColonistDied.DieCount)
            {
                List<double> remove = new List<double>();

                foreach(var time in colonyKvp.Value)
                {
                    if (TimeCycle.TotalHours > time)
                        remove.Add(time);
                }

                foreach (var time in remove)
                    colonyKvp.Value.Remove(time);
            }
        }
    }


    [ModLoader.ModManager]
    public class ColonistDied : IHappinessCause
    {
        public static Dictionary<Colony, List<double>> DieCount { get; set; } = new Dictionary<Colony, List<double>>();
        public static localization.LocalizationHelper LocalizationHelper { get; private set; } = new localization.LocalizationHelper("Happiness");

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnNPCDied, GameLoader.NAMESPACE + ".ColonyManager.ColonistDied.OnNPCDied")]
        public static void OnNPCDied(NPC.NPCBase nPC)
        {
            var cs = ColonyState.GetColonyState(nPC.Colony);

            if (!DieCount.ContainsKey(nPC.Colony))
                DieCount.Add(nPC.Colony, new List<double>());

            DieCount[nPC.Colony].Add(TimeCycle.TotalHours + 24);
        }

        public float Evaluate(Colony colony)
        {
            var cs = ColonyState.GetColonyState(colony);

            if (DieCount.TryGetValue(colony, out var count))
                return (float)(count.Count * cs.Difficulty.UnhappinessPerColonistDeath) * -1f;

            return 0;
        }

        public string GetDescription(Colony colony, Players.Player player)
        {
            if (ColonistDied.DieCount.TryGetValue(colony, out var times))
            {
                var time = times.LastOrDefault();

                return string.Format(LocalizationHelper.LocalizeOrDefault("ColonistDied", player), time - TimeCycle.TotalTime.Value.Hours);
            }
            return "";
        }
    }

    
}
