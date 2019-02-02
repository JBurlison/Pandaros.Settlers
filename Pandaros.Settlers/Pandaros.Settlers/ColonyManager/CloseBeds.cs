using Happiness;
using Pipliz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static BlockEntities.Implementations.BedTracker;

namespace Pandaros.Settlers.ColonyManager
{
    [ModLoader.ModManager]
    public class CloseBeds : IHappinessCause
    {
        private class BedStateObject
        {
            public Colony Colony { get; set; }
            public Dictionary<Bed, Vector3Int> Beds { get; set; } = new Dictionary<Bed, Vector3Int>();
        }

        public static Dictionary<Colony, int> CachedHappiness { get; set; } = new Dictionary<Colony, int>();
        public static localization.LocalizationHelper LocalizationHelper { get; private set; } = new localization.LocalizationHelper("Happiness");


        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnTryChangeBlock, GameLoader.NAMESPACE + ".ColonyManager.CloseBeds.OnTryChangeBlockUser")]
        public static void OnTryChangeBlockUser(ModLoader.OnTryChangeBlockData d)
        {
            if (d.CallbackState == ModLoader.OnTryChangeBlockData.ECallbackState.Cancelled)
                return;

            if (IsBed(d.TypeNew.ItemIndex) || IsBed(d.TypeOld.ItemIndex))
                if (d.RequestOrigin.AsColony != null)
                    CalculateBeds(d.RequestOrigin.AsColony);
                else if (d.RequestOrigin.AsPlayer != null && d.RequestOrigin.AsPlayer.ActiveColony != null)
                    CalculateBeds(d.RequestOrigin.AsPlayer.ActiveColony);
        }

        public float Evaluate(Colony colony)
        {
            if (CachedHappiness.TryGetValue(colony, out var value))
                return value;
            else
                return 0;
        }

        public string GetDescription(Colony colony, Players.Player player)
        {
            return LocalizationHelper.LocalizeOrDefault("BedsClose", player);
        }

        public static void CalculateBeds(Colony colony)
        {
            BedStateObject bso = new BedStateObject();
            bso.Colony = colony;
            ServerManager.BlockEntityTracker.BedTracker.Positions.Foreach(ForEachAction, ref bso);

            Task.Run(() =>
            {
                int happiness = 0;

                foreach (var bed in bso.Beds)
                {
                    var bedEnd = bed.Value.Add(-1, 0, 0);

                    if (bed.Key.BedType == ColonyBuiltIn.ItemTypes.BEDXP)
                    {
                        bedEnd = bed.Value.Add(1, 0, 0);
                    }
                    else if (bed.Key.BedType == ColonyBuiltIn.ItemTypes.BEDZN)
                    {
                        bedEnd = bed.Value.Add(0, 0, -1);
                    }
                    else if (bed.Key.BedType == ColonyBuiltIn.ItemTypes.BEDZP)
                    {
                        bedEnd = bed.Value.Add(0, 0, 1);
                    }

                    if (!IsHappy(bed.Value, bedEnd))
                        happiness -= 1;
                }

                CachedHappiness[colony] = happiness;
            });
        }

        private static bool IsHappy(Vector3Int currentPos, Vector3Int ignorePos)
        {
            int count = 0;
            int touchingBeds = 0;

            var currentN = currentPos.Add(1, 0, 0);
            var currentS = currentPos.Add(-1, 0, 0);
            var currentE = currentPos.Add(0, 0, -1);
            var currentW = currentPos.Add(0, 0, 1);

            var ignoreN = ignorePos.Add(1, 0, 0);
            var ignoreS = ignorePos.Add(-1, 0, 0);
            var ignoreE = ignorePos.Add(0, 0, -1);
            var ignoreW = ignorePos.Add(0, 0, 1);

            EvaluateSpot(ignorePos, ref count, ref touchingBeds, currentN);
            EvaluateSpot(ignorePos, ref count, ref touchingBeds, currentS);
            EvaluateSpot(ignorePos, ref count, ref touchingBeds, currentE);
            EvaluateSpot(ignorePos, ref count, ref touchingBeds, currentW);

            EvaluateSpot(currentPos, ref count, ref touchingBeds, ignoreN);
            EvaluateSpot(currentPos, ref count, ref touchingBeds, ignoreS);
            EvaluateSpot(currentPos, ref count, ref touchingBeds, ignoreE);
            EvaluateSpot(currentPos, ref count, ref touchingBeds, ignoreW);

            return count > 0 && touchingBeds < 3;
        }

        private static void EvaluateSpot(Vector3Int ignorePos, ref int count, ref int touchingBeds, Vector3Int spot)
        {
            if (ignorePos != spot)
            {
                if (World.TryGetTypeAt(spot, out ushort type) && type != ColonyBuiltIn.ItemTypes.AIR)
                {
                    if (IsBed(type))
                        touchingBeds++;
                }
                else
                    count++;
            }
        }

        private static bool IsBed(ushort type)
        {
            return type == ColonyBuiltIn.ItemTypes.BEDXN ||
                   type == ColonyBuiltIn.ItemTypes.BEDXP ||
                   type == ColonyBuiltIn.ItemTypes.BEDZN ||
                   type == ColonyBuiltIn.ItemTypes.BEDZP ||
                   type == ColonyBuiltIn.ItemTypes.BEDENDXN ||
                   type == ColonyBuiltIn.ItemTypes.BEDENDXP ||
                   type == ColonyBuiltIn.ItemTypes.BEDENDZN ||
                   type == ColonyBuiltIn.ItemTypes.BEDENDZP;
        }

        private static void ForEachAction(Vector3Int position, Bed bed, ref BedStateObject stateObject)
        {
            if (bed.Colony == stateObject.Colony && !stateObject.Beds.ContainsKey(bed))
                stateObject.Beds.Add(bed, position);
        }
    }
}
