﻿using Pandaros.API;
using Pandaros.API.Entities;
using Pandaros.API.localization;
using Pipliz;
using Pipliz.JSON;
using Recipes;
using Shared;
using System;
using System.Collections.Generic;

namespace Pandaros.Settlers.Jobs
{
    public enum PatrolType
    {
        RoundRobin = 0,
        Zipper = 1,
        WaitRoundRobin = 2,
        WaitZipper = 3
    }

    [ModLoader.ModManager]
    public static class PatrolTool
    {
        private static readonly Dictionary<Colony, List<KnightState>> _loadedKnights = new Dictionary<Colony, List<KnightState>>();
        private static LocalizationHelper _localizationHelper = new LocalizationHelper(GameLoader.NAMESPACE, "Knights");

        public static ItemTypesServer.ItemTypeRaw Item { get; private set; }
        public static ItemTypesServer.ItemTypeRaw PatrolFlag { get; private set; }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterItemTypesDefined, GameLoader.NAMESPACE + ".Jobs.PatrolTool.RegisterPatrolTool")]
        [ModLoader.ModCallbackProvidesFor("pipliz.server.loadresearchables")]
        public static void RegisterPatrolTool()
        {
            var planks = new InventoryItem(ColonyBuiltIn.ItemTypes.PLANKS.Name, 2);
            var carpet = new InventoryItem(ColonyBuiltIn.ItemTypes.CARPETWHITE.Name, 2);

            var recipe = new Recipe(PatrolFlag.name,
                                    new List<InventoryItem> {planks, carpet},
                                    new RecipeResult(PatrolFlag.ItemIndex, 2),
                                    5);

            ServerManager.RecipeStorage.AddLimitTypeRecipe(ColonyBuiltIn.NpcTypes.CRAFTER, recipe);
            ServerManager.RecipeStorage.AddScienceRequirement(recipe);
        }


        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterSelectedWorld, GameLoader.NAMESPACE + ".Jobs.PatrolTool.AddTextures")]
        [ModLoader.ModCallbackProvidesFor("pipliz.server.registertexturemappingtextures")]
        public static void AddTextures()
        {
            var flagTextureMapping = new ItemTypesServer.TextureMapping(new JSONNode());
            flagTextureMapping.AlbedoPath = GameLoader.BLOCKS_ALBEDO_PATH + "PatrolFlag.png";

            ItemTypesServer.SetTextureMapping(GameLoader.NAMESPACE + ".PatrolFlag", flagTextureMapping);
        }


        [ModLoader.ModCallback(ModLoader.EModCallbackType.AddItemTypes, GameLoader.NAMESPACE + ".Jobs.PatrolTool.AddPatrolTool")]
        [ModLoader.ModCallbackDependsOn("pipliz.server.applymoditempatches")]
        public static void AddPatrolTool(Dictionary<string, ItemTypesServer.ItemTypeRaw> items)
        {
            var patrolToolName = GameLoader.NAMESPACE + ".PatrolTool";
            var patrolToolNode = new JSONNode();
            patrolToolNode["icon"]        = new JSONNode(GameLoader.ICON_PATH + "KnightPatrolTool.png");
            patrolToolNode["isPlaceable"] = new JSONNode(false);

            var categories = new JSONNode(NodeType.Array);
            categories.AddToArray(new JSONNode("job"));
            categories.AddToArray(new JSONNode(GameLoader.NAMESPACE));
            patrolToolNode.SetAs("categories", categories);

            Item = new ItemTypesServer.ItemTypeRaw(patrolToolName, patrolToolNode);
            items.Add(patrolToolName, Item);

            var patrolFlagName = GameLoader.NAMESPACE + ".PatrolFlag";
            var patrolFlagNode = new JSONNode();
            patrolFlagNode["icon"]        = new JSONNode(GameLoader.ICON_PATH + "PatrolFlagItem.png");
            patrolFlagNode["isPlaceable"] = new JSONNode(false);
            patrolFlagNode.SetAs("onRemoveAmount", 0);
            patrolFlagNode.SetAs("isSolid", false);
            patrolFlagNode.SetAs("sideall", "SELF");
            patrolFlagNode.SetAs("mesh", GameLoader.MESH_PATH + "PatrolFlag.obj");

            var patrolFlagCategories = new JSONNode(NodeType.Array);
            patrolFlagCategories.AddToArray(new JSONNode("job"));
            patrolFlagCategories.AddToArray(new JSONNode(GameLoader.NAMESPACE));
            patrolFlagNode.SetAs("categories", patrolFlagCategories);

            PatrolFlag = new ItemTypesServer.ItemTypeRaw(patrolFlagName, patrolFlagNode);
            items.Add(patrolFlagName, PatrolFlag);
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnPlayerConnectedLate, GameLoader.NAMESPACE + ".Jobs.PatrolTool.OnPlayerConnectedLate")]
        [ModLoader.ModCallbackDependsOn(GameLoader.NAMESPACE + ".SettlerManager.OnPlayerConnectedLate")]
        public static void OnPlayerConnectedLate(Players.Player p)
        {
            if (p.GetTempValues(true).GetOrDefault(GameLoader.NAMESPACE + ".Knights", 0f) == 1f)
                GivePlayerPatrolTool(p);
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnPlayerRespawn, GameLoader.NAMESPACE + ".Jobs.PatrolTool.OnPlayerRespawn")]
        public static void OnPlayerRespawn(Players.Player p)
        {
            if (p.GetTempValues(true).GetOrDefault(GameLoader.NAMESPACE + ".Knights", 0f) == 1f)
                GivePlayerPatrolTool(p);
        }

        public static void GivePlayerPatrolTool(Players.Player p)
        {
            var playerStockpile = p.ActiveColony?.Stockpile;
            var hasTool         = false;

            foreach (var item in p.Inventory.Items)
                if (item.Type == Item.ItemIndex)
                {
                    hasTool = true;
                    break;
                }

            if (!hasTool && playerStockpile != null)
                hasTool = playerStockpile.Contains(Item.ItemIndex);

            if (!hasTool && playerStockpile != null)
                playerStockpile.Add(new InventoryItem(Item.ItemIndex));
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnLoadingColony, GameLoader.NAMESPACE + ".Jobs.OnLoadingColony")]
        public static void OnLoadingColony(Colony c, JSONNode n)
        {
            if (n.TryGetChild(GameLoader.NAMESPACE + ".Knights", out var knightsNode))
                foreach (var knightNode in knightsNode.LoopArray())
                {
                    var points = new List<Vector3Int>();

                    foreach (var point in knightNode["PatrolPoints"].LoopArray())
                        points.Add((Vector3Int)point);

                    if (knightNode.TryGetAs("PatrolType", out string patrolTypeStr))
                    {
                        var patrolMode = (PatrolType) Enum.Parse(typeof(PatrolType), patrolTypeStr);

                        if (!_loadedKnights.ContainsKey(c))
                            _loadedKnights.Add(c, new List<KnightState>());

                        _loadedKnights[c].Add(new KnightState {PatrolPoints = points, patrolType = patrolMode});
                    }
                }
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterWorldLoad, GameLoader.NAMESPACE + ".Jobs.PatrolTool.AfterWorldLoad")]
        public static void AfterWorldLoad()
        {
            foreach (var k in _loadedKnights)
            foreach (var kp in k.Value)
            {
                var knight = new Knight(kp.PatrolPoints, k.Key);
                knight.PatrolType = kp.patrolType;
                k.Key.JobFinder.Add(knight);
            }
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnSavingColony, GameLoader.NAMESPACE + ".Jobs.PatrolTool.OnSavingColony")]
        public static void OnSavingColony(Colony c, JSONNode n)
        {
            try
            {
                if (Knight.Knights.ContainsKey(c))
                {
                    if (n.HasChild(GameLoader.NAMESPACE + ".Knights"))
                        n.RemoveChild(GameLoader.NAMESPACE + ".Knights");

                    var knightsNode = new JSONNode(NodeType.Array);

                    foreach (var knight in Knight.Knights[c])
                    {
                        var knightNode = new JSONNode().SetAs(nameof(knight.PatrolType), knight.PatrolType);

                        var patrolPoints = new JSONNode(NodeType.Array);

                        foreach (var point in knight.PatrolPoints)
                            patrolPoints.AddToArray((JSONNode)point);

                        knightNode.SetAs(nameof(knight.PatrolPoints), patrolPoints);

                        knightsNode.AddToArray(knightNode);
                    }

                    n[GameLoader.NAMESPACE + ".Knights"] = knightsNode;
                }
            }
            catch (Exception ex)
            {
                SettlersLogger.LogError(ex);
            }
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnPlayerClicked, GameLoader.NAMESPACE + ".Jobs.PlacePatrol")]
        public static void PlacePatrol(Players.Player player, PlayerClickedData playerClickData)
        {
            if (playerClickData.IsConsumed || player.ActiveColony == null || playerClickData.HitType != PlayerClickedData.EHitType.Block)
                return;

            var click      = playerClickData;
            var state      = PlayerState.GetPlayerState(player);
            var rayCastHit = click.GetVoxelHit();

            if (click.TypeSelected == Item.ItemIndex)
            {
                if (rayCastHit.TypeHit != PatrolFlag.ItemIndex)
                {
                    var flagPoint = rayCastHit.BlockHit.Add(0, 1, 0);

                    if (click.ClickType == PlayerClickedData.EClickType.Left)
                    {
                        var hasFlags = player.TakeItemFromInventory(PatrolFlag.ItemIndex);

                        if (!hasFlags)
                        {
                            if (player.ActiveColony.Stockpile.Contains(PatrolFlag.ItemIndex))
                            {
                                hasFlags = true;
                                player.ActiveColony.Stockpile.TryRemove(PatrolFlag.ItemIndex);
                            }
                        }

                        if (!hasFlags)
                        {
                            PandaChat.Send(player, _localizationHelper, "NoFlags", ChatColor.orange);
                        }
                        else
                        {
                            state.FlagsPlaced.Add(flagPoint);
                            ServerManager.TryChangeBlock(flagPoint, PatrolFlag.ItemIndex);
                            PandaChat.Send(player,_localizationHelper, "CreateJob", ChatColor.orange, state.FlagsPlaced.Count.ToString());
                        }
                    }
                }
                else
                {
                    foreach (var knight in Knight.Knights[player.ActiveColony])
                        if (knight.PatrolPoints.Contains(rayCastHit.BlockHit))
                        {
                            var patrol = string.Empty;

                            if (knight.PatrolType == PatrolType.RoundRobin)
                            {
                                patrol = "RoundRobinDescription";
                                knight.PatrolType = PatrolType.WaitRoundRobin;
                            }
                            if (knight.PatrolType == PatrolType.WaitRoundRobin)
                            {
                                patrol = "ZipperDescription";
                                knight.PatrolType = PatrolType.Zipper;
                            }
                            if (knight.PatrolType == PatrolType.Zipper)
                            {
                                patrol = "WaitRoundRobinDescription";
                                knight.PatrolType = PatrolType.WaitZipper;
                            }
                            else
                            {
                                patrol = "WaitZipperDescription";
                                knight.PatrolType = PatrolType.RoundRobin;
                            }

                            PandaChat.Send(player, _localizationHelper, "PatrolTypeSet", ChatColor.orange, knight.PatrolType.ToString());
                            PandaChat.Send(player, _localizationHelper, patrol, ChatColor.orange);
                            break;
                        }
                }
            }

            if (click.TypeSelected == Item.ItemIndex && click.ClickType == PlayerClickedData.EClickType.Right)
            {
                if (state.FlagsPlaced.Count == 0)
                {
                    PandaChat.Send(player, _localizationHelper, "MustPlaceFlag", ChatColor.orange);
                }
                else
                {
                    var knight = new Knight(new List<Vector3Int>(state.FlagsPlaced), player.ActiveColony);
                    state.FlagsPlaced.Clear();
                    player.ActiveColony.JobFinder.Add(knight);

                    PandaChat.Send(player, _localizationHelper, "Active", ChatColor.orange);

                    player.ActiveColony.JobFinder.Update();
                    player.ActiveColony.SendCommonData();
                }
            }
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnTryChangeBlock, GameLoader.NAMESPACE + ".Jobs.OnTryChangeBlockUser")]
        public static void OnTryChangeBlockUser(ModLoader.OnTryChangeBlockData d)
        {
            try
            {
                if (d.CallbackState == ModLoader.OnTryChangeBlockData.ECallbackState.Cancelled ||
                    d.RequestOrigin.AsPlayer == null ||
                    d.RequestOrigin.AsPlayer.ActiveColony == null ||
                    d.TypeOld == null)
                    return;

                if (d.TypeOld.ItemIndex == PatrolFlag.ItemIndex)
                {
                    var toRemove = default(Knight);

                    if (!Knight.Knights.ContainsKey(d.RequestOrigin.AsPlayer.ActiveColony))
                        Knight.Knights.Add(d.RequestOrigin.AsPlayer.ActiveColony, new List<Knight>());

                    foreach (var knight in Knight.Knights[d.RequestOrigin.AsPlayer.ActiveColony])
                        try
                        {
                            if (knight.PatrolPoints.Contains(d.Position))
                            {
                                toRemove = knight;
                                knight.OnRemove();

                                foreach (var flagPoint in knight.PatrolPoints)
                                    if (flagPoint != d.Position)
                                        if (World.TryGetTypeAt(flagPoint, out ushort objType) &&
                                            objType == PatrolFlag.ItemIndex)
                                        {
                                            ServerManager.TryChangeBlock(flagPoint, ColonyBuiltIn.ItemTypes.AIR.Id);
                                            d.RequestOrigin.AsPlayer.ActiveColony.Stockpile.Add(PatrolFlag.ItemIndex);
                                        }

                                break;
                            }
                        }
                        catch (Exception ex)
                        {
                            SettlersLogger.LogError(ex);
                        }

                    if (toRemove != default(Knight))
                    {
                        PandaChat.Send(d.RequestOrigin.AsPlayer, _localizationHelper, "PatrolsNotActive", ChatColor.orange, toRemove.PatrolPoints.Count.ToString());

                        Knight.Knights[d.RequestOrigin.AsPlayer.ActiveColony].Remove(toRemove);
                        d.RequestOrigin.AsPlayer.ActiveColony.JobFinder.Remove(toRemove);
                        d.RequestOrigin.AsPlayer.ActiveColony.JobFinder.Update();
                    }

                    var state = PlayerState.GetPlayerState(d.RequestOrigin.AsPlayer);
                    if (state.FlagsPlaced.Contains(d.Position))
                    {
                        state.FlagsPlaced.Remove(d.Position);
                        ServerManager.TryChangeBlock(d.Position, ColonyBuiltIn.ItemTypes.AIR.Id);
                    }

                    d.RequestOrigin.AsPlayer.ActiveColony.Stockpile.Add(PatrolFlag.ItemIndex);
                }
            }
            catch (Exception ex)
            {
                SettlersLogger.LogError(ex);
            }
        }

        internal class KnightState
        {
            internal List<Vector3Int> PatrolPoints;
            internal PatrolType patrolType;
        }
    }
}