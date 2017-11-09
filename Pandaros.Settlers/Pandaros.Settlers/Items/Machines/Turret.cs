using BlockTypes.Builtin;
using NPC;
using Pandaros.Settlers.Entities;
using Pipliz;
using Pipliz.JSON;
using Server.Monsters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pandaros.Settlers.Items.Machines
{
    [ModLoader.ModManager]
    public static class Turret
    {
        public class TurretSettings
        {
            public float RepairTime { get; set; }

            public float RefuelTime { get; set; }

            public float ReloadTime { get; set; }

            public float WorkTime { get; set; }

            public Dictionary<float, List<InventoryItem>> RequiredForFix = new Dictionary<float, List<InventoryItem>>();

            public List<InventoryItem> Ammo { get; set; }

            public float AmmoValue { get; set; }

            public float DurabilityPerDoWork { get; set; }

            public float FuelPerDoWork { get; set; }

            public string Name { get; set; }

            public float Damage { get; set; }

            public int Range { get; set; }

            public string OnShootAudio { get; set; }

            public string OnHitAudio { get; set; }

            public ItemTypesServer.ItemTypeRaw TurretItem { get; set; }

            public Managers.AnimationManager.AnimatedObject ProjectileAnimation { get; set; }
        }

        public static Dictionary<string, TurretSettings> TurretTypes = new Dictionary<string, TurretSettings>();

        public const string STONE = "Stone Turret";
        public const string BRONZEARROW = "Bronze Arrow Turret";
        public const string CROSSBOW = "Crossbow Turret";
        public const string MATCHLOCK = "Matchlock Turret";

        public static string STONE_NAMESPACE = GameLoader.NAMESPACE + ".StoneTurret";
        public static string BRONZEARROW_NAMESPACE = GameLoader.NAMESPACE + ".BronzeArrowTurret";
        public static string CROSSBOW_NAMESPACE = GameLoader.NAMESPACE + ".CrossbowTurret";
        public static string MATCHLOCK_NAMESPACE = GameLoader.NAMESPACE + ".MatchlockTurret";

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterAddingBaseTypes, GameLoader.NAMESPACE + ".Items.Machines.Turret.RegisterMachines")]
        [ModLoader.ModCallbackDependsOn(GameLoader.NAMESPACE + ".Items.Machines.Turret.AddTurret")]
        public static void RegisterMachines(Dictionary<string, ItemTypesServer.ItemTypeRaw> items)
        {
            foreach (var turret in TurretTypes)
                MachineManager.RegisterMachineType(turret.Key, new MachineManager.MachineSettings(turret.Value.TurretItem.ItemIndex, Repair, MachineManager.Refuel, Reload, DoWork, turret.Value.RepairTime, turret.Value.RefuelTime, turret.Value.ReloadTime, turret.Value.WorkTime));
        }

        public static ushort Repair(Players.Player player, MachineState machineState) 
        {
            var retval = GameLoader.Repairing_Icon;

            if (machineState.Durability < .75f && TurretTypes.ContainsKey(machineState.MachineType))
            {
                bool repaired = false;
                List<InventoryItem> requiredForFix = new List<InventoryItem>();
                var stockpile = Stockpile.GetStockPile(player);

                foreach (var durability in TurretTypes[machineState.MachineType].RequiredForFix.OrderByDescending(s => s.Key))
                    if (machineState.Durability < durability.Key)
                    {
                        requiredForFix = durability.Value;
                        break;
                    }

                if (stockpile.Contains(requiredForFix))
                {
                    stockpile.TryRemove(requiredForFix);
                    repaired = true;
                }
                else
                    foreach (var item in requiredForFix)
                        if (!stockpile.Contains(item))
                        {
                            retval = item.Type;
                            break;
                        }

                if (repaired)
                    machineState.Durability = MachineState.MAX_DURABILITY[player];
            }

            return retval;
        }

        public static ushort Reload(Players.Player player, MachineState machineState)
        {
            if (TurretTypes.ContainsKey(machineState.MachineType) && machineState.Load < .75f)
            {
                var stockpile = Stockpile.GetStockPile(player);
                
                while (stockpile.Contains(TurretTypes[machineState.MachineType].Ammo) && machineState.Load < MachineState.MAX_LOAD[player])
                {
                    if (stockpile.TryRemove(TurretTypes[machineState.MachineType].Ammo))
                        machineState.Load += TurretTypes[machineState.MachineType].AmmoValue;
                }

                if (machineState.Fuel < MachineState.MAX_FUEL[player])
                    return MachineManager.FuelValues.First().Key;
            }

            return GameLoader.Reload_Icon;
        }
        
        public static void DoWork(Players.Player player, MachineState machineState)
        {
            if (TurretTypes.ContainsKey(machineState.MachineType) &&
                machineState.Durability > 0 && 
                machineState.Fuel > 0 && 
                machineState.NextTimeForWork < Time.SecondsSinceStartDouble)
            {
                var stockpile = Stockpile.GetStockPile(player);

                machineState.Durability -= TurretTypes[machineState.MachineType].DurabilityPerDoWork;
                machineState.Fuel -= TurretTypes[machineState.MachineType].FuelPerDoWork;

                if (machineState.Durability <= 0)
                    PandaChat.SendThrottle(player, $"A {machineState.MachineType} at {machineState.Position} has broken down. Consider adding more Machinist's to keep them running!", ChatColor.maroon);

                if (machineState.Fuel <= 0)
                    PandaChat.SendThrottle(player, $"A {machineState.MachineType} at {machineState.Position} has run out of fuel. Consider adding more Machinist's to keep them running!", ChatColor.maroon);

                if (machineState.Load > 0)
                {
                    var monster = MonsterTracker.Find(machineState.Position.Add(0, 1, 0), TurretTypes[machineState.MachineType].Range, TurretTypes[machineState.MachineType].Damage);
                    
                    if (monster != null)
                    {
                        machineState.Load -= TurretTypes[machineState.MachineType].AmmoValue;

                        if (TurretTypes[machineState.MachineType].OnShootAudio != null)
                            ServerManager.SendAudio(machineState.Position.Vector, TurretTypes[machineState.MachineType].OnShootAudio);

                        if (TurretTypes[machineState.MachineType].OnHitAudio != null)
                            ServerManager.SendAudio(monster.PositionToAimFor, TurretTypes[machineState.MachineType].OnHitAudio);

                        TurretTypes[machineState.MachineType].ProjectileAnimation.SendMoveToInterpolated(machineState.Position.Vector, monster.PositionToAimFor);
                        monster.OnHit(TurretTypes[machineState.MachineType].Damage);
                    }
                }
                else
                    PandaChat.SendThrottle(player, $"A {machineState.MachineType} at {machineState.Position} has run out of Ammo. Consider adding more Machinist's to keep them running!", ChatColor.maroon);

                machineState.NextTimeForWork = machineState.MachineSettings.WorkTime + Time.SecondsSinceStartDouble;
            }
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterItemTypesDefined, GameLoader.NAMESPACE + ".Items.Machines.Turret.RegisterTurret")]
        public static void RegisterTurret()
        {
            var rivets = new InventoryItem(BuiltinBlocks.IronRivet, 6);
            var iron = new InventoryItem(BuiltinBlocks.IronWrought, 2);
            var copperParts = new InventoryItem(BuiltinBlocks.CopperParts, 6);
            var copperNails = new InventoryItem(BuiltinBlocks.CopperNails, 6);
            var tools = new InventoryItem(BuiltinBlocks.CopperTools, 1);
            var planks = new InventoryItem(BuiltinBlocks.Planks, 2);
            var stone = new InventoryItem(BuiltinBlocks.StoneBricks, 4);
            var bronze = new InventoryItem(BuiltinBlocks.BronzePlate, 2);
            var bronzeIngot = new InventoryItem(BuiltinBlocks.BronzeIngot, 3);
            var steelParts = new InventoryItem(BuiltinBlocks.SteelParts, 6);
            var steelIngot = new InventoryItem(BuiltinBlocks.SteelIngot, 2);

            var stoneAmmo = new InventoryItem(BuiltinBlocks.SlingBullet, 10);
            var sling = new InventoryItem(BuiltinBlocks.Sling, 2);

            var arrow = new InventoryItem(BuiltinBlocks.BronzeArrow, 10);
            var bow = new InventoryItem(BuiltinBlocks.Bow, 2);

            var bolt = new InventoryItem(BuiltinBlocks.BronzeArrow, 10);
            var crossbow = new InventoryItem(BuiltinBlocks.Bow, 2);

            var bullet = new InventoryItem(BuiltinBlocks.LeadBullet, 10);
            var gunpowder = new InventoryItem(BuiltinBlocks.GunpowderPouch, 3);
            var matchlock = new InventoryItem(BuiltinBlocks.MatchlockGun, 2);

            var stonerecipe = new Recipe(STONE_NAMESPACE,
                                    new List<InventoryItem>() { planks, copperParts, copperNails, tools, stone, sling, stoneAmmo },
                                    new InventoryItem(TurretTypes[STONE].TurretItem.ItemIndex),
                                    5);

            RecipeStorage.AddOptionalLimitTypeRecipe(Jobs.AdvancedCrafterRegister.JOB_NAME, stonerecipe);

            var bronzeArrowrecipe = new Recipe(BRONZEARROW_NAMESPACE,
                                    new List<InventoryItem>() { planks, bronze, bronzeIngot, copperNails, tools, stone, arrow, bow },
                                    new InventoryItem(TurretTypes[BRONZEARROW].TurretItem.ItemIndex),
                                    5);

            RecipeStorage.AddOptionalLimitTypeRecipe(Jobs.AdvancedCrafterRegister.JOB_NAME, bronzeArrowrecipe);

            var crossbowrecipe = new Recipe(CROSSBOW_NAMESPACE,
                                    new List<InventoryItem>() { planks, iron, rivets, copperNails, tools, stone, bolt, crossbow },
                                    new InventoryItem(TurretTypes[CROSSBOW].TurretItem.ItemIndex),
                                    5);

            RecipeStorage.AddOptionalLimitTypeRecipe(Jobs.AdvancedCrafterRegister.JOB_NAME, crossbowrecipe);

            var matchlockrecipe = new Recipe(MATCHLOCK_NAMESPACE,
                                    new List<InventoryItem>() { planks, steelParts, steelIngot, copperNails, tools, stone, bullet, gunpowder, matchlock },
                                    new InventoryItem(TurretTypes[MATCHLOCK].TurretItem.ItemIndex),
                                    5);

            RecipeStorage.AddOptionalLimitTypeRecipe(Jobs.AdvancedCrafterRegister.JOB_NAME, matchlockrecipe);
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterSelectedWorld, GameLoader.NAMESPACE + ".Items.Machines.Turret.AddTextures"), ModLoader.ModCallbackProvidesFor("pipliz.server.registertexturemappingtextures")]
        public static void AddTextures()
        {
            var textureMapping = new ItemTypesServer.TextureMapping(new JSONNode());
            textureMapping.AlbedoPath = GameLoader.TEXTURE_FOLDER_PANDA + "/albedo/StoneTurret.png";
            textureMapping.NormalPath = GameLoader.TEXTURE_FOLDER_PANDA + "/normal/Turret.png";
            textureMapping.HeightPath = GameLoader.TEXTURE_FOLDER_PANDA + "/height/Turret.png";

            ItemTypesServer.SetTextureMapping(STONE_NAMESPACE + "sides", textureMapping);

            var bronzetextureMapping = new ItemTypesServer.TextureMapping(new JSONNode());
            bronzetextureMapping.AlbedoPath = GameLoader.TEXTURE_FOLDER_PANDA + "/albedo/ArrowTurret.png";
            bronzetextureMapping.NormalPath = GameLoader.TEXTURE_FOLDER_PANDA + "/normal/Turret.png";
            bronzetextureMapping.HeightPath = GameLoader.TEXTURE_FOLDER_PANDA + "/height/Turret.png";

            ItemTypesServer.SetTextureMapping(BRONZEARROW_NAMESPACE + "sides", bronzetextureMapping);

            var crossbowtextureMapping = new ItemTypesServer.TextureMapping(new JSONNode());
            crossbowtextureMapping.AlbedoPath = GameLoader.TEXTURE_FOLDER_PANDA + "/albedo/CrossbowTurret.png";
            crossbowtextureMapping.NormalPath = GameLoader.TEXTURE_FOLDER_PANDA + "/normal/Turret.png";
            crossbowtextureMapping.HeightPath = GameLoader.TEXTURE_FOLDER_PANDA + "/height/Turret.png";

            ItemTypesServer.SetTextureMapping(CROSSBOW_NAMESPACE + "sides", crossbowtextureMapping);

            var matchlocktextureMapping = new ItemTypesServer.TextureMapping(new JSONNode());
            matchlocktextureMapping.AlbedoPath = GameLoader.TEXTURE_FOLDER_PANDA + "/albedo/MatchlockTurret.png";
            matchlocktextureMapping.NormalPath = GameLoader.TEXTURE_FOLDER_PANDA + "/normal/Turret.png";
            matchlocktextureMapping.HeightPath = GameLoader.TEXTURE_FOLDER_PANDA + "/height/Turret.png";

            ItemTypesServer.SetTextureMapping(MATCHLOCK_NAMESPACE + "sides", matchlocktextureMapping);
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterAddingBaseTypes, GameLoader.NAMESPACE + ".Items.Machines.Turret.AddTurret"), ModLoader.ModCallbackDependsOn("pipliz.blocknpcs.addlittypes")]
        public static void AddTurret(Dictionary<string, ItemTypesServer.ItemTypeRaw> items)
        {
            AddStoneTurret(items);
            AddBronzeArrowTurret(items);
            AddCrossbowTurret(items);
            AddMatchlockTurret(items);
        }

        private static void AddStoneTurret(Dictionary<string, ItemTypesServer.ItemTypeRaw> items)
        {
            var turretName = STONE_NAMESPACE;
            var turretNode = new JSONNode()
              .SetAs("icon", GameLoader.ICON_FOLDER_PANDA + "/StoneTurret.png")
            .  SetAs("isPlaceable", true)
              .SetAs("onPlaceAudio", "stonePlace")
              .SetAs("onRemoveAudio", "stoneDelete")
              .SetAs("sideall", "stonebricks")
              .SetAs("onRemoveAmount", 1)
              .SetAs("isSolid", true)
              .SetAs("sidex+", STONE_NAMESPACE + "sides")
              .SetAs("sidex-", STONE_NAMESPACE + "sides")
              .SetAs("sidez+", STONE_NAMESPACE + "sides")
              .SetAs("sidez-", STONE_NAMESPACE + "sides")
              .SetAs("npcLimit", 0);

            var item = new ItemTypesServer.ItemTypeRaw(turretName, turretNode);

            var turretSettings = new TurretSettings()
            {
                TurretItem = item,
                Ammo = new List<InventoryItem>() { new InventoryItem(BuiltinBlocks.SlingBullet) },
                AmmoValue = 0.05f,
                Damage = 50f,
                DurabilityPerDoWork = 0.002f,
                FuelPerDoWork = 0.01f,
                Name = STONE,
                OnShootAudio = "sling",
                OnHitAudio = "fleshHit",
                Range = 17,
                WorkTime = 1.5f,
                RefuelTime = 4,
                ReloadTime = 5,
                RepairTime = 10,
                RequiredForFix = new Dictionary<float, List<InventoryItem>>()
                {
                    { 10f, new List<InventoryItem>() { new InventoryItem(BuiltinBlocks.StoneBricks, 1), new InventoryItem(BuiltinBlocks.Planks, 1), new InventoryItem(BuiltinBlocks.CopperParts, 6), new InventoryItem(BuiltinBlocks.CopperNails, 6), new InventoryItem(BuiltinBlocks.Sling, 2) } },
                    { 30f, new List<InventoryItem>() { new InventoryItem(BuiltinBlocks.StoneBricks, 1), new InventoryItem(BuiltinBlocks.Planks, 1), new InventoryItem(BuiltinBlocks.CopperParts, 4), new InventoryItem(BuiltinBlocks.CopperNails, 4), new InventoryItem(BuiltinBlocks.Sling, 1) } },
                    { 50f, new List<InventoryItem>() { new InventoryItem(BuiltinBlocks.StoneBricks, 1), new InventoryItem(BuiltinBlocks.Planks, 1), new InventoryItem(BuiltinBlocks.CopperParts, 2), new InventoryItem(BuiltinBlocks.CopperNails, 4) } },
                    { 75f, new List<InventoryItem>() { new InventoryItem(BuiltinBlocks.StoneBricks, 1), new InventoryItem(BuiltinBlocks.Planks, 1), new InventoryItem(BuiltinBlocks.CopperParts, 1), new InventoryItem(BuiltinBlocks.CopperNails, 2) } },
                },
                ProjectileAnimation = Managers.AnimationManager.AnimatedObjects[Managers.AnimationManager.SLINGBULLET]
            };

            TurretTypes[STONE] = turretSettings;
            items.Add(turretName, item);
        }

        private static void AddBronzeArrowTurret(Dictionary<string, ItemTypesServer.ItemTypeRaw> items)
        {
            var turretName = BRONZEARROW_NAMESPACE;
            var turretNode = new JSONNode()
              .SetAs("icon", GameLoader.ICON_FOLDER_PANDA + "/ArrowTurret.png")
              .SetAs("isPlaceable", true)
              .SetAs("onPlaceAudio", "stonePlace")
              .SetAs("onRemoveAudio", "stoneDelete")
              .SetAs("sideall", "stonebricks")
              .SetAs("onRemoveAmount", 1)
              .SetAs("isSolid", true)
              .SetAs("sidex+", BRONZEARROW_NAMESPACE + "sides")
              .SetAs("sidex-", BRONZEARROW_NAMESPACE + "sides")
              .SetAs("sidez+", BRONZEARROW_NAMESPACE + "sides")
              .SetAs("sidez-", BRONZEARROW_NAMESPACE + "sides")
              .SetAs("npcLimit", 0);

            var item = new ItemTypesServer.ItemTypeRaw(turretName, turretNode);

            var turretSettings = new TurretSettings()
            {
                TurretItem = item,
                Ammo = new List<InventoryItem>() { new InventoryItem(BuiltinBlocks.BronzeArrow) },
                AmmoValue = 0.05f,
                Damage = 100f,
                DurabilityPerDoWork = 0.004f,
                FuelPerDoWork = 0.02f,
                Name = BRONZEARROW,
                OnShootAudio = "bowShoot",
                OnHitAudio = "fleshHit",
                Range = 25,
                WorkTime = 3f,
                RefuelTime = 5,
                ReloadTime = 6,
                RepairTime = 12,
                RequiredForFix = new Dictionary<float, List<InventoryItem>>()
                {
                    { 10f, new List<InventoryItem>() { new InventoryItem(BuiltinBlocks.StoneBricks, 1), new InventoryItem(BuiltinBlocks.Planks, 1), new InventoryItem(BuiltinBlocks.CopperParts, 6), new InventoryItem(BuiltinBlocks.CopperNails, 6), new InventoryItem(BuiltinBlocks.Bow, 2) } },
                    { 30f, new List<InventoryItem>() { new InventoryItem(BuiltinBlocks.StoneBricks, 1), new InventoryItem(BuiltinBlocks.Planks, 1), new InventoryItem(BuiltinBlocks.CopperParts, 4), new InventoryItem(BuiltinBlocks.CopperNails, 4), new InventoryItem(BuiltinBlocks.Bow, 1) } },
                    { 50f, new List<InventoryItem>() { new InventoryItem(BuiltinBlocks.StoneBricks, 1), new InventoryItem(BuiltinBlocks.Planks, 1), new InventoryItem(BuiltinBlocks.CopperParts, 2), new InventoryItem(BuiltinBlocks.CopperNails, 4) } },
                    { 75f, new List<InventoryItem>() { new InventoryItem(BuiltinBlocks.StoneBricks, 1), new InventoryItem(BuiltinBlocks.Planks, 1), new InventoryItem(BuiltinBlocks.CopperParts, 1), new InventoryItem(BuiltinBlocks.CopperNails, 2) } },
                },
                ProjectileAnimation = Managers.AnimationManager.AnimatedObjects[Managers.AnimationManager.ARROW]
            };

            TurretTypes[BRONZEARROW] = turretSettings;
            items.Add(turretName, item);
        }

        private static void AddCrossbowTurret(Dictionary<string, ItemTypesServer.ItemTypeRaw> items)
        {
            var turretName = CROSSBOW_NAMESPACE;
            var turretNode = new JSONNode()
              .SetAs("icon", GameLoader.ICON_FOLDER_PANDA + "/CrossbowTurret.png")
              .SetAs("isPlaceable", true)
              .SetAs("onPlaceAudio", "stonePlace")
              .SetAs("onRemoveAudio", "stoneDelete")
              .SetAs("sideall", "stonebricks")
              .SetAs("onRemoveAmount", 1)
              .SetAs("isSolid", true)
              .SetAs("sidex+", CROSSBOW_NAMESPACE + "sides")
              .SetAs("sidex-", CROSSBOW_NAMESPACE + "sides")
              .SetAs("sidez+", CROSSBOW_NAMESPACE + "sides")
              .SetAs("sidez-", CROSSBOW_NAMESPACE + "sides")
              .SetAs("npcLimit", 0);

            var item = new ItemTypesServer.ItemTypeRaw(turretName, turretNode);

            var turretSettings = new TurretSettings()
            {
                TurretItem = item,
                Ammo = new List<InventoryItem>() { new InventoryItem(BuiltinBlocks.CrossbowBolt) },
                AmmoValue = 0.05f,
                Damage = 300f,
                DurabilityPerDoWork = 0.005f,
                FuelPerDoWork = 0.02f,
                Name = CROSSBOW,
                OnShootAudio = "bowShoot",
                OnHitAudio = "fleshHit",
                Range = 30,
                WorkTime = 4f,
                RefuelTime = 6,
                ReloadTime = 7,
                RepairTime = 13,
                RequiredForFix = new Dictionary<float, List<InventoryItem>>()
                {
                    { 10f, new List<InventoryItem>() { new InventoryItem(BuiltinBlocks.StoneBricks, 1), new InventoryItem(BuiltinBlocks.Planks, 1), new InventoryItem(BuiltinBlocks.IronRivet, 6), new InventoryItem(BuiltinBlocks.CopperNails, 6), new InventoryItem(BuiltinBlocks.Crossbow, 2) } },
                    { 30f, new List<InventoryItem>() { new InventoryItem(BuiltinBlocks.StoneBricks, 1), new InventoryItem(BuiltinBlocks.Planks, 1), new InventoryItem(BuiltinBlocks.IronRivet, 4), new InventoryItem(BuiltinBlocks.CopperNails, 4), new InventoryItem(BuiltinBlocks.Crossbow, 1) } },
                    { 50f, new List<InventoryItem>() { new InventoryItem(BuiltinBlocks.StoneBricks, 1), new InventoryItem(BuiltinBlocks.Planks, 1), new InventoryItem(BuiltinBlocks.IronRivet, 2), new InventoryItem(BuiltinBlocks.CopperNails, 4) } },
                    { 75f, new List<InventoryItem>() { new InventoryItem(BuiltinBlocks.StoneBricks, 1), new InventoryItem(BuiltinBlocks.Planks, 1), new InventoryItem(BuiltinBlocks.IronRivet, 1), new InventoryItem(BuiltinBlocks.CopperNails, 2) } },
                },
                ProjectileAnimation = Managers.AnimationManager.AnimatedObjects[Managers.AnimationManager.CROSSBOWBOLT]
            };

            TurretTypes[CROSSBOW] = turretSettings;
            items.Add(turretName, item);
        }

        private static void AddMatchlockTurret(Dictionary<string, ItemTypesServer.ItemTypeRaw> items)
        {
            var turretName = MATCHLOCK_NAMESPACE;
            var turretNode = new JSONNode()
              .SetAs("icon", GameLoader.ICON_FOLDER_PANDA + "/MatchlockTurret.png")
              .SetAs("isPlaceable", true)
              .SetAs("onPlaceAudio", "stonePlace")
              .SetAs("onRemoveAudio", "stoneDelete")
              .SetAs("sideall", "stonebricks")
              .SetAs("onRemoveAmount", 1)
              .SetAs("isSolid", true)
              .SetAs("sidex+", MATCHLOCK_NAMESPACE + "sides")
              .SetAs("sidex-", MATCHLOCK_NAMESPACE + "sides")
              .SetAs("sidez+", MATCHLOCK_NAMESPACE + "sides")
              .SetAs("sidez-", MATCHLOCK_NAMESPACE + "sides")
              .SetAs("npcLimit", 0);

            var item = new ItemTypesServer.ItemTypeRaw(turretName, turretNode);

            var turretSettings = new TurretSettings()
            {
                TurretItem = item,
                Ammo = new List<InventoryItem>() { new InventoryItem(BuiltinBlocks.LeadBullet), new InventoryItem(BuiltinBlocks.GunpowderPouch) },
                AmmoValue = 0.05f,
                Damage = 500f,
                DurabilityPerDoWork = 0.01f,
                FuelPerDoWork = 0.04f,
                Name = MATCHLOCK,
                OnShootAudio = "matchlock",
                OnHitAudio = "fleshHit",
                Range = 35,
                WorkTime = 6f,
                RefuelTime = 7,
                ReloadTime = 8,
                RepairTime = 14,
                RequiredForFix = new Dictionary<float, List<InventoryItem>>()
                {
                    { 10f, new List<InventoryItem>() { new InventoryItem(BuiltinBlocks.StoneBricks, 1), new InventoryItem(BuiltinBlocks.Planks, 1), new InventoryItem(BuiltinBlocks.SteelParts, 6), new InventoryItem(BuiltinBlocks.CopperNails, 6), new InventoryItem(BuiltinBlocks.MatchlockGun, 2) } },
                    { 30f, new List<InventoryItem>() { new InventoryItem(BuiltinBlocks.StoneBricks, 1), new InventoryItem(BuiltinBlocks.Planks, 1), new InventoryItem(BuiltinBlocks.SteelParts, 4), new InventoryItem(BuiltinBlocks.CopperNails, 4), new InventoryItem(BuiltinBlocks.MatchlockGun, 1) } },
                    { 50f, new List<InventoryItem>() { new InventoryItem(BuiltinBlocks.StoneBricks, 1), new InventoryItem(BuiltinBlocks.Planks, 1), new InventoryItem(BuiltinBlocks.SteelParts, 2), new InventoryItem(BuiltinBlocks.CopperNails, 4) } },
                    { 75f, new List<InventoryItem>() { new InventoryItem(BuiltinBlocks.StoneBricks, 1), new InventoryItem(BuiltinBlocks.Planks, 1), new InventoryItem(BuiltinBlocks.SteelParts, 1), new InventoryItem(BuiltinBlocks.CopperNails, 2) } },
                },
                ProjectileAnimation = Managers.AnimationManager.AnimatedObjects[Managers.AnimationManager.LEADBULLET]
            };

            TurretTypes[MATCHLOCK] = turretSettings;
            items.Add(turretName, item);
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnTryChangeBlockUser, GameLoader.NAMESPACE + ".Items.Machines.Turret.OnTryChangeBlockUser")]
        public static bool OnTryChangeBlockUser(ModLoader.OnTryChangeBlockUserData d)
        {
            var turret = TurretTypes.FirstOrDefault(t => t.Value.TurretItem.ItemIndex == d.typeToBuild).Value;

            if (turret != null && d.typeTillNow == BuiltinBlocks.Air)
            {
                PandaChat.Send(d.requestedBy, $"{turret.Name} is ready! Ensure you have a Machinist around to run the machine!");
                MachineManager.RegisterMachineState(d.requestedBy, new MachineState(d.voxelHit.Add(0, 1, 0), d.requestedBy, turret.Name));
            }

            return true;
        }
    }
}
