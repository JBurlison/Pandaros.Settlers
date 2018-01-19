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
        public enum GatePosition
        {
            Open,
            Closed,
            MovingOpen,
            MovingClosed,
        }

        public class GateState
        {
            public GatePosition State { get; set; }
            public VoxelSide Orientation { get; set; }
            public Vector3Int Position { get; set; }

            public GateState(GatePosition state, VoxelSide rotation, Vector3Int pos)
            {
                State = state;
                Orientation = rotation;
                Position = pos;
            }

            public GateState(JSONNode node)
            {
                State = (GatePosition)Enum.Parse(typeof(GatePosition), node.GetAs<string>(nameof(State)));
                Orientation = (VoxelSide)Enum.Parse(typeof(VoxelSide), node.GetAs<string>(nameof(Orientation)));
                Position = (Vector3Int)node.GetAs<JSONNode>(nameof(Position));
            }

            public virtual JSONNode ToJsonNode()
            {
                var baseNode = new JSONNode();

                baseNode.SetAs(nameof(State), State.ToString());
                baseNode.SetAs(nameof(Orientation), Orientation.ToString());
                baseNode.SetAs(nameof(Position), (JSONNode)Position);

                return baseNode;
            }
        }

        const double GateLeverCooldown = 4;
        private const string DoorOpen = "DoorOpen";
        private const float TravelTime = 4.0f;

        public static ItemTypesServer.ItemTypeRaw Item { get; private set; }
        public static ItemTypesServer.ItemTypeRaw GateItem { get; private set; }
        public static ItemTypesServer.ItemTypeRaw GateItemXP { get; private set; }
        public static ItemTypesServer.ItemTypeRaw GateItemXN { get; private set; }
        public static ItemTypesServer.ItemTypeRaw GateItemZP { get; private set; }
        public static ItemTypesServer.ItemTypeRaw GateItemZN { get; private set; }

        static MeshedObjectTypeSettings _gateXMinusItemObjSettings = new MeshedObjectTypeSettings(GameLoader.NAMESPACE + ".GateXMinusAnimated", GameLoader.MESH_FOLDER_PANDA + "/gatex-.obj", GameLoader.NAMESPACE + ".Gate");
        static MeshedObjectTypeSettings _gateXPlusItemObjSettings = new MeshedObjectTypeSettings(GameLoader.NAMESPACE + ".GateXPlusAnimated", GameLoader.MESH_FOLDER_PANDA + "/gatex+.obj", GameLoader.NAMESPACE + ".Gate");
        static MeshedObjectTypeSettings _gateZMinusItemObjSettings = new MeshedObjectTypeSettings(GameLoader.NAMESPACE + ".GateZMinusAnimated", GameLoader.MESH_FOLDER_PANDA + "/gatez-.obj", GameLoader.NAMESPACE + ".Gate");
        static MeshedObjectTypeSettings _gateZPlusItemObjSettings = new MeshedObjectTypeSettings(GameLoader.NAMESPACE + ".GateZPlusAnimated", GameLoader.MESH_FOLDER_PANDA + "/gatez+.obj", GameLoader.NAMESPACE + ".Gate");

        private static Dictionary<Players.Player, Dictionary<Vector3Int, GateState>> _gatePositions = new Dictionary<Players.Player, Dictionary<Vector3Int, GateState>>();

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
                
                if (!_gatePositions.ContainsKey(player))
                    _gatePositions.Add(player, new Dictionary<Vector3Int, GateState>());

                Dictionary<GateState, Vector3Int> moveGates = new Dictionary<GateState, Vector3Int>();

                foreach (var gate in _gatePositions[player])
                {
                    if (gate.Value.State == GatePosition.MovingClosed || gate.Value.State == GatePosition.MovingOpen)
                        continue;

                    if (World.TryGetTypeAt(gate.Key, out var gateType))
                    {
                        if (gate.Value.Orientation == VoxelSide.None)
                        {
                            if (gateType == GateItemXN.ItemIndex)
                                gate.Value.Orientation = VoxelSide.xMin;
                            else if (gateType == GateItemXP.ItemIndex)
                                gate.Value.Orientation = VoxelSide.xPlus;
                            else if (gateType == GateItemZN.ItemIndex)
                                gate.Value.Orientation = VoxelSide.zMin;
                            else if (gateType == GateItemZP.ItemIndex)
                                gate.Value.Orientation = VoxelSide.zPlus;
                        }

                        if ((gateType != GateItem.ItemIndex &&
                            gateType != GateItemXN.ItemIndex &&
                            gateType != GateItemXP.ItemIndex &&
                            gateType != GateItemZN.ItemIndex &&
                            gateType != GateItemZP.ItemIndex))
                        {
                            switch (gate.Value.Orientation)
                            {
                                case VoxelSide.xMin:
                                    ServerManager.TryChangeBlock(gate.Key, GateItemXN.ItemIndex);
                                    break;
                                case VoxelSide.xPlus:
                                    ServerManager.TryChangeBlock(gate.Key, GateItemXP.ItemIndex);
                                    break;
                                case VoxelSide.zMin:
                                    ServerManager.TryChangeBlock(gate.Key, GateItemZN.ItemIndex);
                                    break;
                                case VoxelSide.zPlus:
                                    ServerManager.TryChangeBlock(gate.Key, GateItemZP.ItemIndex);
                                    break;

                                default:
                                    ServerManager.TryChangeBlock(gate.Key, GateItemXN.ItemIndex);
                                    break;
                            }
                        }
                    }

                    if (TimeCycle.IsDay && gate.Value.State == GatePosition.Open)
                        continue;

                    if (!TimeCycle.IsDay && gate.Value.State == GatePosition.Closed)
                        continue;

                    float dis = Vector3.Distance(machineState.Position.Vector, gate.Key.Vector);

                    if (dis <= 21)
                    {
                        int offset = 2;

                        if (!TimeCycle.IsDay)
                            offset = -2;

                        moveGates.Add(gate.Value, gate.Key.Add(0, offset, 0));
                    }
                }

                foreach (var mkvp in moveGates)
                {
                    if (_gatePositions[player].ContainsKey(mkvp.Key.Position))
                        _gatePositions[player].Remove(mkvp.Key.Position);
                }

                foreach (var mkvp in moveGates)
                {
                    _gatePositions[player].Add(mkvp.Value, mkvp.Key);

                    ServerManager.TryChangeBlock(mkvp.Key.Position, BuiltinBlocks.Air);

                    int newOffset = -1;

                    if (!TimeCycle.IsDay)
                        newOffset = 1;

                    switch(mkvp.Key.Orientation)
                    {
                        case VoxelSide.xMin:
                            ClientMeshedObject.SendMoveOnceInterpolatedPosition(mkvp.Key.Position.Vector, mkvp.Value.Add(0, newOffset, 0).Vector, TravelTime, _gateXMinusItemObjSettings);
                            break;
                        case VoxelSide.xPlus:
                            ClientMeshedObject.SendMoveOnceInterpolatedPosition(mkvp.Key.Position.Vector, mkvp.Value.Add(0, newOffset, 0).Vector, TravelTime, _gateXPlusItemObjSettings);
                            break;
                        case VoxelSide.zMin:
                            ClientMeshedObject.SendMoveOnceInterpolatedPosition(mkvp.Key.Position.Vector, mkvp.Value.Add(0, newOffset, 0).Vector, TravelTime, _gateZMinusItemObjSettings);
                            break;
                        case VoxelSide.zPlus:
                            ClientMeshedObject.SendMoveOnceInterpolatedPosition(mkvp.Key.Position.Vector, mkvp.Value.Add(0, newOffset, 0).Vector, TravelTime, _gateZPlusItemObjSettings);
                            break;

                        default:
                            ClientMeshedObject.SendMoveOnceInterpolatedPosition(mkvp.Key.Position.Vector, mkvp.Value.Add(0, newOffset, 0).Vector, TravelTime, _gateXMinusItemObjSettings);
                            break;
                    }

                    var moveState = GatePosition.MovingClosed;

                    if (TimeCycle.IsDay)
                        moveState = GatePosition.MovingOpen;

                    mkvp.Key.State = moveState;
                    mkvp.Key.Position = mkvp.Value;

                    var thread = new Thread(() =>
                    {
                        Thread.Sleep(8000);

                        var state = GatePosition.Closed;

                        if (TimeCycle.IsDay)
                            state = GatePosition.Open;

                        mkvp.Key.State = state;

                        switch (mkvp.Key.Orientation)
                        {
                            case VoxelSide.xMin:
                                ServerManager.TryChangeBlock(mkvp.Value, GateItemXN.ItemIndex);
                                break;
                            case VoxelSide.xPlus:
                                ServerManager.TryChangeBlock(mkvp.Value, GateItemXP.ItemIndex);
                                break;
                            case VoxelSide.zMin:
                                ServerManager.TryChangeBlock(mkvp.Value, GateItemZN.ItemIndex);
                                break;
                            case VoxelSide.zPlus:
                                ServerManager.TryChangeBlock(mkvp.Value, GateItemZP.ItemIndex);
                                break;

                            default:
                                ServerManager.TryChangeBlock(mkvp.Value, GateItemXN.ItemIndex);
                                break;
                        }
                    });

                    thread.IsBackground = true;
                    thread.Start();
                }

                if (moveGates.Count > 0)
                {
                    ServerManager.SendAudio(machineState.Position.Vector, GameLoader.NAMESPACE + "GateLeverMachineAudio");

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
                                    24);

            ItemTypesServer.LoadSortOrder(Item.name, ItemTypesServer.ORDER_JOBBLOCK);
            ItemTypesServer.LoadSortOrder(GateItem.name, ItemTypesServer.ORDER_JOBBLOCK);
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
            var GateXplusName = GateName + "x+";
            var GateXminusName = GateName + "x-";
            var GateZminusName = GateName + "z-";
            var GateZplusName = GateName + "z+";
            var GateNode = new JSONNode();
            GateNode["icon"] = new JSONNode(GameLoader.ICON_FOLDER_PANDA + "/Gate.png");
            GateNode["isPlaceable"] = new JSONNode(true);
            GateNode.SetAs("onRemoveAmount", 0);
            GateNode.SetAs("onPlaceAudio", GameLoader.NAMESPACE + "Metal");
            GateNode.SetAs("onRemoveAudio", GameLoader.NAMESPACE + "MetalRemove");
            GateNode.SetAs("isSolid", true);
            GateNode.SetAs("sideall", GameLoader.NAMESPACE + ".Gate");
            GateNode.SetAs<bool>("isRotatable", true)
                    .SetAs<string>("rotatablex+", GateXplusName)
                    .SetAs<string>("rotatablex-", GateXminusName)
                    .SetAs<string>("rotatablez+", GateZminusName)
                    .SetAs<string>("rotatablez-", GateZplusName);

            GateItem = new ItemTypesServer.ItemTypeRaw(GateName, GateNode);
            items.Add(GateName, GateItem);

            
            var GateXminuNode = new JSONNode();
            GateXminuNode.SetAs("needsBase", true);
            GateXminuNode.SetAs("parentType", GateName);
            GateXminuNode.SetAs("mesh", GameLoader.MESH_FOLDER_PANDA + "/gatex-.obj");
            GateItemXN = new ItemTypesServer.ItemTypeRaw(GateXminusName, GateXminuNode);
            items.Add(GateXminusName, GateItemXN);

            
            var GateXplusNode = new JSONNode();
            GateXplusNode.SetAs("needsBase", true);
            GateXplusNode.SetAs("parentType", GateName);
            GateXplusNode.SetAs("mesh", GameLoader.MESH_FOLDER_PANDA + "/gatex+.obj");
            GateItemXP = new ItemTypesServer.ItemTypeRaw(GateXplusName, GateXplusNode);
            items.Add(GateXplusName, GateItemXP);

            
            var GateZminuNode = new JSONNode();
            GateZminuNode.SetAs("needsBase", true);
            GateZminuNode.SetAs("parentType", GateName);
            GateZminuNode.SetAs("mesh", GameLoader.MESH_FOLDER_PANDA + "/gatez-.obj");
            GateItemZN = new ItemTypesServer.ItemTypeRaw(GateZminusName, GateZminuNode);
            items.Add(GateZminusName, GateItemZN);

            
            var GateZplusNode = new JSONNode();
            GateZplusNode.SetAs("needsBase", true);
            GateZplusNode.SetAs("parentType", GateName);
            GateZplusNode.SetAs("mesh", GameLoader.MESH_FOLDER_PANDA + "/gatez+.obj");
            GateItemZP = new ItemTypesServer.ItemTypeRaw(GateZplusName, GateZplusNode);
            items.Add(GateZplusName, GateItemZP);

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
                    if (pos.Value.State == GatePosition.MovingOpen)
                        pos.Value.State = GatePosition.Open;

                    if (pos.Value.State == GatePosition.MovingClosed)
                        pos.Value.State = GatePosition.Closed;

                    var node = new JSONNode()
                        .SetAs("pos", (JSONNode)pos.Key)
                        .SetAs("state", pos.Value.ToJsonNode());
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
                    _gatePositions.Add(p, new Dictionary<Vector3Int, GateState>());

                foreach (var gateNode in gateNodes.LoopArray())
                    _gatePositions[p].Add((Vector3Int)gateNode.GetAs<JSONNode>("pos"), new GateState(gateNode.GetAs<JSONNode>("state")));
            }
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnTryChangeBlockUser, GameLoader.NAMESPACE + ".Items.Machines.GateLever.OnTryChangeBlockUser")]
        public static bool OnTryChangeBlockUser(ModLoader.OnTryChangeBlockUserData d)
        {
            if (d.TypeNew == Item.ItemIndex && d.typeTillNow == BuiltinBlocks.Air)
            {
                MachineManager.RegisterMachineState(d.requestedBy, new MachineState(d.VoxelToChange, d.requestedBy, nameof(GateLever)));
            }
            else if (d.typeTillNow == BuiltinBlocks.Air && (d.TypeNew == GateItem.ItemIndex ||
                                                            d.TypeNew == GateItemXN.ItemIndex ||
                                                            d.TypeNew == GateItemXP.ItemIndex ||
                                                            d.TypeNew == GateItemZN.ItemIndex ||
                                                            d.TypeNew == GateItemZP.ItemIndex ))
            {
                if (!_gatePositions.ContainsKey(d.requestedBy))
                    _gatePositions.Add(d.requestedBy, new Dictionary<Vector3Int, GateState>());
                
                _gatePositions[d.requestedBy].Add(d.VoxelToChange, new GateState(GatePosition.Closed, VoxelSide.None, d.VoxelToChange));
            }

            if (d.TypeNew == BuiltinBlocks.Air)
            {
                if (!_gatePositions.ContainsKey(d.requestedBy))
                    _gatePositions.Add(d.requestedBy, new Dictionary<Vector3Int, GateState>());

                if (_gatePositions[d.requestedBy].ContainsKey(d.VoxelToChange))
                {
                    _gatePositions[d.requestedBy].Remove(d.VoxelToChange);

                    if (!Inventory.GetInventory(d.requestedBy).TryAdd(GateItem.ItemIndex))
                    {
                        Stockpile.GetStockPile(d.requestedBy).Add(GateItem.ItemIndex);
                    }
                }
            }

            return true;
        }
    }
}
