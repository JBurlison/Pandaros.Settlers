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

namespace Pandaros.Settlers.ColonyManager
{
    public class ColonistDied : IHappinessCause
    {
        public static Dictionary<Colony, int> DieCount { get; set; } = new Dictionary<Colony, int>();

        public float Evaluate(Colony colony)
        {
            return 0;
        }

        public string GetDescription(Colony colony, Players.Player player)
        {
            return "";
        }
    }

    public class SlowGather : IHappinessEffect
    {
        public string GetDescription(Colony colony, Players.Player player)
        {
            throw new NotImplementedException();
        }
    }
}
