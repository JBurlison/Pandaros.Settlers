using BlockTypes.Builtin;
using Pipliz.JSON;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pandaros.Settlers.Items.Machines
{
    [ModLoader.ModManager]
    public static class ElementalTurrets
    {
        public const string AIRTURRET = "Air Turret";
        public const string EARTHTURRET = "Earth Turret";
        public const string FIRETURRET = "Fire Turret";
        public const string WATERTURRET = "Water Turret";

        public static readonly string AIRTURRET_NAMESPACE = GameLoader.NAMESPACE + ".AirTurret";
        public static readonly string EARTHTURRET_NAMESPACE = GameLoader.NAMESPACE + ".EarthTurret";
        public static readonly string FIRETURRET_NAMESPACE = GameLoader.NAMESPACE + ".FireTurret";
        public static readonly string WATERTURRET_NAMESPACE = GameLoader.NAMESPACE + ".WaterTurret";


        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterSelectedWorld, GameLoader.NAMESPACE + ".Items.Machines.ElementalTurrets.RegisterAudio"),
            ModLoader.ModCallbackProvidesFor("pipliz.server.loadaudiofiles"), ModLoader.ModCallbackDependsOn("pipliz.server.registeraudiofiles")]
        public static void RegisterAudio()
        {
            GameLoader.AddSoundFile(AIRTURRET_NAMESPACE, new List<string>()
            {
                GameLoader.AUDIO_FOLDER_PANDA + "/AirTurret1.ogg",
                GameLoader.AUDIO_FOLDER_PANDA + "/AirTurret2.ogg",
                GameLoader.AUDIO_FOLDER_PANDA + "/AirTurret3.ogg",
                GameLoader.AUDIO_FOLDER_PANDA + "/AirTurret4.ogg",
                GameLoader.AUDIO_FOLDER_PANDA + "/AirTurret5.ogg"
            });
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterItemTypesDefined, GameLoader.NAMESPACE + ".Items.Machines.ElementalTurrets.RegisterTurret"),
            ModLoader.ModCallbackDependsOn(GameLoader.NAMESPACE + ".Items.Machines.Turret.RegisterTurret")]
        public static void RegisterTurret()
        {
            var planks = new InventoryItem(BuiltinBlocks.Planks, 50);
            var stone = new InventoryItem(BuiltinBlocks.StoneBricks, 100);
            var tools = new InventoryItem(BuiltinBlocks.CopperTools, 20);
            var mana = new InventoryItem(Mana.Item.ItemIndex, 4);
            var elemen = new InventoryItem(Elementium.Item.ItemIndex, 2);
            var airStone = new InventoryItem(AirStone.Item.ItemIndex, 5);
            var fireStone = new InventoryItem(FireStone.Item.ItemIndex, 5);
            var waterStone = new InventoryItem(WaterStone.Item.ItemIndex, 5);
            var earthStone = new InventoryItem(EarthStone.Item.ItemIndex, 5);
            var voidStone = new InventoryItem(Void.Item.ItemIndex, 4);

            var stonerecipe = new Recipe(AIRTURRET_NAMESPACE,
                                    new List<InventoryItem>() { planks, elemen, mana, tools, stone, airStone },
                                    new InventoryItem(Turret.TurretSettings[AIRTURRET].TurretItem.ItemIndex),
                                    5);

            RecipeStorage.AddOptionalLimitTypeRecipe(Jobs.AdvancedCrafterRegister.JOB_NAME, stonerecipe);
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterAddingBaseTypes, GameLoader.NAMESPACE + ".Items.Machines.ElementalTurrets.AddTurret"), ModLoader.ModCallbackDependsOn("pipliz.blocknpcs.addlittypes"), ModLoader.ModCallbackDependsOn(GameLoader.NAMESPACE + ".Items.Machines.Turret.AddTurret")]
        public static void AddTurret(Dictionary<string, ItemTypesServer.ItemTypeRaw> items)
        {

        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterSelectedWorld, GameLoader.NAMESPACE + ".Items.Machines.ElementalTurrets.AddTextures"), ModLoader.ModCallbackProvidesFor("pipliz.server.registertexturemappingtextures")]
        public static void AddTextures()
        {
            var textureMapping = new ItemTypesServer.TextureMapping(new JSONNode());
            textureMapping.AlbedoPath = GameLoader.TEXTURE_FOLDER_PANDA + "/albedo/AirTurret.png";
            textureMapping.NormalPath = GameLoader.TEXTURE_FOLDER_PANDA + "/normal/Turret.png";
            textureMapping.HeightPath = GameLoader.TEXTURE_FOLDER_PANDA + "/height/Turret.png";

            ItemTypesServer.SetTextureMapping(AIRTURRET_NAMESPACE + "sides", textureMapping);
        }

        private static void AddAirTurretTurretSettings()
        {
            var turretSettings = new Turret.TurretSetting()
            {
                TurretItem = Turret.TurretTypes[AIRTURRET],
                Ammo = new List<InventoryItem>() { new InventoryItem(AirStone.Item.ItemIndex) },
                AmmoValue = 0.20f,
                Damage = 500f,
                DurabilityPerDoWork = 0.008f,
                FuelPerDoWork = 0.02f,
                Name = AIRTURRET,
                OnShootAudio = AIRTURRET_NAMESPACE,
                OnHitAudio = AIRTURRET_NAMESPACE,
                Range = 25,
                WorkTime = 14f,
                RefuelTime = 8,
                ReloadTime = 9,
                RepairTime = 15,
                RequiredForFix = new Dictionary<float, List<InventoryItem>>()
                {
                    { 75f, new List<InventoryItem>() { new InventoryItem(BuiltinBlocks.StoneBricks, 1), new InventoryItem(BuiltinBlocks.SteelParts, 1), new InventoryItem(BuiltinBlocks.CopperNails, 1) } },
                    { 50f, new List<InventoryItem>() { new InventoryItem(BuiltinBlocks.StoneBricks, 1), new InventoryItem(BuiltinBlocks.SteelParts, 2), new InventoryItem(BuiltinBlocks.CopperNails, 2) } },
                    { 30f, new List<InventoryItem>() { new InventoryItem(BuiltinBlocks.StoneBricks, 1), new InventoryItem(BuiltinBlocks.Planks, 1), new InventoryItem(BuiltinBlocks.SteelParts, 3), new InventoryItem(BuiltinBlocks.CopperNails, 3), new InventoryItem(BuiltinBlocks.MatchlockGun, 1) } },
                    { 10f, new List<InventoryItem>() { new InventoryItem(BuiltinBlocks.StoneBricks, 1), new InventoryItem(BuiltinBlocks.Planks, 1), new InventoryItem(BuiltinBlocks.SteelParts, 4), new InventoryItem(BuiltinBlocks.CopperNails, 4), new InventoryItem(BuiltinBlocks.MatchlockGun, 2) } },
                },
                ProjectileAnimation = Managers.AnimationManager.AnimatedObjects[Managers.AnimationManager.LEADBULLET]
            };

            Turret.TurretSettings[AIRTURRET] = turretSettings;
        }

        private static void AddStoneTurret(Dictionary<string, ItemTypesServer.ItemTypeRaw> items)
        {
            var turretName = AIRTURRET_NAMESPACE;
            var turretNode = new JSONNode()
              .SetAs("icon", GameLoader.ICON_FOLDER_PANDA + "/StoneTurret.png")
              .SetAs("isPlaceable", true)
              .SetAs("onPlaceAudio", "stonePlace")
              .SetAs("onRemoveAudio", "stoneDelete")
              .SetAs("sideall", "stonebricks")
              .SetAs("onRemoveAmount", 1)
              .SetAs("isSolid", true)
              .SetAs("sidex+", AIRTURRET_NAMESPACE + "sides")
              .SetAs("sidex-", AIRTURRET_NAMESPACE + "sides")
              .SetAs("sidez+", AIRTURRET_NAMESPACE + "sides")
              .SetAs("sidez-", AIRTURRET_NAMESPACE + "sides")
              .SetAs("npcLimit", 0);

            var item = new ItemTypesServer.ItemTypeRaw(turretName, turretNode);
            Turret.TurretTypes[AIRTURRET] = item;
            items.Add(turretName, item);
        }
    }
}
