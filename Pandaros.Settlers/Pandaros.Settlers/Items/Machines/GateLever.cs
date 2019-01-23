using BlockTypes;
using MeshedObjects;
using Pandaros.Settlers.Entities;
using Pandaros.Settlers.Jobs;
using Pandaros.Settlers.Jobs.Roaming;
using Pandaros.Settlers.Managers;
using Pipliz;
using Pipliz.JSON;
using Recipes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Time = Pipliz.Time;

namespace Pandaros.Settlers.Items.Machines
{
    public class GateLeverRegister : IRoamingJobObjective
    {
        public string name => nameof(GateLever);
        public float WorkTime => 4;
        public ushort ItemIndex => GateLever.Item.ItemIndex;
        public Dictionary<string, IRoamingJobObjectiveAction> ActionCallbacks { get; } = new Dictionary<string, IRoamingJobObjectiveAction>()
        {
            { MachineConstants.REFUEL, new RefuelMachineAction() },
            { MachineConstants.REPAIR, new RepairGateLever() },
            { MachineConstants.RELOAD, new ReloadGateLever() }
        };

        public string ObjectiveCategory => MachineConstants.MECHANICAL;

        public void DoWork(Colony colony, RoamingJobState state)
        {
            GateLever.DoWork(colony, state);
        }
    }

    public class RepairGateLever : IRoamingJobObjectiveAction
    {
        public string name => MachineConstants.REPAIR;

        public float TimeToPreformAction => 10;

        public string AudoKey => GameLoader.NAMESPACE + ".HammerAudio";

        public ushort ObjectiveLoadEmptyIcon => GameLoader.Repairing_Icon;

        public ushort PreformAction(Colony colony, RoamingJobState state)
        {
            return GateLever.Repair(colony, state);
        }
    }

    public class ReloadGateLever : IRoamingJobObjectiveAction
    {
        public string name => MachineConstants.RELOAD;

        public float TimeToPreformAction => 5;

        public string AudoKey => GameLoader.NAMESPACE + ".ReloadingAudio";

        public ushort ObjectiveLoadEmptyIcon => GameLoader.Reload_Icon;

        public ushort PreformAction(Colony colony, RoamingJobState state)
        {
            return GateLever.Reload(colony, state);
        }
    }


    [ModLoader.ModManager]
    public static class GateLever
    {
        public enum GatePosition
        {
            Open,
            Closed,
            MovingOpen,
            MovingClosed
        }

        private const double GateLeverCooldown = 4;
        private const string DoorOpen = "DoorOpen";
        private const float TravelTime = 4.0f;

        private static readonly MeshedObjectTypeSettings _gateXMinusItemObjSettings =
            new MeshedObjectTypeSettings(GameLoader.NAMESPACE + ".GateXMinusAnimated",
                                         GameLoader.MESH_PATH + "gatex-.obj", GameLoader.NAMESPACE + ".Gate");

        private static readonly MeshedObjectTypeSettings _gateXPlusItemObjSettings =
            new MeshedObjectTypeSettings(GameLoader.NAMESPACE + ".GateXPlusAnimated",
                                         GameLoader.MESH_PATH + "gatex+.obj", GameLoader.NAMESPACE + ".Gate");

        private static readonly MeshedObjectTypeSettings _gateZMinusItemObjSettings =
            new MeshedObjectTypeSettings(GameLoader.NAMESPACE + ".GateZMinusAnimated",
                                         GameLoader.MESH_PATH + "gatez-.obj", GameLoader.NAMESPACE + ".Gate");

        private static readonly MeshedObjectTypeSettings _gateZPlusItemObjSettings =
            new MeshedObjectTypeSettings(GameLoader.NAMESPACE + ".GateZPlusAnimated",
                                         GameLoader.MESH_PATH + "gatez+.obj", GameLoader.NAMESPACE + ".Gate");

        private static readonly Dictionary<Colony, Dictionary<Vector3Int, GateState>> _gatePositions = new Dictionary<Colony, Dictionary<Vector3Int, GateState>>();

        public static ItemTypesServer.ItemTypeRaw Item { get; private set; }
        public static ItemTypesServer.ItemTypeRaw GateItem { get; private set; }
        public static ItemTypesServer.ItemTypeRaw GateItemXP { get; private set; }
        public static ItemTypesServer.ItemTypeRaw GateItemXN { get; private set; }
        public static ItemTypesServer.ItemTypeRaw GateItemZP { get; private set; }
        public static ItemTypesServer.ItemTypeRaw GateItemZN { get; private set; }


        public static ushort Repair(Colony colony, RoamingJobState machineState)
        {
            var retval = GameLoader.Repairing_Icon;

            if (colony.OwnerIsOnline() && machineState.GetActionEnergy(MachineConstants.REPAIR) < .75f)
            {
                var repaired       = false;
                var requiredForFix = new List<InventoryItem>();

                requiredForFix.Add(new InventoryItem(BuiltinBlocks.CopperTools, 1));
                requiredForFix.Add(new InventoryItem(BuiltinBlocks.CopperParts, 1));

                if (machineState.GetActionEnergy(MachineConstants.REPAIR) < .10f)
                {
                    requiredForFix.Add(new InventoryItem(BuiltinBlocks.CopperParts, 4));
                    requiredForFix.Add(new InventoryItem(BuiltinBlocks.Planks, 1));
                }
                else if (machineState.GetActionEnergy(MachineConstants.REPAIR) < .30f)
                {
                    requiredForFix.Add(new InventoryItem(BuiltinBlocks.CopperParts, 3));
                }
                else if (machineState.GetActionEnergy(MachineConstants.REPAIR) < .50f)
                {
                    requiredForFix.Add(new InventoryItem(BuiltinBlocks.CopperParts, 2));
                }

                if (colony.Stockpile.Contains(requiredForFix))
                {
                    colony.Stockpile.TryRemove(requiredForFix);
                    repaired = true;
                }
                else
                {
                    foreach (var item in requiredForFix)
                        if (!colony.Stockpile.Contains(item))
                        {
                            retval = item.Type;
                            break;
                        }
                }

                if (repaired)
                    machineState.ResetActionToMaxLoad(MachineConstants.REPAIR);
            }
            

            return retval;
        }

        public static ushort Reload(Colony colony, RoamingJobState machineState)
        {
            if (colony.OwnerIsOnline())
                machineState.ResetActionToMaxLoad(MachineConstants.RELOAD);

            return GameLoader.Reload_Icon;
        }

        public static void DoWork(Colony colony, RoamingJobState machineState)
        {
            if (!colony.OwnerIsOnline() && Configuration.OfflineColonies || colony.OwnerIsOnline())
                if (machineState.GetActionEnergy(MachineConstants.REPAIR) > 0 &&
                    machineState.GetActionEnergy(MachineConstants.RELOAD) > 0 &&
                    machineState.GetActionEnergy(MachineConstants.REFUEL) > 0 &&
                    machineState.NextTimeForWork < Time.SecondsSinceStartDouble)
                {
                    if (!machineState.TempValues.Contains(DoorOpen))
                        machineState.TempValues.Set(DoorOpen, false);

                    if (!_gatePositions.ContainsKey(colony))
                        _gatePositions.Add(colony, new Dictionary<Vector3Int, GateState>());

                    var moveGates = new Dictionary<GateState, Vector3Int>();
                    bool bossesEnabled = ColonyState.GetColonyState(colony).BossesEnabled;

                    foreach (var gate in _gatePositions[colony])
                    {
                        if (gate.Value.State == GatePosition.MovingClosed ||
                            gate.Value.State == GatePosition.MovingOpen)
                            continue;

                        if (World.TryGetTypeAt(gate.Key, out ushort gateType))
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

                            if (gateType != GateItem.ItemIndex &&
                                gateType != GateItemXN.ItemIndex &&
                                gateType != GateItemXP.ItemIndex &&
                                gateType != GateItemZN.ItemIndex &&
                                gateType != GateItemZP.ItemIndex)
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

                        if (bossesEnabled)
                        {
                            if (TimeCycle.IsDay && !MonsterManager.BossActive && gate.Value.State == GatePosition.Open)
                                continue;
                        }
                        else if (TimeCycle.IsDay && gate.Value.State == GatePosition.Open)
                        {
                            continue;
                        }

                        if (bossesEnabled)
                        {
                            if ((!TimeCycle.IsDay || MonsterManager.BossActive) &&
                                gate.Value.State == GatePosition.Closed)
                                continue;
                        }
                        else if (!TimeCycle.IsDay && gate.Value.State == GatePosition.Closed)
                        {
                            continue;
                        }

                        var dis = UnityEngine.Vector3.Distance(machineState.Position.Vector, gate.Key.Vector);

                        if (dis <= 21)
                        {
                            var offset = 2;

                            if (bossesEnabled)
                            {
                                if (!TimeCycle.IsDay || MonsterManager.BossActive)
                                    offset = -2;
                            }
                            else if (!TimeCycle.IsDay)
                            {
                                offset = -2;
                            }

                            moveGates.Add(gate.Value, gate.Key.Add(0, offset, 0));
                        }
                    }

                    foreach (var mkvp in moveGates)
                        if (_gatePositions[colony].ContainsKey(mkvp.Key.Position))
                            _gatePositions[colony].Remove(mkvp.Key.Position);

                    foreach (var mkvp in moveGates)
                    {
                        _gatePositions[colony].Add(mkvp.Value, mkvp.Key);

                        ServerManager.TryChangeBlock(mkvp.Key.Position, BuiltinBlocks.Air);

                        var newOffset = -1;

                        if (bossesEnabled)
                        {
                            if (!TimeCycle.IsDay || MonsterManager.BossActive)
                                newOffset = 1;
                        }
                        else if (!TimeCycle.IsDay)
                        {
                            newOffset = 1;
                        }

                        switch (mkvp.Key.Orientation)
                        {
                            case VoxelSide.xMin:

                                ClientMeshedObject.SendMoveOnceInterpolatedPosition(mkvp.Key.Position.Vector,
                                                                                    mkvp.Value.Add(0, newOffset, 0)
                                                                                        .Vector, TravelTime,
                                                                                    _gateXMinusItemObjSettings);

                                break;
                            case VoxelSide.xPlus:

                                ClientMeshedObject.SendMoveOnceInterpolatedPosition(mkvp.Key.Position.Vector,
                                                                                    mkvp.Value.Add(0, newOffset, 0)
                                                                                        .Vector, TravelTime,
                                                                                    _gateXPlusItemObjSettings);

                                break;
                            case VoxelSide.zMin:

                                ClientMeshedObject.SendMoveOnceInterpolatedPosition(mkvp.Key.Position.Vector,
                                                                                    mkvp.Value.Add(0, newOffset, 0)
                                                                                        .Vector, TravelTime,
                                                                                    _gateZMinusItemObjSettings);

                                break;
                            case VoxelSide.zPlus:

                                ClientMeshedObject.SendMoveOnceInterpolatedPosition(mkvp.Key.Position.Vector,
                                                                                    mkvp.Value.Add(0, newOffset, 0)
                                                                                        .Vector, TravelTime,
                                                                                    _gateZPlusItemObjSettings);

                                break;

                            default:

                                ClientMeshedObject.SendMoveOnceInterpolatedPosition(mkvp.Key.Position.Vector,
                                                                                    mkvp.Value.Add(0, newOffset, 0)
                                                                                        .Vector, TravelTime,
                                                                                    _gateXMinusItemObjSettings);

                                break;
                        }

                        var moveState = GatePosition.MovingClosed;

                        if (bossesEnabled)
                        {
                            if (TimeCycle.IsDay && !MonsterManager.BossActive)
                                moveState = GatePosition.MovingOpen;
                        }
                        else if (TimeCycle.IsDay)
                        {
                            moveState = GatePosition.MovingOpen;
                        }

                        mkvp.Key.State    = moveState;
                        mkvp.Key.Position = mkvp.Value;

                        var thread = new Thread(() =>
                        {
                            Thread.Sleep(8000);

                            var state = GatePosition.Closed;

                            if (bossesEnabled)
                            {
                                if (TimeCycle.IsDay && !MonsterManager.BossActive)
                                    state = GatePosition.Open;
                            }
                            else if (TimeCycle.IsDay)
                            {
                                state = GatePosition.Open;
                            }

                            mkvp.Key.State = state;

                            ThreadManager.InvokeOnMainThread(() =>
                            {
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
                        });

                        thread.IsBackground = true;
                        thread.Start();
                    }

                    if (moveGates.Count > 0)
                    {
                        ServerManager.SendAudio(machineState.Position.Vector,
                                                GameLoader.NAMESPACE + ".GateLeverMachineAudio");

                        machineState.SubtractFromActionEnergy(MachineConstants.REPAIR, 0.01f);
                        machineState.SubtractFromActionEnergy(MachineConstants.REFUEL, 0.03f);
                    }

                    machineState.NextTimeForWork = machineState.RoamingJobSettings.WorkTime + Time.SecondsSinceStartDouble;
                }
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterItemTypesDefined,
            GameLoader.NAMESPACE + ".Items.Machines.GateLever.RegisterGateLever")]
        public static void RegisterGateLever()
        {
            var rivets      = new InventoryItem(BuiltinBlocks.IronRivet, 6);
            var iron        = new InventoryItem(BuiltinBlocks.IronWrought, 2);
            var copperParts = new InventoryItem(BuiltinBlocks.CopperParts, 6);
            var copperNails = new InventoryItem(BuiltinBlocks.CopperNails, 6);
            var tools       = new InventoryItem(BuiltinBlocks.CopperTools, 1);
            var planks      = new InventoryItem(BuiltinBlocks.Planks, 4);

            var recipe = new Recipe(Item.name,
                                    new List<InventoryItem>
                                    {
                                        planks,
                                        iron,
                                        rivets,
                                        copperParts,
                                        copperNails,
                                        tools,
                                        planks
                                    },
                                    new ItemTypes.ItemTypeDrops(Item.ItemIndex),
                                    5);

            ServerManager.RecipeStorage.AddOptionalLimitTypeRecipe(AdvancedCrafterRegister.JOB_NAME, recipe);

            var gate = new Recipe(GateItem.name,
                                  new List<InventoryItem> {iron, rivets, tools},
                                  new ItemTypes.ItemTypeDrops(GateItem.ItemIndex),
                                  24);

            ServerManager.RecipeStorage.AddOptionalLimitTypeRecipe(AdvancedCrafterRegister.JOB_NAME, gate);
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterSelectedWorld, GameLoader.NAMESPACE + ".Items.Machines.GateLever.AddTextures")]
        [ModLoader.ModCallbackProvidesFor("pipliz.server.registertexturemappingtextures")]
        public static void AddTextures()
        {
            var GateLeverTextureMapping = new ItemTypesServer.TextureMapping(new JSONNode());
            GateLeverTextureMapping.AlbedoPath = GameLoader.BLOCKS_ALBEDO_PATH + "Lever.png";

            ItemTypesServer.SetTextureMapping(GameLoader.NAMESPACE + ".GateLever", GateLeverTextureMapping);

            var GateTextureMapping = new ItemTypesServer.TextureMapping(new JSONNode());
            GateTextureMapping.AlbedoPath = GameLoader.BLOCKS_ALBEDO_PATH + "gate.png";

            ItemTypesServer.SetTextureMapping(GameLoader.NAMESPACE + ".Gate", GateTextureMapping);
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AddItemTypes, GameLoader.NAMESPACE + ".Items.Machines.GateLever.AddGateLever")]
        [ModLoader.ModCallbackDependsOn("pipliz.blocknpcs.addlittypes")]
        public static void AddGateLever(Dictionary<string, ItemTypesServer.ItemTypeRaw> items)
        {
            var GateLeverName = GameLoader.NAMESPACE + ".GateLever";
            var GateLeverNode = new JSONNode();
            GateLeverNode["icon"]        = new JSONNode(GameLoader.ICON_PATH + "GateLever.png");
            GateLeverNode["isPlaceable"] = new JSONNode(true);
            GateLeverNode.SetAs("onRemoveAmount", 1);
            GateLeverNode.SetAs("onPlaceAudio", "stonePlace");
            GateLeverNode.SetAs("onRemoveAudio", "stoneDelete");
            GateLeverNode.SetAs("isSolid", true);
            GateLeverNode.SetAs("sideall", GameLoader.NAMESPACE + ".GateLever");
            GateLeverNode.SetAs("mesh", GameLoader.MESH_PATH + "Lever.obj");

            var categories = new JSONNode(NodeType.Array);
            categories.AddToArray(new JSONNode("machine"));
            categories.AddToArray(new JSONNode("gate"));
            GateLeverNode.SetAs("categories", categories);

            Item = new ItemTypesServer.ItemTypeRaw(GateLeverName, GateLeverNode);
            items.Add(GateLeverName, Item);


            var GateName       = GameLoader.NAMESPACE + ".Gate";
            var GateXplusName  = GateName + "x+";
            var GateXminusName = GateName + "x-";
            var GateZminusName = GateName + "z-";
            var GateZplusName  = GateName + "z+";
            var GateNode       = new JSONNode();
            GateNode["icon"]        = new JSONNode(GameLoader.ICON_PATH + "Gate.png");
            GateNode["isPlaceable"] = new JSONNode(true);
            GateNode.SetAs("onRemoveAmount", 0);
            GateNode.SetAs("onPlaceAudio", GameLoader.NAMESPACE + "Metal");
            GateNode.SetAs("onRemoveAudio", GameLoader.NAMESPACE + "MetalRemove");
            GateNode.SetAs("isSolid", true);
            GateNode.SetAs("sideall", GameLoader.NAMESPACE + ".Gate");

            GateNode.SetAs("isRotatable", true)
                    .SetAs("rotatablex+", GateXplusName)
                    .SetAs("rotatablex-", GateXminusName)
                    .SetAs("rotatablez+", GateZminusName)
                    .SetAs("rotatablez-", GateZplusName);

            var gateCategories = new JSONNode(NodeType.Array);
            gateCategories.AddToArray(new JSONNode("machine"));
            gateCategories.AddToArray(new JSONNode("gate"));
            GateNode.SetAs("categories", gateCategories);

            GateItem = new ItemTypesServer.ItemTypeRaw(GateName, GateNode);
            items.Add(GateName, GateItem);


            var GateXminuNode = new JSONNode();
            GateXminuNode.SetAs("needsBase", true);
            GateXminuNode.SetAs("parentType", GateName);
            GateXminuNode.SetAs("mesh", GameLoader.MESH_PATH + "gatex-.obj");
            GateItemXN = new ItemTypesServer.ItemTypeRaw(GateXminusName, GateXminuNode);
            items.Add(GateXminusName, GateItemXN);


            var GateXplusNode = new JSONNode();
            GateXplusNode.SetAs("needsBase", true);
            GateXplusNode.SetAs("parentType", GateName);
            GateXplusNode.SetAs("mesh", GameLoader.MESH_PATH + "gatex+.obj");
            GateItemXP = new ItemTypesServer.ItemTypeRaw(GateXplusName, GateXplusNode);
            items.Add(GateXplusName, GateItemXP);


            var GateZminuNode = new JSONNode();
            GateZminuNode.SetAs("needsBase", true);
            GateZminuNode.SetAs("parentType", GateName);
            GateZminuNode.SetAs("mesh", GameLoader.MESH_PATH + "gatez-.obj");
            GateItemZN = new ItemTypesServer.ItemTypeRaw(GateZminusName, GateZminuNode);
            items.Add(GateZminusName, GateItemZN);


            var GateZplusNode = new JSONNode();
            GateZplusNode.SetAs("needsBase", true);
            GateZplusNode.SetAs("parentType", GateName);
            GateZplusNode.SetAs("mesh", GameLoader.MESH_PATH + "gatez+.obj");
            GateItemZP = new ItemTypesServer.ItemTypeRaw(GateZplusName, GateZplusNode);
            items.Add(GateZplusName, GateItemZP);

            MeshedObjectType.Register(_gateXMinusItemObjSettings);
            MeshedObjectType.Register(_gateXPlusItemObjSettings);
            MeshedObjectType.Register(_gateZMinusItemObjSettings);
            MeshedObjectType.Register(_gateZPlusItemObjSettings);
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnSavingColony, GameLoader.NAMESPACE + ".Items.Machines.GateLever.OnSavingColony")]
        public static void OnSavingColony(Colony c, JSONNode n)
        {
            if (_gatePositions.ContainsKey(c))
            {
                if (n.HasChild(GameLoader.NAMESPACE + ".Gates"))
                    n.RemoveChild(GameLoader.NAMESPACE + ".Gates");

                var gateNode = new JSONNode(NodeType.Array);

                foreach (var pos in _gatePositions[c])
                {
                    if (pos.Value.State == GatePosition.MovingOpen)
                        pos.Value.State = GatePosition.Open;

                    if (pos.Value.State == GatePosition.MovingClosed)
                        pos.Value.State = GatePosition.Closed;

                    var node = new JSONNode()
                              .SetAs("pos", (Vector3Int)pos.Key)
                              .SetAs("state", pos.Value.ToJsonNode());

                    gateNode.AddToArray(node);
                }

                n[GameLoader.NAMESPACE + ".Gates"] = gateNode;
            }
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnLoadingColony, GameLoader.NAMESPACE + ".Items.Machines.GateLever.OnLoadingColony")]
        public static void OnLoadingColony(Colony c, JSONNode n)
        {
            if (n.TryGetChild(GameLoader.NAMESPACE + ".Gates", out var gateNodes))
            {
                if (!_gatePositions.ContainsKey(c))
                    _gatePositions.Add(c, new Dictionary<Vector3Int, GateState>());

                foreach (var gateNode in gateNodes.LoopArray())
                    _gatePositions[c].Add((Vector3Int)gateNode.GetAs<JSONNode>("pos"),
                                          new GateState(gateNode.GetAs<JSONNode>("state")));
            }
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnTryChangeBlock, GameLoader.NAMESPACE + ".Items.Machines.GateLever.OnTryChangeBlockUser")]
        public static void OnTryChangeBlockUser(ModLoader.OnTryChangeBlockData d)
        {
            if (d.CallbackState == ModLoader.OnTryChangeBlockData.ECallbackState.Cancelled ||
                d.RequestOrigin.AsPlayer == null ||
                d.RequestOrigin.AsPlayer.ID.type == NetworkID.IDType.Server ||
                d.RequestOrigin.AsPlayer.ID.type == NetworkID.IDType.Invalid ||
                d.RequestOrigin.AsPlayer.ActiveColony == null)
                    return;

            if (d.TypeOld.ItemIndex == BuiltinBlocks.Air && (d.TypeNew.ItemIndex == GateItem.ItemIndex ||
                                                        d.TypeNew.ItemIndex == GateItemXN.ItemIndex ||
                                                        d.TypeNew.ItemIndex == GateItemXP.ItemIndex ||
                                                        d.TypeNew.ItemIndex == GateItemZN.ItemIndex ||
                                                        d.TypeNew.ItemIndex == GateItemZP.ItemIndex))
            {
                if (!_gatePositions.ContainsKey(d.RequestOrigin.AsPlayer.ActiveColony))
                    _gatePositions.Add(d.RequestOrigin.AsPlayer.ActiveColony, new Dictionary<Vector3Int, GateState>());

                _gatePositions[d.RequestOrigin.AsPlayer.ActiveColony].Add(d.Position, new GateState(GatePosition.Closed, VoxelSide.None, d.Position));
            }

            if (d.TypeNew.ItemIndex == BuiltinBlocks.Air)
            {
                if (!_gatePositions.ContainsKey(d.RequestOrigin.AsPlayer.ActiveColony))
                    _gatePositions.Add(d.RequestOrigin.AsPlayer.ActiveColony, new Dictionary<Vector3Int, GateState>());

                if (_gatePositions[d.RequestOrigin.AsPlayer.ActiveColony].ContainsKey(d.Position))
                {
                    _gatePositions[d.RequestOrigin.AsPlayer.ActiveColony].Remove(d.Position);

                    if (!d.RequestOrigin.AsPlayer.Inventory.TryAdd(GateItem.ItemIndex))
                       d.RequestOrigin.AsPlayer.ActiveColony.Stockpile.Add(GateItem.ItemIndex);
                }
            }
        }

        public class GateState
        {
            public GateState(GatePosition state, VoxelSide rotation, Vector3Int pos)
            {
                State       = state;
                Orientation = rotation;
                Position    = pos;
            }

            public GateState(JSONNode node)
            {
                State       = (GatePosition) Enum.Parse(typeof(GatePosition), node.GetAs<string>(nameof(State)));
                Orientation = (VoxelSide) Enum.Parse(typeof(VoxelSide), node.GetAs<string>(nameof(Orientation)));
                Position    = (Vector3Int)node.GetAs<JSONNode>(nameof(Position));
            }

            public GatePosition State { get; set; }
            public VoxelSide Orientation { get; set; }
            public Vector3Int Position { get; set; }

            public virtual JSONNode ToJsonNode()
            {
                var baseNode = new JSONNode();

                baseNode.SetAs(nameof(State), State.ToString());
                baseNode.SetAs(nameof(Orientation), Orientation.ToString());
                baseNode.SetAs(nameof(Position), (JSONNode)Position);

                return baseNode;
            }
        }
    }
}