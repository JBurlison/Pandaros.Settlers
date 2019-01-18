using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BlockTypes;
using Pandaros.Settlers.Items.Reagents;
using Pandaros.Settlers.Models;
using UnityEngine;

namespace Pandaros.Settlers.Items.PlayerMagicItems
{
    public class HealthBoosterRecipe : ICSRecipe
    {
        public static string NAME = GameLoader.NAMESPACE + ".HealthboosterRecipe";

        public Dictionary<ItemId, int> Requirements { get; set; } = new Dictionary<ItemId, int>()
        {
            { ItemId.GetItemId(Adamantine.NAME), 20 },
            { ItemId.GetItemId(Elementium.Item.name), 30 },
            { ItemId.GetItemId(BuiltinBlocks.ScienceBagAdvanced), 30 },
            { ItemId.GetItemId(Mana.Item.name), 50 }
        };

        public Dictionary<ItemId, int> Results { get; set; } = new Dictionary<ItemId, int>()
        {
            { ItemId.GetItemId(HealthBooster.NAME), 1 }
        };

        public CraftPriority Priority { get; set; } = CraftPriority.Medium;

        public bool IsOptional { get; set; } = false;

        public int DefautLimit { get; set; } = 1;

        public string Job { get; set; } = Jobs.SorcererRegister.JOB_NAME;

        public string Name => NAME;
    }

    public class HealthBooster : CSType, IPlayerMagicItem
    {
        public static string NAME = GameLoader.NAMESPACE + ".Healthbooster";

        public override string Name { get; set; } = NAME;
        public override bool? isPlaceable => false;
        public override List<string> categories { get; set; } = new List<string>()
        {
            "MagicItem"
        };
        public override string icon { get; set; } = GameLoader.ICON_PATH + "HealthBooster.png";

        

        public float MovementSpeed { get; set; }

        public float JumpPower { get; set; }

        public float FlySpeed { get; set; }

        public float MoveSpeed { get; set; }

        public float LightRange { get; set; }

        public string LightColor { get; set; }

        public float FallDamage { get; set; }

        public float FallDamagePerUnit { get; set; }

        public float BuildDistance { get; set; }

        public bool IsMagical { get; set; }
        public float Skilled { get; set; }

        public float HPTickRegen { get; set; } = 5;

        public float MissChance { get; set; }

        public DamageType ElementalArmor { get; set; }

        public Dictionary<DamageType, float> AdditionalResistance { get; set; } = new Dictionary<DamageType, float>();

        public Dictionary<DamageType, float> Damage { get; set; } = new Dictionary<DamageType, float>();

        public float Luck { get; set; }

        public void Update()
        {
            
        }
    }
}
