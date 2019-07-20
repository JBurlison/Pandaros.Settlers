using Pandaros.API;
using Pandaros.API.Jobs.Roaming;
using Pandaros.API.Research;
using Pandaros.Settlers.Items.Machines;
using Pandaros.Settlers.Jobs;
using Science;
using System.Collections.Generic;

namespace Pandaros.Settlers.Research
{
    public class MachineResearch
    {
        /// <summary>
        ///     This is reqquired to make sure jobs get registered before research
        /// </summary>
        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterItemTypesDefined, "BLOCKNPCS_WORKAROUND")]
        [ModLoader.ModCallbackDependsOn("pipliz.blocknpcs.registerjobs")]
        [ModLoader.ModCallbackProvidesFor("pipliz.server.loadresearchables")]
        static void Dummy()
        {

        }

        public class AddMachines : PandaResearch
        {
            public override string name => GameLoader.NAMESPACE + ".Machines";

            public override string IconDirectory => GameLoader.ICON_PATH;

            public override bool AddLevelToName => false;

            public override int NumberOfLevels => 1;

            public override int BaseIterationCount => 20;

            public override Dictionary<int, List<InventoryItem>> RequiredItems => new Dictionary<int, List<InventoryItem>>()
            {
                {
                    0,
                    new List<InventoryItem>()
                    {
                        new InventoryItem(ColonyBuiltIn.ItemTypes.IRONWROUGHT.Id),
                        new InventoryItem(ColonyBuiltIn.ItemTypes.PLANKS.Id, 5),
                        new InventoryItem(ColonyBuiltIn.ItemTypes.COPPERTOOLS.Id),
                        new InventoryItem(ColonyBuiltIn.ItemTypes.LINEN.Id)
                    }
                }
            };

            public override Dictionary<int, List<string>> Dependancies => new Dictionary<int, List<string>>()
            {
                {
                    0,
                    new List<string>()
                    {
                        ColonyBuiltIn.Research.IRONSMELTING
                    }
                }
            };

            public override Dictionary<int, List<RecipeUnlock>> Unlocks => new Dictionary<int, List<RecipeUnlock>>()
            {
                {
                    1,
                    new List<RecipeUnlock>()
                    {
                        new RecipeUnlock(SettlersBuiltIn.ItemTypes.MINER, ERecipeUnlockType.Recipe),
                        new RecipeUnlock(SettlersBuiltIn.ItemTypes.GATELEVER, ERecipeUnlockType.Recipe),
                        new RecipeUnlock(SettlersBuiltIn.ItemTypes.GATE, ERecipeUnlockType.Recipe),
                        new RecipeUnlock(SettlersBuiltIn.ItemTypes.BRONZEARROWTURRET, ERecipeUnlockType.Recipe),
                        new RecipeUnlock(SettlersBuiltIn.ItemTypes.CROSSBOWTURRET, ERecipeUnlockType.Recipe),
                        new RecipeUnlock(SettlersBuiltIn.ItemTypes.STONETURRET, ERecipeUnlockType.Recipe),
                        new RecipeUnlock(SettlersBuiltIn.ItemTypes.MATCHLOCKTURRET, ERecipeUnlockType.Recipe),
                        new RecipeUnlock(AdvancedCrafterRegister.JOB_ITEM_KEY, ERecipeUnlockType.Recipe)
                    }
                }
            };
        }

        public class AddInventoryUpgrade : PandaResearch
        {
            public override string name => GameLoader.NAMESPACE + ".InventoryUpgrade";

            public override string IconDirectory => GameLoader.ICON_PATH;

            public override float BaseValue => 0.5f;

            public override Dictionary<int, List<InventoryItem>> RequiredItems => new Dictionary<int, List<InventoryItem>>()
            {
                {
                    0,
                    new List<InventoryItem>()
                    {
                        new InventoryItem(ColonyBuiltIn.ItemTypes.IRONBLOCK.Id),
                        new InventoryItem(ColonyBuiltIn.ItemTypes.PLANKS.Id, 5),
                        new InventoryItem(ColonyBuiltIn.ItemTypes.STEELINGOT.Id, 2),
                        new InventoryItem(ColonyBuiltIn.ItemTypes.BRONZECOIN.Id, 5),
                        new InventoryItem(ColonyBuiltIn.ItemTypes.GOLDCOIN.Id, 5)
                    }
                }
            };

            public override Dictionary<int, List<string>> Dependancies => new Dictionary<int, List<string>>()
            {
                {
                    0,
                    new List<string>()
                    {
                        SettlersBuiltIn.Research.MACHINES1
                    }
                }
            };

            public override void ResearchComplete(object sender, ResearchCompleteEventArgs e)
            {
                RoamingJobState.SetActionsMaxEnergy(MachineConstants.INVENTORY, e.Manager.Colony, MachineConstants.MECHANICAL, RoamingJobState.DEFAULT_MAX + (RoamingJobState.DEFAULT_MAX * e.Research.Value));
            }
        }

        public class AddImprovedDuarability : PandaResearch
        {
            public override string name => GameLoader.NAMESPACE + ".ImprovedDurability";

            public override string IconDirectory => GameLoader.ICON_PATH;

            public override float BaseValue => 0.1f;

            public override Dictionary<int, List<InventoryItem>> RequiredItems => new Dictionary<int, List<InventoryItem>>()
            {
                {
                    0,
                    new List<InventoryItem>()
                    {
                        new InventoryItem(ColonyBuiltIn.ItemTypes.IRONBLOCK.Id),
                        new InventoryItem(ColonyBuiltIn.ItemTypes.PLANKS.Id, 5),
                        new InventoryItem(ColonyBuiltIn.ItemTypes.STEELINGOT.Id, 2),
                        new InventoryItem(ColonyBuiltIn.ItemTypes.GOLDCOIN.Id, 10)
                    }
                }
            };

            public override Dictionary<int, List<string>> Dependancies => new Dictionary<int, List<string>>()
            {
                {
                    0,
                    new List<string>()
                    {
                        SettlersBuiltIn.Research.MACHINES1
                    }
                }
            };

            public override void ResearchComplete(object sender, ResearchCompleteEventArgs e)
            {
                RoamingJobState.SetActionsMaxEnergy(MachineConstants.REPAIR, e.Manager.Colony, MachineConstants.MECHANICAL, RoamingJobState.DEFAULT_MAX + (RoamingJobState.DEFAULT_MAX * e.Research.Value));
            }
        }


        public class AddImprovedFuelCapacity : PandaResearch
        {
            public override string name => GameLoader.NAMESPACE + ".ImprovedFuelCapacity";

            public override string IconDirectory => GameLoader.ICON_PATH;

            public override float BaseValue => 0.1f;

            public override Dictionary<int, List<InventoryItem>> RequiredItems => new Dictionary<int, List<InventoryItem>>()
            {
                {
                    0,
                    new List<InventoryItem>()
                    {
                        new InventoryItem(ColonyBuiltIn.ItemTypes.IRONBLOCK.Id),
                        new InventoryItem(ColonyBuiltIn.ItemTypes.PLANKS.Id, 5),
                        new InventoryItem(ColonyBuiltIn.ItemTypes.STEELINGOT.Id, 2),
                        new InventoryItem(ColonyBuiltIn.ItemTypes.GOLDCOIN.Id, 10)
                    }
                }
            };

            public override Dictionary<int, List<string>> Dependancies => new Dictionary<int, List<string>>()
            {
                {
                    0,
                    new List<string>()
                    {
                        SettlersBuiltIn.Research.MACHINES1
                    }
                }
            };

            public override void ResearchComplete(object sender, ResearchCompleteEventArgs e)
            {
                RoamingJobState.SetActionsMaxEnergy(MachineConstants.REFUEL, e.Manager.Colony, MachineConstants.MECHANICAL, RoamingJobState.DEFAULT_MAX + (RoamingJobState.DEFAULT_MAX * e.Research.Value));
            }
        }

        public class AddIncreasedCapacity : PandaResearch
        {
            public override string name => GameLoader.NAMESPACE + ".IncreasedCapacity";

            public override string IconDirectory => GameLoader.ICON_PATH;

            public override float BaseValue => 0.1f;

            public override Dictionary<int, List<InventoryItem>> RequiredItems => new Dictionary<int, List<InventoryItem>>()
            {
                {
                    0,
                    new List<InventoryItem>()
                    {
                        new InventoryItem(ColonyBuiltIn.ItemTypes.IRONBLOCK.Id),
                        new InventoryItem(ColonyBuiltIn.ItemTypes.PLANKS.Id, 5),
                        new InventoryItem(ColonyBuiltIn.ItemTypes.STEELINGOT.Id, 2),
                        new InventoryItem(ColonyBuiltIn.ItemTypes.GOLDCOIN.Id, 10)
                    }
                }
            };

            public override Dictionary<int, List<string>> Dependancies => new Dictionary<int, List<string>>()
            {
                {
                    0,
                    new List<string>()
                    {
                        SettlersBuiltIn.Research.MACHINES1
                    }
                }
            };

            public override void ResearchComplete(object sender, ResearchCompleteEventArgs e)
            {
                RoamingJobState.SetActionsMaxEnergy(MachineConstants.RELOAD, e.Manager.Colony, MachineConstants.MECHANICAL, RoamingJobState.DEFAULT_MAX + (RoamingJobState.DEFAULT_MAX * e.Research.Value));
            }
        }
    }
}
