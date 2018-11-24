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

    public class LootTable : ILootTable, IJsonSerializable, IJsonDeserializable
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

        public void JsonDeerialize(JSONNode node)
        {
            if (node.TryGetAs(nameof(Name), out string name))
                Name = name;

            if (node.TryGetAs(nameof(LootPoolList), out JSONNode list))
                foreach (var item in list.LoopArray())
                    LootPoolList.Add(new LootPoolEntry(item));
        }

        public JSONNode JsonSerialize()
        {
            JSONNode node = new JSONNode();
            JSONNode lootPools = new JSONNode(NodeType.Array);

            node.SetAs(nameof(Name), Name);

            foreach (var pool in LootPoolList)
                lootPools.AddToArray(pool.JsonSerialize());

            node.SetAs(nameof(LootPoolList), LootPoolList);

            return node;
        }
    }

    public class LootPoolEntry : IJsonSerializable, IJsonDeserializable
    {
        public string Item { get; private set; }

        public double Weight { get; private set; }

        public int MinCount { get; private set; }

        public int MaxCount { get; private set; }

        public LootPoolEntry(string item, int min, int max, double weight = 0)
        {
            Item = item;
            Weight = weight;
            MinCount = min;
            MaxCount = max;
        }

        public LootPoolEntry(JSONNode node)
        {
            JsonDeerialize(node);
        }

        public JSONNode JsonSerialize()
        {
            JSONNode node = new JSONNode();

            node.SetAs(nameof(Item), Item);
            node.SetAs(nameof(Weight), Weight);
            node.SetAs(nameof(MinCount), MinCount);
            node.SetAs(nameof(MaxCount), MaxCount);

            return node;
        }

        public void JsonDeerialize(JSONNode node)
        {
            if (node.TryGetAs(nameof(Item), out string itemName))
                Item = itemName;

            if (node.TryGetAs(nameof(Weight), out double wight))
                Weight = wight;

            if (node.TryGetAs(nameof(MinCount), out int minCount))
                MinCount = minCount;

            if (node.TryGetAs(nameof(Item), out int maxCount))
                MaxCount = maxCount;
        }
    }
}
