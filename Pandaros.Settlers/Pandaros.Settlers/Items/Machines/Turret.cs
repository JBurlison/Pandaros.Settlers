using Monsters;
using Pandaros.API;
using Pandaros.API.Jobs.Roaming;
using Pandaros.API.Models;
using Pandaros.API.Server;
using Pandaros.Settlers.Jobs;
using Pipliz;
using Pipliz.JSON;
using Recipes;
using Shared;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Pandaros.Settlers.Items.Machines
{
    public class TurretRegister : IRoamingJobObjective
    {
        public TurretRegister(Turret.TurretSetting setting)
        {
            name = setting.Name;
            WorkTime = setting.WorkTime;
            ItemIndex = ItemId.GetItemId(Turret.TurretTypes[setting.Name].ItemIndex);
        }

        public string name { get; private set; }
        public float WorkTime { get; private set; }
        public ItemId ItemIndex { get; private set; }

        public Dictionary<string, IRoamingJobObjectiveAction> ActionCallbacks { get; } = new Dictionary<string, IRoamingJobObjectiveAction>()
        {
            { MachineConstants.REFUEL, new RefuelMachineAction() },
            { MachineConstants.REPAIR, new RepairTurret() },
            { MachineConstants.RELOAD, new ReloadTurret() }
        };

        public string ObjectiveCategory => MachineConstants.MECHANICAL;

        public float WatchArea => 21;

        public void DoWork(Colony c, RoamingJobState state)
        {
            Turret.DoWork(c, state);
        }
    }

    public class RepairTurret : IRoamingJobObjectiveAction
    {
        public string name => MachineConstants.REPAIR;
        public float ActionEnergyMinForFix => .5f;
        public float TimeToPreformAction => 10;

        public string AudioKey => GameLoader.NAMESPACE + ".HammerAudio";

        public ItemId ObjectiveLoadEmptyIcon => ItemId.GetItemId(GameLoader.NAMESPACE + ".Repairing");

        public ItemId PreformAction(Colony colony, RoamingJobState state)
        {
            return Turret.Repair(colony, state);
        }
    }

    public class ReloadTurret : IRoamingJobObjectiveAction
    {
        public string name => MachineConstants.RELOAD;
        public float ActionEnergyMinForFix => .5f;
        public float TimeToPreformAction => 5;

        public string AudioKey => GameLoader.NAMESPACE + ".ReloadingAudio";

        public ItemId ObjectiveLoadEmptyIcon => ItemId.GetItemId(GameLoader.NAMESPACE + ".Reload");

        public ItemId PreformAction(Colony colony, RoamingJobState state)
        {
            return Turret.Reload(colony, state);
        }
    }

    [ModLoader.ModManager]
    public static class Turret
    {
        public static Dictionary<string, TurretSetting> TurretSettings = new Dictionary<string, TurretSetting>();

        public static Dictionary<string, ItemTypesServer.ItemTypeRaw> TurretTypes = new Dictionary<string, ItemTypesServer.ItemTypeRaw>();

        public static readonly string STONE_NAMESPACE = GameLoader.NAMESPACE + ".StoneTurret";
        public static readonly string BRONZEARROW_NAMESPACE = GameLoader.NAMESPACE + ".BronzeArrowTurret";
        public static readonly string CROSSBOW_NAMESPACE = GameLoader.NAMESPACE + ".CrossbowTurret";
        public static readonly string MATCHLOCK_NAMESPACE = GameLoader.NAMESPACE + ".MatchlockTurret";

        public static ItemId Repair(Colony colony, RoamingJobState machineState)
        {
            var retval = ItemId.GetItemId(GameLoader.NAMESPACE + ".Repairing");

            try
            {
                if (machineState.GetActionEnergy(MachineConstants.REPAIR) < .75f && TurretSettings.ContainsKey(machineState.RoamObjective))
                {
                    var repaired       = false;
                    var requiredForFix = new List<InventoryItem>();
                    var stockpile      = colony.Stockpile;

                    foreach (var durability in TurretSettings[machineState.RoamObjective]
                                                .RequiredForFix.OrderByDescending(s => s.Key))
                        if (machineState.GetActionEnergy(MachineConstants.REPAIR) < durability.Key)
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
                    {
                        foreach (var item in requiredForFix)
                            if (!stockpile.Contains(item) && item.Type != 0)
                            {
                                retval = ItemId.GetItemId(item.Type);
                                break;
                            }
                    }

                    if (repaired)
                        machineState.ResetActionToMaxLoad(MachineConstants.REPAIR);
                }
            }
            catch (Exception ex)
            {
                SettlersLogger.LogError(ex);
            }

            return retval;
        }

        public static ItemId Reload(Colony colony, RoamingJobState machineState)
        {
            var retval = ItemId.GetItemId(GameLoader.NAMESPACE + ".Reload");

            try
            {
                if (TurretSettings.ContainsKey(machineState.RoamObjective) && machineState.GetActionEnergy(MachineConstants.RELOAD) < .75f)
                {
                    var stockpile = colony.Stockpile;

                    while (stockpile.Contains(TurretSettings[machineState.RoamObjective].Ammo) &&
                            machineState.GetActionEnergy(MachineConstants.RELOAD) <= RoamingJobState.GetActionsMaxEnergy(MachineConstants.RELOAD, colony, MachineConstants.MECHANICAL))
                        if (stockpile.TryRemove(TurretSettings[machineState.RoamObjective].Ammo))
                        {
                            machineState.AddToActionEmergy(MachineConstants.RELOAD, TurretSettings[machineState.RoamObjective].AmmoReloadValue);

                            if (TurretSettings[machineState.RoamObjective].Ammo.Any(itm => itm.Type == ColonyBuiltIn.ItemTypes.GUNPOWDERPOUCH))
                                stockpile.Add(ColonyBuiltIn.ItemTypes.LINENPOUCH);
                        }

                    if (machineState.GetActionEnergy(MachineConstants.RELOAD) < RoamingJobState.GetActionsMaxEnergy(MachineConstants.RELOAD, colony, MachineConstants.MECHANICAL))
                        retval = ItemId.GetItemId(TurretSettings[machineState.RoamObjective].Ammo.FirstOrDefault(ammo => !stockpile.Contains(ammo)).Type);
                }
            }
            catch (Exception ex)
            {
                SettlersLogger.LogError(ex);
            }

            return retval;
        }

        public static void DoWork(Colony colony, RoamingJobState machineState)
        {
            try
            {
                if (TurretSettings.ContainsKey(machineState.RoamObjective) &&
                    machineState.GetActionEnergy(MachineConstants.REPAIR) > 0 &&
                    machineState.GetActionEnergy(MachineConstants.REFUEL) > 0 &&
                    machineState.NextTimeForWork < Time.SecondsSinceStartDouble)
                {
                    var stockpile = colony.Stockpile;

                    machineState.SubtractFromActionEnergy(MachineConstants.REPAIR, TurretSettings[machineState.RoamObjective].DurabilityPerDoWork);
                    machineState.SubtractFromActionEnergy(MachineConstants.REFUEL, TurretSettings[machineState.RoamObjective].FuelPerDoWork);

                    if (machineState.GetActionEnergy(MachineConstants.RELOAD) > 0)
                    {
                        var totalDamage = TurretSettings[machineState.RoamObjective].TotalDamage;

                        var monster = MonsterTracker.Find(machineState.Position.Add(0, 1, 0),
                                                            TurretSettings[machineState.RoamObjective].Range,
                                                            totalDamage);

                        if (monster == null)
                            monster = MonsterTracker.Find(machineState.Position.Add(1, 0, 0),
                                                            TurretSettings[machineState.RoamObjective].Range,
                                                            totalDamage);

                        if (monster == null)
                            monster = MonsterTracker.Find(machineState.Position.Add(-1, 0, 0),
                                                            TurretSettings[machineState.RoamObjective].Range,
                                                            totalDamage);

                        if (monster == null)
                            monster = MonsterTracker.Find(machineState.Position.Add(0, -1, 0),
                                                            TurretSettings[machineState.RoamObjective].Range,
                                                            totalDamage);

                        if (monster == null)
                            monster = MonsterTracker.Find(machineState.Position.Add(0, 0, 1),
                                                            TurretSettings[machineState.RoamObjective].Range,
                                                            totalDamage);

                        if (monster == null)
                            monster = MonsterTracker.Find(machineState.Position.Add(0, 0, -1),
                                                            TurretSettings[machineState.RoamObjective].Range,
                                                            totalDamage);

                        if (monster != null)
                        {
                            machineState.SubtractFromActionEnergy(MachineConstants.RELOAD, TurretSettings[machineState.RoamObjective].AmmoValue);

                            if (World.TryGetTypeAt(machineState.Position.Add(0, 1, 0), out ushort above) && above == ColonyBuiltIn.ItemTypes.AIR.Id)
                                Indicator.SendIconIndicatorNear(machineState.Position.Add(0, 1, 0).Vector,
                                                                new IndicatorState(TurretSettings[machineState.RoamObjective].WorkTime,
                                                                                    TurretSettings[machineState.RoamObjective].Ammo.FirstOrDefault().Type));

                            if (TurretSettings[machineState.RoamObjective].OnShootAudio != null)
                                AudioManager.SendAudio(machineState.Position.Vector, TurretSettings[machineState.RoamObjective].OnShootAudio);

                            if (TurretSettings[machineState.RoamObjective].OnHitAudio != null)
                                AudioManager.SendAudio(monster.PositionToAimFor,TurretSettings[machineState.RoamObjective].OnHitAudio);

                            TurretSettings[machineState.RoamObjective]
                                .ProjectileAnimation
                                .SendMoveToInterpolated(machineState.Position.Vector, monster.PositionToAimFor);

                            ServerManager.SendParticleTrail(machineState.Position.Vector, monster.PositionToAimFor, 2);
                            monster.OnHit(totalDamage, machineState, ModLoader.OnHitData.EHitSourceType.Misc);
                        }
                    }

                    machineState.NextTimeForWork =
                        machineState.RoamingJobSettings.WorkTime + Time.SecondsSinceStartDouble;
                }
            }
            catch (Exception ex)
            {
                SettlersLogger.LogError(ex, $"Turret shoot for {machineState.RoamObjective}");
            }
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterItemTypesDefined,
            GameLoader.NAMESPACE + ".Items.Machines.Turret.RegisterTurret")]
        [ModLoader.ModCallbackProvidesFor("pipliz.server.loadresearchables")]
        public static void RegisterTurret()
        {
            var rivets      = new InventoryItem(ColonyBuiltIn.ItemTypes.IRONRIVET.Name, 6);
            var iron        = new InventoryItem(ColonyBuiltIn.ItemTypes.IRONWROUGHT.Name, 2);
            var copperParts = new InventoryItem(ColonyBuiltIn.ItemTypes.COPPERPARTS.Name, 6);
            var copperNails = new InventoryItem(ColonyBuiltIn.ItemTypes.COPPERNAILS.Name, 6);
            var tools       = new InventoryItem(ColonyBuiltIn.ItemTypes.COPPERTOOLS.Name, 1);
            var planks      = new InventoryItem(ColonyBuiltIn.ItemTypes.PLANKS.Name, 2);
            var stone       = new InventoryItem(ColonyBuiltIn.ItemTypes.STONEBRICKS.Name, 4);
            var bronze      = new InventoryItem(ColonyBuiltIn.ItemTypes.BRONZEPLATE.Name, 2);
            var bronzeIngot = new InventoryItem(ColonyBuiltIn.ItemTypes.BRONZEINGOT.Name, 3);
            var steelParts  = new InventoryItem(ColonyBuiltIn.ItemTypes.STEELPARTS.Name, 6);
            var steelIngot  = new InventoryItem(ColonyBuiltIn.ItemTypes.STEELINGOT.Name, 2);

            var stoneAmmo = new InventoryItem(ColonyBuiltIn.ItemTypes.SLINGBULLET.Name, 10);
            var sling     = new InventoryItem(ColonyBuiltIn.ItemTypes.SLING.Name, 2);

            var arrow = new InventoryItem(ColonyBuiltIn.ItemTypes.BRONZEARROW.Name, 10);
            var bow   = new InventoryItem(ColonyBuiltIn.ItemTypes.BOW.Name, 2);

            var bolt     = new InventoryItem(ColonyBuiltIn.ItemTypes.CROSSBOWBOLT.Name, 10);
            var crossbow = new InventoryItem(ColonyBuiltIn.ItemTypes.CROSSBOW.Name, 2);

            var bullet    = new InventoryItem(ColonyBuiltIn.ItemTypes.LEADBULLET.Name, 10);
            var gunpowder = new InventoryItem(ColonyBuiltIn.ItemTypes.GUNPOWDERPOUCH.Name, 3);
            var matchlock = new InventoryItem(ColonyBuiltIn.ItemTypes.MATCHLOCKGUN.Name, 2);

            AddStoneTurretSettings();
            AddBronzeArrowTurretSettings();
            AddCrossBowTurretSettings();
            AddMatchlockTurretSettings();

            var stonerecipe = new Recipe(STONE_NAMESPACE,
                                         new List<InventoryItem>
                                         {
                                             planks,
                                             copperParts,
                                             copperNails,
                                             tools,
                                             stone,
                                             sling,
                                             stoneAmmo
                                         },
                                         new RecipeResult(TurretSettings[STONE_NAMESPACE].TurretItem.ItemIndex),
                                         5);

            ServerManager.RecipeStorage.AddLimitTypeRecipe(AdvancedCrafterRegister.JOB_NAME, stonerecipe);

            var bronzeArrowrecipe = new Recipe(BRONZEARROW_NAMESPACE,
                                               new List<InventoryItem>
                                               {
                                                   planks,
                                                   bronze,
                                                   bronzeIngot,
                                                   copperNails,
                                                   tools,
                                                   stone,
                                                   arrow,
                                                   bow
                                               },
                                               new RecipeResult(TurretSettings[BRONZEARROW_NAMESPACE].TurretItem.ItemIndex),
                                               5);

            ServerManager.RecipeStorage.AddLimitTypeRecipe(AdvancedCrafterRegister.JOB_NAME, bronzeArrowrecipe);

            var crossbowrecipe = new Recipe(CROSSBOW_NAMESPACE,
                                            new List<InventoryItem>
                                            {
                                                planks,
                                                iron,
                                                rivets,
                                                copperNails,
                                                tools,
                                                stone,
                                                bolt,
                                                crossbow
                                            },
                                            new RecipeResult(TurretSettings[CROSSBOW_NAMESPACE].TurretItem.ItemIndex),
                                            5);

            ServerManager.RecipeStorage.AddLimitTypeRecipe(AdvancedCrafterRegister.JOB_NAME, crossbowrecipe);

            var matchlockrecipe = new Recipe(MATCHLOCK_NAMESPACE,
                                             new List<InventoryItem>
                                             {
                                                 planks,
                                                 steelParts,
                                                 steelIngot,
                                                 copperNails,
                                                 tools,
                                                 stone,
                                                 bullet,
                                                 gunpowder,
                                                 matchlock
                                             },
                                             new RecipeResult(TurretSettings[MATCHLOCK_NAMESPACE].TurretItem.ItemIndex),
                                             5);

            ServerManager.RecipeStorage.AddLimitTypeRecipe(AdvancedCrafterRegister.JOB_NAME, matchlockrecipe);

            foreach (var turret in TurretSettings)
                RoamingJobManager.RegisterObjectiveType(new TurretRegister(turret.Value));
        }


        private static void AddStoneTurretSettings()
        {
            var turretSettings = new TurretSetting
            {
                TurretItem          = TurretTypes[STONE_NAMESPACE],
                Ammo                = new List<InventoryItem> {new InventoryItem(ColonyBuiltIn.ItemTypes.SLINGBULLET.Name)},
                AmmoValue           = 0.04f,
                DurabilityPerDoWork = 0.005f,
                FuelPerDoWork       = 0.003f,
                Name                = STONE_NAMESPACE,
                OnShootAudio        = "sling",
                OnHitAudio          = "fleshHit",
                Range               = 10,
                WorkTime            = 5f,
                RefuelTime          = 5,
                ReloadTime          = 6,
                RepairTime          = 11,
                RequiredForFix = new Dictionary<float, List<InventoryItem>>
                {
                    {
                        75f,
                        new List<InventoryItem>
                        {
                            new InventoryItem(ColonyBuiltIn.ItemTypes.STONEBRICKS.Name, 1),
                            new InventoryItem(ColonyBuiltIn.ItemTypes.COPPERPARTS.Name, 1),
                            new InventoryItem(ColonyBuiltIn.ItemTypes.COPPERNAILS.Name, 1)
                        }
                    },
                    {
                        50f,
                        new List<InventoryItem>
                        {
                            new InventoryItem(ColonyBuiltIn.ItemTypes.STONEBRICKS.Name, 1),
                            new InventoryItem(ColonyBuiltIn.ItemTypes.COPPERPARTS.Name, 2),
                            new InventoryItem(ColonyBuiltIn.ItemTypes.COPPERNAILS.Name, 2)
                        }
                    },
                    {
                        30f,
                        new List<InventoryItem>
                        {
                            new InventoryItem(ColonyBuiltIn.ItemTypes.STONEBRICKS.Name, 1),
                            new InventoryItem(ColonyBuiltIn.ItemTypes.PLANKS.Name, 1),
                            new InventoryItem(ColonyBuiltIn.ItemTypes.COPPERPARTS.Name, 3),
                            new InventoryItem(ColonyBuiltIn.ItemTypes.COPPERNAILS.Name, 3),
                            new InventoryItem(ColonyBuiltIn.ItemTypes.SLING.Name, 1)
                        }
                    },
                    {
                        10f,
                        new List<InventoryItem>
                        {
                            new InventoryItem(ColonyBuiltIn.ItemTypes.STONEBRICKS.Name, 1),
                            new InventoryItem(ColonyBuiltIn.ItemTypes.PLANKS.Name, 1),
                            new InventoryItem(ColonyBuiltIn.ItemTypes.COPPERPARTS.Name, 4),
                            new InventoryItem(ColonyBuiltIn.ItemTypes.COPPERNAILS.Name, 4),
                            new InventoryItem(ColonyBuiltIn.ItemTypes.SLING.Name, 2)
                        }
                    }
                },
                ProjectileAnimation = AnimationManager.AnimatedObjects[AnimationManager.SLINGBULLET]
            };

            turretSettings.Damage[DamageType.Physical] = 50;

            TurretSettings[STONE_NAMESPACE] = turretSettings;
        }

        private static void AddBronzeArrowTurretSettings()
        {
            var turretSettings = new TurretSetting
            {
                TurretItem          = TurretTypes[BRONZEARROW_NAMESPACE],
                Ammo                = new List<InventoryItem> {new InventoryItem(ColonyBuiltIn.ItemTypes.BRONZEARROW.Name)},
                AmmoValue           = 0.04f,
                DurabilityPerDoWork = 0.003f,
                FuelPerDoWork       = 0.01f,
                Name                = BRONZEARROW_NAMESPACE,
                OnShootAudio        = "bowShoot",
                OnHitAudio          = "fleshHit",
                Range               = 15,
                WorkTime            = 7f,
                RefuelTime          = 6,
                ReloadTime          = 7,
                RepairTime          = 13,
                RequiredForFix = new Dictionary<float, List<InventoryItem>>
                {
                    {
                        75f,
                        new List<InventoryItem>
                        {
                            new InventoryItem(ColonyBuiltIn.ItemTypes.STONEBRICKS.Name, 1),
                            new InventoryItem(ColonyBuiltIn.ItemTypes.COPPERPARTS.Name, 1),
                            new InventoryItem(ColonyBuiltIn.ItemTypes.COPPERNAILS.Name, 1)
                        }
                    },
                    {
                        50f,
                        new List<InventoryItem>
                        {
                            new InventoryItem(ColonyBuiltIn.ItemTypes.STONEBRICKS.Name, 1),
                            new InventoryItem(ColonyBuiltIn.ItemTypes.COPPERPARTS.Name, 2),
                            new InventoryItem(ColonyBuiltIn.ItemTypes.COPPERNAILS.Name, 2)
                        }
                    },
                    {
                        30f,
                        new List<InventoryItem>
                        {
                            new InventoryItem(ColonyBuiltIn.ItemTypes.STONEBRICKS.Name, 1),
                            new InventoryItem(ColonyBuiltIn.ItemTypes.PLANKS.Name, 1),
                            new InventoryItem(ColonyBuiltIn.ItemTypes.COPPERPARTS.Name, 3),
                            new InventoryItem(ColonyBuiltIn.ItemTypes.COPPERNAILS.Name, 3),
                            new InventoryItem(ColonyBuiltIn.ItemTypes.BOW.Name, 1)
                        }
                    },
                    {
                        10f,
                        new List<InventoryItem>
                        {
                            new InventoryItem(ColonyBuiltIn.ItemTypes.STONEBRICKS.Name, 1),
                            new InventoryItem(ColonyBuiltIn.ItemTypes.PLANKS.Name, 1),
                            new InventoryItem(ColonyBuiltIn.ItemTypes.COPPERPARTS.Name, 4),
                            new InventoryItem(ColonyBuiltIn.ItemTypes.COPPERNAILS.Name, 4),
                            new InventoryItem(ColonyBuiltIn.ItemTypes.BOW.Name, 2)
                        }
                    }
                },
                ProjectileAnimation = AnimationManager.AnimatedObjects[AnimationManager.ARROW]
            };

            turretSettings.Damage[DamageType.Physical] = 100;

            TurretSettings[BRONZEARROW_NAMESPACE] = turretSettings;
        }

        private static void AddCrossBowTurretSettings()
        {
            var turretSettings = new TurretSetting
            {
                TurretItem          = TurretTypes[CROSSBOW_NAMESPACE],
                Ammo                = new List<InventoryItem> {new InventoryItem(ColonyBuiltIn.ItemTypes.CROSSBOWBOLT.Name)},
                AmmoValue           = 0.04f,
                DurabilityPerDoWork = 0.005f,
                FuelPerDoWork       = 0.02f,
                Name                = CROSSBOW_NAMESPACE,
                OnShootAudio        = "bowShoot",
                OnHitAudio          = "fleshHit",
                Range               = 20,
                WorkTime            = 10f,
                RefuelTime          = 7,
                ReloadTime          = 8,
                RepairTime          = 14,
                RequiredForFix = new Dictionary<float, List<InventoryItem>>
                {
                    {
                        75f,
                        new List<InventoryItem>
                        {
                            new InventoryItem(ColonyBuiltIn.ItemTypes.STONEBRICKS.Name, 1),
                            new InventoryItem(ColonyBuiltIn.ItemTypes.IRONRIVET.Name, 1),
                            new InventoryItem(ColonyBuiltIn.ItemTypes.COPPERNAILS.Name, 1)
                        }
                    },
                    {
                        50f,
                        new List<InventoryItem>
                        {
                            new InventoryItem(ColonyBuiltIn.ItemTypes.STONEBRICKS.Name, 1),
                            new InventoryItem(ColonyBuiltIn.ItemTypes.IRONRIVET.Name, 2),
                            new InventoryItem(ColonyBuiltIn.ItemTypes.COPPERNAILS.Name, 2)
                        }
                    },
                    {
                        30f,
                        new List<InventoryItem>
                        {
                            new InventoryItem(ColonyBuiltIn.ItemTypes.STONEBRICKS.Name, 1),
                            new InventoryItem(ColonyBuiltIn.ItemTypes.PLANKS.Name, 1),
                            new InventoryItem(ColonyBuiltIn.ItemTypes.IRONRIVET.Name, 3),
                            new InventoryItem(ColonyBuiltIn.ItemTypes.COPPERNAILS.Name, 3),
                            new InventoryItem(ColonyBuiltIn.ItemTypes.CROSSBOW.Name, 1)
                        }
                    },
                    {
                        10f,
                        new List<InventoryItem>
                        {
                            new InventoryItem(ColonyBuiltIn.ItemTypes.STONEBRICKS.Name, 1),
                            new InventoryItem(ColonyBuiltIn.ItemTypes.PLANKS.Name, 1),
                            new InventoryItem(ColonyBuiltIn.ItemTypes.IRONRIVET.Name, 4),
                            new InventoryItem(ColonyBuiltIn.ItemTypes.COPPERNAILS.Name, 4),
                            new InventoryItem(ColonyBuiltIn.ItemTypes.CROSSBOW.Name, 2)
                        }
                    }
                },
                ProjectileAnimation = AnimationManager.AnimatedObjects[AnimationManager.CROSSBOWBOLT]
            };

            turretSettings.Damage[DamageType.Physical] = 300;

            TurretSettings[CROSSBOW_NAMESPACE] = turretSettings;
        }

        private static void AddMatchlockTurretSettings()
        {
            var turretSettings = new TurretSetting
            {
                TurretItem = TurretTypes[MATCHLOCK_NAMESPACE],
                Ammo = new List<InventoryItem>
                {
                    new InventoryItem(ColonyBuiltIn.ItemTypes.LEADBULLET.Name),
                    new InventoryItem(ColonyBuiltIn.ItemTypes.GUNPOWDERPOUCH.Name)
                },
                AmmoValue           = 0.04f,
                DurabilityPerDoWork = 0.008f,
                FuelPerDoWork       = 0.02f,
                Name                = MATCHLOCK_NAMESPACE,
                OnShootAudio        = "matchlock",
                OnHitAudio          = "fleshHit",
                Range               = 25,
                WorkTime            = 14f,
                RefuelTime          = 8,
                ReloadTime          = 9,
                RepairTime          = 15,
                RequiredForFix = new Dictionary<float, List<InventoryItem>>
                {
                    {
                        75f,
                        new List<InventoryItem>
                        {
                            new InventoryItem(ColonyBuiltIn.ItemTypes.STONEBRICKS.Name, 1),
                            new InventoryItem(ColonyBuiltIn.ItemTypes.STEELPARTS.Name, 1),
                            new InventoryItem(ColonyBuiltIn.ItemTypes.COPPERNAILS.Name, 1)
                        }
                    },
                    {
                        50f,
                        new List<InventoryItem>
                        {
                            new InventoryItem(ColonyBuiltIn.ItemTypes.STONEBRICKS.Name, 1),
                            new InventoryItem(ColonyBuiltIn.ItemTypes.STEELPARTS.Name, 2),
                            new InventoryItem(ColonyBuiltIn.ItemTypes.COPPERNAILS.Name, 2)
                        }
                    },
                    {
                        30f,
                        new List<InventoryItem>
                        {
                            new InventoryItem(ColonyBuiltIn.ItemTypes.STONEBRICKS.Name, 1),
                            new InventoryItem(ColonyBuiltIn.ItemTypes.PLANKS.Name, 1),
                            new InventoryItem(ColonyBuiltIn.ItemTypes.STEELPARTS.Name, 3),
                            new InventoryItem(ColonyBuiltIn.ItemTypes.COPPERNAILS.Name, 3),
                            new InventoryItem(ColonyBuiltIn.ItemTypes.MATCHLOCKGUN.Name, 1)
                        }
                    },
                    {
                        10f,
                        new List<InventoryItem>
                        {
                            new InventoryItem(ColonyBuiltIn.ItemTypes.STONEBRICKS.Name, 1),
                            new InventoryItem(ColonyBuiltIn.ItemTypes.PLANKS.Name, 1),
                            new InventoryItem(ColonyBuiltIn.ItemTypes.STEELPARTS.Name, 4),
                            new InventoryItem(ColonyBuiltIn.ItemTypes.COPPERNAILS.Name, 4),
                            new InventoryItem(ColonyBuiltIn.ItemTypes.MATCHLOCKGUN.Name, 2)
                        }
                    }
                },
                ProjectileAnimation = AnimationManager.AnimatedObjects[AnimationManager.LEADBULLET]
            };

            turretSettings.Damage[DamageType.Physical] = 500;

            TurretSettings[MATCHLOCK_NAMESPACE] = turretSettings;
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterSelectedWorld,
            GameLoader.NAMESPACE + ".Items.Machines.Turret.AddTextures")]
        [ModLoader.ModCallbackProvidesFor("pipliz.server.registertexturemappingtextures")]
        public static void AddTextures()
        {
            var textureMapping = new ItemTypesServer.TextureMapping(new JSONNode());
            textureMapping.AlbedoPath = GameLoader.BLOCKS_ALBEDO_PATH + "StoneTurret.png";
            textureMapping.NormalPath = GameLoader.BLOCKS_NORMAL_PATH + "Turret.png";
            textureMapping.HeightPath = GameLoader.BLOCKS_HEIGHT_PATH + "Turret.png";

            ItemTypesServer.SetTextureMapping(STONE_NAMESPACE + "sides", textureMapping);

            var bronzetextureMapping = new ItemTypesServer.TextureMapping(new JSONNode());
            bronzetextureMapping.AlbedoPath = GameLoader.BLOCKS_ALBEDO_PATH + "ArrowTurret.png";
            bronzetextureMapping.NormalPath = GameLoader.BLOCKS_NORMAL_PATH + "Turret.png";
            bronzetextureMapping.HeightPath = GameLoader.BLOCKS_HEIGHT_PATH + "Turret.png";

            ItemTypesServer.SetTextureMapping(BRONZEARROW_NAMESPACE + "sides", bronzetextureMapping);

            var crossbowtextureMapping = new ItemTypesServer.TextureMapping(new JSONNode());
            crossbowtextureMapping.AlbedoPath = GameLoader.BLOCKS_ALBEDO_PATH + "CrossbowTurret.png";
            crossbowtextureMapping.NormalPath = GameLoader.BLOCKS_NORMAL_PATH + "Turret.png";
            crossbowtextureMapping.HeightPath = GameLoader.BLOCKS_HEIGHT_PATH + "Turret.png";

            ItemTypesServer.SetTextureMapping(CROSSBOW_NAMESPACE + "sides", crossbowtextureMapping);

            var matchlocktextureMapping = new ItemTypesServer.TextureMapping(new JSONNode());
            matchlocktextureMapping.AlbedoPath = GameLoader.BLOCKS_ALBEDO_PATH + "MatchlockTurret.png";
            matchlocktextureMapping.NormalPath = GameLoader.BLOCKS_NORMAL_PATH + "Turret.png";
            matchlocktextureMapping.HeightPath = GameLoader.BLOCKS_HEIGHT_PATH + "Turret.png";

            ItemTypesServer.SetTextureMapping(MATCHLOCK_NAMESPACE + "sides", matchlocktextureMapping);
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AddItemTypes,
            GameLoader.NAMESPACE + ".Items.Machines.Turret.AddTurret")]
        [ModLoader.ModCallbackDependsOn("pipliz.server.applymoditempatches")]
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
                            .SetAs("icon", GameLoader.ICON_PATH + "StoneTurret.png")
                            .SetAs("isPlaceable", true)
                            .SetAs("onPlaceAudio", "stonePlace")
                            .SetAs("onRemoveAudio", "stoneDelete")
                            .SetAs("sideall", ColonyBuiltIn.ItemTypes.STONEBRICKS.Name)
                            .SetAs("onRemoveAmount", 1)
                            .SetAs("isSolid", true)
                            .SetAs("sidex+", STONE_NAMESPACE + "sides")
                            .SetAs("sidex-", STONE_NAMESPACE + "sides")
                            .SetAs("sidez+", STONE_NAMESPACE + "sides")
                            .SetAs("sidez-", STONE_NAMESPACE + "sides")
                            .SetAs("npcLimit", 0)
                            .SetAs("maxStackSize", 300);

            var categories = new JSONNode(NodeType.Array);
            categories.AddToArray(new JSONNode("machine"));
            categories.AddToArray(new JSONNode("turret"));
            turretNode.SetAs("categories", categories);

            var item = new ItemTypesServer.ItemTypeRaw(turretName, turretNode);
            TurretTypes[STONE_NAMESPACE] = item;
            items.Add(turretName, item);
        }

        private static void AddBronzeArrowTurret(Dictionary<string, ItemTypesServer.ItemTypeRaw> items)
        {
            var turretName = BRONZEARROW_NAMESPACE;

            var turretNode = new JSONNode()
                            .SetAs("icon", GameLoader.ICON_PATH + "ArrowTurret.png")
                            .SetAs("isPlaceable", true)
                            .SetAs("onPlaceAudio", "stonePlace")
                            .SetAs("onRemoveAudio", "stoneDelete")
                            .SetAs("sideall", ColonyBuiltIn.ItemTypes.STONEBRICKS.Name)
                            .SetAs("onRemoveAmount", 1)
                            .SetAs("isSolid", true)
                            .SetAs("sidex+", BRONZEARROW_NAMESPACE + "sides")
                            .SetAs("sidex-", BRONZEARROW_NAMESPACE + "sides")
                            .SetAs("sidez+", BRONZEARROW_NAMESPACE + "sides")
                            .SetAs("sidez-", BRONZEARROW_NAMESPACE + "sides")
                            .SetAs("npcLimit", 0)
                            .SetAs("maxStackSize", 300);

            var categories = new JSONNode(NodeType.Array);
            categories.AddToArray(new JSONNode("machine"));
            categories.AddToArray(new JSONNode("turret"));
            turretNode.SetAs("categories", categories);

            var item = new ItemTypesServer.ItemTypeRaw(turretName, turretNode);
            TurretTypes[BRONZEARROW_NAMESPACE] = item;
            items.Add(turretName, item);
        }

        private static void AddCrossbowTurret(Dictionary<string, ItemTypesServer.ItemTypeRaw> items)
        {
            var turretName = CROSSBOW_NAMESPACE;

            var turretNode = new JSONNode()
                            .SetAs("icon", GameLoader.ICON_PATH + "CrossbowTurret.png")
                            .SetAs("isPlaceable", true)
                            .SetAs("onPlaceAudio", "stonePlace")
                            .SetAs("onRemoveAudio", "stoneDelete")
                            .SetAs("sideall", ColonyBuiltIn.ItemTypes.STONEBRICKS.Name)
                            .SetAs("onRemoveAmount", 1)
                            .SetAs("isSolid", true)
                            .SetAs("sidex+", CROSSBOW_NAMESPACE + "sides")
                            .SetAs("sidex-", CROSSBOW_NAMESPACE + "sides")
                            .SetAs("sidez+", CROSSBOW_NAMESPACE + "sides")
                            .SetAs("sidez-", CROSSBOW_NAMESPACE + "sides")
                            .SetAs("npcLimit", 0)
                            .SetAs("maxStackSize", 300);

            var categories = new JSONNode(NodeType.Array);
            categories.AddToArray(new JSONNode("machine"));
            categories.AddToArray(new JSONNode("turret"));
            turretNode.SetAs("categories", categories);

            var item = new ItemTypesServer.ItemTypeRaw(turretName, turretNode);
            TurretTypes[CROSSBOW_NAMESPACE] = item;
            items.Add(turretName, item);
        }

        private static void AddMatchlockTurret(Dictionary<string, ItemTypesServer.ItemTypeRaw> items)
        {
            var turretName = MATCHLOCK_NAMESPACE;

            var turretNode = new JSONNode()
                            .SetAs("icon", GameLoader.ICON_PATH + "MatchlockTurret.png")
                            .SetAs("isPlaceable", true)
                            .SetAs("onPlaceAudio", "stonePlace")
                            .SetAs("onRemoveAudio", "stoneDelete")
                            .SetAs("sideall", ColonyBuiltIn.ItemTypes.STONEBRICKS.Name)
                            .SetAs("onRemoveAmount", 1)
                            .SetAs("isSolid", true)
                            .SetAs("sidex+", MATCHLOCK_NAMESPACE + "sides")
                            .SetAs("sidex-", MATCHLOCK_NAMESPACE + "sides")
                            .SetAs("sidez+", MATCHLOCK_NAMESPACE + "sides")
                            .SetAs("sidez-", MATCHLOCK_NAMESPACE + "sides")
                            .SetAs("npcLimit", 0)
                            .SetAs("maxStackSize", 300);

            var categories = new JSONNode(NodeType.Array);
            categories.AddToArray(new JSONNode("machine"));
            categories.AddToArray(new JSONNode("turret"));
            turretNode.SetAs("categories", categories);

            var item = new ItemTypesServer.ItemTypeRaw(turretName, turretNode);
            TurretTypes[MATCHLOCK_NAMESPACE] = item;
            items.Add(turretName, item);
        }

        public class TurretSetting : IPandaDamage
        {
            private float _ammoReloadVal = -1;

            public Dictionary<float, List<InventoryItem>> RequiredForFix = new Dictionary<float, List<InventoryItem>>();

            public float RepairTime { get; set; }

            public float RefuelTime { get; set; }

            public float ReloadTime { get; set; }

            public float WorkTime { get; set; }

            public List<InventoryItem> Ammo { get; set; }

            public float AmmoValue { get; set; }

            public float AmmoReloadValue
            {
                get
                {
                    if (_ammoReloadVal != -1)
                        return _ammoReloadVal;

                    return AmmoValue;
                }
                set => _ammoReloadVal = value;
            }

            public float DurabilityPerDoWork { get; set; }

            public float FuelPerDoWork { get; set; }

            public string Name { get; set; }

            public float TotalDamage
            {
                get { return Damage.Sum(kvp => kvp.Value); }
            }

            public int Range { get; set; }

            public string OnShootAudio { get; set; }

            public string OnHitAudio { get; set; }

            public ItemTypesServer.ItemTypeRaw TurretItem { get; set; }

            public AnimationManager.AnimatedObject ProjectileAnimation { get; set; }

            public Dictionary<DamageType, float> Damage { get; set; } = new Dictionary<DamageType, float>();
        }
    }
}