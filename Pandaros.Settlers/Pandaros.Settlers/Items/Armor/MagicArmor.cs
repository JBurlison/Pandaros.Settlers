using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandaros.Settlers.Items.Armor
{
    public class MagicArmor : PlayerMagicItem, IArmor
    {
        public virtual ArmorFactory.ArmorSlot Slot { get; set; }

        public virtual float ArmorRating { get; set; }

        public virtual int Durability { get; set; }

        public virtual ItemTypesServer.ItemTypeRaw ItemType { get; set; }
    }
}
