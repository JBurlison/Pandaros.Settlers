using BlockTypes.Builtin;
using NPC;
using Pandaros.Settlers.Entities;
using Pandaros.Settlers.Managers;
using Pipliz;
using Pipliz.JSON;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pandaros.Settlers.Items.Machines
{
    [ModLoader.ModManager]
    public static class Miner
    {
        const double MinerCooldown = 4;
        
        public static ItemTypesServer.ItemTypeRaw Item { get; private set; }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterAddingBaseTypes, GameLoader.NAMESPACE + ".Items.Machines.Miner.RegisterMachines")]
        public static void RegisterMachines(Dictionary<string, ItemTypesServer.ItemTypeRaw> items)
        {
            MachineManager.RegisterMachineType(nameof(Miner), new MachineManager.MachineSettings(Item.ItemIndex, Repair, MachineManager.Refuel, Reload, DoWork, 10, 4, 5, 4));
        }

        public static ushort Repair(Players.Player player, MachineState machineState)
        {
            var retval = GameLoader.Repairing_Icon;

            if ((!player.IsConnected && Configuration.OfflineColonies) || player.IsConnected)
            {
                var ps = PlayerState.GetPlayerState(player);

                if (machineState.Durability < .75f)
                {
                    bool repaired = false;
                    List<InventoryItem> requiredForFix = new List<InventoryItem>();
                    var stockpile = Stockpile.GetStockPile(player);

                    requiredForFix.Add(new InventoryItem(BuiltinBlocks.Planks, 1));
                    requiredForFix.Add(new InventoryItem(BuiltinBlocks.CopperNails, 1));

                    if (machineState.Durability < .10f)
                    {
                        requiredForFix.Add(new InventoryItem(BuiltinBlocks.IronWrought, 1));
                        requiredForFix.Add(new InventoryItem(BuiltinBlocks.CopperParts, 4));
                        requiredForFix.Add(new InventoryItem(BuiltinBlocks.IronRivet, 1));
                        requiredForFix.Add(new InventoryItem(BuiltinBlocks.CopperTools, 1));
                    }
                    else if (machineState.Durability < .30f)
                    {
                        requiredForFix.Add(new InventoryItem(BuiltinBlocks.IronWrought, 1));
                        requiredForFix.Add(new InventoryItem(BuiltinBlocks.CopperParts, 2));
                        requiredForFix.Add(new InventoryItem(BuiltinBlocks.CopperTools, 1));
                    }
                    else if (machineState.Durability < .50f)
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
                        foreach (var item in requiredForFix)
                            if (!stockpile.Contains(item))
                            {
                                retval = item.Type;
                                break;
                            }

                    if (!MachineState.MAX_DURABILITY.ContainsKey(player))
                        MachineState.MAX_DURABILITY[player] = MachineState.DEFAULT_MAX_DURABILITY;

                    if (repaired)
                        machineState.Durability = MachineState.MAX_DURABILITY[player];
                }
            }

            return retval;
        }
        
        public static ushort Reload(Players.Player player, MachineState machineState)
        {
            return GameLoader.Waiting_Icon;
        }

        public static void DoWork(Players.Player player, MachineState machineState)
        {
            if ((!player.IsConnected && Configuration.OfflineColonies) || player.IsConnected)
            {
                if (machineState.Durability > 0 &&
                machineState.Fuel > 0 &&
                machineState.NextTimeForWork < Time.SecondsSinceStartDouble)
                {
                    machineState.Durability -= 0.02f;
                    machineState.Fuel -= 0.05f;

                    if (machineState.Durability < 0)
                        machineState.Durability = 0;

                    if (machineState.Fuel <= 0)
                        machineState.Fuel = 0;

                    if (World.TryGetTypeAt(machineState.Position.Add(0, -1, 0), out ushort itemBelow))
                    {
                        List<ItemTypes.ItemTypeDrops> itemList = ItemTypes.GetType(itemBelow).OnRemoveItems;
                        Server.Indicator.SendIconIndicatorNear(machineState.Position.Add(0, 1, 0).Vector, new Shared.IndicatorState((float)MinerCooldown, itemList.FirstOrDefault().item.Type));

                        for (int i = 0; i < itemList.Count; i++)
                            if (Pipliz.Random.NextDouble() <= itemList[i].chance)
                                Stockpile.GetStockPile(player).Add(itemList[i].item);

                        ServerManager.SendAudio(machineState.Position.Vector, GameLoader.NAMESPACE + "MiningMachineAudio");
                    }

                    machineState.NextTimeForWork = machineState.MachineSettings.WorkTime + Time.SecondsSinceStartDouble;
                }
            }
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterSelectedWorld, GameLoader.NAMESPACE + ".Items.Machines.Miner.RegisterAudio"),
            ModLoader.ModCallbackProvidesFor("pipliz.server.loadaudiofiles"), ModLoader.ModCallbackDependsOn("pipliz.server.registeraudiofiles")]
        public static void RegisterAudio()
        {
            GameLoader.AddSoundFile(GameLoader.NAMESPACE + "MiningMachineAudio", new List<string>() { GameLoader.AUDIO_FOLDER_PANDA + "/MiningMachine.ogg" });
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterItemTypesDefined, GameLoader.NAMESPACE + ".Items.Machines.Miner.RegisterMiner")]
        public static void RegisterMiner()
        {
            var rivets = new InventoryItem(BuiltinBlocks.IronRivet, 6);
            var iron = new InventoryItem(BuiltinBlocks.IronWrought, 2);
            var copperParts = new InventoryItem(BuiltinBlocks.CopperParts, 6);
            var copperNails = new InventoryItem(BuiltinBlocks.CopperNails, 6);
            var tools = new InventoryItem(BuiltinBlocks.CopperTools, 1);
            var planks = new InventoryItem(BuiltinBlocks.Planks, 4);
            var pickaxe = new InventoryItem(BuiltinBlocks.BronzePickaxe, 2);

            var recipe = new Recipe(Item.name,
                                    new List<InventoryItem>() { planks, iron, rivets, copperParts, copperNails, tools, planks, pickaxe },
                                    new InventoryItem(Item.ItemIndex),
                                    5);

            //ItemTypesServer.LoadSortOrder(Item.name, GameLoader.GetNextItemSortIndex());
            RecipeStorage.AddOptionalLimitTypeRecipe(Jobs.AdvancedCrafterRegister.JOB_NAME, recipe);
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterSelectedWorld, GameLoader.NAMESPACE + ".Items.Machines.Miner.AddTextures"), ModLoader.ModCallbackProvidesFor("pipliz.server.registertexturemappingtextures")]
        public static void AddTextures()
        {
            var minerTextureMapping = new ItemTypesServer.TextureMapping(new JSONNode());
            minerTextureMapping.AlbedoPath = GameLoader.TEXTURE_FOLDER_PANDA + "/MiningMachine.png";

            ItemTypesServer.SetTextureMapping(GameLoader.NAMESPACE + ".Miner", minerTextureMapping);
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterAddingBaseTypes, GameLoader.NAMESPACE + ".Items.Machines.Miner.AddMiner"), ModLoader.ModCallbackDependsOn("pipliz.blocknpcs.addlittypes")]
        public static void AddMiner(Dictionary<string, ItemTypesServer.ItemTypeRaw> items)
        {
            var minerName = GameLoader.NAMESPACE + ".Miner";
            var minerFlagNode = new JSONNode();
            minerFlagNode["icon"] = new JSONNode(GameLoader.ICON_FOLDER_PANDA + "/MiningMachine.png");
            minerFlagNode["isPlaceable"] = new JSONNode(true);
            minerFlagNode.SetAs("onRemoveAmount", 1);
            minerFlagNode.SetAs("onPlaceAudio", "stonePlace");
            minerFlagNode.SetAs("onRemoveAudio", "stoneDelete");
            minerFlagNode.SetAs("isSolid", true);
            minerFlagNode.SetAs("sideall", "SELF");
            minerFlagNode.SetAs("mesh", GameLoader.MESH_FOLDER_PANDA + "/MiningMachine.obj");

            Item = new ItemTypesServer.ItemTypeRaw(minerName, minerFlagNode);
            items.Add(minerName, Item);
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnTryChangeBlock, GameLoader.NAMESPACE + ".Items.Machines.Miner.OnTryChangeBlockUser")]
        public static bool OnTryChangeBlockUser(ModLoader.OnTryChangeBlockData d)
        {
            if (d.TypeNew == Item.ItemIndex && d.TypeOld == BuiltinBlocks.Air)
            {
                if (World.TryGetTypeAt(d.Position.Add(0, -1, 0), out ushort itemBelow))
                {
                    if (CanMineBlock(itemBelow))
                    {
                        MachineManager.RegisterMachineState(d.RequestedByPlayer, new MachineState(d.Position, d.RequestedByPlayer, nameof(Miner)));
                        return true;
                    }
                }

                PandaChat.Send(d.RequestedByPlayer, "The mining machine must be placed on stone or ore.", ChatColor.orange);
                return false;
            }

            return true;
        }

        public static bool CanMineBlock(ushort itemMined)
        {
            return ItemTypes.TryGetType(itemMined, out var item) &&
                        item.CustomDataNode.TryGetAs("minerIsMineable", out bool minable) &&
                        minable;
        }
    }
}
