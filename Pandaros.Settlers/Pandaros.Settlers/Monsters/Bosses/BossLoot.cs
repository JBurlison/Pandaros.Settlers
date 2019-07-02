using Pandaros.Settlers.Items;
using Pandaros.Settlers.Models;
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

        public override string name => LootTableName;

        public BossLoot()
        {
            LootPoolList.Add(new LootPoolEntry(SettlersBuiltIn.ItemTypes.MANA, 1, 20));
            MonsterTypes.Add("Boss");
        }
    }

   
}
