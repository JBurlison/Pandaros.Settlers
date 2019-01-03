﻿using Pandaros.Settlers.Entities;
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
    public class SkilledLegs1Recipe : ICSRecipe
    {
        public static string NAME = GameLoader.NAMESPACE + ".SkilledLegs1Recipe";

        public Dictionary<ItemId, int> Requirements { get; set; } = new Dictionary<ItemId, int>()
        {
            { ItemId.GetItemId(Adamantine.NAME), 12 },
            { ItemId.GetItemId(AirStone.Item.name), 13 },
            { ItemId.GetItemId(EarthStone.Item.name), 13 },
            { ItemId.GetItemId(WaterStone.Item.name), 13 },
            { ItemId.GetItemId(FireStone.Item.name), 13 },
            { ItemId.GetItemId(Esper.Item.name), 11 },
            { ItemId.GetItemId("Pandaros.Settlers.SkilledLegs"), 1 }
        };

        public Dictionary<ItemId, int> Results { get; set; } = new Dictionary<ItemId, int>()
        {
            { ItemId.GetItemId(SkilledLegs1.NAME), 1 }
        };

        public CraftPriority Priority { get; set; } = CraftPriority.Medium;

        public bool IsOptional { get; set; } = true;

        public int DefautLimit { get; set; } = 1;

        public string Job { get; set; } = Jobs.SorcererRegister.JOB_NAME;

        public string Name => NAME;
    }

    public class SkilledLegs1 : CSType, IArmor, IPlayerMagicItem
    {
        public static string NAME = GameLoader.NAMESPACE + ".SkilledLegs1";

        public override string Name { get; set; } = NAME;
        public override bool? isPlaceable => false;
        public override List<string> categories { get; set; } = new List<string>()
        {
            "armor",
            "MagicItem"
        };

        public override string icon { get; set; } = GameLoader.ICON_PATH + "SkilledLegs1.png";

        public Players.Player Owner { get; set; }

        public float MovementSpeed { get; set; }

        public float JumpPower { get; set; }

        public float FlySpeed { get; set; }

        public float MoveSpeed { get; set; }

        public float LightRange { get; set; }

        public UnityEngine.Color LightColor { get; set; }

        public float FallDamage { get; set; }

        public float FallDamagePerUnit { get; set; }

        public float BuildDistance { get; set; }

        public bool IsMagical { get; set; } = true;
        public float Skilled { get; set; } = .02f;

        public float HPTickRegen { get; set; }

        public float MissChance { get; set; }

        public DamageType ElementalArmor { get; set; }

        public Dictionary<DamageType, float> AdditionalResistance { get; set; } = new Dictionary<DamageType, float>();

        public Dictionary<DamageType, float> Damage { get; set; } = new Dictionary<DamageType, float>();

        public float Luck { get; set; }

        public float ArmorRating { get; } = .13f;

        public int Durability { get; set; } = 500;

        public ItemTypesServer.ItemTypeRaw ItemType { get; }

        public ArmorSlot Slot { get; } = ArmorSlot.Legs;

        public void Update()
        {

        }
    }
}