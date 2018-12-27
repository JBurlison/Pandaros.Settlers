using System.Collections.Generic;
using Pipliz;

namespace Pandaros.Settlers.Items.Weapons
{
    public class WeaponMetadata : IWeapon
    {
        Dictionary<DamageType, float> _damage = new Dictionary<DamageType, float>();

        public WeaponMetadata(float damage, int durability, string name, ItemTypesServer.ItemTypeRaw item)
        {
            _damage.Add(DamageType.Physical, damage);
            Name       = name;
            ItemType   = item;
            Durability = durability;
        }

        public string Name { get; }

        public ItemTypesServer.ItemTypeRaw ItemType { get; }

        public int Durability { get; set; }

        public float HPTickRegen => 0;

        public float MissChance => 0;

        public DamageType ElementalArmor => DamageType.Physical;

        public Dictionary<DamageType, float> AdditionalResistance => new Dictionary<DamageType, float>();

        public float Luck => 0;

        public float Skilled { get; set; }
        public bool IsMagical { get; set; }

        Dictionary<DamageType, float> IPandaDamage.Damage => _damage;

        public void Update()
        {
            
        }
    }
}