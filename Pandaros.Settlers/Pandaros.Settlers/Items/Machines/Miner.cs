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

            if (machineState.Durability < .75f)
            {
                bool repaired = false;
                List<InventoryItem> requiredForFix = new List<InventoryItem>();
                var stockpile = Stockpile.GetStockPile(player);

                requiredForFix.Add(new InventoryItem(BuiltinBlocks.Planks, 1));
                requiredForFix.Add(new InventoryItem(BuiltinBlocks.IronRivet, 1));
                requiredForFix.Add(new InventoryItem(BuiltinBlocks.CopperNails, 2));
                requiredForFix.Add(new InventoryItem(BuiltinBlocks.CopperTools, 1));

                if (machineState.Durability < .10f)
                {
                    requiredForFix.Add(new InventoryItem(BuiltinBlocks.IronWrought, 1));
                    requiredForFix.Add(new InventoryItem(BuiltinBlocks.CopperParts, 4));
                }
                else if (machineState.Durability < .30f)
                {
                    requiredForFix.Add(new InventoryItem(BuiltinBlocks.IronWrought, 1));
                    requiredForFix.Add(new InventoryItem(BuiltinBlocks.CopperParts, 2));
                }
                else if (machineState.Durability < .50f)
                {
                    requiredForFix.Add(new InventoryItem(BuiltinBlocks.CopperParts, 1));
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

            return retval;
        }
        
        public static ushort Reload(Players.Player player, MachineState machineState)
        {
            return GameLoader.Waiting_Icon;
        }

        public static void DoWork(Players.Player player, MachineState machineState)
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

                if (machineState.Durability <= 0)
                    PandaChat.SendThrottle(player, $"A mining machine at {machineState.Position} has broken down. Consider adding more Machinist's to keep them running!", ChatColor.maroon);

                if (machineState.Fuel <= 0)
                    PandaChat.SendThrottle(player, $"A mining machine at {machineState.Position} has run out of fuel. Consider adding more Machinist's to keep them running!", ChatColor.maroon);

                if (World.TryGetTypeAt(machineState.Position.Add(0, -1, 0), out ushort itemBelow))
                {
                    List<ItemTypes.ItemTypeDrops> itemList = ItemTypes.GetType(itemBelow).OnRemoveItems;

                    for (int i = 0; i < itemList.Count; i++)
                        if (Pipliz.Random.NextDouble() <= itemList[i].chance)
                            Stockpile.GetStockPile(player).Add(itemList[i].item);

                    ServerManager.SendAudio(machineState.Position.Vector, GameLoader.NAMESPACE + "MiningMachineAudio");
                }

                machineState.NextTimeForWork = machineState.MachineSettings.WorkTime + Time.SecondsSinceStartDouble;
            }
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterSelectedWorld, GameLoader.NAMESPACE + ".Items.Machines.Miner.RegisterAudio"),
            ModLoader.ModCallbackProvidesFor("pipliz.server.loadaudiofiles"), ModLoader.ModCallbackDependsOn("pipliz.server.registeraudiofiles")]
        public static void RegisterAudio()
        {
            var node = new JSONNode();
            node.SetAs("clipCollectionName", GameLoader.NAMESPACE + "MiningMachineAudio");

            var fileListNode = new JSONNode(NodeType.Array);
            var audoFileNode = new JSONNode()
                .SetAs("path", GameLoader.AUDIO_FOLDER_PANDA + "/MiningMachine.ogg")
                .SetAs("audioGroup", "Effects");

            fileListNode.AddToArray(audoFileNode);
            node.SetAs("fileList", fileListNode);

            ItemTypesServer.AudioFilesJSON.AddToArray(node);
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
            minerFlagNode.SetAs("isSolid", true);
            minerFlagNode.SetAs("sideall", "SELF");
            minerFlagNode.SetAs("mesh", GameLoader.MESH_FOLDER_PANDA + "/MiningMachine.obj");

            Item = new ItemTypesServer.ItemTypeRaw(minerName, minerFlagNode);
            items.Add(minerName, Item);
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnTryChangeBlockUser, GameLoader.NAMESPACE + ".Items.Machines.Miner.OnTryChangeBlockUser")]
        public static bool OnTryChangeBlockUser(ModLoader.OnTryChangeBlockUserData d)
        {
            if (d.typeToBuild == Item.ItemIndex && d.typeTillNow == BuiltinBlocks.Air)
            {
                if (World.TryGetTypeAt(d.voxelHit, out ushort itemBelow))
                {
                    if (CanMineBlock(itemBelow))
                    {
                        PandaChat.Send(d.requestedBy, $"{ItemTypes.IndexLookup.GetName(itemBelow).Replace("infinite", "")} ready to Mine! Ensure you have a Machinist around to run the machine!");
                        MachineManager.RegisterMachineState(d.requestedBy, new MachineState(d.voxelHit.Add(0, 1, 0), d.requestedBy, nameof(Miner)));
                        return true;
                    }
                }

                PandaChat.Send(d.requestedBy, "The mining machine must be placed on stone or ore.", ChatColor.orange);
                return false;
            }

            return true;
        }

        public static bool CanMineBlock(ushort itemMined)
        {
            return itemMined == BuiltinBlocks.StoneBlock ||
                    itemMined == BuiltinBlocks.DarkStone ||
                    itemMined == BuiltinBlocks.InfiniteClay ||
                    itemMined == BuiltinBlocks.InfiniteCoal ||
                    itemMined == BuiltinBlocks.InfiniteCopper ||
                    itemMined == BuiltinBlocks.InfiniteGalena ||
                    itemMined == BuiltinBlocks.InfiniteGold ||
                    itemMined == BuiltinBlocks.InfiniteGypsum ||
                    itemMined == BuiltinBlocks.InfiniteIron ||
                    itemMined == BuiltinBlocks.InfiniteSalpeter ||
                    itemMined == BuiltinBlocks.InfiniteStone ||
                    itemMined == BuiltinBlocks.InfiniteTin;
        }
    }
}
