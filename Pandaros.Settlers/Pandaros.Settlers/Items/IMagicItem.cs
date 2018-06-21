using System.Collections.Generic;
using NPC;

namespace Pandaros.Settlers.Items
{
    public interface IMagicItem
    {
        string Name { get; }
        NPCBase Owner { get; set; }

        IMagicEffect Effect { get; }

        string Icon { get; }

        List<InventoryItem> CraftingCost { get; }

        void Update();
    }
}