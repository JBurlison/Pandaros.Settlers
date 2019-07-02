using Pandaros.Settlers.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandaros.Settlers.Items
{
    public class LootTable : ILootTable
    {
        public virtual string name { get; private set; }
        public virtual List<LootPoolEntry> LootPoolList { get; private set; } = new List<LootPoolEntry>();
        public virtual List<string> MonsterTypes { get; set; } = new List<string>();

        public Dictionary<ushort, int> GetDrops(double luckModifier = 0)
        {
            var dic = new Dictionary<ushort, int>();

            double weightSum = 0;
            double roll = Pipliz.Random.Next() + luckModifier;

            foreach (LootPoolEntry drop in LootPoolList)
            {
                weightSum += drop.Weight;

                if (roll > weightSum && ItemTypes.IndexLookup.StringLookupTable.TryGetItem(drop.Item, out ItemTypes.ItemType itemAction))
                {
                    dic[itemAction.GetRootParentType().ItemIndex] = Pipliz.Random.Next(drop.MinCount, drop.MaxCount + 1);
                }
            }

            return dic;
        }
    }
}
