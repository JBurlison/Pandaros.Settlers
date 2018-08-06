using Pandaros.Settlers.Items;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandaros.Settlers.Monsters.Bosses
{
    public class BossLoot : LootTable
    {
        public const string LootTableName = "Pandaros.Settlers.Monsters.Bosses";

        public override string Name => LootTableName;

        public BossLoot()
        {
            LootPoolList.Add(new LootPoolEntry(Mana.Item.name, 1, 10));
        }
    }

   
}
