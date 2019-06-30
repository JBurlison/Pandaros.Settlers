using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandaros.Settlers.Models
{
    public class LootPoolEntry
    {
        public string Item { get; private set; }

        public double Weight { get; private set; }

        public int MinCount { get; private set; }

        public int MaxCount { get; private set; }

        public LootPoolEntry() { }

        public LootPoolEntry(string item, int min, int max, double weight = 0)
        {
            Item = item;
            Weight = weight;
            MinCount = min;
            MaxCount = max;
        }
    }
}
