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
    public static class Porter
    {
        const double PorterCooldown = 4;

        public static ItemTypesServer.ItemTypeRaw Item { get; private set; }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterAddingBaseTypes, GameLoader.NAMESPACE + ".Items.Machines.Porter.RegisterMachines")]
        public static void RegisterMachines(Dictionary<string, ItemTypesServer.ItemTypeRaw> items)
        {
            MachineManager.RegisterMachineType(nameof(Porter), new MachineManager.MachineSettings(Item.ItemIndex, Repair, MachineManager.Refuel, Reload, DoWork, 10, 4, 5, 4));
        }

        public static ushort Repair(Players.Player player, MachineState machineState)
        {
            var retval = GameLoader.Repairing_Icon;
            var ps = PlayerState.GetPlayerState(player);

            if (machineState.Durability < .75f + ps.Difficulty.MachineThreashHold)
            {
                bool repaired = false;
                List<InventoryItem> requiredForFix = new List<InventoryItem>();
                var stockpile = Stockpile.GetStockPile(player);
                
                requiredForFix.Add(new InventoryItem(BuiltinBlocks.CopperTools, 1));
                requiredForFix.Add(new InventoryItem(BuiltinBlocks.CopperParts, 1));

                if (machineState.Durability < .10f)
                {
                    requiredForFix.Add(new InventoryItem(BuiltinBlocks.CopperParts, 4));
                    requiredForFix.Add(new InventoryItem(BuiltinBlocks.Planks, 1));
                }
                else if (machineState.Durability < .30f)
                {
                    requiredForFix.Add(new InventoryItem(BuiltinBlocks.CopperParts, 3));
                }
                else if (machineState.Durability < .50f)
                {
                    requiredForFix.Add(new InventoryItem(BuiltinBlocks.CopperParts, 2));
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
                    machineState.Durability = MachineState.MAX_DURABILITY[player] + ps.Difficulty.MachineThreashHold;
            }

            return retval;
        }
        
        public static ushort Reload(Players.Player player, MachineState machineState)
        {
            if (!MachineState.MAX_LOAD.ContainsKey(player))
                MachineState.MAX_LOAD[player] = MachineState.DEFAULT_MAX_LOAD;

            machineState.Load = MachineState.MAX_LOAD[player];

            return GameLoader.Reload_Icon;
        }

        public static void DoWork(Players.Player player, MachineState machineState)
        {
            if (machineState.Durability > 0 && 
                machineState.Fuel > 0 && 
                machineState.NextTimeForWork < Time.SecondsSinceStartDouble)
            {

                if (TimeCycle.IsDay)
                {
                    machineState.Durability -= 0.05f;
                    machineState.Fuel -= 0.10f;
                    machineState.Load = 0f;

                    if (machineState.Durability < 0)
                        machineState.Durability = 0;

                    if (machineState.Fuel <= 0)
                        machineState.Fuel = 0;
                }
                
                machineState.NextTimeForWork = machineState.MachineSettings.WorkTime + Time.SecondsSinceStartDouble;
            }
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterSelectedWorld, GameLoader.NAMESPACE + ".Items.Machines.Porter.RegisterAudio"),
            ModLoader.ModCallbackProvidesFor("pipliz.server.loadaudiofiles"), ModLoader.ModCallbackDependsOn("pipliz.server.registeraudiofiles")]
        public static void RegisterAudio()
        {
            GameLoader.AddSoundFile(GameLoader.NAMESPACE + "MiningMachineAudio", new List<string>() { GameLoader.AUDIO_FOLDER_PANDA + "/MiningMachine.ogg" });
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterItemTypesDefined, GameLoader.NAMESPACE + ".Items.Machines.Porter.RegisterPorter")]
        public static void RegisterPorter()
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

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterSelectedWorld, GameLoader.NAMESPACE + ".Items.Machines.Porter.AddTextures"), ModLoader.ModCallbackProvidesFor("pipliz.server.registertexturemappingtextures")]
        public static void AddTextures()
        {
            var PorterTextureMapping = new ItemTypesServer.TextureMapping(new JSONNode());
            PorterTextureMapping.AlbedoPath = GameLoader.TEXTURE_FOLDER_PANDA + "/MiningMachine.png";

            ItemTypesServer.SetTextureMapping(GameLoader.NAMESPACE + ".Porter", PorterTextureMapping);
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterAddingBaseTypes, GameLoader.NAMESPACE + ".Items.Machines.Porter.AddPorter"), ModLoader.ModCallbackDependsOn("pipliz.blocknpcs.addlittypes")]
        public static void AddPorter(Dictionary<string, ItemTypesServer.ItemTypeRaw> items)
        {
            var PorterName = GameLoader.NAMESPACE + ".Porter";
            var PorterFlagNode = new JSONNode();
            PorterFlagNode["icon"] = new JSONNode(GameLoader.ICON_FOLDER_PANDA + "/MiningMachine.png");
            PorterFlagNode["isPlaceable"] = new JSONNode(true);
            PorterFlagNode.SetAs("onRemoveAmount", 1);
            PorterFlagNode.SetAs("onPlaceAudio", "stonePlace");
            PorterFlagNode.SetAs("onRemoveAudio", "stoneDelete");
            PorterFlagNode.SetAs("isSolid", true);
            PorterFlagNode.SetAs("sideall", "SELF");
            PorterFlagNode.SetAs("mesh", GameLoader.MESH_FOLDER_PANDA + "/MiningMachine.obj");

            Item = new ItemTypesServer.ItemTypeRaw(PorterName, PorterFlagNode);
            items.Add(PorterName, Item);
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnTryChangeBlockUser, GameLoader.NAMESPACE + ".Items.Machines.Porter.OnTryChangeBlockUser")]
        public static bool OnTryChangeBlockUser(ModLoader.OnTryChangeBlockUserData d)
        {
            if (d.typeToBuild == Item.ItemIndex && d.typeTillNow == BuiltinBlocks.Air)
            {
                if (World.TryGetTypeAt(d.VoxelToChange.Add(0, -1, 0), out ushort itemBelow))
                {
                    if (CanMineBlock(itemBelow))
                    {
                        MachineManager.RegisterMachineState(d.requestedBy, new MachineState(d.VoxelToChange, d.requestedBy, nameof(Porter)));
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
