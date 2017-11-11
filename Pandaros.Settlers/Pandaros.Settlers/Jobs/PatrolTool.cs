using BlockTypes.Builtin;
using NPC;
using Pandaros.Settlers.Entities;
using Pandaros.Settlers.Items;
using Pipliz.JSON;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Pandaros.Settlers.Research;
using Pipliz;
using Server.FileTable;

namespace Pandaros.Settlers.Jobs
{
    public enum PatrolType
    {
        RoundRobin,
        Zipper
    }

    [ModLoader.ModManager]
    public static class PatrolTool
    {
        public static ItemTypesServer.ItemTypeRaw Item { get; private set; }
        public static ItemTypesServer.ItemTypeRaw PatrolFlag { get; private set; }
        
        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterItemTypesDefined, GameLoader.NAMESPACE + ".Jobs.PatrolTool.RegisterPatrolTool")]
        public static void RegisterPatrolTool()
        {
            var planks = new InventoryItem(BuiltinBlocks.Planks, 2);
            var carpet = new InventoryItem(BuiltinBlocks.CarpetRed, 2);

            var recipe = new Recipe(PatrolFlag.name,
                                    new List<InventoryItem>() { planks, carpet },
                                    new InventoryItem(PatrolFlag.ItemIndex, 2),
                                    5);

            RecipeStorage.AddOptionalLimitTypeRecipe(ItemFactory.JOB_CRAFTER, recipe);
        }


        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterSelectedWorld, GameLoader.NAMESPACE + ".Jobs.PatrolTool.AddTextures"), ModLoader.ModCallbackProvidesFor("pipliz.server.registertexturemappingtextures")]
        public static void AddTextures()
        {
            var flagTextureMapping = new ItemTypesServer.TextureMapping(new JSONNode());
            flagTextureMapping.AlbedoPath = GameLoader.TEXTURE_FOLDER_PANDA + "/PatrolFlag.png";

            ItemTypesServer.SetTextureMapping(GameLoader.NAMESPACE + ".PatrolFlag", flagTextureMapping);
        }


        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterAddingBaseTypes, GameLoader.NAMESPACE + ".Jobs.PatrolTool.AddPatrolTool"), ModLoader.ModCallbackDependsOn("pipliz.blocknpcs.addlittypes")]
        public static void AddPatrolTool(Dictionary<string, ItemTypesServer.ItemTypeRaw> items)
        {
            var patrolToolName = GameLoader.NAMESPACE + ".PatrolTool";
            var patrolToolNode = new JSONNode();
            patrolToolNode["icon"] = new JSONNode(GameLoader.ICON_FOLDER_PANDA + "/KnightPatrolTool.png");
            patrolToolNode["isPlaceable"] = new JSONNode(false);

            Item = new ItemTypesServer.ItemTypeRaw(patrolToolName, patrolToolNode);
            items.Add(patrolToolName, Item);

            var patrolFlagName = GameLoader.NAMESPACE + ".PatrolFlag";
            var patrolFlagNode = new JSONNode();
            patrolFlagNode["icon"] = new JSONNode(GameLoader.ICON_FOLDER_PANDA + "/PatrolFlagItem.png");
            patrolFlagNode["isPlaceable"] = new JSONNode(false);
            patrolFlagNode.SetAs("onRemoveAmount", 0);
            patrolFlagNode.SetAs("isSolid", false);
            patrolFlagNode.SetAs("sideall", "SELF");
            patrolFlagNode.SetAs("mesh", GameLoader.MESH_FOLDER_PANDA + "/PatrolFlag.obj");

            PatrolFlag = new ItemTypesServer.ItemTypeRaw(patrolFlagName, patrolFlagNode);
            items.Add(patrolFlagName, PatrolFlag);
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnPlayerConnectedLate, GameLoader.NAMESPACE + ".Jobs.PatrolTool.OnPlayerConnectedLate"), ModLoader.ModCallbackDependsOn(GameLoader.NAMESPACE + ".SettlerManager.OnPlayerConnectedLate")]
        public static void OnPlayerConnectedLate(Players.Player p)
        {
            if (p.IsConnected && p.GetTempValues(true).GetOrDefault(PandaResearch.GetResearchKey(PandaResearch.Knights), 0f) == 1f)
                GivePlayerPatrolTool(p);
        }

        public static void GivePlayerPatrolTool(Players.Player p)
        {
            var playerStockpile = Stockpile.GetStockPile(p);
            bool hasTool = false;

            foreach (var item in Inventory.GetInventory(p).Items)
                if (item.Type == Item.ItemIndex)
                {
                    hasTool = true;
                    break;
                }

            if (!hasTool)
                hasTool = playerStockpile.Contains(Item.ItemIndex);

            if (!hasTool)
                playerStockpile.Add(new InventoryItem(Item.ItemIndex));
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnLoadingPlayer, GameLoader.NAMESPACE + ".Jobs.OnLoadingPlayer")]
        public static void OnLoadingPlayer(JSONNode n, Players.Player p)
        {
            if (n.TryGetChild(GameLoader.NAMESPACE + ".Knights", out var knightsNode))
            {
                foreach (var knightNode in knightsNode.LoopArray())
                {
                    var points = new List<Vector3Int>();

                    foreach (var point in knightNode["PatrolPoints"].LoopArray())
                        points.Add((Vector3Int)point);

                    if (knightNode.TryGetAs("PatrolType", out string patrolTypeStr))
                    {
                        var patrolMode = (PatrolType)Enum.Parse(typeof(PatrolType), patrolTypeStr);

                        if (!_loadedKnights.ContainsKey(p))
                            _loadedKnights.Add(p, new List<KnightState>());

                        _loadedKnights[p].Add(new KnightState() { PatrolPoints = points, patrolType = patrolMode });
                    }
                }
            }

            JobTracker.Update();
        }

        internal class KnightState
        {
            internal PatrolType patrolType;
            internal List<Vector3Int> PatrolPoints;
        }

        static Dictionary<Players.Player, List<KnightState>> _loadedKnights = new Dictionary<Players.Player, List<KnightState>>();

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterWorldLoad, GameLoader.NAMESPACE + ".Jobs.PatrolTool.AfterWorldLoad"), ModLoader.ModCallbackProvidesFor("pipliz.apiprovider.jobs.load")]
        public static void AfterWorldLoad()
        {
            foreach (var k in _loadedKnights)
            {
                foreach (var kp in k.Value)
                {
                    var knight = new Knight(kp.PatrolPoints, k.Key);
                    knight.PatrolType = kp.patrolType;
                    JobTracker.Add(knight);
                }
            }
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnSavingPlayer, GameLoader.NAMESPACE + ".Jobs.PatrolTool.OnSavingPlayer")]
        public static void OnSavingPlayer(JSONNode n, Players.Player p)
        {
            if (Knight.Knights.ContainsKey(p))
            {
                if (n.HasChild(GameLoader.NAMESPACE + ".Knights"))
                    n.RemoveChild(GameLoader.NAMESPACE + ".Knights");

                var knightsNode = new JSONNode(NodeType.Array);

                foreach (var knight in Knight.Knights[p])
                {
                    var knightNode = new JSONNode()
                        .SetAs(nameof(knight.PatrolType), knight.PatrolType);

                    var patrolPoints = new JSONNode(NodeType.Array);

                    foreach (var point in knight.PatrolPoints)
                        patrolPoints.AddToArray((JSONNode)point);

                    knightNode.SetAs(nameof(knight.PatrolPoints), patrolPoints);

                    knightsNode.AddToArray(knightNode);
                }

                n[GameLoader.NAMESPACE + ".Knights"] = knightsNode;
            }
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnPlayerClicked, GameLoader.NAMESPACE + ".Jobs.PlacePatrol")]
        public static void PlacePatrol(Players.Player player, Pipliz.Box<Shared.PlayerClickedData> boxedData)
        {
            if (boxedData.item1.IsConsumed)
                return;

            var click = boxedData.item1;
            Shared.VoxelRayCastHit rayCastHit = click.rayCastHit;
            var state = PlayerState.GetPlayerState(player);

            if (rayCastHit.rayHitType == Shared.RayHitType.Block &&
                click.typeSelected == Item.ItemIndex)
            {
                var stockpile = Stockpile.GetStockPile(player);

                if (click.typeHit != PatrolFlag.ItemIndex)
                {
                    var flagPoint = rayCastHit.voxelHit.Add(0, 1, 0);

                    if (click.clickType == Shared.PlayerClickedData.ClickType.Left)
                    {
                        bool hasFlags = player.TakeItemFromInventory(PatrolFlag.ItemIndex);

                        if (!hasFlags)
                        {
                            var playerStock = Stockpile.GetStockPile(player);

                            if (playerStock.Contains(PatrolFlag.ItemIndex))
                            {
                                hasFlags = true;
                                playerStock.TryRemove(PatrolFlag.ItemIndex);
                            }
                        }

                        if (!hasFlags)
                        {
                            PandaChat.Send(player, "You have no patrol flags in your stockpile or inventory.", ChatColor.orange);
                        }
                        else
                        {
                            state.FlagsPlaced.Add(flagPoint);
                            ServerManager.TryChangeBlock(flagPoint, PatrolFlag.ItemIndex);
                            PandaChat.Send(player, $"Patrol Point number {state.FlagsPlaced.Count} Registered! Right click to create Job.", ChatColor.orange);
                        }
                    }
                }
                else
                {
                    foreach (var knight in Knight.Knights[player])
                        if (knight.PatrolPoints.Contains(rayCastHit.voxelHit))
                        {
                            string patrol = string.Empty;

                            if (knight.PatrolType == PatrolType.RoundRobin)
                            {
                                patrol = "The knight will patrol from the first to last point, then, work its way backwords to the first. Good for patrolling a secion of a wall";
                                knight.PatrolType = PatrolType.Zipper;
                            }
                            else
                            {
                                patrol = "The knight will patrol from the first to last point, start over at the first point. Good for circles";
                                knight.PatrolType = PatrolType.RoundRobin;
                            }

                            PandaChat.Send(player, $"Patrol type set to {knight.PatrolType}!", ChatColor.orange);
                            PandaChat.Send(player, patrol, ChatColor.orange);
                            break;
                        }
                }
            }

            if (click.typeSelected == Item.ItemIndex && click.clickType == Shared.PlayerClickedData.ClickType.Right)
            {
                if (state.FlagsPlaced.Count == 0)
                {
                    PandaChat.Send(player, "You must place patrol flags using left click before setting the patrol.", ChatColor.orange);
                }
                else
                {
                    var knight = new Knight(new List<Pipliz.Vector3Int>(state.FlagsPlaced), player);
                    state.FlagsPlaced.Clear();
                    JobTracker.Add(knight);
                    PandaChat.Send(player, "Patrol Active! To stop the patrol pick up any of the patrol flags in the patrol.", ChatColor.orange);
                    JobTracker.Update();
                }
            }
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnTryChangeBlockUser, GameLoader.NAMESPACE + ".Jobs.OnTryChangeBlockUser")]
        public static bool OnTryChangeBlockUser(ModLoader.OnTryChangeBlockUserData d)
        {
            if (d.typeTillNow == PatrolFlag.ItemIndex)
            {
                Knight toRemove = default(Knight);

                var state = PlayerState.GetPlayerState(d.requestedBy);
                var stockpile = Stockpile.GetStockPile(d.requestedBy);

                foreach (var knight in Knight.Knights[d.requestedBy])
                {
                    try
                    {
                        if (knight.PatrolPoints.Contains(d.voxelHit))
                        {
                            knight.OnRemove();

                            foreach (var flagPoint in knight.PatrolPoints)
                                if (flagPoint != d.voxelHit)
                                {
                                    ServerManager.TryChangeBlock(flagPoint, BuiltinBlocks.Air);
                                    stockpile.Add(PatrolFlag.ItemIndex);
                                }

                            break;
                        }
                    }
                    catch (Exception ex)
                    {
                        PandaLogger.LogError(ex);
                    }
                }
                   
                if (toRemove != default(Knight))
                {
                    PandaChat.Send(d.requestedBy, $"Patrol with {toRemove.PatrolPoints.Count} patrol points no longer active.", ChatColor.orange);
                    Knight.Knights[d.requestedBy].Remove(toRemove);

                    if (((JobTracker.JobFinder)JobTracker.GetOrCreateJobFinder(d.requestedBy)).openJobs.Contains(toRemove))
                        ((JobTracker.JobFinder)JobTracker.GetOrCreateJobFinder(d.requestedBy)).openJobs.Remove(toRemove);

                    JobTracker.Update();
                }

                if (state.FlagsPlaced.Contains(d.voxelHit))
                {
                    state.FlagsPlaced.Remove(d.voxelHit);
                    ServerManager.TryChangeBlock(d.voxelHit, BuiltinBlocks.Air);
                }

                stockpile.Add(PatrolFlag.ItemIndex);
            }

            return true;
        }
    }
}
