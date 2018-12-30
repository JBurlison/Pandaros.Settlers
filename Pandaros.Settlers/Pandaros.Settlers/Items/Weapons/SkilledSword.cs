using Pandaros.Settlers.Items.Reagents;
using Pandaros.Settlers.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandaros.Settlers.Items.Weapons
{
    public class SkilledSwordRecipe : ICSRecipe
    {
        public static string NAME = GameLoader.NAMESPACE + ".SkilledSwordRecipe";

        public Dictionary<ItemId, int> Requirements { get; set; } = new Dictionary<ItemId, int>()
        {
            { ItemId.GetItemId(Adamantine.NAME), 2 },
            { ItemId.GetItemId(AirStone.Item.name), 3 },
            { ItemId.GetItemId(EarthStone.Item.name), 3 },
            { ItemId.GetItemId(WaterStone.Item.name), 3 },
            { ItemId.GetItemId(FireStone.Item.name), 3 },
            { ItemId.GetItemId(Esper.Item.name), 1 },
            { ItemId.GetItemId("Pandaros.Settlers.SteelSword"), 1 }
        };

        public Dictionary<ItemId, int> Results { get; set; } = new Dictionary<ItemId, int>()
        {
            { ItemId.GetItemId(SkilledSword.NAME), 1 }
        };

        public CraftPriority Priority { get; set; } = CraftPriority.Medium;

        public bool IsOptional { get; set; } = false;

        public int DefautLimit { get; set; } = 1;

        public string Job { get; set; } = Jobs.SorcererRegister.JOB_NAME;

        public string Name => NAME;
    }

    public class SkilledSword : CSType, IWeapon
    {
        public static string NAME = GameLoader.NAMESPACE + ".SkilledSword";

        public override string Name { get; set; } = NAME;

        public override bool? isPlaceable => false;
        public override List<string> categories { get; set; } = new List<string>()
        {
            "weapon",
            "MagicItem"
        };

        public override string icon { get; set; } = GameLoader.ICON_PATH + "SkilledSword.png";

        public ItemTypesServer.ItemTypeRaw ItemType { get; }

        public int Durability { get; set; } = 1000;

        public float HPTickRegen => 0;

        public float MissChance => 0;

        public DamageType ElementalArmor => DamageType.Physical;

        public Dictionary<DamageType, float> AdditionalResistance => new Dictionary<DamageType, float>();

        public float Luck => 0.01f;

        public float Skilled { get; set; } = .01f;
        public bool IsMagical { get; set; } = true;

        Dictionary<DamageType, float> IPandaDamage.Damage { get; } = new Dictionary<DamageType, float>()
        {
            { DamageType.Physical, 500f }
        };

        public void Update()
        {

        }
    }
}
