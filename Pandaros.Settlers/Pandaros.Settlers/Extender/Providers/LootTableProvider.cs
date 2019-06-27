﻿using Pandaros.Settlers.Items;
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
            PandaLogger.LogToFile("-------------------Loot Tables Loaded----------------------");
            var i = 0;

            foreach (var item in LoadedAssembalies)
            {
                if (Activator.CreateInstance(item) is ILootTable lootTable &&
                    !string.IsNullOrEmpty(lootTable.name))
                {
                    LootTables.Lookup[lootTable.name] = lootTable;
                    sb.Append($"{lootTable.name}, ");
                    i++;

                    if (i > 5)
                    {
                        i = 0;
                        sb.AppendLine();
                    }
                }
            }

            PandaLogger.LogToFile(sb.ToString());
            PandaLogger.LogToFile("---------------------------------------------------------");
        }
    }
}
