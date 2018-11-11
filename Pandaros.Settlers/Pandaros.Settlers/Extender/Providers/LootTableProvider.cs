using Pandaros.Settlers.Items;
using Pandaros.Settlers.Items.Armor;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pandaros.Settlers.Extender.Providers
{
    public class LootTableProvider : IAfterWorldLoad
    {
        public List<Type> LoadedAssembalies { get; } = new List<Type>();

        public string InterfaceName => nameof(ILootTable);
        public Type ClassType => null;

        public void AfterWorldLoad()
        {
            StringBuilder sb = new StringBuilder();
            PandaLogger.Log(ChatColor.lime, "-------------------Loot Tables Loaded----------------------");

            foreach (var item in LoadedAssembalies)
            {
                if (Activator.CreateInstance(item) is ILootTable lootTable &&
                    !string.IsNullOrEmpty(lootTable.Name))
                {
                    LootTables.Lookup[lootTable.Name] = lootTable;
                    sb.Append($"{lootTable.Name}, ");
                    
                }
            }

            PandaLogger.Log(ChatColor.lime, sb.ToString());
            PandaLogger.Log(ChatColor.lime, "---------------------------------------------------------");
        }
    }
}
