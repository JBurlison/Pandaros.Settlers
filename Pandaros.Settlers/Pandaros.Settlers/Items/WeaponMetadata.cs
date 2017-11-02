using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pandaros.Settlers.Items
{
    public class WeaponMetadata
    {
        public MetalType Metal { get; private set; }

        public WeaponType WeaponType { get; private set; }

        public float Damage { get; private set; }

        public ItemTypesServer.ItemTypeRaw ItemType { get; private set; }

        public int Durability { get; set; }

        public WeaponMetadata(float damage, int durability, MetalType metalType, WeaponType weaponType, ItemTypesServer.ItemTypeRaw item)
        {
            Damage = damage;
            Metal = metalType;
            WeaponType = weaponType;
            ItemType = item;
            Durability = durability;
        }
    }
}
