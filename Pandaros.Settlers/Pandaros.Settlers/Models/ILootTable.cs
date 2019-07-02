using Pipliz.JSON;
using System.Collections.Generic;
using System.Linq;

namespace Pandaros.Settlers.Models
{
    public interface ILootTable : INameable
    {
        List<string> MonsterTypes { get; set; }

        List<LootPoolEntry> LootPoolList { get; }

        Dictionary<ushort, int> GetDrops(float luckModifier);
    }
}
