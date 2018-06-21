using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static Pandaros.Settlers.Items.ArmorFactory;

namespace Pandaros.Settlers.Items.Armor
{
    public interface IArmor
    {
        IMagicEffect MagicEffect { get; }
        ArmorSlot Slot { get; }
        float ArmorRating { get; }
        int Durability { get; set; }
        string Name { get; }
        ItemTypesServer.ItemTypeRaw ItemType { get; }
    }
}
