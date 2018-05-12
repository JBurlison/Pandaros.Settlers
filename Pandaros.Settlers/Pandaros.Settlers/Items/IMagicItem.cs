using System.Collections.Generic;
using NPC;

namespace Pandaros.Settlers.Items
{
    public interface IMagicItem
    {
        string Name { get; set; }
        NPCBase Owner { get; set; }

        IMagicEffect Effect { get; set; }

        string Icon { get; set; }

        List<InventoryItem> CraftingCost { get; set; }

        void Update();
    }
}