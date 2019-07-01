using Pandaros.Settlers.Jobs;
using Pandaros.Settlers.Server;
using Pipliz.JSON;
using Recipes;
using System.Collections.Generic;

namespace Pandaros.Settlers.Items.Machines
{
    [ModLoader.ModManager]
    public static class ElementalTurrets
    {
        public static readonly string AIRTURRET_NAMESPACE = GameLoader.NAMESPACE + ".AirTurret";
        public static readonly string EARTHTURRET_NAMESPACE = GameLoader.NAMESPACE + ".EarthTurret";
        public static readonly string FIRETURRET_NAMESPACE = GameLoader.NAMESPACE + ".FireTurret";
        public static readonly string WATERTURRET_NAMESPACE = GameLoader.NAMESPACE + ".WaterTurret";
        public static readonly string VOIDTURRET_NAMESPACE = GameLoader.NAMESPACE + ".VoidTurret";

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterItemTypesDefined, GameLoader.NAMESPACE + ".Items.Machines.ElementalTurrets.RegisterTurret")]
        [ModLoader.ModCallbackProvidesFor(GameLoader.NAMESPACE + ".Items.Machines.Turret.RegisterTurret")]
        [ModLoader.ModCallbackProvidesFor("pipliz.server.loadresearchables")]
        public static void RegisterTurret()
        {
            var planks     = new InventoryItem(ColonyBuiltIn.ItemTypes.PLANKS.Name, 50);
            var stone      = new InventoryItem(ColonyBuiltIn.ItemTypes.STONEBRICKS.Name, 100);
            var tools      = new InventoryItem(ColonyBuiltIn.ItemTypes.COPPERTOOLS.Name, 20);
            var mana       = new InventoryItem(SettlersBuiltIn.ItemTypes.MANA.Name, 4);
            var elemen     = new InventoryItem(Elementium.Item.ItemIndex, 2);
            var esper      = new InventoryItem(Esper.Item.ItemIndex, 1);
            var esperMax   = new InventoryItem(Esper.Item.ItemIndex, 10);
            var airStone   = new InventoryItem(AirStone.Item.ItemIndex, 2);
            var fireStone  = new InventoryItem(FireStone.Item.ItemIndex, 2);
            var waterStone = new InventoryItem(WaterStone.Item.ItemIndex, 2);
            var earthStone = new InventoryItem(SettlersBuiltIn.ItemTypes.EARTHSTONE.Id, 2);
            var voidStone  = new InventoryItem(Void.Item.ItemIndex, 1);

            AddAirTurretTurretSettings();
            AddEarthTurretTurretSettings();
            AddWaterTurretTurretSettings();
            AddFireTurretTurretSettings();
            AddVoidTurretTurretSettings();

            var airRecipe = new Recipe(AIRTURRET_NAMESPACE,
                                       new List<InventoryItem> {planks, elemen, mana, tools, stone, airStone, esper},
                                       new RecipeResult(Turret.TurretSettings[AIRTURRET_NAMESPACE].TurretItem.ItemIndex),
                                       5);

            ServerManager.RecipeStorage.AddLimitTypeRecipe(AdvancedCrafterRegister.JOB_NAME, airRecipe);
            ServerManager.RecipeStorage.AddScienceRequirement(airRecipe);

            var earthRecipe = new Recipe(EARTHTURRET_NAMESPACE,
                                         new List<InventoryItem>
                                         {
                                             planks,
                                             elemen,
                                             mana,
                                             tools,
                                             stone,
                                             earthStone,
                                             esper
                                         },
                                         new RecipeResult(Turret.TurretSettings[EARTHTURRET_NAMESPACE].TurretItem.ItemIndex),
                                         5);

            ServerManager.RecipeStorage.AddLimitTypeRecipe(AdvancedCrafterRegister.JOB_NAME, earthRecipe);
            ServerManager.RecipeStorage.AddScienceRequirement(earthRecipe);

            var fireRecipe = new Recipe(FIRETURRET_NAMESPACE,
                                        new List<InventoryItem> {planks, elemen, mana, tools, stone, fireStone, esper},
                                        new RecipeResult(Turret.TurretSettings[FIRETURRET_NAMESPACE].TurretItem.ItemIndex),
                                        5);

            ServerManager.RecipeStorage.AddLimitTypeRecipe(AdvancedCrafterRegister.JOB_NAME, fireRecipe);
            ServerManager.RecipeStorage.AddScienceRequirement(fireRecipe);

            var waterRecipe = new Recipe(WATERTURRET_NAMESPACE,
                                         new List<InventoryItem>
                                         {
                                             planks,
                                             elemen,
                                             mana,
                                             tools,
                                             stone,
                                             waterStone,
                                             esper
                                         },
                                         new RecipeResult(Turret.TurretSettings[WATERTURRET_NAMESPACE].TurretItem.ItemIndex),
                                         5);

            ServerManager.RecipeStorage.AddLimitTypeRecipe(AdvancedCrafterRegister.JOB_NAME, waterRecipe);
            ServerManager.RecipeStorage.AddScienceRequirement(waterRecipe);

            var voidRecipe = new Recipe(VOIDTURRET_NAMESPACE,
                                        new List<InventoryItem>
                                        {
                                            planks,
                                            elemen,
                                            mana,
                                            tools,
                                            stone,
                                            voidStone,
                                            esperMax
                                        },
                                        new RecipeResult(Turret.TurretSettings[VOIDTURRET_NAMESPACE].TurretItem.ItemIndex),
                                        5);

            ServerManager.RecipeStorage.AddLimitTypeRecipe(AdvancedCrafterRegister.JOB_NAME, voidRecipe);
            ServerManager.RecipeStorage.AddScienceRequirement(voidRecipe);
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AddItemTypes, GameLoader.NAMESPACE + ".Items.Machines.ElementalTurrets.AddTurret")]
        [ModLoader.ModCallbackDependsOn("pipliz.server.applymoditempatches")]
        [ModLoader.ModCallbackDependsOn(GameLoader.NAMESPACE + ".Items.Machines.Turret.AddTurret")]
        public static void AddTurret(Dictionary<string, ItemTypesServer.ItemTypeRaw> items)
        {
            AddAirTurret(items);
            AddEarthTurret(items);
            AddFireTurret(items);
            AddWaterTurret(items);
            AddVoidTurret(items);
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterSelectedWorld,  GameLoader.NAMESPACE + ".Items.Machines.ElementalTurrets.AddTextures")]
        [ModLoader.ModCallbackProvidesFor("pipliz.server.registertexturemappingtextures")]
        public static void AddTextures()
        {
            var textureMapping = new ItemTypesServer.TextureMapping(new JSONNode());
            textureMapping.AlbedoPath = GameLoader.BLOCKS_ALBEDO_PATH + "AirTurret.png";
            textureMapping.NormalPath = GameLoader.BLOCKS_NORMAL_PATH + "Turret.png";
            textureMapping.HeightPath = GameLoader.BLOCKS_HEIGHT_PATH + "Turret.png";

            ItemTypesServer.SetTextureMapping(AIRTURRET_NAMESPACE + "sides", textureMapping);

            var earthTexture = new ItemTypesServer.TextureMapping(new JSONNode());
            earthTexture.AlbedoPath = GameLoader.BLOCKS_ALBEDO_PATH + "EarthTurret.png";
            earthTexture.NormalPath = GameLoader.BLOCKS_NORMAL_PATH + "Turret.png";
            earthTexture.HeightPath = GameLoader.BLOCKS_HEIGHT_PATH + "Turret.png";

            ItemTypesServer.SetTextureMapping(EARTHTURRET_NAMESPACE + "sides", earthTexture);

            var fireTexture = new ItemTypesServer.TextureMapping(new JSONNode());
            fireTexture.AlbedoPath = GameLoader.BLOCKS_ALBEDO_PATH + "FireTurret.png";
            fireTexture.NormalPath = GameLoader.BLOCKS_NORMAL_PATH + "Turret.png";
            fireTexture.HeightPath = GameLoader.BLOCKS_HEIGHT_PATH + "Turret.png";

            ItemTypesServer.SetTextureMapping(FIRETURRET_NAMESPACE + "sides", fireTexture);

            var waterTexture = new ItemTypesServer.TextureMapping(new JSONNode());
            waterTexture.AlbedoPath = GameLoader.BLOCKS_ALBEDO_PATH + "WaterTurret.png";
            waterTexture.NormalPath = GameLoader.BLOCKS_NORMAL_PATH + "Turret.png";
            waterTexture.HeightPath = GameLoader.BLOCKS_HEIGHT_PATH + "Turret.png";

            ItemTypesServer.SetTextureMapping(WATERTURRET_NAMESPACE + "sides", waterTexture);

            var voidTexture = new ItemTypesServer.TextureMapping(new JSONNode());
            voidTexture.AlbedoPath = GameLoader.BLOCKS_ALBEDO_PATH + "VoidTurret.png";
            voidTexture.NormalPath = GameLoader.BLOCKS_NORMAL_PATH + "Turret.png";
            voidTexture.HeightPath = GameLoader.BLOCKS_HEIGHT_PATH + "Turret.png";

            ItemTypesServer.SetTextureMapping(VOIDTURRET_NAMESPACE + "sides", voidTexture);
        }

        private static void AddVoidTurretTurretSettings()
        {
            var turretSettings = new Turret.TurretSetting
            {
                TurretItem          = Turret.TurretTypes[VOIDTURRET_NAMESPACE],
                Ammo                = new List<InventoryItem> {new InventoryItem(Void.Item.ItemIndex)},
                AmmoValue           = 0.04f,
                AmmoReloadValue     = 1f,
                DurabilityPerDoWork = 0.008f,
                FuelPerDoWork       = 0.02f,
                Name                = VOIDTURRET_NAMESPACE,
                OnShootAudio        = VOIDTURRET_NAMESPACE,
                OnHitAudio          = VOIDTURRET_NAMESPACE,
                Range               = 25,
                WorkTime            = 13f,
                RefuelTime          = 15,
                ReloadTime          = 15,
                RepairTime          = 20,
                RequiredForFix = new Dictionary<float, List<InventoryItem>>
                {
                    {
                        75f,
                        new List<InventoryItem>
                        {
                            new InventoryItem(ColonyBuiltIn.ItemTypes.STONEBRICKS.Name, 10),
                            new InventoryItem(SettlersBuiltIn.ItemTypes.MANA.Name, 1),
                            new InventoryItem(ColonyBuiltIn.ItemTypes.COPPERNAILS.Name, 1),
                            new InventoryItem(Esper.Item.ItemIndex, 1)
                        }
                    },
                    {
                        50f,
                        new List<InventoryItem>
                        {
                            new InventoryItem(ColonyBuiltIn.ItemTypes.STONEBRICKS.Name, 10),
                            new InventoryItem(SettlersBuiltIn.ItemTypes.MANA.Name, 2),
                            new InventoryItem(ColonyBuiltIn.ItemTypes.COPPERNAILS.Name, 2),
                            new InventoryItem(Esper.Item.ItemIndex, 2)
                        }
                    },
                    {
                        30f,
                        new List<InventoryItem>
                        {
                            new InventoryItem(ColonyBuiltIn.ItemTypes.STONEBRICKS.Name, 15),
                            new InventoryItem(ColonyBuiltIn.ItemTypes.PLANKS.Name, 1),
                            new InventoryItem(SettlersBuiltIn.ItemTypes.MANA.Name, 3),
                            new InventoryItem(ColonyBuiltIn.ItemTypes.COPPERNAILS.Name, 3),
                            new InventoryItem(Esper.Item.ItemIndex, 3),
                            new InventoryItem(Void.Item.ItemIndex, 1)
                        }
                    },
                    {
                        10f,
                        new List<InventoryItem>
                        {
                            new InventoryItem(ColonyBuiltIn.ItemTypes.STONEBRICKS.Name, 20),
                            new InventoryItem(ColonyBuiltIn.ItemTypes.PLANKS.Name, 1),
                            new InventoryItem(SettlersBuiltIn.ItemTypes.MANA.Name, 4),
                            new InventoryItem(ColonyBuiltIn.ItemTypes.COPPERNAILS.Name, 4),
                            new InventoryItem(Esper.Item.ItemIndex, 4),
                            new InventoryItem(Void.Item.ItemIndex, 2)
                        }
                    }
                },
                ProjectileAnimation = AnimationManager.AnimatedObjects[AnimationManager.LEADBULLET]
            };

            turretSettings.Damage[DamageType.Void]     = 400;
            turretSettings.Damage[DamageType.Physical] = 100;

            Turret.TurretSettings[VOIDTURRET_NAMESPACE] = turretSettings;
        }

        private static void AddFireTurretTurretSettings()
        {
            var turretSettings = new Turret.TurretSetting
            {
                TurretItem          = Turret.TurretTypes[FIRETURRET_NAMESPACE],
                Ammo                = new List<InventoryItem> {new InventoryItem(FireStone.Item.ItemIndex)},
                AmmoValue           = 0.02f,
                AmmoReloadValue     = 0.5f,
                DurabilityPerDoWork = 0.008f,
                FuelPerDoWork       = 0.02f,
                Name                = FIRETURRET_NAMESPACE,
                OnShootAudio        = FIRETURRET_NAMESPACE,
                OnHitAudio          = FIRETURRET_NAMESPACE,
                Range               = 25,
                WorkTime            = 13f,
                RefuelTime          = 15,
                ReloadTime          = 15,
                RepairTime          = 20,
                RequiredForFix = new Dictionary<float, List<InventoryItem>>
                {
                    {
                        75f,
                        new List<InventoryItem>
                        {
                            new InventoryItem(ColonyBuiltIn.ItemTypes.STONEBRICKS.Name, 10),
                            new InventoryItem(SettlersBuiltIn.ItemTypes.MANA.Name, 1),
                            new InventoryItem(ColonyBuiltIn.ItemTypes.COPPERNAILS.Name, 1)
                        }
                    },
                    {
                        50f,
                        new List<InventoryItem>
                        {
                            new InventoryItem(ColonyBuiltIn.ItemTypes.STONEBRICKS.Name, 10),
                            new InventoryItem(SettlersBuiltIn.ItemTypes.MANA.Name, 2),
                            new InventoryItem(ColonyBuiltIn.ItemTypes.COPPERNAILS.Name, 2)
                        }
                    },
                    {
                        30f,
                        new List<InventoryItem>
                        {
                            new InventoryItem(ColonyBuiltIn.ItemTypes.STONEBRICKS.Name, 15),
                            new InventoryItem(ColonyBuiltIn.ItemTypes.PLANKS.Name, 1),
                            new InventoryItem(SettlersBuiltIn.ItemTypes.MANA.Name, 3),
                            new InventoryItem(ColonyBuiltIn.ItemTypes.COPPERNAILS.Name, 3),
                            new InventoryItem(Esper.Item.ItemIndex, 1)
                        }
                    },
                    {
                        10f,
                        new List<InventoryItem>
                        {
                            new InventoryItem(ColonyBuiltIn.ItemTypes.STONEBRICKS.Name, 20),
                            new InventoryItem(ColonyBuiltIn.ItemTypes.PLANKS.Name, 1),
                            new InventoryItem(SettlersBuiltIn.ItemTypes.MANA.Name, 4),
                            new InventoryItem(ColonyBuiltIn.ItemTypes.COPPERNAILS.Name, 4),
                            new InventoryItem(Esper.Item.ItemIndex, 2)
                        }
                    }
                },
                ProjectileAnimation = AnimationManager.AnimatedObjects[AnimationManager.LEADBULLET]
            };

            turretSettings.Damage[DamageType.Fire]     = 400;
            turretSettings.Damage[DamageType.Physical] = 100;

            Turret.TurretSettings[FIRETURRET_NAMESPACE] = turretSettings;
        }

        private static void AddWaterTurretTurretSettings()
        {
            var turretSettings = new Turret.TurretSetting
            {
                TurretItem          = Turret.TurretTypes[WATERTURRET_NAMESPACE],
                Ammo                = new List<InventoryItem> {new InventoryItem(WaterStone.Item.ItemIndex)},
                AmmoValue           = 0.02f,
                AmmoReloadValue     = 0.5f,
                DurabilityPerDoWork = 0.008f,
                FuelPerDoWork       = 0.02f,
                Name                = WATERTURRET_NAMESPACE,
                OnShootAudio        = WATERTURRET_NAMESPACE,
                OnHitAudio          = WATERTURRET_NAMESPACE,
                Range               = 25,
                WorkTime            = 13f,
                RefuelTime          = 15,
                ReloadTime          = 15,
                RepairTime          = 20,
                RequiredForFix = new Dictionary<float, List<InventoryItem>>
                {
                    {
                        75f,
                        new List<InventoryItem>
                        {
                            new InventoryItem(ColonyBuiltIn.ItemTypes.STONEBRICKS.Name, 10),
                            new InventoryItem(SettlersBuiltIn.ItemTypes.MANA.Name, 1),
                            new InventoryItem(ColonyBuiltIn.ItemTypes.COPPERNAILS.Name, 1)
                        }
                    },
                    {
                        50f,
                        new List<InventoryItem>
                        {
                            new InventoryItem(ColonyBuiltIn.ItemTypes.STONEBRICKS.Name, 10),
                            new InventoryItem(SettlersBuiltIn.ItemTypes.MANA.Name, 2),
                            new InventoryItem(ColonyBuiltIn.ItemTypes.COPPERNAILS.Name, 2)
                        }
                    },
                    {
                        30f,
                        new List<InventoryItem>
                        {
                            new InventoryItem(ColonyBuiltIn.ItemTypes.STONEBRICKS.Name, 15),
                            new InventoryItem(ColonyBuiltIn.ItemTypes.PLANKS.Name, 1),
                            new InventoryItem(SettlersBuiltIn.ItemTypes.MANA.Name, 3),
                            new InventoryItem(ColonyBuiltIn.ItemTypes.COPPERNAILS.Name, 3),
                            new InventoryItem(Esper.Item.ItemIndex, 1)
                        }
                    },
                    {
                        10f,
                        new List<InventoryItem>
                        {
                            new InventoryItem(ColonyBuiltIn.ItemTypes.STONEBRICKS.Name, 20),
                            new InventoryItem(ColonyBuiltIn.ItemTypes.PLANKS.Name, 1),
                            new InventoryItem(SettlersBuiltIn.ItemTypes.MANA.Name, 4),
                            new InventoryItem(ColonyBuiltIn.ItemTypes.COPPERNAILS.Name, 4),
                            new InventoryItem(Esper.Item.ItemIndex, 2)
                        }
                    }
                },
                ProjectileAnimation = AnimationManager.AnimatedObjects[AnimationManager.LEADBULLET]
            };

            turretSettings.Damage[DamageType.Water]    = 400;
            turretSettings.Damage[DamageType.Physical] = 100;

            Turret.TurretSettings[WATERTURRET_NAMESPACE] = turretSettings;
        }

        private static void AddEarthTurretTurretSettings()
        {
            var turretSettings = new Turret.TurretSetting
            {
                TurretItem          = Turret.TurretTypes[EARTHTURRET_NAMESPACE],
                Ammo                = new List<InventoryItem> {new InventoryItem(SettlersBuiltIn.ItemTypes.EARTHSTONE.Id) },
                AmmoValue           = 0.02f,
                AmmoReloadValue     = 0.5f,
                DurabilityPerDoWork = 0.008f,
                FuelPerDoWork       = 0.02f,
                Name                = EARTHTURRET_NAMESPACE,
                OnShootAudio        = EARTHTURRET_NAMESPACE,
                OnHitAudio          = EARTHTURRET_NAMESPACE,
                Range               = 25,
                WorkTime            = 13f,
                RefuelTime          = 15,
                ReloadTime          = 15,
                RepairTime          = 20,
                RequiredForFix = new Dictionary<float, List<InventoryItem>>
                {
                    {
                        75f,
                        new List<InventoryItem>
                        {
                            new InventoryItem(ColonyBuiltIn.ItemTypes.STONEBRICKS.Name, 10),
                            new InventoryItem(SettlersBuiltIn.ItemTypes.MANA.Name, 1),
                            new InventoryItem(ColonyBuiltIn.ItemTypes.COPPERNAILS.Name, 1)
                        }
                    },
                    {
                        50f,
                        new List<InventoryItem>
                        {
                            new InventoryItem(ColonyBuiltIn.ItemTypes.STONEBRICKS.Name, 10),
                            new InventoryItem(SettlersBuiltIn.ItemTypes.MANA.Name, 2),
                            new InventoryItem(ColonyBuiltIn.ItemTypes.COPPERNAILS.Name, 2)
                        }
                    },
                    {
                        30f,
                        new List<InventoryItem>
                        {
                            new InventoryItem(ColonyBuiltIn.ItemTypes.STONEBRICKS.Name, 15),
                            new InventoryItem(ColonyBuiltIn.ItemTypes.PLANKS.Name, 1),
                            new InventoryItem(SettlersBuiltIn.ItemTypes.MANA.Name, 3),
                            new InventoryItem(ColonyBuiltIn.ItemTypes.COPPERNAILS.Name, 3),
                            new InventoryItem(Esper.Item.ItemIndex, 1)
                        }
                    },
                    {
                        10f,
                        new List<InventoryItem>
                        {
                            new InventoryItem(ColonyBuiltIn.ItemTypes.STONEBRICKS.Name, 20),
                            new InventoryItem(ColonyBuiltIn.ItemTypes.PLANKS.Name, 1),
                            new InventoryItem(SettlersBuiltIn.ItemTypes.MANA.Name, 4),
                            new InventoryItem(ColonyBuiltIn.ItemTypes.COPPERNAILS.Name, 4),
                            new InventoryItem(Esper.Item.ItemIndex, 2)
                        }
                    }
                },
                ProjectileAnimation = AnimationManager.AnimatedObjects[AnimationManager.LEADBULLET]
            };

            turretSettings.Damage[DamageType.Earth]    = 400;
            turretSettings.Damage[DamageType.Physical] = 100;

            Turret.TurretSettings[EARTHTURRET_NAMESPACE] = turretSettings;
        }

        private static void AddAirTurretTurretSettings()
        {
            var turretSettings = new Turret.TurretSetting
            {
                TurretItem          = Turret.TurretTypes[AIRTURRET_NAMESPACE],
                Ammo                = new List<InventoryItem> {new InventoryItem(AirStone.Item.ItemIndex)},
                AmmoValue           = 0.02f,
                AmmoReloadValue     = 0.5f,
                DurabilityPerDoWork = 0.008f,
                FuelPerDoWork       = 0.02f,
                Name                = AIRTURRET_NAMESPACE,
                OnShootAudio        = AIRTURRET_NAMESPACE,
                OnHitAudio          = AIRTURRET_NAMESPACE,
                Range               = 25,
                WorkTime            = 13f,
                RefuelTime          = 15,
                ReloadTime          = 15,
                RepairTime          = 20,
                RequiredForFix = new Dictionary<float, List<InventoryItem>>
                {
                    {
                        75f,
                        new List<InventoryItem>
                        {
                            new InventoryItem(ColonyBuiltIn.ItemTypes.STONEBRICKS.Name, 10),
                            new InventoryItem(SettlersBuiltIn.ItemTypes.MANA.Name, 1),
                            new InventoryItem(ColonyBuiltIn.ItemTypes.COPPERNAILS.Name, 1)
                        }
                    },
                    {
                        50f,
                        new List<InventoryItem>
                        {
                            new InventoryItem(ColonyBuiltIn.ItemTypes.STONEBRICKS.Name, 10),
                            new InventoryItem(SettlersBuiltIn.ItemTypes.MANA.Name, 2),
                            new InventoryItem(ColonyBuiltIn.ItemTypes.COPPERNAILS.Name, 2)
                        }
                    },
                    {
                        30f,
                        new List<InventoryItem>
                        {
                            new InventoryItem(ColonyBuiltIn.ItemTypes.STONEBRICKS.Name, 15),
                            new InventoryItem(ColonyBuiltIn.ItemTypes.PLANKS.Name, 1),
                            new InventoryItem(SettlersBuiltIn.ItemTypes.MANA.Name, 3),
                            new InventoryItem(ColonyBuiltIn.ItemTypes.COPPERNAILS.Name, 3),
                            new InventoryItem(Esper.Item.ItemIndex, 1)
                        }
                    },
                    {
                        10f,
                        new List<InventoryItem>
                        {
                            new InventoryItem(ColonyBuiltIn.ItemTypes.STONEBRICKS.Name, 20),
                            new InventoryItem(ColonyBuiltIn.ItemTypes.PLANKS.Name, 1),
                            new InventoryItem(SettlersBuiltIn.ItemTypes.MANA.Name, 4),
                            new InventoryItem(ColonyBuiltIn.ItemTypes.COPPERNAILS.Name, 4),
                            new InventoryItem(Esper.Item.ItemIndex, 2)
                        }
                    }
                },
                ProjectileAnimation = AnimationManager.AnimatedObjects[AnimationManager.LEADBULLET]
            };

            turretSettings.Damage[DamageType.Air]      = 400;
            turretSettings.Damage[DamageType.Physical] = 100;

            Turret.TurretSettings[AIRTURRET_NAMESPACE] = turretSettings;
        }

        private static void AddVoidTurret(Dictionary<string, ItemTypesServer.ItemTypeRaw> items)
        {
            var turretName = VOIDTURRET_NAMESPACE;

            var turretNode = new JSONNode()
                            .SetAs("icon", GameLoader.ICON_PATH + "VoidTurret.png")
                            .SetAs("isPlaceable", "true")
                            .SetAs("onPlaceAudio", "stonePlace")
                            .SetAs("onRemoveAudio", "stoneDelete")
                            .SetAs("onRemoveAmount", 1)
                            .SetAs("sideall", ColonyBuiltIn.ItemTypes.STONEBRICKS.Name)
                            .SetAs("isSolid", "true")
                            .SetAs("sidex+", VOIDTURRET_NAMESPACE + "sides")
                            .SetAs("sidex-", VOIDTURRET_NAMESPACE + "sides")
                            .SetAs("sidez+", VOIDTURRET_NAMESPACE + "sides")
                            .SetAs("sidez-", VOIDTURRET_NAMESPACE + "sides");

            var categories = new JSONNode(NodeType.Array);
            categories.AddToArray(new JSONNode("machine"));
            categories.AddToArray(new JSONNode("turret"));
            categories.AddToArray(new JSONNode("magic"));
            turretNode.SetAs("categories", categories);

            var item = new ItemTypesServer.ItemTypeRaw(turretName, turretNode);
            Turret.TurretTypes[VOIDTURRET_NAMESPACE] = item;
            items.Add(turretName, item);
        }

        private static void AddWaterTurret(Dictionary<string, ItemTypesServer.ItemTypeRaw> items)
        {
            var turretName = WATERTURRET_NAMESPACE;

            var turretNode = new JSONNode()
                            .SetAs("icon", GameLoader.ICON_PATH + "WaterTurret.png")
                            .SetAs("isPlaceable", "true")
                            .SetAs("onPlaceAudio", "stonePlace")
                            .SetAs("onRemoveAudio", "stoneDelete")
                            .SetAs("onRemoveAmount", 1)
                            .SetAs("sideall", ColonyBuiltIn.ItemTypes.STONEBRICKS.Name)
                            .SetAs("isSolid", "true")
                            .SetAs("sidex+", WATERTURRET_NAMESPACE + "sides")
                            .SetAs("sidex-", WATERTURRET_NAMESPACE + "sides")
                            .SetAs("sidez+", WATERTURRET_NAMESPACE + "sides")
                            .SetAs("sidez-", WATERTURRET_NAMESPACE + "sides");

            var categories = new JSONNode(NodeType.Array);
            categories.AddToArray(new JSONNode("machine"));
            categories.AddToArray(new JSONNode("turret"));
            categories.AddToArray(new JSONNode("magic"));
            turretNode.SetAs("categories", categories);

            var item = new ItemTypesServer.ItemTypeRaw(turretName, turretNode);
            Turret.TurretTypes[WATERTURRET_NAMESPACE] = item;
            items.Add(turretName, item);
        }

        private static void AddEarthTurret(Dictionary<string, ItemTypesServer.ItemTypeRaw> items)
        {
            var turretName = EARTHTURRET_NAMESPACE;

            var turretNode = new JSONNode()
                            .SetAs("icon", GameLoader.ICON_PATH + "EarthTurret.png")
                            .SetAs("isPlaceable", "true")
                            .SetAs("onPlaceAudio", "stonePlace")
                            .SetAs("onRemoveAudio", "stoneDelete")
                            .SetAs("onRemoveAmount", 1)
                            .SetAs("sideall", ColonyBuiltIn.ItemTypes.STONEBRICKS.Name)
                            .SetAs("isSolid", "true")
                            .SetAs("sidex+", EARTHTURRET_NAMESPACE + "sides")
                            .SetAs("sidex-", EARTHTURRET_NAMESPACE + "sides")
                            .SetAs("sidez+", EARTHTURRET_NAMESPACE + "sides")
                            .SetAs("sidez-", EARTHTURRET_NAMESPACE + "sides");

            var categories = new JSONNode(NodeType.Array);
            categories.AddToArray(new JSONNode("machine"));
            categories.AddToArray(new JSONNode("turret"));
            categories.AddToArray(new JSONNode("magic"));
            turretNode.SetAs("categories", categories);

            var item = new ItemTypesServer.ItemTypeRaw(turretName, turretNode);
            Turret.TurretTypes[EARTHTURRET_NAMESPACE] = item;
            items.Add(turretName, item);
        }

        private static void AddFireTurret(Dictionary<string, ItemTypesServer.ItemTypeRaw> items)
        {
            var turretName = FIRETURRET_NAMESPACE;

            var turretNode = new JSONNode()
                            .SetAs("icon", GameLoader.ICON_PATH + "FireTurret.png")
                            .SetAs("isPlaceable", "true")
                            .SetAs("onPlaceAudio", "stonePlace")
                            .SetAs("onRemoveAudio", "stoneDelete")
                            .SetAs("onRemoveAmount", 1)
                            .SetAs("sideall", ColonyBuiltIn.ItemTypes.STONEBRICKS.Name)
                            .SetAs("isSolid", "true")
                            .SetAs("sidex+", FIRETURRET_NAMESPACE + "sides")
                            .SetAs("sidex-", FIRETURRET_NAMESPACE + "sides")
                            .SetAs("sidez+", FIRETURRET_NAMESPACE + "sides")
                            .SetAs("sidez-", FIRETURRET_NAMESPACE + "sides");

            var categories = new JSONNode(NodeType.Array);
            categories.AddToArray(new JSONNode("machine"));
            categories.AddToArray(new JSONNode("turret"));
            categories.AddToArray(new JSONNode("magic"));
            turretNode.SetAs("categories", categories);

            var item = new ItemTypesServer.ItemTypeRaw(turretName, turretNode);
            Turret.TurretTypes[FIRETURRET_NAMESPACE] = item;
            items.Add(turretName, item);
        }

        private static void AddAirTurret(Dictionary<string, ItemTypesServer.ItemTypeRaw> items)
        {
            var turretName = AIRTURRET_NAMESPACE;

            var turretNode = new JSONNode()
                            .SetAs("icon", GameLoader.ICON_PATH + "AirTurret.png")
                            .SetAs("isPlaceable", "true")
                            .SetAs("onPlaceAudio", "stonePlace")
                            .SetAs("onRemoveAudio", "stoneDelete")
                            .SetAs("onRemoveAmount", 1)
                            .SetAs("sideall", ColonyBuiltIn.ItemTypes.STONEBRICKS.Name)
                            .SetAs("isSolid", "true")
                            .SetAs("sidex+", AIRTURRET_NAMESPACE + "sides")
                            .SetAs("sidex-", AIRTURRET_NAMESPACE + "sides")
                            .SetAs("sidez+", AIRTURRET_NAMESPACE + "sides")
                            .SetAs("sidez-", AIRTURRET_NAMESPACE + "sides");

            var categories = new JSONNode(NodeType.Array);
            categories.AddToArray(new JSONNode("machine"));
            categories.AddToArray(new JSONNode("turret"));
            categories.AddToArray(new JSONNode("magic"));
            turretNode.SetAs("categories", categories);

            var item = new ItemTypesServer.ItemTypeRaw(turretName, turretNode);
            Turret.TurretTypes[AIRTURRET_NAMESPACE] = item;
            items.Add(turretName, item);
        }
    }
}