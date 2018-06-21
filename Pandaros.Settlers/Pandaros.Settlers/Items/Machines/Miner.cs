using System.Collections.Generic;
using System.Linq;
using BlockTypes.Builtin;
using Pandaros.Settlers.Entities;
using Pandaros.Settlers.Jobs;
using Pandaros.Settlers.Jobs.Roaming;
using Pandaros.Settlers.Managers;
using Pipliz;
using Pipliz.JSON;
using Server;
using Shared;

namespace Pandaros.Settlers.Items.Machines
{
    public class MinerRegister : IRoamingJobObjective
    {
        public string Name => nameof(Miner);
        public float WorkTime => 4;
        public ushort ItemIndex => Miner.Item.ItemIndex;
        public Dictionary<string, IRoamingJobObjectiveAction> ActionCallbacks { get; } = new Dictionary<string, IRoamingJobObjectiveAction>()
        {
            { MachineConstants.REFUEL, new RefuelMachineAction() },
            { MachineConstants.REPAIR, new RepairMiner() },
            { MachineConstants.RELOAD, new ReloadMiner() }
        };

        public string ObjectiveCategory => MachineConstants.MECHANICAL;

        public void DoWork(Players.Player player, RoamingJobState state)
        {
            Miner.DoWork(player, state);
        }
    }

    public class RepairMiner : IRoamingJobObjectiveAction
    {
        public string Name => MachineConstants.REPAIR;

        public float TimeToPreformAction => 10;

        public string AudoKey => GameLoader.NAMESPACE + ".HammerAudio";

        public ushort ObjectiveLoadEmptyIcon => GameLoader.Repairing_Icon;

        public ushort PreformAction(Players.Player player, RoamingJobState state)
        {
            return Miner.Repair(player, state);
        }
    }

    public class ReloadMiner : IRoamingJobObjectiveAction
    {
        public string Name => MachineConstants.RELOAD;

        public float TimeToPreformAction => 5;

        public string AudoKey => GameLoader.NAMESPACE + ".ReloadingAudio";

        public ushort ObjectiveLoadEmptyIcon => GameLoader.Reload_Icon;

        public ushort PreformAction(Players.Player player, RoamingJobState state)
        {
            return Miner.Reload(player, state);
        }
    }


    [ModLoader.ModManager]
    public static class Miner
    {
        private const double MinerCooldown = 4;

        public static ItemTypesServer.ItemTypeRaw Item { get; private set; }

        public static ushort Repair(Players.Player player, RoamingJobState machineState)
        {
            var retval = GameLoader.Repairing_Icon;

            if (!player.IsConnected && Configuration.OfflineColonies || player.IsConnected)
            {
                var ps = PlayerState.GetPlayerState(player);

                if (machineState.GetActionEnergy(MachineConstants.REPAIR) < .75f)
                {
                    var repaired       = false;
                    var requiredForFix = new List<InventoryItem>();
                    var stockpile      = Stockpile.GetStockPile(player);

                    requiredForFix.Add(new InventoryItem(BuiltinBlocks.Planks, 1));
                    requiredForFix.Add(new InventoryItem(BuiltinBlocks.CopperNails, 1));

                    if (machineState.GetActionEnergy(MachineConstants.REPAIR) < .10f)
                    {
                        requiredForFix.Add(new InventoryItem(BuiltinBlocks.IronWrought, 1));
                        requiredForFix.Add(new InventoryItem(BuiltinBlocks.CopperParts, 4));
                        requiredForFix.Add(new InventoryItem(BuiltinBlocks.IronRivet, 1));
                        requiredForFix.Add(new InventoryItem(BuiltinBlocks.CopperTools, 1));
                    }
                    else if (machineState.GetActionEnergy(MachineConstants.REPAIR) < .30f)
                    {
                        requiredForFix.Add(new InventoryItem(BuiltinBlocks.IronWrought, 1));
                        requiredForFix.Add(new InventoryItem(BuiltinBlocks.CopperParts, 2));
                        requiredForFix.Add(new InventoryItem(BuiltinBlocks.CopperTools, 1));
                    }
                    else if (machineState.GetActionEnergy(MachineConstants.REPAIR) < .50f)
                    {
                        requiredForFix.Add(new InventoryItem(BuiltinBlocks.CopperParts, 1));
                        requiredForFix.Add(new InventoryItem(BuiltinBlocks.CopperTools, 1));
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

        public static ushort Reload(Players.Player player, RoamingJobState machineState)
        {
            return GameLoader.Waiting_Icon;
        }

        public static void DoWork(Players.Player player, RoamingJobState machineState)
        {
            if (!player.IsConnected && Configuration.OfflineColonies || player.IsConnected)
                if (machineState.GetActionEnergy(MachineConstants.REPAIR) > 0 &&
                    machineState.GetActionEnergy(MachineConstants.REFUEL) > 0 &&
                    machineState.NextTimeForWork < Time.SecondsSinceStartDouble)
                {
                    machineState.SubtractFromActionEnergy(MachineConstants.REPAIR, 0.02f);
                    machineState.SubtractFromActionEnergy(MachineConstants.REFUEL, 0.05f);

                    if (World.TryGetTypeAt(machineState.Position.Add(0, -1, 0), out var itemBelow))
                    {
                        var itemList = ItemTypes.GetType(itemBelow).OnRemoveItems;

                        Indicator.SendIconIndicatorNear(machineState.Position.Add(0, 1, 0).Vector,
                                                        new IndicatorState((float) MinerCooldown,
                                                                           itemList.FirstOrDefault().item.Type));

                        for (var i = 0; i < itemList.Count; i++)
                            if (Random.NextDouble() <= itemList[i].chance)
                                Stockpile.GetStockPile(player).Add(itemList[i].item);

                        ServerManager.SendAudio(machineState.Position.Vector,
                                                GameLoader.NAMESPACE + ".MiningMachineAudio");
                    }

                    machineState.NextTimeForWork = machineState.RoamingJobSettings.WorkTime + Time.SecondsSinceStartDouble;
                }
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterItemTypesDefined, GameLoader.NAMESPACE + ".Items.Machines.Miner.RegisterMiner")]
        public static void RegisterMiner()
        {
            var rivets      = new InventoryItem(BuiltinBlocks.IronRivet, 6);
            var iron        = new InventoryItem(BuiltinBlocks.IronWrought, 2);
            var copperParts = new InventoryItem(BuiltinBlocks.CopperParts, 6);
            var copperNails = new InventoryItem(BuiltinBlocks.CopperNails, 6);
            var tools       = new InventoryItem(BuiltinBlocks.CopperTools, 1);
            var planks      = new InventoryItem(BuiltinBlocks.Planks, 4);
            var pickaxe     = new InventoryItem(BuiltinBlocks.BronzePickaxe, 2);

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
                                    new InventoryItem(Item.ItemIndex),
                                    5);

            RecipeStorage.AddOptionalLimitTypeRecipe(AdvancedCrafterRegister.JOB_NAME, recipe);
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterSelectedWorld, GameLoader.NAMESPACE + ".Items.Machines.Miner.AddTextures")]
        [ModLoader.ModCallbackProvidesFor("pipliz.server.registertexturemappingtextures")]
        public static void AddTextures()
        {
            var minerTextureMapping = new ItemTypesServer.TextureMapping(new JSONNode());
            minerTextureMapping.AlbedoPath = GameLoader.BLOCKS_ALBEDO_PATH + "MiningMachine.png";

            ItemTypesServer.SetTextureMapping(GameLoader.NAMESPACE + ".Miner", minerTextureMapping);
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterAddingBaseTypes,  GameLoader.NAMESPACE + ".Items.Machines.Miner.AddMiner")]
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

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnTryChangeBlock,  GameLoader.NAMESPACE + ".Items.Machines.Miner.OnTryChangeBlockUser")]
        public static void OnTryChangeBlockUser(ModLoader.OnTryChangeBlockData d)
        {
            if (d.CallbackState == ModLoader.OnTryChangeBlockData.ECallbackState.Cancelled)
                return;

            if (d.TypeNew == Item.ItemIndex && d.TypeOld == BuiltinBlocks.Air)
            {
                if (World.TryGetTypeAt(d.Position.Add(0, -1, 0), out var itemBelow))
                    if (CanMineBlock(itemBelow))
                    {
                        RoamingJobManager.RegisterRoamingJobState(d.RequestedByPlayer,
                                                            new RoamingJobState(d.Position, d.RequestedByPlayer,
                                                                             nameof(Miner)));

                        return;
                    }

                PandaChat.Send(d.RequestedByPlayer, "The mining machine must be placed on stone or ore.", ChatColor.orange);
                d.CallbackState = ModLoader.OnTryChangeBlockData.ECallbackState.Cancelled;
            }
        }

        public static bool CanMineBlock(ushort itemMined)
        {
            return ItemTypes.TryGetType(itemMined, out var item) &&
                   item.CustomDataNode.TryGetAs("minerIsMineable", out bool minable) &&
                   minable;
        }
    }
}