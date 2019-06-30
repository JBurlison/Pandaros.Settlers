using Pipliz.JSON;
using System.Collections.Generic;
using System.Linq;

namespace Pandaros.Settlers.Models
{
    public interface ILootTable : INameable
    {
        List<LootPoolEntry> LootPoolList { get; }

        Dictionary<ushort, int> GetDrops(double luckModifier);
    }
}
