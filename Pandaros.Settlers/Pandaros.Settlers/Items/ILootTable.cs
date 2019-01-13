using Pipliz.JSON;
using System.Collections.Generic;
using System.Linq;

namespace Pandaros.Settlers.Items
{
    public interface ILootTable : INameable
    {
        List<LootPoolEntry> LootPoolList { get; }

        Dictionary<ushort, int> GetDrops(double luckModifier);
    }

    public class LootTable : ILootTable
    {
        public virtual string Name { get; private set; }

        public virtual List<LootPoolEntry> LootPoolList { get; private set; } = new List<LootPoolEntry>();

        public Dictionary<ushort, int> GetDrops(double luckModifier = 0)
        {
            var dic = new Dictionary<ushort, int>();

            double weightSum = 0;
            double roll = Pipliz.Random.Next() + luckModifier;

            foreach (LootPoolEntry drop in LootPoolList)
            {
                weightSum += drop.Weight;

                if (roll > weightSum && ItemTypes.IndexLookup.IndexLookupTable.TryGetItem(drop.Item, out ItemTypes.ItemType itemAction))
                {
                    dic[itemAction.GetRootParentType().ItemIndex] = Pipliz.Random.Next(drop.MinCount, drop.MaxCount + 1);
                }
            }

            return dic;
        }
    }

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
