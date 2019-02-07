using Pandaros.Settlers.Jobs;
using Pandaros.Settlers.Jobs.Roaming;
using Pipliz;
using Pipliz.JSON;
using Recipes;
using Shared;
using System.Collections.Generic;
using System.Linq;

namespace Pandaros.Settlers.Items.Machines
{
    public class MinerRegister : IRoamingJobObjective
    {
        public string name => nameof(Miner);
        public float WorkTime => 4;
        public ushort ItemIndex => Miner.Item.ItemIndex;
        public Dictionary<string, IRoamingJobObjectiveAction> ActionCallbacks { get; } = new Dictionary<string, IRoamingJobObjectiveAction>()
        {
            { MachineConstants.REFUEL, new RefuelMachineAction() },
            { MachineConstants.REPAIR, new RepairMiner() },
            { MachineConstants.RELOAD, new ReloadMiner() }
        };

        public string ObjectiveCategory => MachineConstants.MECHANICAL;

        public void DoWork(Colony player, RoamingJobState state)
        {
            Miner.DoWork(player, state);
        }
    }

    public class RepairMiner : IRoamingJobObjectiveAction
    {
        public string name => MachineConstants.REPAIR;

        public float TimeToPreformAction => 10;

        public string AudoKey => GameLoader.NAMESPACE + ".HammerAudio";

        public ushort ObjectiveLoadEmptyIcon => GameLoader.Repairing_Icon;

        public ushort PreformAction(Colony player, RoamingJobState state)
        {
            return Miner.Repair(player, state);
        }
    }

    public class ReloadMiner : IRoamingJobObjectiveAction
    {
        public string name => MachineConstants.RELOAD;

        public float TimeToPreformAction => 5;

        public string AudoKey => GameLoader.NAMESPACE + ".ReloadingAudio";

        public ushort ObjectiveLoadEmptyIcon => GameLoader.Reload_Icon;

        public ushort PreformAction(Colony player, RoamingJobState state)
        {
            return Miner.Reload(player, state);
        }
    }


    [ModLoader.ModManager]
    public static class Miner
    {

        public static ItemTypesServer.ItemTypeRaw Item { get; private set; }

        public static ushort Repair(Colony colony, RoamingJobState machineState)
        {
            var retval = GameLoader.Repairing_Icon;

            if (!colony.OwnerIsOnline() && Configuration.OfflineColonies || colony.OwnerIsOnline())
            {
                if (machineState.GetActionEnergy(MachineConstants.REPAIR) < .75f)
                {
                    var repaired       = false;
                    var requiredForFix = new List<InventoryItem>();
                    var stockpile = colony.Stockpile;

                    requiredForFix.Add(new InventoryItem(ColonyBuiltIn.ItemTypes.PLANKS.Name, 1));
                    requiredForFix.Add(new InventoryItem(ColonyBuiltIn.ItemTypes.COPPERNAILS.Name, 1));

                    if (machineState.GetActionEnergy(MachineConstants.REPAIR) < .10f)
                    {
                        requiredForFix.Add(new InventoryItem(ColonyBuiltIn.ItemTypes.IRONWROUGHT.Name, 1));
                        requiredForFix.Add(new InventoryItem(ColonyBuiltIn.ItemTypes.COPPERPARTS.Name, 4));
                        requiredForFix.Add(new InventoryItem(ColonyBuiltIn.ItemTypes.IRONRIVET.Name, 1));
                        requiredForFix.Add(new InventoryItem(ColonyBuiltIn.ItemTypes.COPPERTOOLS.Name, 1));
                    }
                    else if (machineState.GetActionEnergy(MachineConstants.REPAIR) < .30f)
                    {
                        requiredForFix.Add(new InventoryItem(ColonyBuiltIn.ItemTypes.IRONWROUGHT.Name, 1));
                        requiredForFix.Add(new InventoryItem(ColonyBuiltIn.ItemTypes.COPPERPARTS.Name, 2));
                        requiredForFix.Add(new InventoryItem(ColonyBuiltIn.ItemTypes.COPPERTOOLS.Name, 1));
                    }
                    else if (machineState.GetActionEnergy(MachineConstants.REPAIR) < .50f)
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
                                retval = item.Type;
                                break;
                            }
                    }

                    if (repaired)
                        machineState.ResetActionToMaxLoad(MachineConstants.REPAIR);
                }
            }

            return retval;
        }

        public static ushort Reload(Colony player, RoamingJobState machineState)
        {
            return GameLoader.Waiting_Icon;
        }

        public static void DoWork(Colony colony, RoamingJobState machineState)
        {
            if ((!colony.OwnerIsOnline() && Configuration.OfflineColonies) || colony.OwnerIsOnline())
                if (machineState.GetActionEnergy(MachineConstants.REPAIR) > 0 &&
                    machineState.GetActionEnergy(MachineConstants.REFUEL) > 0 &&
                    machineState.NextTimeForWork < Time.SecondsSinceStartDouble)
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

                            for (var i = 0; i < itemList.Count; i++)
                                if (Random.NextDouble() <= itemList[i].chance)
                                    colony.Stockpile.Add(itemList[i].item);

                            ServerManager.SendAudio(machineState.Position.Vector, GameLoader.NAMESPACE + ".MiningMachineAudio");
                            Indicator.SendIconIndicatorNear(machineState.Position.Add(0, 1, 0).Vector, new IndicatorState(mineTime, itemList.FirstOrDefault().item.Type));
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

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterItemTypesDefined, GameLoader.NAMESPACE + ".Items.Machines.Miner.RegisterMiner")]
        public static void RegisterMiner()
        {
            var rivets      = new InventoryItem(ColonyBuiltIn.ItemTypes.IRONRIVET.Name, 6);
            var iron        = new InventoryItem(ColonyBuiltIn.ItemTypes.IRONWROUGHT.Name, 2);
            var copperParts = new InventoryItem(ColonyBuiltIn.ItemTypes.COPPERPARTS.Name, 6);
            var copperNails = new InventoryItem(ColonyBuiltIn.ItemTypes.COPPERNAILS.Name, 6);
            var tools       = new InventoryItem(ColonyBuiltIn.ItemTypes.COPPERTOOLS.Name, 1);
            var planks      = new InventoryItem(ColonyBuiltIn.ItemTypes.PLANKS.Name, 4);
            var pickaxe     = new InventoryItem(ColonyBuiltIn.ItemTypes.BRONZEPICKAXE.Name, 2);

            var recipe = new Recipe(Item.name,
                                    new List<InventoryItem>
                                    {
                                        planks,
                                        iron,
                                        rivets,
                                        copperParts,
                                        copperNails,
                                        tools,
                                        planks,
                                        pickaxe
                                    },
                                    new ItemTypes.ItemTypeDrops(Item.ItemIndex),
                                    5);

            ServerManager.RecipeStorage.AddOptionalLimitTypeRecipe(AdvancedCrafterRegister.JOB_NAME, recipe);
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterSelectedWorld, GameLoader.NAMESPACE + ".Items.Machines.Miner.AddTextures")]
        [ModLoader.ModCallbackProvidesFor("pipliz.server.registertexturemappingtextures")]
        public static void AddTextures()
        {
            var minerTextureMapping = new ItemTypesServer.TextureMapping(new JSONNode());
            minerTextureMapping.AlbedoPath = GameLoader.BLOCKS_ALBEDO_PATH + "MiningMachine.png";

            ItemTypesServer.SetTextureMapping(GameLoader.NAMESPACE + ".Miner", minerTextureMapping);
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AddItemTypes,  GameLoader.NAMESPACE + ".Items.Machines.Miner.AddMiner")]
        [ModLoader.ModCallbackDependsOn("pipliz.blocknpcs.addlittypes")]
        public static void AddMiner(Dictionary<string, ItemTypesServer.ItemTypeRaw> items)
        {
            var minerName     = GameLoader.NAMESPACE + ".Miner";
            var minerFlagNode = new JSONNode();
            minerFlagNode["icon"]        = new JSONNode(GameLoader.ICON_PATH + "MiningMachine.png");
            minerFlagNode["isPlaceable"] = new JSONNode(true);
            minerFlagNode.SetAs("onRemoveAmount", 1);
            minerFlagNode.SetAs("onPlaceAudio", "stonePlace");
            minerFlagNode.SetAs("onRemoveAudio", "stoneDelete");
            minerFlagNode.SetAs("isSolid", true);
            minerFlagNode.SetAs("sideall", "SELF");
            minerFlagNode.SetAs("mesh", GameLoader.MESH_PATH + "MiningMachine.obj");

            var categories = new JSONNode(NodeType.Array);
            categories.AddToArray(new JSONNode("machine"));
            minerFlagNode.SetAs("categories", categories);

            Item = new ItemTypesServer.ItemTypeRaw(minerName, minerFlagNode);
            items.Add(minerName, Item);
        }

        public static bool CanMineBlock(ushort itemMined)
        {
            return ItemTypes.TryGetType(itemMined, out ItemTypes.ItemType item) &&
                   item.CustomDataNode.TryGetAs("minerIsMineable", out bool minable) &&
                   minable;
        }
    }
}