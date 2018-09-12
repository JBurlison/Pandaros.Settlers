using Pipliz.JSON;
using System.Collections.Generic;

// TODO: eveluate monster drops.
// Add lick to player and player magic items.
namespace Pandaros.Settlers.Items
{
    public interface ILootTable : INameable, IJsonSerializable
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

                if (roll > weightSum)
                {
                    if (ItemTypesServer.TryGetType(drop.Item, out var itemAction))
                    {
                        dic[itemAction.typeMain] = Pipliz.Random.Next(drop.MinCount, drop.MaxCount + 1);
                    }
                }
            }

            return dic;
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

    public class LootPoolEntry : IJsonSerializable
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

        public JSONNode JsonSerialize()
        {
            JSONNode node = new JSONNode();

            node.SetAs(nameof(Item), Item);
            node.SetAs(nameof(Weight), Weight);
            node.SetAs(nameof(MinCount), MinCount);
            node.SetAs(nameof(MaxCount), MaxCount);

            return node;
        }
    }
}
