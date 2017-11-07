using BlockTypes.Builtin;
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
        public class MinerState
        {
            const double MINETIME = 4;
            static System.Random _rand = new System.Random();

            public static int MAX_DURABILITY { get; set; } = 200;


            public Vector3Int Position { get; private set; }
            public int Durability { get; set; } = MAX_DURABILITY;

            private ushort itemBelow;
            
            
            private double _nextMineTime = Time.SecondsSinceStartDouble + _rand.NextDouble(0.0, 5.0);

            public MinerState(Vector3Int pos)
            {
                Position = pos;
                World.TryGetTypeAt(Position.Add(0, -1, 0), out itemBelow);
            }

            public MinerState(JSONNode baseNode)
            {

            }

            public JSONNode ToJsonNode()
            {
                var baseNode = new JSONNode();

                return baseNode;
            }

            public void MineNode()
            {
                if (Time.SecondsSinceStartDouble > _nextMineTime)
                {
                    if (itemBelow == BuiltinBlocks.StoneBlock ||
                        itemBelow == BuiltinBlocks.InfiniteClay ||
                        itemBelow == BuiltinBlocks.InfiniteCoal ||
                        itemBelow == BuiltinBlocks.InfiniteCopper ||
                        itemBelow == BuiltinBlocks.InfiniteGalena ||
                        itemBelow == BuiltinBlocks.InfiniteGold ||
                        itemBelow == BuiltinBlocks.InfiniteGypsum ||
                        itemBelow == BuiltinBlocks.InfiniteIron ||
                        itemBelow == BuiltinBlocks.InfiniteSalpeter ||
                        itemBelow == BuiltinBlocks.InfiniteStone ||
                        itemBelow == BuiltinBlocks.InfiniteTin)
                    {
                    }

                    _nextMineTime = Time.SecondsSinceStartDouble + MINETIME;
                }
            }
        }



        public static ItemTypesServer.ItemTypeRaw Item { get; private set; }
        private static Dictionary<Players.Player, Dictionary<Vector3Int, MinerState>> _minerLocations = new Dictionary<Players.Player, Dictionary<Vector3Int, MinerState>>();

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

            RecipeStorage.AddOptionalLimitTypeRecipe(ItemFactory.JOB_CRAFTER, recipe);
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterSelectedWorld, GameLoader.NAMESPACE + ".Items.Machines.Miner.AddTextures"), ModLoader.ModCallbackProvidesFor("pipliz.server.registertexturemappingtextures")]
        public static void AddTextures()
        {
            var minerTextureMapping = new ItemTypesServer.TextureMapping(new JSONNode());
            minerTextureMapping.AlbedoPath = GameLoader.TEXTURE_FOLDER_PANDA.Replace("\\", "/") + "/MiningMachine.png";

            ItemTypesServer.SetTextureMapping(GameLoader.NAMESPACE + ".Miner", minerTextureMapping);
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnLoadingPlayer, GameLoader.NAMESPACE + ".Items.Machines.Miner.OnLoadingPlayer")]
        public static void OnLoadingPlayer(JSONNode n, Players.Player p)
        {
            if (n.TryGetChild(GameLoader.NAMESPACE + ".Miners", out var minersNode))
            {
                if (!_minerLocations.ContainsKey(p))
                    _minerLocations.Add(p, new Dictionary<Vector3Int, MinerState>());

                foreach (var node in minersNode.LoopArray())
                    _minerLocations[p][(Vector3Int)node] = new MinerState(node);
            }
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnSavingPlayer, GameLoader.NAMESPACE + ".Items.Machines.Miner.PatrolTool.OnSavingPlayer")]
        public static void OnSavingPlayer(JSONNode n, Players.Player p)
        {
            if (_minerLocations.ContainsKey(p))
            {
                if (n.HasChild(GameLoader.NAMESPACE + ".Miners"))
                    n.RemoveChild(GameLoader.NAMESPACE + ".Miners");

                var minersNode = new JSONNode(NodeType.Array);

                foreach (var node in _minerLocations[p])
                    minersNode.AddToArray(node.Value.ToJsonNode());

                n[GameLoader.NAMESPACE + ".Miners"] = minersNode;
            }
        }


        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterAddingBaseTypes, GameLoader.NAMESPACE + ".Items.Machines.Miner.AddPatrolTool"), ModLoader.ModCallbackDependsOn("pipliz.blocknpcs.addlittypes")]
        public static void AddPatrolTool(Dictionary<string, ItemTypesServer.ItemTypeRaw> items)
        {
            var minterName = GameLoader.NAMESPACE + ".Miner";
            var minerFlagNode = new JSONNode();
            minerFlagNode["icon"] = new JSONNode(GameLoader.ICON_FOLDER_PANDA.Replace("\\", "/") + "/MiningMachine.png");
            minerFlagNode["isPlaceable"] = new JSONNode(true);
            minerFlagNode.SetAs("onRemoveAmount", 1);
            minerFlagNode.SetAs("isSolid", true);
            minerFlagNode.SetAs("sideall", "SELF");
            minerFlagNode.SetAs("mesh", GameLoader.MESH_FOLDER_PANDA.Replace("\\", "/") + "/MiningMachine.obj");

            Item = new ItemTypesServer.ItemTypeRaw(minterName, minerFlagNode);
            items.Add(minterName, Item);
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnTryChangeBlockUser, GameLoader.NAMESPACE + ".Items.Machines.Miner.OnTryChangeBlockUser")]
        public static bool OnTryChangeBlockUser(ModLoader.OnTryChangeBlockUserData d)
        {
            if (!_minerLocations.ContainsKey(d.requestedBy))
                _minerLocations.Add(d.requestedBy, new Dictionary<Vector3Int, MinerState>());

            if (d.typeTillNow == Item.ItemIndex && d.typeToBuild == BuiltinBlocks.Air && _minerLocations[d.requestedBy].ContainsKey(d.voxelHit))
                _minerLocations[d.requestedBy].Remove(d.voxelHit);

            if (d.typeToBuild == Item.ItemIndex && d.typeTillNow == BuiltinBlocks.Air)
            {
                var below = d.voxelHit;

                if (World.TryGetTypeAt(below, out ushort itemBelow))
                {
                    if (itemBelow == BuiltinBlocks.StoneBlock ||
                        itemBelow == BuiltinBlocks.InfiniteClay ||
                        itemBelow == BuiltinBlocks.InfiniteCoal ||
                        itemBelow == BuiltinBlocks.InfiniteCopper ||
                        itemBelow == BuiltinBlocks.InfiniteGalena ||
                        itemBelow == BuiltinBlocks.InfiniteGold ||
                        itemBelow == BuiltinBlocks.InfiniteGypsum ||
                        itemBelow == BuiltinBlocks.InfiniteIron ||
                        itemBelow == BuiltinBlocks.InfiniteSalpeter ||
                        itemBelow == BuiltinBlocks.InfiniteStone ||
                        itemBelow == BuiltinBlocks.InfiniteTin)
                    {
                        return true;
                    }
                }

                PandaChat.Send(d.requestedBy, "The mining machine must be placed on stone or ore.", ChatColor.orange);
                return false;
            }

            return true;
        }
    }
}
