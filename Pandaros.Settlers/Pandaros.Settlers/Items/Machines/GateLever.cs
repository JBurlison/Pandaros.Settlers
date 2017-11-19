using BlockTypes.Builtin;
using NPC;
using Pandaros.Settlers.Entities;
using Pandaros.Settlers.Managers;
using Pipliz;
using Pipliz.JSON;
using Server.MeshedObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using UnityEngine;
using static Pandaros.Settlers.Managers.AnimationManager;

namespace Pandaros.Settlers.Items.Machines
{
    [ModLoader.ModManager]
    public static class GateLever
    {
        const double GateLeverCooldown = 4;
        private const string DoorOpen = "DoorOpen";

        public static ItemTypesServer.ItemTypeRaw Item { get; private set; }
        public static ItemTypesServer.ItemTypeRaw GateItem { get; private set; }
        static MeshedObjectTypeSettings _gateXMinusItemObjSettings = new MeshedObjectTypeSettings(GameLoader.NAMESPACE + ".GateXMinusAnimated", GameLoader.MESH_FOLDER_PANDA + "/gatex-.obj", GameLoader.NAMESPACE + ".Gate");
        static MeshedObjectTypeSettings _gateXPlusItemObjSettings = new MeshedObjectTypeSettings(GameLoader.NAMESPACE + ".GateXPlusAnimated", GameLoader.MESH_FOLDER_PANDA + "/gatex+.obj", GameLoader.NAMESPACE + ".Gate");
        static MeshedObjectTypeSettings _gateZMinusItemObjSettings = new MeshedObjectTypeSettings(GameLoader.NAMESPACE + ".GateZMinusAnimated", GameLoader.MESH_FOLDER_PANDA + "/gatez-.obj", GameLoader.NAMESPACE + ".Gate");
        static MeshedObjectTypeSettings _gateZPlusItemObjSettings = new MeshedObjectTypeSettings(GameLoader.NAMESPACE + ".GateZPlusAnimated", GameLoader.MESH_FOLDER_PANDA + "/gatez+.obj", GameLoader.NAMESPACE + ".Gate");

        private static Dictionary<Players.Player, Dictionary<Vector3Int, bool>> _gatePositions = new Dictionary<Players.Player, Dictionary<Vector3Int, bool>>();

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterAddingBaseTypes, GameLoader.NAMESPACE + ".Items.Machines.GateLever.RegisterMachines")]
        public static void RegisterMachines(Dictionary<string, ItemTypesServer.ItemTypeRaw> items)
        {
            MachineManager.RegisterMachineType(nameof(GateLever), new MachineManager.MachineSettings(Item.ItemIndex, Repair, MachineManager.Refuel, Reload, DoWork, 10, 4, 5, 4));
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterSelectedWorld, GameLoader.NAMESPACE + ".Items.Machines.GateLever.RegisterAudio"),
            ModLoader.ModCallbackProvidesFor("pipliz.server.loadaudiofiles"), ModLoader.ModCallbackDependsOn("pipliz.server.registeraudiofiles")]
        public static void RegisterAudio()
        {
            GameLoader.AddSoundFile(GameLoader.NAMESPACE + "GateLeverMachineAudio", new List<string>() { GameLoader.AUDIO_FOLDER_PANDA + "/CastleDoorOpen.ogg" });

            GameLoader.AddSoundFile(GameLoader.NAMESPACE + "Metal", new List<string>()
            {
                GameLoader.AUDIO_FOLDER_PANDA + "/Metal.ogg"
            });

            GameLoader.AddSoundFile(GameLoader.NAMESPACE + "MetalRemove", new List<string>()
            {
                GameLoader.AUDIO_FOLDER_PANDA + "/MetalRemove.ogg"
            });
        }

        public static ushort Repair(Players.Player player, MachineState machineState)
        {
            var retval = GameLoader.Repairing_Icon;
            var ps = PlayerState.GetPlayerState(player);

            if (machineState.Durability < .75f)
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
                    machineState.Durability = MachineState.MAX_DURABILITY[player];
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
                machineState.Load > 0 &&
                machineState.NextTimeForWork < Pipliz.Time.SecondsSinceStartDouble)
            {
                if (!machineState.TempValues.Contains(DoorOpen))
                    machineState.TempValues.Set(DoorOpen, false);

                bool soundPlayed = false;

                if (!_gatePositions.ContainsKey(player))
                    _gatePositions.Add(player, new Dictionary<Vector3Int, bool>());

                Dictionary<Vector3Int, Vector3Int> moveGates = new Dictionary<Vector3Int, Vector3Int>();

                foreach (var gate in _gatePositions[player])
                {
                    if (TimeCycle.IsDay && gate.Value)
                        continue;

                    if (!TimeCycle.IsDay && !gate.Value)
                        continue;

                    float dis = Vector3.Distance(machineState.Position.Vector, gate.Key.Vector);

                    if (dis <= 21)
                    {
                        if (!soundPlayed)
                        {
                            soundPlayed = true;
                            ServerManager.SendAudio(machineState.Position.Vector, GameLoader.NAMESPACE + "GateLeverMachineAudio");
                        }

                        int offset = 2;

                        if (!TimeCycle.IsDay)
                            offset = -2;

                        moveGates.Add(gate.Key, gate.Key.Add(0, offset, 0));
                    }
                }

                foreach (var mkvp in moveGates)
                {
                        if (_gatePositions[player].ContainsKey(mkvp.Key))
                            _gatePositions[player].Remove(mkvp.Key);

                        _gatePositions[player].Add(mkvp.Value, TimeCycle.IsDay);

                        ServerManager.TryChangeBlock(mkvp.Key, BuiltinBlocks.Air);

                        int newOffset = -1;

                        if (!TimeCycle.IsDay)
                            newOffset = 1;

                        ClientMeshedObject.SendMoveOnceInterpolatedPosition(mkvp.Key.Vector, mkvp.Value.Add(0, newOffset, 0).Vector, 4.5f, _gateXMinusItemObjSettings);

                        if (World.TryGetTypeAt(mkvp.Value, out var actualType) && actualType == BuiltinBlocks.Air)
                        {
                            (new Thread(() =>
                            {
                                Thread.Sleep(9290);

                                if (World.TryGetTypeAt(mkvp.Value, out actualType) && actualType == BuiltinBlocks.Air)
                                    ServerManager.TryChangeBlock(mkvp.Value, GateItem.ItemIndex);

                            })).Start();
                        }
                    
                }

                if (moveGates.Count > 0)
                {
                    machineState.Durability -= 0.01f;
                    machineState.Fuel -= 0.03f;

                    if (machineState.Durability < 0)
                        machineState.Durability = 0;

                    if (machineState.Fuel <= 0)
                        machineState.Fuel = 0;
                }
                
                machineState.NextTimeForWork = machineState.MachineSettings.WorkTime + Pipliz.Time.SecondsSinceStartDouble;
            }
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterItemTypesDefined, GameLoader.NAMESPACE + ".Items.Machines.GateLever.RegisterGateLever")]
        public static void RegisterGateLever()
        {
            var rivets = new InventoryItem(BuiltinBlocks.IronRivet, 6);
            var iron = new InventoryItem(BuiltinBlocks.IronWrought, 2);
            var copperParts = new InventoryItem(BuiltinBlocks.CopperParts, 6);
            var copperNails = new InventoryItem(BuiltinBlocks.CopperNails, 6);
            var tools = new InventoryItem(BuiltinBlocks.CopperTools, 1);
            var planks = new InventoryItem(BuiltinBlocks.Planks, 4);

            var recipe = new Recipe(Item.name,
                                    new List<InventoryItem>() { planks, iron, rivets, copperParts, copperNails, tools, planks },
                                    new InventoryItem(Item.ItemIndex),
                                    5);

            RecipeStorage.AddOptionalLimitTypeRecipe(Jobs.AdvancedCrafterRegister.JOB_NAME, recipe);

            var gate = new Recipe(GateItem.name,
                                    new List<InventoryItem>() { iron, rivets, tools },
                                    new InventoryItem(GateItem.ItemIndex),
                                    5);

            RecipeStorage.AddOptionalLimitTypeRecipe(Jobs.AdvancedCrafterRegister.JOB_NAME, gate);
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterSelectedWorld, GameLoader.NAMESPACE + ".Items.Machines.GateLever.AddTextures"), ModLoader.ModCallbackProvidesFor("pipliz.server.registertexturemappingtextures")]
        public static void AddTextures()
        {
            var GateLeverTextureMapping = new ItemTypesServer.TextureMapping(new JSONNode());
            GateLeverTextureMapping.AlbedoPath = GameLoader.TEXTURE_FOLDER_PANDA + "/albedo/Lever.png";

            ItemTypesServer.SetTextureMapping(GameLoader.NAMESPACE + ".GateLever", GateLeverTextureMapping);

            var GateTextureMapping = new ItemTypesServer.TextureMapping(new JSONNode());
            GateTextureMapping.AlbedoPath = GameLoader.TEXTURE_FOLDER_PANDA + "/albedo/gate.png";

            ItemTypesServer.SetTextureMapping(GameLoader.NAMESPACE + ".Gate", GateTextureMapping);
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterAddingBaseTypes, GameLoader.NAMESPACE + ".Items.Machines.GateLever.AddGateLever"), ModLoader.ModCallbackDependsOn("pipliz.blocknpcs.addlittypes")]
        public static void AddGateLever(Dictionary<string, ItemTypesServer.ItemTypeRaw> items)
        {
            var GateLeverName = GameLoader.NAMESPACE + ".GateLever";
            var GateLeverNode = new JSONNode();
            GateLeverNode["icon"] = new JSONNode(GameLoader.ICON_FOLDER_PANDA + "/GateLever.png");
            GateLeverNode["isPlaceable"] = new JSONNode(true);
            GateLeverNode.SetAs("onRemoveAmount", 1);
            GateLeverNode.SetAs("onPlaceAudio", "stonePlace");
            GateLeverNode.SetAs("onRemoveAudio", "stoneDelete");
            GateLeverNode.SetAs("isSolid", true);
            GateLeverNode.SetAs("sideall", GameLoader.NAMESPACE + ".GateLever");
            GateLeverNode.SetAs("mesh", GameLoader.MESH_FOLDER_PANDA + "/Lever.obj");

            Item = new ItemTypesServer.ItemTypeRaw(GateLeverName, GateLeverNode);
            items.Add(GateLeverName, Item);

            var GateName = GameLoader.NAMESPACE + ".Gate";
            var GateNode = new JSONNode();
            GateNode["icon"] = new JSONNode(GameLoader.ICON_FOLDER_PANDA + "/Gate.png");
            GateNode["isPlaceable"] = new JSONNode(true);
            GateNode.SetAs("onRemoveAmount", 1);
            GateNode.SetAs("onPlaceAudio", GameLoader.NAMESPACE + "Metal");
            GateNode.SetAs("onRemoveAudio", GameLoader.NAMESPACE + "MetalRemove");
            GateNode.SetAs("isSolid", true);
            GateNode.SetAs("sideall", GameLoader.NAMESPACE + ".Gate");

            GateItem = new ItemTypesServer.ItemTypeRaw(GateName, GateNode);
            items.Add(GateName, GateItem);

            var GateXminusName = GateName + "x-";
            var GateXminuNode = new JSONNode();
            GateXminuNode.SetAs("needsBase", true);
            GateXminuNode.SetAs("parentType", GateName);
            GateXminuNode.SetAs("mesh", GameLoader.MESH_FOLDER_PANDA + "/gatex-.obj");
            items.Add(GateXminusName, new ItemTypesServer.ItemTypeRaw(GateXminusName, GateXminuNode));

            var GateXplusName = GateName + "x+";
            var GateXplusNode = new JSONNode();
            GateXplusNode.SetAs("needsBase", true);
            GateXplusNode.SetAs("parentType", GateName);
            GateXplusNode.SetAs("mesh", GameLoader.MESH_FOLDER_PANDA + "/gatex+.obj");
            items.Add(GateXplusName, new ItemTypesServer.ItemTypeRaw(GateXplusName, GateXplusNode));

            var GateZminusName = GateName + "z-";
            var GateZminuNode = new JSONNode();
            GateZminuNode.SetAs("needsBase", true);
            GateZminuNode.SetAs("parentType", GateName);
            GateZminuNode.SetAs("mesh", GameLoader.MESH_FOLDER_PANDA + "/gatez-.obj");
            items.Add(GateZminusName, new ItemTypesServer.ItemTypeRaw(GateZminusName, GateZminuNode));

            var GateZplusName = GateName + "z+";
            var GateZplusNode = new JSONNode();
            GateXminuNode.SetAs("needsBase", true);
            GateXminuNode.SetAs("parentType", GateName);
            GateXminuNode.SetAs("mesh", GameLoader.MESH_FOLDER_PANDA + "/gatez+.obj");
            items.Add(GateZplusName, new ItemTypesServer.ItemTypeRaw(GateZplusName, GateZplusNode));

            MeshedObjectType.Register(_gateXMinusItemObjSettings);
            MeshedObjectType.Register(_gateXPlusItemObjSettings);
            MeshedObjectType.Register(_gateZMinusItemObjSettings);
            MeshedObjectType.Register(_gateZPlusItemObjSettings);
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnSavingPlayer, GameLoader.NAMESPACE + ".Items.Machines.GateLever.OnSavingPlayer")]
        public static void OnSavingPlayer(JSONNode n, Players.Player p)
        {
            if (_gatePositions.ContainsKey(p))
            {
                if (n.HasChild(GameLoader.NAMESPACE + ".Gates"))
                    n.RemoveChild(GameLoader.NAMESPACE + ".Gates");

                var gateNode = new JSONNode(NodeType.Array);

                foreach (var pos in _gatePositions[p])
                {
                    var node = new JSONNode()
                        .SetAs("pos", (JSONNode)pos.Key)
                        .SetAs("open", pos.Value);
                    gateNode.AddToArray(node);
                }

                n[GameLoader.NAMESPACE + ".Gates"] = gateNode;
            }
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnLoadingPlayer, GameLoader.NAMESPACE + ".Items.Machines.GateLever.OnLoadingPlayer")]
        public static void OnLoadingPlayer(JSONNode n, Players.Player p)
        {
            if (n.TryGetChild(GameLoader.NAMESPACE + ".Gates", out var gateNodes))
            {
                if (!_gatePositions.ContainsKey(p))
                    _gatePositions.Add(p, new Dictionary<Vector3Int, bool>());

                foreach (var gateNode in gateNodes.LoopArray())
                    _gatePositions[p].Add((Vector3Int)gateNode.GetAs<JSONNode>("pos"), gateNode.GetAs<bool>("open"));
            }
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnTryChangeBlockUser, GameLoader.NAMESPACE + ".Items.Machines.GateLever.OnTryChangeBlockUser")]
        public static bool OnTryChangeBlockUser(ModLoader.OnTryChangeBlockUserData d)
        {
            if (d.TypeNew == Item.ItemIndex && d.typeTillNow == BuiltinBlocks.Air)
            {
                MachineManager.RegisterMachineState(d.requestedBy, new MachineState(d.VoxelToChange, d.requestedBy, nameof(GateLever)));
            }
            else if (d.TypeNew == GateItem.ItemIndex && d.typeTillNow == BuiltinBlocks.Air)
            {
                if (!_gatePositions.ContainsKey(d.requestedBy))
                    _gatePositions.Add(d.requestedBy, new Dictionary<Vector3Int, bool>());

                _gatePositions[d.requestedBy].Add(d.VoxelToChange, false);
            }

            if (d.TypeNew == BuiltinBlocks.Air)
            {
                if (!_gatePositions.ContainsKey(d.requestedBy))
                    _gatePositions.Add(d.requestedBy, new Dictionary<Vector3Int, bool>());

                if (_gatePositions[d.requestedBy].ContainsKey(d.VoxelToChange))
                    _gatePositions[d.requestedBy].Remove(d.VoxelToChange);
            }

            return true;
        }
    }
}
