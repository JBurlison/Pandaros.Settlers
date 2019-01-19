using Pipliz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Pandaros.Settlers.Items.Armor.ArmorFactory;

namespace Pandaros.Settlers.Items.Armor
{
    public class ArmorMetadata : IArmor
    {
        public ArmorMetadata(float armorRating, int durability, string name,
                             ItemTypesServer.ItemTypeRaw itemType, ArmorSlot slot)
        {
            ArmorRating = armorRating;
            Durability = durability;
            this.name = name;
            ItemType = itemType;
            Slot = slot;
        }

        public float ArmorRating { get; }

        public int Durability { get; set; }

        public string name { get; }

        public ItemTypesServer.ItemTypeRaw ItemType { get; }

        public ArmorSlot Slot { get; }

        public float HPTickRegen => 0;

        public float MissChance => 0;

        public DamageType ElementalArmor => DamageType.Physical;

        public Dictionary<DamageType, float> AdditionalResistance => new Dictionary<DamageType, float>();

        public Dictionary<DamageType, float> Damage => new Dictionary<DamageType, float>();

        public Vector3Int Position { get; set; }

        public float Luck => 0;

        public float Skilled { get; set; }
        public bool IsMagical { get; set; }

        public void Update()
        {

        }
    }
}
