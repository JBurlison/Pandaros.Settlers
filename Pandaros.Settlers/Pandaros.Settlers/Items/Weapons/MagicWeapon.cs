using Pandaros.Settlers.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandaros.Settlers.Items.Weapons
{
    public class MagicWeapon : CSType, IWeapon
    {
        public virtual int WepDurability { get; set; }

        public virtual float HPTickRegen { get; set; }

        public virtual float MissChance { get; set; }

        public virtual DamageType ElementalArmor { get; set; } = DamageType.Physical;

        public virtual Dictionary<DamageType, float> AdditionalResistance { get; set; } = new Dictionary<DamageType, float>();

        public virtual float Luck { get; set; }

        public virtual float Skilled { get; set; }

        public virtual bool IsMagical { get; set; }

        public virtual Dictionary<DamageType, float> Damage { get; set; } = new Dictionary<DamageType, float>();

        public void Update()
        {

        }
    }
}
