using Happiness;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandaros.Settlers.ColonyManager
{
    public class CloseBeds : IHappinessCause
    {
        public static Dictionary<Colony, int> CloseBedCount { get; set; } = new Dictionary<Colony, int>();

        public float Evaluate(Colony colony)
        {
            return 0;
        }

        public string GetDescription(Colony colony, Players.Player player)
        {
            return "";
        }
    }
}
