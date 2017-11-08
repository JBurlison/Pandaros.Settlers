using BlockTypes.Builtin;
using NPC;
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

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterWorldLoad, GameLoader.NAMESPACE + ".Items.Machines.AfterWorldLoad"), ModLoader.ModCallbackDependsOn(GameLoader.NAMESPACE + ".AfterWorldLoad")]
        public static void AfterWorldLoad()
        {
            MachineManager.RegisterMachineType(nameof(Miner), new MachineManager.MachineCallback(Repair, Refuel, DoWork));
        }

        public static void Repair(NPCBase.NPCState npcState, Players.Player player, MachineState machineState)
        {

        }

        public static void Refuel(NPCBase.NPCState npcState, Players.Player player, MachineState machineState)
        {

        }

        public static void DoWork(Players.Player player, MachineState machineState)
        {

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
            var sling = new InventoryItem(BuiltinBlocks.Linen, 3);

            var recipe = new Recipe(Item.name,
                                    new List<InventoryItem>() { planks, iron, rivets, copperParts, copperNails, tools, planks, sling },
                                    new InventoryItem(Item.ItemIndex),
                                    5);

            RecipeStorage.AddOptionalLimitTypeRecipe(Jobs.AdvancedCrafterRegister.JOB_NAME, recipe);
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterSelectedWorld, GameLoader.NAMESPACE + ".Items.Machines.Miner.AddTextures"), ModLoader.ModCallbackProvidesFor("pipliz.server.registertexturemappingtextures")]
        public static void AddTextures()
        {
            var minerTextureMapping = new ItemTypesServer.TextureMapping(new JSONNode());
            minerTextureMapping.AlbedoPath = GameLoader.TEXTURE_FOLDER_PANDA.Replace("\\", "/") + "/MiningMachine.png";

            ItemTypesServer.SetTextureMapping(GameLoader.NAMESPACE + ".Miner", minerTextureMapping);
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterAddingBaseTypes, GameLoader.NAMESPACE + ".Items.Machines.Miner.AddMiner"), ModLoader.ModCallbackDependsOn("pipliz.blocknpcs.addlittypes")]
        public static void AddMiner(Dictionary<string, ItemTypesServer.ItemTypeRaw> items)
        {
            var minerName = GameLoader.NAMESPACE + ".Miner";
            var minerFlagNode = new JSONNode();
            minerFlagNode["icon"] = new JSONNode(GameLoader.ICON_FOLDER_PANDA.Replace("\\", "/") + "/MiningMachine.png");
            minerFlagNode["isPlaceable"] = new JSONNode(true);
            minerFlagNode.SetAs("onRemoveAmount", 1);
            minerFlagNode.SetAs("isSolid", true);
            minerFlagNode.SetAs("sideall", "SELF");
            minerFlagNode.SetAs("mesh", GameLoader.MESH_FOLDER_PANDA.Replace("\\", "/") + "/MiningMachine.obj");

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
