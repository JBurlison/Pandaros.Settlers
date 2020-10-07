using Pandaros.API;
using Pandaros.API.Items;
using Pandaros.API.Jobs.Roaming;
using Pandaros.API.Models;
using Pandaros.Settlers.Jobs;
using Pipliz;
using Recipes;
using Shared;
using System.Collections.Generic;
using System.Linq;

namespace Pandaros.Settlers.Items.Machines
{
    public class MinerBlock : CSType
    {
        public override string name { get; set; } = GameLoader.NAMESPACE + ".Miner";
        public override string icon { get; set; } = GameLoader.ICON_PATH + "MiningMachine.png";
        public override bool? isPlaceable { get; set; } = true;
        public override string onPlaceAudio { get; set; } = "stonePlace";
        public override string onRemoveAudio { get; set; } = "stoneDelete";
        public override bool? isSolid { get; set; } = true;
        public override string sideall { get; set; } = "SELF";
        public override string mesh { get; set; } = GameLoader.MESH_PATH + "MiningMachine.obj";
        public override List<string> categories { get; set; } = new List<string>()
        {
            "machine"
        };
    }

    public class MinerTexture : CSTextureMapping
    {
        public override string name => GameLoader.NAMESPACE + ".Miner";
        public override string albedo => GameLoader.BLOCKS_ALBEDO_PATH + "MiningMachine.png";
    }

    public class MinerRecipe : ICSRecipe
    {
        public List<RecipeItem> requires => new List<RecipeItem>()
        {
            new RecipeItem(ColonyBuiltIn.ItemTypes.IRONRIVET.Name, 6),
            new RecipeItem(ColonyBuiltIn.ItemTypes.IRONWROUGHT.Name, 2),
            new RecipeItem(ColonyBuiltIn.ItemTypes.COPPERPARTS.Name, 6),
            new RecipeItem(ColonyBuiltIn.ItemTypes.COPPERNAILS.Name, 6),
            new RecipeItem(ColonyBuiltIn.ItemTypes.COPPERTOOLS.Name, 1),
            new RecipeItem(ColonyBuiltIn.ItemTypes.PLANKS.Name, 4),
            new RecipeItem(ColonyBuiltIn.ItemTypes.BRONZEPICKAXE.Name, 2)
        };

        public List<RecipeResult> results => new List<RecipeResult>()
        {
            new RecipeResult(SettlersBuiltIn.ItemTypes.MINER.Name)
        };

        public CraftPriority defaultPriority => CraftPriority.Medium;

        public bool isOptional => true;

        public int defaultLimit => 5;

        public string Job => AdvancedCrafterRegister.JOB_NAME;

        public string name => SettlersBuiltIn.ItemTypes.MINER.Name;

        public List<string> JobBlock => new List<string>();
    }

    public class MinerRegister : IRoamingJobObjective
    {
        public float WorkTime => 4;
        public float WatchArea => 21;
        public ItemId ItemIndex => ItemId.GetItemId(GameLoader.NAMESPACE + ".Miner");
        public Dictionary<string, IRoamingJobObjectiveAction> ActionCallbacks { get; } = new Dictionary<string, IRoamingJobObjectiveAction>()
        {
            { MachineConstants.REFUEL, new RefuelMachineAction() },
            { MachineConstants.REPAIR, new RepairMiner() },
            { MachineConstants.RELOAD, new ReloadMiner() },
            { MachineConstants.INVENTORY, new UnloadMiner() }
        };

        public string ObjectiveCategory => MachineConstants.MECHANICAL;

        public void DoWork(Colony colony, RoamingJobState machineState)
        {
            if (machineState.NextTimeForWork < Time.SecondsSinceStartDouble &&
                machineState.GetActionEnergy(MachineConstants.REPAIR) > 0 &&
                machineState.GetActionEnergy(MachineConstants.REFUEL) > 0)
            {
                machineState.SubtractFromActionEnergy(MachineConstants.REPAIR, 0.02f);
                machineState.SubtractFromActionEnergy(MachineConstants.REFUEL, 0.05f);

                if (World.TryGetTypeAt(machineState.Position.Add(0, -1, 0), out ItemTypes.ItemType itemBelow) &&
                    itemBelow.CustomDataNode != null &&
                    itemBelow.CustomDataNode.TryGetAs("minerIsMineable", out bool minable) &&
                    minable)
                {
                    var itemList = itemBelow.OnRemoveItems;

                    if (itemList != null && itemList.Count > 0)
                    {
                        var mineTime = itemBelow.CustomDataNode.GetAsOrDefault("minerMiningTime", machineState.RoamingJobSettings.WorkTime);
                        machineState.NextTimeForWork = mineTime + Time.SecondsSinceStartDouble;
                        var items = machineState.TempValues.GetOrDefault<List<InventoryItem>>("MinedItems", new List<InventoryItem>());
                        int remainingItems = Math.RoundToInt(machineState.GetActionEnergy(MachineConstants.INVENTORY) / .05f) - items.Count;

                        if (remainingItems != 0)
                            for (var i = 0; i < itemList.Count; i++)
                                if (Random.NextDouble() <= itemList[i].chance)
                                {
                                    if (remainingItems != 0)
                                    {
                                        items.Add(itemList[i].item);
                                        remainingItems--;
                                        machineState.SubtractFromActionEnergy(MachineConstants.INVENTORY, .05f);
                                    }
                                }

                        machineState.TempValues.Set("MinedItems", items);

                        if (remainingItems != 0)
                        {
                            AudioManager.SendAudio(machineState.Position.Vector, GameLoader.NAMESPACE + ".MiningMachineAudio");
                            Indicator.SendIconIndicatorNear(machineState.Position.Add(0, 1, 0).Vector, new IndicatorState(mineTime, itemList.FirstOrDefault().item.Type));
                        }
                        else
                        {
                            machineState.SetActionEnergy(MachineConstants.INVENTORY, 0);
                            Indicator.SendIconIndicatorNear(machineState.Position.Add(0, 1, 0).Vector, new IndicatorState(mineTime, GameLoader.NAMESPACE + ".Inventory", true, false));
                        }
                    }
                    else
                    {
                        machineState.NextTimeForWork = machineState.RoamingJobSettings.WorkTime + Time.SecondsSinceStartDouble;
                        Indicator.SendIconIndicatorNear(machineState.Position.Add(0, 1, 0).Vector, new IndicatorState(machineState.RoamingJobSettings.WorkTime, ColonyBuiltIn.ItemTypes.ERRORIDLE.Name));
                    }
                }
                else
                {
                    machineState.NextTimeForWork = machineState.RoamingJobSettings.WorkTime + Time.SecondsSinceStartDouble;
                    Indicator.SendIconIndicatorNear(machineState.Position.Add(0, 1, 0).Vector, new IndicatorState(machineState.RoamingJobSettings.WorkTime, ColonyBuiltIn.ItemTypes.ERRORIDLE.Name));
                }

            }
        }
    }

    public class RepairMiner : IRoamingJobObjectiveAction
    {
        public string name => MachineConstants.REPAIR;
        public float ActionEnergyMinForFix => .5f;
        public float TimeToPreformAction => 10;

        public string AudioKey => GameLoader.NAMESPACE + ".HammerAudio";

        public ItemId ObjectiveLoadEmptyIcon => ItemId.GetItemId(GameLoader.NAMESPACE + ".Repairing");

        public ItemId PreformAction(Colony colony, RoamingJobState state)
        {
            var retval = ItemId.GetItemId(GameLoader.NAMESPACE + ".Repairing");

            if (state.GetActionEnergy(MachineConstants.REPAIR) < .75f)
            {
                var repaired = false;
                var requiredForFix = new List<InventoryItem>();
                var stockpile = colony.Stockpile;

                requiredForFix.Add(new InventoryItem(ColonyBuiltIn.ItemTypes.PLANKS.Name, 1));
                requiredForFix.Add(new InventoryItem(ColonyBuiltIn.ItemTypes.COPPERNAILS.Name, 1));

                if (state.GetActionEnergy(MachineConstants.REPAIR) < .10f)
                {
                    requiredForFix.Add(new InventoryItem(ColonyBuiltIn.ItemTypes.IRONWROUGHT.Name, 1));
                    requiredForFix.Add(new InventoryItem(ColonyBuiltIn.ItemTypes.COPPERPARTS.Name, 4));
                    requiredForFix.Add(new InventoryItem(ColonyBuiltIn.ItemTypes.IRONRIVET.Name, 1));
                    requiredForFix.Add(new InventoryItem(ColonyBuiltIn.ItemTypes.COPPERTOOLS.Name, 1));
                }
                else if (state.GetActionEnergy(MachineConstants.REPAIR) < .30f)
                {
                    requiredForFix.Add(new InventoryItem(ColonyBuiltIn.ItemTypes.IRONWROUGHT.Name, 1));
                    requiredForFix.Add(new InventoryItem(ColonyBuiltIn.ItemTypes.COPPERPARTS.Name, 2));
                    requiredForFix.Add(new InventoryItem(ColonyBuiltIn.ItemTypes.COPPERTOOLS.Name, 1));
                }
                else if (state.GetActionEnergy(MachineConstants.REPAIR) < .50f)
                {
                    requiredForFix.Add(new InventoryItem(ColonyBuiltIn.ItemTypes.COPPERPARTS.Name, 1));
                    requiredForFix.Add(new InventoryItem(ColonyBuiltIn.ItemTypes.COPPERTOOLS.Name, 1));
                }

                if (stockpile.Contains(requiredForFix))
                {
                    stockpile.TryRemove(requiredForFix);
                    repaired = true;
                }
                else
                {
                    foreach (var item in requiredForFix)
                        if (!stockpile.Contains(item))
                        {
                            retval = ItemId.GetItemId(item.Type);
                            break;
                        }
                }

                if (repaired)
                    state.ResetActionToMaxLoad(MachineConstants.REPAIR);
            }
            
            return retval;
        }
    }

    public class UnloadMiner : IRoamingJobObjectiveAction
    {
        public string name => MachineConstants.INVENTORY;

        public float TimeToPreformAction => 5;

        public string AudioKey => GameLoader.NAMESPACE + ".ReloadingAudio";

        public ItemId ObjectiveLoadEmptyIcon => ItemId.GetItemId(GameLoader.NAMESPACE + ".Inventory");

        public float ActionEnergyMinForFix => .5f;

        public ItemId PreformAction(Colony player, RoamingJobState state)
        {
            var items = state.TempValues.GetOrDefault<List<InventoryItem>>("MinedItems", new List<InventoryItem>());

            foreach (var item in items)
                player.Stockpile.Add(item);

            items.Clear();
            state.ResetActionToMaxLoad(MachineConstants.INVENTORY);
            state.TempValues.Set("MinedItems", items);

            return ItemId.GetItemId(GameLoader.NAMESPACE + ".Inventory");
        }
    }

    public class ReloadMiner : IRoamingJobObjectiveAction
    {
        public string name => MachineConstants.RELOAD;
        public float ActionEnergyMinForFix => .5f;
        public float TimeToPreformAction => 5;

        public string AudioKey => GameLoader.NAMESPACE + ".ReloadingAudio";

        public ItemId ObjectiveLoadEmptyIcon => ItemId.GetItemId(GameLoader.NAMESPACE + ".Reload");

        public ItemId PreformAction(Colony player, RoamingJobState state)
        {
            return ItemId.GetItemId(GameLoader.NAMESPACE + ".Waiting");
        }
    }
}