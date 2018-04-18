using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pandaros.Settlers.Items
{
    public interface IMagicItem
    {
        NPC.NPCBase Owner { get; set; }

        IMagicEffect Effect { get; set; }

        string Icon { get; set; }

        void Update();

        List<InventoryItem> CraftingCost { get; set; }
    }
}
