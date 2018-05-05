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
        public const string VOIDTURRET = "Void Turret";

        public static readonly string AIRTURRET_NAMESPACE = GameLoader.NAMESPACE + ".AirTurret";
        public static readonly string EARTHTURRET_NAMESPACE = GameLoader.NAMESPACE + ".EarthTurret";
        public static readonly string FIRETURRET_NAMESPACE = GameLoader.NAMESPACE + ".FireTurret";
        public static readonly string WATERTURRET_NAMESPACE = GameLoader.NAMESPACE + ".WaterTurret";
        public static readonly string VOIDTURRET_NAMESPACE = GameLoader.NAMESPACE + ".VoidTurret";

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterItemTypesDefined, GameLoader.NAMESPACE + ".Items.Machines.ElementalTurrets.RegisterTurret"),
            ModLoader.ModCallbackProvidesFor(GameLoader.NAMESPACE + ".Items.Machines.Turret.RegisterTurret")]
        public static void RegisterTurret()
        {
            var planks = new InventoryItem(BuiltinBlocks.Planks, 50);
            var stone = new InventoryItem(BuiltinBlocks.StoneBricks, 100);
            var tools = new InventoryItem(BuiltinBlocks.CopperTools, 20);
            var mana = new InventoryItem(Mana.Item.ItemIndex, 4);
            var elemen = new InventoryItem(Elementium.Item.ItemIndex, 2);
            var esper = new InventoryItem(Esper.Item.ItemIndex, 1);
            var esperMax = new InventoryItem(Esper.Item.ItemIndex, 10);
            var airStone = new InventoryItem(AirStone.Item.ItemIndex, 2);
            var fireStone = new InventoryItem(FireStone.Item.ItemIndex, 2);
            var waterStone = new InventoryItem(WaterStone.Item.ItemIndex, 2);
            var earthStone = new InventoryItem(EarthStone.Item.ItemIndex, 2);
            var voidStone = new InventoryItem(Void.Item.ItemIndex, 1);

            AddAirTurretTurretSettings();
            AddEarthTurretTurretSettings();
            AddWaterTurretTurretSettings();
            AddFireTurretTurretSettings();
            AddVoidTurretTurretSettings();

            var airRecipe = new Recipe(AIRTURRET_NAMESPACE,
                                    new List<InventoryItem>() { planks, elemen, mana, tools, stone, airStone, esper },
                                    new InventoryItem(Turret.TurretSettings[AIRTURRET].TurretItem.ItemIndex),
                                    5);

            RecipeStorage.AddOptionalLimitTypeRecipe(Jobs.AdvancedCrafterRegister.JOB_NAME, airRecipe);

            var earthRecipe = new Recipe(EARTHTURRET_NAMESPACE,
                                    new List<InventoryItem>() { planks, elemen, mana, tools, stone, earthStone, esper },
                                    new InventoryItem(Turret.TurretSettings[EARTHTURRET].TurretItem.ItemIndex),
                                    5, true);

            RecipeStorage.AddOptionalLimitTypeRecipe(Jobs.AdvancedCrafterRegister.JOB_NAME, earthRecipe);

            var fireRecipe = new Recipe(FIRETURRET_NAMESPACE,
                                    new List<InventoryItem>() { planks, elemen, mana, tools, stone, fireStone, esper },
                                    new InventoryItem(Turret.TurretSettings[FIRETURRET].TurretItem.ItemIndex),
                                    5, true);

            RecipeStorage.AddOptionalLimitTypeRecipe(Jobs.AdvancedCrafterRegister.JOB_NAME, fireRecipe);

            var waterRecipe = new Recipe(WATERTURRET_NAMESPACE,
                                    new List<InventoryItem>() { planks, elemen, mana, tools, stone, waterStone, esper },
                                    new InventoryItem(Turret.TurretSettings[WATERTURRET].TurretItem.ItemIndex),
                                    5, true);

            RecipeStorage.AddOptionalLimitTypeRecipe(Jobs.AdvancedCrafterRegister.JOB_NAME, waterRecipe);

            var voidRecipe = new Recipe(VOIDTURRET_NAMESPACE,
                                    new List<InventoryItem>() { planks, elemen, mana, tools, stone, voidStone, esperMax },
                                    new InventoryItem(Turret.TurretSettings[VOIDTURRET].TurretItem.ItemIndex),
                                    5, true);

            RecipeStorage.AddOptionalLimitTypeRecipe(Jobs.AdvancedCrafterRegister.JOB_NAME, voidRecipe);
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterAddingBaseTypes, GameLoader.NAMESPACE + ".Items.Machines.ElementalTurrets.AddTurret"),
            ModLoader.ModCallbackDependsOn("pipliz.blocknpcs.addlittypes"), 
            ModLoader.ModCallbackDependsOn(GameLoader.NAMESPACE + ".Items.Machines.Turret.AddTurret")]
        public static void AddTurret(Dictionary<string, ItemTypesServer.ItemTypeRaw> items)
        {
            AddAirTurret(items);
            AddEarthTurret(items);
            AddFireTurret(items);
            AddWaterTurret(items);
            AddVoidTurret(items);
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterSelectedWorld, GameLoader.NAMESPACE + ".Items.Machines.ElementalTurrets.AddTextures"), 
            ModLoader.ModCallbackProvidesFor("pipliz.server.registertexturemappingtextures")]
        public static void AddTextures()
        {
            var textureMapping = new ItemTypesServer.TextureMapping(new JSONNode());
            textureMapping.AlbedoPath = "AirTurret.png";
            textureMapping.NormalPath = "Turret.png";
            textureMapping.HeightPath = "Turret.png";

            ItemTypesServer.SetTextureMapping(AIRTURRET_NAMESPACE + "sides", textureMapping);

            var earthTexture = new ItemTypesServer.TextureMapping(new JSONNode());
            earthTexture.AlbedoPath = "EarthTurret.png";
            earthTexture.NormalPath = "Turret.png";
            earthTexture.HeightPath = "Turret.png";

            ItemTypesServer.SetTextureMapping(EARTHTURRET_NAMESPACE + "sides", earthTexture);

            var fireTexture = new ItemTypesServer.TextureMapping(new JSONNode());
            fireTexture.AlbedoPath = "FireTurret.png";
            fireTexture.NormalPath = "Turret.png";
            fireTexture.HeightPath = "Turret.png";

            ItemTypesServer.SetTextureMapping(FIRETURRET_NAMESPACE + "sides", fireTexture);

            var waterTexture = new ItemTypesServer.TextureMapping(new JSONNode());
            waterTexture.AlbedoPath = "WaterTurret.png";
            waterTexture.NormalPath = "Turret.png";
            waterTexture.HeightPath = "Turret.png";

            ItemTypesServer.SetTextureMapping(WATERTURRET_NAMESPACE + "sides", waterTexture);

            var voidTexture = new ItemTypesServer.TextureMapping(new JSONNode());
            voidTexture.AlbedoPath = "VoidTurret.png";
            voidTexture.NormalPath = "Turret.png";
            voidTexture.HeightPath = "Turret.png";

            ItemTypesServer.SetTextureMapping(VOIDTURRET_NAMESPACE + "sides", voidTexture);
        }

        private static void AddVoidTurretTurretSettings()
        {
            var turretSettings = new Turret.TurretSetting()
            {
                TurretItem = Turret.TurretTypes[VOIDTURRET],
                Ammo = new List<InventoryItem>() { new InventoryItem(Void.Item.ItemIndex) },
                AmmoValue = 0.04f,
                AmmoReloadValue = 1f,
                DurabilityPerDoWork = 0.008f,
                FuelPerDoWork = 0.02f,
                Name = VOIDTURRET,
                OnShootAudio = VOIDTURRET_NAMESPACE,
                OnHitAudio = VOIDTURRET_NAMESPACE,
                Range = 25,
                WorkTime = 13f,
                RefuelTime = 15,
                ReloadTime = 15,
                RepairTime = 20,
                RequiredForFix = new Dictionary<float, List<InventoryItem>>()
                {
                    { 75f, new List<InventoryItem>() { new InventoryItem(BuiltinBlocks.StoneBricks, 10), new InventoryItem(Mana.Item.ItemIndex, 1), new InventoryItem(BuiltinBlocks.CopperNails, 1), new InventoryItem(Esper.Item.ItemIndex, 1) } },
                    { 50f, new List<InventoryItem>() { new InventoryItem(BuiltinBlocks.StoneBricks, 10), new InventoryItem(Mana.Item.ItemIndex, 2), new InventoryItem(BuiltinBlocks.CopperNails, 2), new InventoryItem(Esper.Item.ItemIndex, 2) } },
                    { 30f, new List<InventoryItem>() { new InventoryItem(BuiltinBlocks.StoneBricks, 15), new InventoryItem(BuiltinBlocks.Planks, 1), new InventoryItem(Mana.Item.ItemIndex, 3), new InventoryItem(BuiltinBlocks.CopperNails, 3), new InventoryItem(Esper.Item.ItemIndex, 3), new InventoryItem(Void.Item.ItemIndex, 1) } },
                    { 10f, new List<InventoryItem>() { new InventoryItem(BuiltinBlocks.StoneBricks, 20), new InventoryItem(BuiltinBlocks.Planks, 1), new InventoryItem(Mana.Item.ItemIndex, 4), new InventoryItem(BuiltinBlocks.CopperNails, 4), new InventoryItem(Esper.Item.ItemIndex, 4), new InventoryItem(Void.Item.ItemIndex, 2) } },
                },
                ProjectileAnimation = Managers.AnimationManager.AnimatedObjects[Managers.AnimationManager.LEADBULLET]
            };
            turretSettings.Damage[DamageType.Void] = 400;
            turretSettings.Damage[DamageType.Physical] = 100;

            Turret.TurretSettings[VOIDTURRET] = turretSettings;
        }

        private static void AddFireTurretTurretSettings()
        {
            var turretSettings = new Turret.TurretSetting()
            {
                TurretItem = Turret.TurretTypes[FIRETURRET],
                Ammo = new List<InventoryItem>() { new InventoryItem(FireStone.Item.ItemIndex) },
                AmmoValue = 0.02f,
                AmmoReloadValue = 0.5f,
                DurabilityPerDoWork = 0.008f,
                FuelPerDoWork = 0.02f,
                Name = FIRETURRET,
                OnShootAudio = FIRETURRET_NAMESPACE,
                OnHitAudio = FIRETURRET_NAMESPACE,
                Range = 25,
                WorkTime = 13f,
                RefuelTime = 15,
                ReloadTime = 15,
                RepairTime = 20,
                RequiredForFix = new Dictionary<float, List<InventoryItem>>()
                {
                    { 75f, new List<InventoryItem>() { new InventoryItem(BuiltinBlocks.StoneBricks, 10), new InventoryItem(Mana.Item.ItemIndex, 1), new InventoryItem(BuiltinBlocks.CopperNails, 1) } },
                    { 50f, new List<InventoryItem>() { new InventoryItem(BuiltinBlocks.StoneBricks, 10), new InventoryItem(Mana.Item.ItemIndex, 2), new InventoryItem(BuiltinBlocks.CopperNails, 2) } },
                    { 30f, new List<InventoryItem>() { new InventoryItem(BuiltinBlocks.StoneBricks, 15), new InventoryItem(BuiltinBlocks.Planks, 1), new InventoryItem(Mana.Item.ItemIndex, 3), new InventoryItem(BuiltinBlocks.CopperNails, 3), new InventoryItem(Esper.Item.ItemIndex, 1) } },
                    { 10f, new List<InventoryItem>() { new InventoryItem(BuiltinBlocks.StoneBricks, 20), new InventoryItem(BuiltinBlocks.Planks, 1), new InventoryItem(Mana.Item.ItemIndex, 4), new InventoryItem(BuiltinBlocks.CopperNails, 4), new InventoryItem(Esper.Item.ItemIndex, 2) } },
                },
                ProjectileAnimation = Managers.AnimationManager.AnimatedObjects[Managers.AnimationManager.LEADBULLET]
            };
            turretSettings.Damage[DamageType.Fire] = 400;
            turretSettings.Damage[DamageType.Physical] = 100;

            Turret.TurretSettings[FIRETURRET] = turretSettings;
        }

        private static void AddWaterTurretTurretSettings()
        {
            var turretSettings = new Turret.TurretSetting()
            {
                TurretItem = Turret.TurretTypes[WATERTURRET],
                Ammo = new List<InventoryItem>() { new InventoryItem(WaterStone.Item.ItemIndex) },
                AmmoValue = 0.02f,
                AmmoReloadValue = 0.5f,
                DurabilityPerDoWork = 0.008f,
                FuelPerDoWork = 0.02f,
                Name = WATERTURRET,
                OnShootAudio = WATERTURRET_NAMESPACE,
                OnHitAudio = WATERTURRET_NAMESPACE,
                Range = 25,
                WorkTime = 13f,
                RefuelTime = 15,
                ReloadTime = 15,
                RepairTime = 20,
                RequiredForFix = new Dictionary<float, List<InventoryItem>>()
                {
                    { 75f, new List<InventoryItem>() { new InventoryItem(BuiltinBlocks.StoneBricks, 10), new InventoryItem(Mana.Item.ItemIndex, 1), new InventoryItem(BuiltinBlocks.CopperNails, 1) } },
                    { 50f, new List<InventoryItem>() { new InventoryItem(BuiltinBlocks.StoneBricks, 10), new InventoryItem(Mana.Item.ItemIndex, 2), new InventoryItem(BuiltinBlocks.CopperNails, 2) } },
                    { 30f, new List<InventoryItem>() { new InventoryItem(BuiltinBlocks.StoneBricks, 15), new InventoryItem(BuiltinBlocks.Planks, 1), new InventoryItem(Mana.Item.ItemIndex, 3), new InventoryItem(BuiltinBlocks.CopperNails, 3), new InventoryItem(Esper.Item.ItemIndex, 1) } },
                    { 10f, new List<InventoryItem>() { new InventoryItem(BuiltinBlocks.StoneBricks, 20), new InventoryItem(BuiltinBlocks.Planks, 1), new InventoryItem(Mana.Item.ItemIndex, 4), new InventoryItem(BuiltinBlocks.CopperNails, 4), new InventoryItem(Esper.Item.ItemIndex, 2) } },
                },
                ProjectileAnimation = Managers.AnimationManager.AnimatedObjects[Managers.AnimationManager.LEADBULLET]
            };
            turretSettings.Damage[DamageType.Water] = 400;
            turretSettings.Damage[DamageType.Physical] = 100;

            Turret.TurretSettings[WATERTURRET] = turretSettings;
        }

        private static void AddEarthTurretTurretSettings()
        {
            var turretSettings = new Turret.TurretSetting()
            {
                TurretItem = Turret.TurretTypes[EARTHTURRET],
                Ammo = new List<InventoryItem>() { new InventoryItem(EarthStone.Item.ItemIndex) },
                AmmoValue = 0.02f,
                AmmoReloadValue = 0.5f,
                DurabilityPerDoWork = 0.008f,
                FuelPerDoWork = 0.02f,
                Name = EARTHTURRET,
                OnShootAudio = EARTHTURRET_NAMESPACE,
                OnHitAudio = EARTHTURRET_NAMESPACE,
                Range = 25,
                WorkTime = 13f,
                RefuelTime = 15,
                ReloadTime = 15,
                RepairTime = 20,
                RequiredForFix = new Dictionary<float, List<InventoryItem>>()
                {
                    { 75f, new List<InventoryItem>() { new InventoryItem(BuiltinBlocks.StoneBricks, 10), new InventoryItem(Mana.Item.ItemIndex, 1), new InventoryItem(BuiltinBlocks.CopperNails, 1) } },
                    { 50f, new List<InventoryItem>() { new InventoryItem(BuiltinBlocks.StoneBricks, 10), new InventoryItem(Mana.Item.ItemIndex, 2), new InventoryItem(BuiltinBlocks.CopperNails, 2) } },
                    { 30f, new List<InventoryItem>() { new InventoryItem(BuiltinBlocks.StoneBricks, 15), new InventoryItem(BuiltinBlocks.Planks, 1), new InventoryItem(Mana.Item.ItemIndex, 3), new InventoryItem(BuiltinBlocks.CopperNails, 3), new InventoryItem(Esper.Item.ItemIndex, 1) } },
                    { 10f, new List<InventoryItem>() { new InventoryItem(BuiltinBlocks.StoneBricks, 20), new InventoryItem(BuiltinBlocks.Planks, 1), new InventoryItem(Mana.Item.ItemIndex, 4), new InventoryItem(BuiltinBlocks.CopperNails, 4), new InventoryItem(Esper.Item.ItemIndex, 2) } },
                },
                ProjectileAnimation = Managers.AnimationManager.AnimatedObjects[Managers.AnimationManager.LEADBULLET]
            };
            turretSettings.Damage[DamageType.Earth] = 400;
            turretSettings.Damage[DamageType.Physical] = 100;

            Turret.TurretSettings[EARTHTURRET] = turretSettings;
        }

        private static void AddAirTurretTurretSettings()
        {
            var turretSettings = new Turret.TurretSetting()
            {
                TurretItem = Turret.TurretTypes[AIRTURRET],
                Ammo = new List<InventoryItem>() { new InventoryItem(AirStone.Item.ItemIndex) },
                AmmoValue = 0.02f,
                AmmoReloadValue = 0.5f,
                DurabilityPerDoWork = 0.008f,
                FuelPerDoWork = 0.02f,
                Name = AIRTURRET,
                OnShootAudio = AIRTURRET_NAMESPACE,
                OnHitAudio = AIRTURRET_NAMESPACE,
                Range = 25,
                WorkTime = 13f,
                RefuelTime = 15,
                ReloadTime = 15,
                RepairTime = 20,
                RequiredForFix = new Dictionary<float, List<InventoryItem>>()
                {
                    { 75f, new List<InventoryItem>() { new InventoryItem(BuiltinBlocks.StoneBricks, 10), new InventoryItem(Mana.Item.ItemIndex, 1), new InventoryItem(BuiltinBlocks.CopperNails, 1) } },
                    { 50f, new List<InventoryItem>() { new InventoryItem(BuiltinBlocks.StoneBricks, 10), new InventoryItem(Mana.Item.ItemIndex, 2), new InventoryItem(BuiltinBlocks.CopperNails, 2) } },
                    { 30f, new List<InventoryItem>() { new InventoryItem(BuiltinBlocks.StoneBricks, 15), new InventoryItem(BuiltinBlocks.Planks, 1), new InventoryItem(Mana.Item.ItemIndex, 3), new InventoryItem(BuiltinBlocks.CopperNails, 3), new InventoryItem(Esper.Item.ItemIndex, 1) } },
                    { 10f, new List<InventoryItem>() { new InventoryItem(BuiltinBlocks.StoneBricks, 20), new InventoryItem(BuiltinBlocks.Planks, 1), new InventoryItem(Mana.Item.ItemIndex, 4), new InventoryItem(BuiltinBlocks.CopperNails, 4), new InventoryItem(Esper.Item.ItemIndex, 2) } },
                },
                ProjectileAnimation = Managers.AnimationManager.AnimatedObjects[Managers.AnimationManager.LEADBULLET]
            };
            turretSettings.Damage[DamageType.Air] = 400;
            turretSettings.Damage[DamageType.Physical] = 100;

            Turret.TurretSettings[AIRTURRET] = turretSettings;
        }

        private static void AddVoidTurret(Dictionary<string, ItemTypesServer.ItemTypeRaw> items)
        {
            var turretName = VOIDTURRET_NAMESPACE;
            var turretNode = new JSONNode()
              .SetAs("icon", "VoidTurret.png")
              .SetAs("isPlaceable", true)
              .SetAs("onPlaceAudio", "stonePlace")
              .SetAs("onRemoveAudio", "stoneDelete")
              .SetAs("onRemoveAmount", 1)
              .SetAs("sideall", "stonebricks")
              .SetAs("isSolid", true)
              .SetAs("sidex+", VOIDTURRET_NAMESPACE + "sides")
              .SetAs("sidex-", VOIDTURRET_NAMESPACE + "sides")
              .SetAs("sidez+", VOIDTURRET_NAMESPACE + "sides")
              .SetAs("sidez-", VOIDTURRET_NAMESPACE + "sides")
              .SetAs("npcLimit", 0);

            JSONNode categories = new JSONNode(NodeType.Array);
            categories.AddToArray(new JSONNode("machine"));
            categories.AddToArray(new JSONNode("turret"));
            categories.AddToArray(new JSONNode("magic"));
            turretNode.SetAs("categories", categories);

            var item = new ItemTypesServer.ItemTypeRaw(turretName, turretNode);
            Turret.TurretTypes[VOIDTURRET] = item;
            items.Add(turretName, item);
        }

        private static void AddWaterTurret(Dictionary<string, ItemTypesServer.ItemTypeRaw> items)
        {
            var turretName = WATERTURRET_NAMESPACE;
            var turretNode = new JSONNode()
              .SetAs("icon", "WaterTurret.png")
              .SetAs("isPlaceable", true)
              .SetAs("onPlaceAudio", "stonePlace")
              .SetAs("onRemoveAudio", "stoneDelete")
              .SetAs("onRemoveAmount", 1)
              .SetAs("sideall", "stonebricks")
              .SetAs("isSolid", true)
              .SetAs("sidex+", WATERTURRET_NAMESPACE + "sides")
              .SetAs("sidex-", WATERTURRET_NAMESPACE + "sides")
              .SetAs("sidez+", WATERTURRET_NAMESPACE + "sides")
              .SetAs("sidez-", WATERTURRET_NAMESPACE + "sides")
              .SetAs("npcLimit", 0);

            JSONNode categories = new JSONNode(NodeType.Array);
            categories.AddToArray(new JSONNode("machine"));
            categories.AddToArray(new JSONNode("turret"));
            categories.AddToArray(new JSONNode("magic"));
            turretNode.SetAs("categories", categories);

            var item = new ItemTypesServer.ItemTypeRaw(turretName, turretNode);
            Turret.TurretTypes[WATERTURRET] = item;
            items.Add(turretName, item);
        }

        private static void AddEarthTurret(Dictionary<string, ItemTypesServer.ItemTypeRaw> items)
        {
            var turretName = EARTHTURRET_NAMESPACE;
            var turretNode = new JSONNode()
              .SetAs("icon", "EarthTurret.png")
              .SetAs("isPlaceable", true)
              .SetAs("onPlaceAudio", "stonePlace")
              .SetAs("onRemoveAudio", "stoneDelete")
              .SetAs("onRemoveAmount", 1)
              .SetAs("sideall", "stonebricks")
              .SetAs("isSolid", true)
              .SetAs("sidex+", EARTHTURRET_NAMESPACE + "sides")
              .SetAs("sidex-", EARTHTURRET_NAMESPACE + "sides")
              .SetAs("sidez+", EARTHTURRET_NAMESPACE + "sides")
              .SetAs("sidez-", EARTHTURRET_NAMESPACE + "sides")
              .SetAs("npcLimit", 0);

            JSONNode categories = new JSONNode(NodeType.Array);
            categories.AddToArray(new JSONNode("machine"));
            categories.AddToArray(new JSONNode("turret"));
            categories.AddToArray(new JSONNode("magic"));
            turretNode.SetAs("categories", categories);

            var item = new ItemTypesServer.ItemTypeRaw(turretName, turretNode);
            Turret.TurretTypes[EARTHTURRET] = item;
            items.Add(turretName, item);
        }

        private static void AddFireTurret(Dictionary<string, ItemTypesServer.ItemTypeRaw> items)
        {
            var turretName = FIRETURRET_NAMESPACE;
            var turretNode = new JSONNode()
              .SetAs("icon", "FireTurret.png")
              .SetAs("isPlaceable", true)
              .SetAs("onPlaceAudio", "stonePlace")
              .SetAs("onRemoveAudio", "stoneDelete")
              .SetAs("onRemoveAmount", 1)
              .SetAs("sideall", "stonebricks")
              .SetAs("isSolid", true)
              .SetAs("sidex+", FIRETURRET_NAMESPACE + "sides")
              .SetAs("sidex-", FIRETURRET_NAMESPACE + "sides")
              .SetAs("sidez+", FIRETURRET_NAMESPACE + "sides")
              .SetAs("sidez-", FIRETURRET_NAMESPACE + "sides")
              .SetAs("npcLimit", 0);

            JSONNode categories = new JSONNode(NodeType.Array);
            categories.AddToArray(new JSONNode("machine"));
            categories.AddToArray(new JSONNode("turret"));
            categories.AddToArray(new JSONNode("magic"));
            turretNode.SetAs("categories", categories);

            var item = new ItemTypesServer.ItemTypeRaw(turretName, turretNode);
            Turret.TurretTypes[FIRETURRET] = item;
            items.Add(turretName, item);
        }

        private static void AddAirTurret(Dictionary<string, ItemTypesServer.ItemTypeRaw> items)
        {
            var turretName = AIRTURRET_NAMESPACE;
            var turretNode = new JSONNode()
              .SetAs("icon", "AirTurret.png")
              .SetAs("isPlaceable", true)
              .SetAs("onPlaceAudio", "stonePlace")
              .SetAs("onRemoveAudio", "stoneDelete")
              .SetAs("onRemoveAmount", 1)
              .SetAs("sideall", "stonebricks")
              .SetAs("isSolid", true)
              .SetAs("sidex+", AIRTURRET_NAMESPACE + "sides")
              .SetAs("sidex-", AIRTURRET_NAMESPACE + "sides")
              .SetAs("sidez+", AIRTURRET_NAMESPACE + "sides")
              .SetAs("sidez-", AIRTURRET_NAMESPACE + "sides")
              .SetAs("npcLimit", 0);

            JSONNode categories = new JSONNode(NodeType.Array);
            categories.AddToArray(new JSONNode("machine"));
            categories.AddToArray(new JSONNode("turret"));
            categories.AddToArray(new JSONNode("magic"));
            turretNode.SetAs("categories", categories);

            var item = new ItemTypesServer.ItemTypeRaw(turretName, turretNode);
            Turret.TurretTypes[AIRTURRET] = item;
            items.Add(turretName, item);
        }
    }
}
