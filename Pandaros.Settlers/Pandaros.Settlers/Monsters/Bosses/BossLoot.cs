using Pandaros.API.Items;
using Pandaros.API.Models;

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
