using Pandaros.Settlers.Entities;
using Pandaros.Settlers.Items.Reagents;
using Pandaros.Settlers.Models;
using Pipliz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Pandaros.Settlers.Items.Armor.ArmorFactory;

namespace Pandaros.Settlers.Items.Armor.Magical
{
    public class BootsOfFallingRecipe : ICSRecipe
    {
        public static string NAME = GameLoader.NAMESPACE + ".BootsOfFallingRecipe";

        public Dictionary<ItemId, int> Requirements { get; set; } = new Dictionary<ItemId, int>()
        {
            { ItemId.GetItemId(Adamantine.NAME), 20 },
            { ItemId.GetItemId(Elementium.Item.name), 30 },
            { ItemId.GetItemId(AirStone.Item.name), 30 },
            { ItemId.GetItemId(EarthStone.Item.name), 30 },
            { ItemId.GetItemId(WaterStone.Item.name), 30 },
            { ItemId.GetItemId(Esper.Item.name), 1 },
            { ItemId.GetItemId(Mana.Item.name), 50 }
        };

        public Dictionary<ItemId, int> Results { get; set; } = new Dictionary<ItemId, int>()
        {
            { ItemId.GetItemId(BootsOfFalling.NAME), 1 }
        };

        public CraftPriority Priority { get; set; } = CraftPriority.Medium;

        public bool IsOptional { get; set; } = false;

        public int DefautLimit { get; set; } = 1;

        public string Job { get; set; } = Jobs.SorcererRegister.JOB_NAME;

        public string Name => NAME;
    }

    public class BootsOfFalling : CSType, IArmor, IPlayerMagicItem
    {
        public static string NAME = GameLoader.NAMESPACE + ".BootsOfFalling";

        public override string Name { get; set; } = NAME;
        public override bool? isPlaceable => false;
        public override List<string> categories { get; set; } = new List<string>()
        {
            "armor",
            "MagicItem"
        };

        public override string icon { get; set; } = GameLoader.ICON_PATH + "BootsOfFalling.png";

        

        public float MovementSpeed { get; set; }

        public float JumpPower { get; set; }

        public float FlySpeed { get; set; }

        public float MoveSpeed { get; set; }

        public float LightRange { get; set; }

        public string LightColor { get; set; }

        public float FallDamage { get; set; } = PlayerState.GetPlayerVariables().GetAs<float>("FallDamageBaseDamage") * -1;

        public float FallDamagePerUnit { get; set; } = PlayerState.GetPlayerVariables().GetAs<float>("FallDamagePerUnit") * -1;

        public float BuildDistance { get; set; }

        public bool IsMagical { get; set; } = true;
        public float Skilled { get; set; }

        public float HPTickRegen { get; set; }

        public float MissChance { get; set; }

        public DamageType ElementalArmor { get; set; }

        public Dictionary<DamageType, float> AdditionalResistance { get; set; } = new Dictionary<DamageType, float>();

        public Dictionary<DamageType, float> Damage { get; set; } = new Dictionary<DamageType, float>();

        public float Luck { get; set; }

        public float ArmorRating { get; }

        public int Durability { get; set; } = int.MaxValue;

        public ItemTypesServer.ItemTypeRaw ItemType { get; }

        public ArmorSlot Slot { get; } = ArmorSlot.Boots;

        public void Update()
        {

        }
    }
}
