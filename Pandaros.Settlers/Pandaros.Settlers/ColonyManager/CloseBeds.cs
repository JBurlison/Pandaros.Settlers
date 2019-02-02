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
    public class CloseBeds : IHappinessCause
    {
        private class BedStateObject
        {
            public Colony Colony { get; set; }
            public Dictionary<Bed, Vector3Int> Beds { get; set; } = new Dictionary<Bed, Vector3Int>();
        }

        public static Dictionary<Colony, int> CloseBedCount { get; set; } = new Dictionary<Colony, int>();

        public float Evaluate(Colony colony)
        {
            BedStateObject bso = new BedStateObject();
            bso.Colony = colony;
            ServerManager.BlockEntityTracker.BedTracker.Positions.Foreach(ForEachAction, ref bso);

            foreach (var bed in bso.Beds)
            {
                
            }

            return 0;
        }

        private void ForEachAction(Vector3Int position, Bed bed, ref BedStateObject stateObject)
        {
            if (bed.Colony == stateObject.Colony && !stateObject.Beds.ContainsKey(bed))
                stateObject.Beds.Add(bed, position);
        }

        public string GetDescription(Colony colony, Players.Player player)
        {
            return "";
        }
    }
}
