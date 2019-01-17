using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Pandaros.Settlers.Items
{
    public class PlayerMagicItem : CSType, IPlayerMagicItem
    {
        

        public virtual float MovementSpeed { get; set; }

        public virtual float JumpPower { get; set; }

        public virtual float FlySpeed { get; set; }

        public virtual float MoveSpeed { get; set; }

        public virtual float LightRange { get; set; }

        public virtual Color LightColor { get; set; }

        public virtual float FallDamage { get; set; }

        public virtual float FallDamagePerUnit { get; set; }

        public virtual float BuildDistance { get; set; }

        public virtual bool IsMagical { get; set; }
        public virtual float Skilled { get; set; }

        public virtual float HPTickRegen { get; set; }

        public virtual float MissChance { get; set; }

        public virtual DamageType ElementalArmor { get; set; }

        public virtual Dictionary<DamageType, float> AdditionalResistance { get; set; }

        public virtual Dictionary<DamageType, float> Damage { get; set; }

        public virtual float Luck { get; set; }

        public virtual void Update()
        {
            
        }
    }
}
