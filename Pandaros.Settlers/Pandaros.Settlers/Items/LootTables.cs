using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandaros.Settlers.Items
{
    public static class LootTables
    {
        public static Dictionary<string, ILootTable> Lookup { get; private set; } = new Dictionary<string, ILootTable>();
    }
}
