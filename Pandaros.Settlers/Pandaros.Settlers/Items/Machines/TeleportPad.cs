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
using UnityEngine;

namespace Pandaros.Settlers.Items.Machines
{
    [ModLoader.ModManager]
    public static class TeleportPad
    {
        const int TeleportPadCooldown = 30;
        private static Dictionary<Players.Player, Dictionary<Vector3Int, Vector3Int>> _paired = new Dictionary<Players.Player, Dictionary<Vector3Int, Vector3Int>>();
        private static Dictionary<Players.Player, int> _cooldown = new Dictionary<Players.Player, int>();

        public static ItemTypesServer.ItemTypeRaw Item { get; private set; }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterAddingBaseTypes, GameLoader.NAMESPACE + ".Items.Machines.TeleportPad.RegisterMachines")]
        public static void RegisterMachines(Dictionary<string, ItemTypesServer.ItemTypeRaw> items)
        {
            MachineManager.MachineRemoved += MachineManager_MachineRemoved;
            MachineManager.RegisterMachineType(nameof(TeleportPad), new MachineManager.MachineSettings(Item.ItemIndex, Repair, Refuel, Reload, DoWork, 10, 4, 5, 10));
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

                requiredForFix.Add(new InventoryItem(BuiltinBlocks.StoneBricks, 5));
                requiredForFix.Add(new InventoryItem(BuiltinBlocks.ScienceBagBasic, 2));

                if (machineState.Durability < .10f)
                {
                    requiredForFix.Add(new InventoryItem(BuiltinBlocks.ScienceBagAdvanced, 2));
                    requiredForFix.Add(new InventoryItem(BuiltinBlocks.ScienceBagColony, 2));
                    requiredForFix.Add(new InventoryItem(BuiltinBlocks.Crystal, 2));
                }
                else if (machineState.Durability < .30f)
                {
                    requiredForFix.Add(new InventoryItem(BuiltinBlocks.ScienceBagAdvanced, 1));
                    requiredForFix.Add(new InventoryItem(BuiltinBlocks.ScienceBagColony, 2));
                    requiredForFix.Add(new InventoryItem(BuiltinBlocks.Crystal, 2));
                }
                else if (machineState.Durability < .50f)
                {
                    requiredForFix.Add(new InventoryItem(BuiltinBlocks.ScienceBagAdvanced, 1));
                    requiredForFix.Add(new InventoryItem(BuiltinBlocks.Crystal, 1));
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
                {
                    machineState.Durability = MachineState.MAX_DURABILITY[player];

                    if (!_paired.ContainsKey(player))
                        _paired.Add(player, new Dictionary<Vector3Int, Vector3Int>());

                    if (_paired[player].ContainsKey(machineState.Position) &&
                        GetPadAt(machineState.Owner, _paired[player][machineState.Position], out var ms))
                        ms.Durability = MachineState.MAX_DURABILITY[player];
                }
            }

            return retval;
        }
        
        public static ushort Reload(Players.Player player, MachineState machineState)
        {
            return GameLoader.Waiting_Icon;
        }

        public static ushort Refuel(Players.Player player, MachineState machineState)
        {
            var ps = PlayerState.GetPlayerState(player);

            if (machineState.Fuel < .75f)
            {
                MachineState paired = null;

                if (!_paired.ContainsKey(player))
                    _paired.Add(player, new Dictionary<Vector3Int, Vector3Int>());

                if (_paired[player].ContainsKey(machineState.Position))
                    GetPadAt(machineState.Owner, _paired[player][machineState.Position], out paired);

                if (!MachineState.MAX_FUEL.ContainsKey(player))
                    MachineState.MAX_FUEL[player] = MachineState.DEFAULT_MAX_FUEL;

                var stockpile = Stockpile.GetStockPile(player);

                while (stockpile.TryRemove(Mana.Item.ItemIndex) &&
                        machineState.Fuel < MachineState.MAX_FUEL[player])
                {
                    machineState.Fuel += 0.20f;

                    if (paired != null)
                        paired.Fuel += 0.20f;
                }
                
                if (machineState.Fuel < MachineState.MAX_FUEL[player])
                    return Mana.Item.ItemIndex;
            }

            return GameLoader.Refuel_Icon;
        }

        public static void DoWork(Players.Player player, MachineState machineState)
        {
            if (!_paired.ContainsKey(player))
                _paired.Add(player, new Dictionary<Vector3Int, Vector3Int>());

            if (_paired[player].ContainsKey(machineState.Position) &&
                GetPadAt(player, _paired[player][machineState.Position], out var ms) &&
                machineState.Durability > 0 && 
                machineState.Fuel > 0 && 
                machineState.NextTimeForWork < Pipliz.Time.SecondsSinceStartDouble)
            {
                machineState.Durability -= 0.01f;
                machineState.Fuel -= 0.05f;

                if (machineState.Durability < 0)
                    machineState.Durability = 0;

                if (machineState.Fuel <= 0)
                    machineState.Fuel = 0;

                machineState.NextTimeForWork = machineState.MachineSettings.WorkTime + Pipliz.Time.SecondsSinceStartDouble;
            }
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterSelectedWorld, GameLoader.NAMESPACE + ".Items.Machines.TeleportPad.RegisterAudio"),
            ModLoader.ModCallbackProvidesFor("pipliz.server.loadaudiofiles"), ModLoader.ModCallbackDependsOn("pipliz.server.registeraudiofiles")]
        public static void RegisterAudio()
        {
            GameLoader.AddSoundFile(GameLoader.NAMESPACE + ".TeleportPadMachineAudio", new List<string>() { GameLoader.AUDIO_FOLDER_PANDA + "/Teleport.ogg" });
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterItemTypesDefined, GameLoader.NAMESPACE + ".Items.Machines.TeleportPad.RegisterTeleportPad")]
        public static void RegisterTeleportPad()
        {
            var rivets = new InventoryItem(BuiltinBlocks.IronRivet, 6);
            var steel = new InventoryItem(BuiltinBlocks.SteelIngot, 5);
            var sbb = new InventoryItem(BuiltinBlocks.ScienceBagBasic, 20);
            var sbc = new InventoryItem(BuiltinBlocks.ScienceBagColony, 20);
            var sba = new InventoryItem(BuiltinBlocks.ScienceBagAdvanced, 20);
            var crystal = new InventoryItem(BuiltinBlocks.Crystal, 5);
            var stone = new InventoryItem(BuiltinBlocks.StoneBricks, 50);
            var mana = new InventoryItem(Mana.Item.ItemIndex, 100);

            var recipe = new Recipe(Item.name,
                                    new List<InventoryItem>() { crystal, steel, rivets, sbb, sbc, sba, crystal, stone },
                                    new InventoryItem(Item.ItemIndex),
                                    6);

            //ItemTypesServer.LoadSortOrder(Item.name, GameLoader.GetNextItemSortIndex());
            RecipeStorage.AddOptionalLimitTypeRecipe(Jobs.AdvancedCrafterRegister.JOB_NAME, recipe);
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterSelectedWorld, GameLoader.NAMESPACE + ".Items.Machines.TeleportPad.AddTextures"), ModLoader.ModCallbackProvidesFor("pipliz.server.registertexturemappingtextures")]
        public static void AddTextures()
        {
            var TeleportPadTextureMapping = new ItemTypesServer.TextureMapping(new JSONNode());
            TeleportPadTextureMapping.AlbedoPath = GameLoader.TEXTURE_FOLDER_PANDA + "/albedo/TeleportPad.png";
            TeleportPadTextureMapping.EmissivePath = GameLoader.TEXTURE_FOLDER_PANDA + "/emissive/TeleportPad.png";

            ItemTypesServer.SetTextureMapping(GameLoader.NAMESPACE + ".TeleportPad", TeleportPadTextureMapping);
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterAddingBaseTypes, GameLoader.NAMESPACE + ".Items.Machines.TeleportPad.AddTeleportPad"), ModLoader.ModCallbackDependsOn("pipliz.blocknpcs.addlittypes")]
        public static void AddTeleportPad(Dictionary<string, ItemTypesServer.ItemTypeRaw> items)
        {
            var TeleportPadName = GameLoader.NAMESPACE + ".TeleportPad";
            var TeleportPadNode = new JSONNode();
            TeleportPadNode["icon"] = new JSONNode(GameLoader.ICON_FOLDER_PANDA + "/TeleportPad.png");
            TeleportPadNode["isPlaceable"] = new JSONNode(true);
            TeleportPadNode.SetAs("onRemoveAmount", 1);
            TeleportPadNode.SetAs("onPlaceAudio", "stonePlace");
            TeleportPadNode.SetAs("onRemoveAudio", "stoneDelete");
            TeleportPadNode.SetAs("isSolid", false);
            TeleportPadNode.SetAs("sideall", "SELF");
            TeleportPadNode.SetAs("mesh", GameLoader.MESH_FOLDER_PANDA + "/TeleportPad.obj");

            var TeleportPadCollidersNode = new JSONNode(NodeType.Array);
            TeleportPadCollidersNode.AddToArray(new JSONNode("{-0.5, -0.5, -0.5}, {0.5, -0.3, 0.5}"));
            TeleportPadNode.SetAs("boxColliders", TeleportPadCollidersNode);

            var TeleportPadCustomNode = new JSONNode();
            TeleportPadCustomNode.SetAs("useEmissiveMap", true);

            var torchNode = new JSONNode();
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

        public static bool GetPadAt(Players.Player p, Vector3Int pos, out MachineState state)
        {
            lock (MachineManager.Machines)
                if (MachineManager.Machines.ContainsKey(p) && 
                    MachineManager.Machines[p].ContainsKey(pos))
                    if (MachineManager.Machines[p][pos].MachineType == nameof(TeleportPad))
                    {
                        state = MachineManager.Machines[p][pos];
                        return true;
                    }

            state = null;
            return false;
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnSavingPlayer, GameLoader.NAMESPACE + ".Items.Machines.Teleportpad.OnSavingPlayer")]
        public static void OnSavingPlayer(JSONNode n, Players.Player p)
        {
            if (_paired.ContainsKey(p))
            {
                if (n.HasChild(GameLoader.NAMESPACE + ".Teleportpads"))
                    n.RemoveChild(GameLoader.NAMESPACE + ".Teleportpads");

                var teleporters = new JSONNode(NodeType.Array);

                foreach (var pad in _paired[p])
                {
                    var kvpNode = new JSONNode();
                    kvpNode.SetAs("Key", (JSONNode)pad.Key);
                    kvpNode.SetAs("Value", (JSONNode)pad.Value);
                    teleporters.AddToArray(kvpNode);
                }

                n[GameLoader.NAMESPACE + ".Teleportpads"] = teleporters;
            }
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnLoadingPlayer, GameLoader.NAMESPACE + ".Items.Machines.Teleportpad.OnLoadingPlayer")]
        public static void OnLoadingPlayer(JSONNode n, Players.Player p)
        {
            if (!_paired.ContainsKey(p))
                _paired.Add(p, new Dictionary<Vector3Int, Vector3Int>());

            if (n.TryGetChild(GameLoader.NAMESPACE + ".Teleportpads", out var teleportPads))
                foreach (var pad in teleportPads.LoopArray())
                    _paired[p][(Vector3Int)pad.GetAs<JSONNode>("Key")] = (Vector3Int)pad.GetAs<JSONNode>("Value");
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnPlayerMoved, GameLoader.NAMESPACE + ".Items.Machines.Teleportpad.OnPlayerMoved")]
        public static void OnPlayerMoved(Players.Player p)
        {
            var posBelow = new Vector3Int(p.Position);

            if (_paired.ContainsKey(p) && 
                GetPadAt(p, posBelow, out var machineState) &&
                _paired[p].ContainsKey(machineState.Position) &&
                GetPadAt(machineState.Owner, _paired[p][machineState.Position], out var paired))
            {
                var startInt = Pipliz.Time.SecondsSinceStartInt;

                if (!_cooldown.ContainsKey(p))
                    _cooldown.Add(p, 0);

                if (_cooldown[p] <= startInt)
                {
                    ChatCommands.Implementations.Teleport.TeleportTo(p, paired.Position.Vector);

                    ServerManager.SendAudio(machineState.Position.Vector, GameLoader.NAMESPACE + ".TeleportPadMachineAudio");
                    ServerManager.SendAudio(paired.Position.Vector, GameLoader.NAMESPACE + ".TeleportPadMachineAudio");

                    _cooldown[p] = TeleportPadCooldown + startInt;
                }
            }
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnTryChangeBlockUser, GameLoader.NAMESPACE + ".Items.Machines.Teleportpad.OnTryChangeBlockUser")]
        public static bool OnTryChangeBlockUser(ModLoader.OnTryChangeBlockUserData d)
        {
            if (d.TypeNew == Item.ItemIndex && d.typeTillNow == BuiltinBlocks.Air)
            {
                var ps = PlayerState.GetPlayerState(d.requestedBy);
                var ms = new MachineState(d.VoxelToChange, d.requestedBy, nameof(TeleportPad));

                if (ps.TeleporterPlaced == Vector3Int.invalidPos)
                {
                    ps.TeleporterPlaced = d.VoxelToChange;
                    PandaChat.Send(d.requestedBy, $"Place one more teleportation pad to link to.", ChatColor.orange);
                }
                else
                {
                    if (GetPadAt(d.requestedBy, ps.TeleporterPlaced, out var machineState))
                    {
                        if (!_paired.ContainsKey(d.requestedBy))
                            _paired.Add(d.requestedBy, new Dictionary<Vector3Int, Vector3Int>());

                        _paired[d.requestedBy][ms.Position] = machineState.Position;
                        _paired[d.requestedBy][machineState.Position] = ms.Position;
                        PandaChat.Send(d.requestedBy, $"Teleportation pads linked!", ChatColor.orange);
                        ps.TeleporterPlaced = Vector3Int.invalidPos;
                    }
                    else
                    {
                        ps.TeleporterPlaced = d.VoxelToChange;
                        PandaChat.Send(d.requestedBy, $"Place one more teleportation pad to link to.", ChatColor.orange);
                    }
                }

                MachineManager.RegisterMachineState(d.requestedBy, ms);
            }

            return true;
        }

        private static void MachineManager_MachineRemoved(object sender, EventArgs e)
        {
            var machineState = sender as MachineState;

            if (machineState.MachineType == nameof(TeleportPad))
            {
                var ps = PlayerState.GetPlayerState(machineState.Owner);

                if (!_paired.ContainsKey(machineState.Owner))
                    _paired.Add(machineState.Owner, new Dictionary<Vector3Int, Vector3Int>());

                if (_paired[machineState.Owner].ContainsKey(machineState.Position) &&
                    GetPadAt(machineState.Owner, _paired[machineState.Owner][machineState.Position], out var paired))
                {
                    if (_paired[machineState.Owner].ContainsKey(machineState.Position))
                        _paired[machineState.Owner].Remove(machineState.Position);

                    if (_paired[machineState.Owner].ContainsKey(paired.Position))
                        _paired[machineState.Owner].Remove(paired.Position);

                    MachineManager.RemoveMachine(machineState.Owner, paired.Position, false);
                    ServerManager.TryChangeBlock(paired.Position, BuiltinBlocks.Air);

                    if (!Inventory.GetInventory(machineState.Owner).TryAdd(Item.ItemIndex))
                    {
                        var stockpile = Stockpile.GetStockPile(machineState.Owner);
                        stockpile.Add(Item.ItemIndex);
                    }
                }

                if (machineState.Position == ps.TeleporterPlaced)
                    ps.TeleporterPlaced = Vector3Int.invalidPos;
            }
        }

    }
}
