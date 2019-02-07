using BlockTypes;
using Chatting.Commands;
using Pandaros.Settlers.Entities;
using Pandaros.Settlers.Jobs;
using Pandaros.Settlers.Jobs.Roaming;
using Pipliz;
using Pipliz.JSON;
using Recipes;
using System;
using System.Collections.Generic;
using System.IO;
using Time = Pipliz.Time;

namespace Pandaros.Settlers.Items.Machines
{
    public class TeleportPadRegister : IRoamingJobObjective
    {
        public string name => nameof(TeleportPad);
        public float WorkTime => 10;
        public ushort ItemIndex => TeleportPad.Item.ItemIndex;
        public Dictionary<string, IRoamingJobObjectiveAction> ActionCallbacks { get; } = new Dictionary<string, IRoamingJobObjectiveAction>()
        {
            { MachineConstants.REFUEL, new RefuelTeleportPad() },
            { MachineConstants.REPAIR, new RepairTeleportPad() },
            { MachineConstants.RELOAD, new ReloadTeleportPad() }
        };

        public string ObjectiveCategory => MachineConstants.MECHANICAL;

        public void DoWork(Colony colony, RoamingJobState state)
        {
            TeleportPad.DoWork(colony, state);
        }
    }

    public class RepairTeleportPad : IRoamingJobObjectiveAction
    {
        public string name => MachineConstants.REPAIR;

        public float TimeToPreformAction => 10;

        public string AudoKey => GameLoader.NAMESPACE + ".HammerAudio";

        public ushort ObjectiveLoadEmptyIcon => GameLoader.Repairing_Icon;

        public ushort PreformAction(Colony player, RoamingJobState state)
        {
            return TeleportPad.Repair(player, state);
        }
    }

    public class ReloadTeleportPad : IRoamingJobObjectiveAction
    {
        public string name => MachineConstants.RELOAD;

        public float TimeToPreformAction => 5;

        public string AudoKey => GameLoader.NAMESPACE + ".ReloadingAudio";

        public ushort ObjectiveLoadEmptyIcon => GameLoader.Reload_Icon;

        public ushort PreformAction(Colony colony, RoamingJobState state)
        {
            return TeleportPad.Reload(colony, state);
        }
    }

    public class RefuelTeleportPad : IRoamingJobObjectiveAction
    {
        public string name => MachineConstants.REFUEL;

        public float TimeToPreformAction => 4;

        public string AudoKey => GameLoader.NAMESPACE + ".ReloadingAudio";

        public ushort ObjectiveLoadEmptyIcon => GameLoader.Reload_Icon;

        public ushort PreformAction(Colony player, RoamingJobState state)
        {
            return TeleportPad.Refuel(player, state);
        }
    }


    [ModLoader.ModManager]
    public static class TeleportPad
    {
        private static readonly Dictionary<Vector3Int, Vector3Int> _paired = new Dictionary<Vector3Int, Vector3Int>();
        private static readonly Dictionary<Players.Player, int> _cooldown = new Dictionary<Players.Player, int>();

        public static ItemTypesServer.ItemTypeRaw Item { get; private set; }

        public static ushort Repair(Colony colony, RoamingJobState machineState)
        {
            var retval = GameLoader.Repairing_Icon;

            if (machineState.GetActionEnergy(MachineConstants.REPAIR) < .75f)
            {
                var repaired       = false;
                var requiredForFix = new List<InventoryItem>();
                var stockpile      = colony.Stockpile;

                requiredForFix.Add(new InventoryItem(ColonyBuiltIn.ItemTypes.STONEBRICKS.Name, 5));
                requiredForFix.Add(new InventoryItem(ColonyBuiltIn.ItemTypes.SCIENCEBAGBASIC.Name, 2));

                if (machineState.GetActionEnergy(MachineConstants.REPAIR) < .10f)
                {
                    requiredForFix.Add(new InventoryItem(ColonyBuiltIn.ItemTypes.SCIENCEBAGADVANCED.Name, 2));
                    requiredForFix.Add(new InventoryItem(ColonyBuiltIn.ItemTypes.SCIENCEBAGCOLONY.Name, 2));
                    requiredForFix.Add(new InventoryItem(ColonyBuiltIn.ItemTypes.CRYSTAL.Name, 2));
                }
                else if (machineState.GetActionEnergy(MachineConstants.REPAIR) < .30f)
                {
                    requiredForFix.Add(new InventoryItem(ColonyBuiltIn.ItemTypes.SCIENCEBAGADVANCED.Name, 1));
                    requiredForFix.Add(new InventoryItem(ColonyBuiltIn.ItemTypes.SCIENCEBAGCOLONY.Name, 2));
                    requiredForFix.Add(new InventoryItem(ColonyBuiltIn.ItemTypes.CRYSTAL.Name, 2));
                }
                else if (machineState.GetActionEnergy(MachineConstants.REPAIR) < .50f)
                {
                    requiredForFix.Add(new InventoryItem(ColonyBuiltIn.ItemTypes.SCIENCEBAGADVANCED.Name, 1));
                    requiredForFix.Add(new InventoryItem(ColonyBuiltIn.ItemTypes.CRYSTAL.Name, 1));
                }

                if (stockpile.Contains(requiredForFix))
                {
                    stockpile.TryRemove(requiredForFix);
                    repaired = true;
                }
                else
                {
                    foreach (var item in requiredForFix)
                        if (!stockpile.Contains(item))
                        {
                            retval = item.Type;
                            break;
                        }
                }

                if (repaired)
                {
                    machineState.ResetActionToMaxLoad(MachineConstants.REPAIR);

                    if (_paired.ContainsKey(machineState.Position) &&
                        GetPadAt(_paired[machineState.Position], out var ms))
                        ms.ResetActionToMaxLoad(MachineConstants.REPAIR);
                }
            }

            return retval;
        }

        public static ushort Reload(Colony colony, RoamingJobState machineState)
        {
            return GameLoader.Waiting_Icon;
        }

        public static ushort Refuel(Colony colony, RoamingJobState machineState)
        {
            if (machineState.GetActionEnergy(MachineConstants.REFUEL) < .75f)
            {
                RoamingJobState paired = null;

                if (_paired.ContainsKey(machineState.Position))
                    GetPadAt(_paired[machineState.Position], out paired);

                var stockpile = colony.Stockpile;

                while (stockpile.TryRemove(Mana.Item.ItemIndex) &&
                       machineState.GetActionEnergy(MachineConstants.REFUEL) < RoamingJobState.GetActionsMaxEnergy(MachineConstants.REFUEL, colony, MachineConstants.MECHANICAL))
                {
                    machineState.AddToActionEmergy(MachineConstants.REFUEL, 0.20f);

                    if (paired != null)
                        paired.AddToActionEmergy(MachineConstants.REFUEL, 0.20f);
                }

                if (machineState.GetActionEnergy(MachineConstants.REFUEL) < RoamingJobState.GetActionsMaxEnergy(MachineConstants.REFUEL, colony, MachineConstants.MECHANICAL))
                    return Mana.Item.ItemIndex;
            }

            return GameLoader.Refuel_Icon;
        }

        public static void DoWork(Colony colony, RoamingJobState machineState)
        {
            if (!Configuration.TeleportPadsRequireMachinists)
                return;

            if (!colony.OwnerIsOnline() && Configuration.OfflineColonies || colony.OwnerIsOnline())
                if (_paired.ContainsKey(machineState.Position) &&
                    GetPadAt(_paired[machineState.Position], out var ms) &&
                    machineState.GetActionEnergy(MachineConstants.REPAIR) > 0 &&
                    machineState.GetActionEnergy(MachineConstants.REFUEL) > 0 &&
                    machineState.NextTimeForWork < Time.SecondsSinceStartDouble)
                {
                    machineState.SubtractFromActionEnergy(MachineConstants.REPAIR, 0.01f);
                    machineState.SubtractFromActionEnergy(MachineConstants.REFUEL, 0.05f);

                    machineState.NextTimeForWork = machineState.RoamingJobSettings.WorkTime + Time.SecondsSinceStartDouble;
                }
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterItemTypesDefined, GameLoader.NAMESPACE + ".Items.Machines.TeleportPad.RegisterTeleportPad")]
        public static void RegisterTeleportPad()
        {
            var rivets  = new InventoryItem(ColonyBuiltIn.ItemTypes.IRONRIVET.Name, 6);
            var steel   = new InventoryItem(ColonyBuiltIn.ItemTypes.STEELINGOT.Name, 5);
            var sbb     = new InventoryItem(ColonyBuiltIn.ItemTypes.SCIENCEBAGBASIC.Name, 20);
            var sbc     = new InventoryItem(ColonyBuiltIn.ItemTypes.SCIENCEBAGCOLONY.Name, 20);
            var sba     = new InventoryItem(ColonyBuiltIn.ItemTypes.SCIENCEBAGADVANCED.Name, 20);
            var crystal = new InventoryItem(ColonyBuiltIn.ItemTypes.CRYSTAL.Name, 5);
            var stone   = new InventoryItem(ColonyBuiltIn.ItemTypes.STONEBRICKS.Name, 50);
            var mana    = new InventoryItem(Mana.Item.ItemIndex, 100);

            var recipe = new Recipe(Item.name,
                                    new List<InventoryItem> {crystal, steel, rivets, sbb, sbc, sba, crystal, stone},
                                    new ItemTypes.ItemTypeDrops(Item.ItemIndex),
                                    6);

            ServerManager.RecipeStorage.AddOptionalLimitTypeRecipe(AdvancedCrafterRegister.JOB_NAME, recipe);
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterSelectedWorld, GameLoader.NAMESPACE + ".Items.Machines.TeleportPad.AddTextures")]
        [ModLoader.ModCallbackProvidesFor("pipliz.server.registertexturemappingtextures")]
        public static void AddTextures()
        {
            var TeleportPadTextureMapping = new ItemTypesServer.TextureMapping(new JSONNode());
            TeleportPadTextureMapping.AlbedoPath   = GameLoader.BLOCKS_ALBEDO_PATH + "TeleportPad.png";
            TeleportPadTextureMapping.EmissivePath = GameLoader.BLOCKS_EMISSIVE_PATH + "TeleportPad.png";

            ItemTypesServer.SetTextureMapping(GameLoader.NAMESPACE + ".TeleportPad", TeleportPadTextureMapping);
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AddItemTypes,  GameLoader.NAMESPACE + ".Items.Machines.TeleportPad.AddTeleportPad")]
        [ModLoader.ModCallbackDependsOn("pipliz.blocknpcs.addlittypes")]
        public static void AddTeleportPad(Dictionary<string, ItemTypesServer.ItemTypeRaw> items)
        {
            var TeleportPadName = GameLoader.NAMESPACE + ".TeleportPad";
            var TeleportPadNode = new JSONNode();
            TeleportPadNode["icon"]        = new JSONNode(GameLoader.ICON_PATH + "TeleportPad.png");
            TeleportPadNode["isPlaceable"] = new JSONNode(true);
            TeleportPadNode.SetAs("onRemoveAmount", 1);
            TeleportPadNode.SetAs("onPlaceAudio", "stonePlace");
            TeleportPadNode.SetAs("onRemoveAudio", "stoneDelete");
            TeleportPadNode.SetAs("isSolid", false);
            TeleportPadNode.SetAs("sideall", "SELF");
            TeleportPadNode.SetAs("mesh", GameLoader.MESH_PATH + "TeleportPad.obj");

            var categories = new JSONNode(NodeType.Array);
            categories.AddToArray(new JSONNode("machine"));
            TeleportPadNode.SetAs("categories", categories);


            var TeleportPadCollidersNode = new JSONNode();
            var TeleportPadBoxesNode     = new JSONNode();
            var TeleportPadBoxesMinNode  = new JSONNode(NodeType.Array);
            TeleportPadBoxesMinNode.AddToArray(new JSONNode(-0.5));
            TeleportPadBoxesMinNode.AddToArray(new JSONNode(-0.5));
            TeleportPadBoxesMinNode.AddToArray(new JSONNode(-0.5));
            var TeleportPadBoxesMaxNode = new JSONNode(NodeType.Array);
            TeleportPadBoxesMaxNode.AddToArray(new JSONNode(0.5));
            TeleportPadBoxesMaxNode.AddToArray(new JSONNode(-0.3));
            TeleportPadBoxesMaxNode.AddToArray(new JSONNode(0.5));

            TeleportPadBoxesNode.SetAs("min", TeleportPadBoxesMinNode);
            TeleportPadBoxesNode.SetAs("max", TeleportPadBoxesMaxNode);
            TeleportPadCollidersNode.SetAs("boxes", TeleportPadBoxesNode);
            TeleportPadNode.SetAs("Colliders", TeleportPadCollidersNode);

            var TeleportPadCustomNode = new JSONNode();
            TeleportPadCustomNode.SetAs("useEmissiveMap", true);

            var torchNode  = new JSONNode();
            var aTorchnode = new JSONNode();

            aTorchnode.SetAs("color", "#236B94");
            aTorchnode.SetAs("intensity", 8);
            aTorchnode.SetAs("range", 6);
            aTorchnode.SetAs("volume", 0.5);

            torchNode.SetAs("a", aTorchnode);

            TeleportPadCustomNode.SetAs("torches", torchNode);
            TeleportPadNode.SetAs("customData", TeleportPadCustomNode);

            Item = new ItemTypesServer.ItemTypeRaw(TeleportPadName, TeleportPadNode);
            items.Add(TeleportPadName, Item);
        }

        public static bool GetPadAt(Vector3Int pos, out RoamingJobState state)
        {
            try
            {
                if (pos != null)
                    lock (RoamingJobManager.Objectives)
                    {
                        foreach (var p in RoamingJobManager.Objectives)
                            if (p.Value.ContainsKey(pos))
                                if (p.Value[pos].RoamObjective == nameof(TeleportPad))
                                {
                                    state = p.Value[pos];
                                    return true;
                                }
                    }
            }
            catch (Exception ex)
            {
                PandaLogger.LogError(ex);
            }

            state = null;
            return false;
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnAutoSaveWorld, GameLoader.NAMESPACE + ".Items.Machines.Teleportpad.OnAutoSaveWorld")]
        public static void OnAutoSaveWorld()
        {
            Save();
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnQuitEarly, GameLoader.NAMESPACE + ".Items.Machines.Teleportpad.OnQuitEarly")]
        public static void OnQuitEarly()
        {
            Save();
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnPlayerDisconnected, GameLoader.NAMESPACE + ".Items.Machines.Teleportpad.OnPlayerDisconnected")]
        public static void OnPlayerDisconnected(Players.Player p)
        {
            Save();
        }

        private static void Save()
        {
            if (string.IsNullOrEmpty(RoamingJobManager.MACHINE_JSON))
                return;

            JSONNode n = null;
            
            if (File.Exists(RoamingJobManager.MACHINE_JSON))
                JSON.Deserialize(RoamingJobManager.MACHINE_JSON, out n);

            if (n == null)
                n = new JSONNode();

            if (n.HasChild(GameLoader.NAMESPACE + ".Teleportpads"))
                n.RemoveChild(GameLoader.NAMESPACE + ".Teleportpads");

            var teleporters = new JSONNode(NodeType.Array);

            foreach (var pad in _paired)
            {
                var kvpNode = new JSONNode();
                kvpNode.SetAs("Key", (JSONNode)pad.Key);
                kvpNode.SetAs("Value", (JSONNode)pad.Value);
                teleporters.AddToArray(kvpNode);
            }

            n[GameLoader.NAMESPACE + ".Teleportpads"] = teleporters;

            using (var writer = File.CreateText(RoamingJobManager.MACHINE_JSON))
            {
                n.Serialize(writer, 1, 1);
            }
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterWorldLoad, GameLoader.NAMESPACE + ".Items.Machines.Teleportpad.AfterWorldLoad")]
        public static void AfterWorldLoad()
        {
            if (File.Exists(RoamingJobManager.MACHINE_JSON) &&
                JSON.Deserialize(RoamingJobManager.MACHINE_JSON, out var n) &&
                n.TryGetChild(GameLoader.NAMESPACE + ".Teleportpads", out var teleportPads))
                foreach (var pad in teleportPads.LoopArray())
                    _paired[(Vector3Int)pad.GetAs<JSONNode>("Key")] = (Vector3Int)pad.GetAs<JSONNode>("Value");
        }

        
        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnPlayerMoved, GameLoader.NAMESPACE + ".Items.Machines.Teleportpad.OnPlayerMoved")]
        public static void OnPlayerMoved(Players.Player p, UnityEngine.Vector3 oldPosition)
        {
            try
            {
                var posBelow = new Vector3Int(p.Position);

                if (GetPadAt(posBelow, out var machineState) &&
                    _paired.ContainsKey(machineState.Position) &&
                    GetPadAt(_paired[machineState.Position], out var paired))
                {
                    var startInt = Time.SecondsSinceStartInt;

                    if (!_cooldown.ContainsKey(p))
                        _cooldown.Add(p, 0);

                    if (_cooldown[p] <= startInt)
                    {
                        if (machineState.GetActionEnergy(MachineConstants.REPAIR) <= 0)
                        {
                            PandaChat.Send(p, "This teleporter is in need of repair. Make sure a machinist is near by to maintain it!", ChatColor.red);
                            return;
                        }

                        if (machineState.GetActionEnergy(MachineConstants.REFUEL) <= 0)
                        {
                            PandaChat.Send(p, "This teleporter is in need of mana. Make sure a machinist is near by to maintain it!", ChatColor.red);
                            return;
                        }

                        Teleport.TeleportTo(p, paired.Position.Vector);
                        ServerManager.SendAudio(machineState.Position.Vector, GameLoader.NAMESPACE + ".TeleportPadMachineAudio");
                        ServerManager.SendAudio(paired.Position.Vector, GameLoader.NAMESPACE + ".TeleportPadMachineAudio");
                        _cooldown[p] = Configuration.GetorDefault("TeleportPadCooldown", 15) + startInt;
                    }
                }
            }
            catch (Exception ex)
            {
                PandaLogger.LogError(ex);
            }
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnTryChangeBlock,  GameLoader.NAMESPACE + ".Items.Machines.Teleportpad.OnTryChangeBlockUser")]
        public static void OnTryChangeBlockUser(ModLoader.OnTryChangeBlockData d)
        {
            if (d.CallbackState == ModLoader.OnTryChangeBlockData.ECallbackState.Cancelled)
                return;

            if (d.TypeNew.ItemIndex == Item.ItemIndex && d.TypeOld.ItemIndex == ColonyBuiltIn.ItemTypes.AIR.Id)
            {
                var ps = PlayerState.GetPlayerState(d.RequestOrigin.AsPlayer);
                var ms = new RoamingJobState(d.Position, d.RequestOrigin.AsPlayer.ActiveColony, nameof(TeleportPad));

                if (ps.TeleporterPlaced == Vector3Int.invalidPos)
                {
                    ps.TeleporterPlaced = d.Position;

                    PandaChat.Send(d.RequestOrigin.AsPlayer, $"Place one more teleportation pad to link to.",
                                   ChatColor.orange);
                }
                else
                {
                    if (GetPadAt(ps.TeleporterPlaced, out var machineState))
                    {
                        _paired[ms.Position]           = machineState.Position;
                        _paired[machineState.Position] = ms.Position;
                        PandaChat.Send(d.RequestOrigin.AsPlayer, $"Teleportation pads linked!", ChatColor.orange);
                        ps.TeleporterPlaced = Vector3Int.invalidPos;
                    }
                    else
                    {
                        ps.TeleporterPlaced = d.Position;

                        PandaChat.Send(d.RequestOrigin.AsPlayer, $"Place one more teleportation pad to link to.",
                                       ChatColor.orange);
                    }
                }
            }
        }

        private static void MachineManager_MachineRemoved(object sender, EventArgs e)
        {
            var machineState = sender as RoamingJobState;

            if (machineState != null &&
                machineState.RoamObjective == nameof(TeleportPad))
            {
                if (_paired.ContainsKey(machineState.Position) &&
                    GetPadAt(_paired[machineState.Position], out var paired))
                {
                    if (_paired.ContainsKey(machineState.Position))
                        _paired.Remove(machineState.Position);

                    if (_paired.ContainsKey(paired.Position))
                        _paired.Remove(paired.Position);

                    RoamingJobManager.RemoveObjective(machineState.Colony, paired.Position, false);
                    ServerManager.TryChangeBlock(paired.Position, ColonyBuiltIn.ItemTypes.AIR.Id);
                    machineState.Colony.Stockpile.Add(Item.ItemIndex);
                }

                // TODO where is this called from?
                //if (machineState.Position == ps.TeleporterPlaced)
                //    ps.TeleporterPlaced = Vector3Int.invalidPos;
            }
        }
    }
}