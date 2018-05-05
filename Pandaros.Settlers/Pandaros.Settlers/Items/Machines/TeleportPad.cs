using BlockTypes.Builtin;
using NPC;
using Pandaros.Settlers.Entities;
using Pandaros.Settlers.Managers;
using Pipliz;
using Pipliz.JSON;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Pandaros.Settlers.Items.Machines
{
    [ModLoader.ModManager]
    public static class TeleportPad
    {
        private static Dictionary<Vector3Int, Vector3Int> _paired = new Dictionary<Vector3Int, Vector3Int>();
        private static Dictionary<Players.Player, int> _cooldown = new Dictionary<Players.Player, int>();

        public static ItemTypesServer.ItemTypeRaw Item { get; private set; }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterAddingBaseTypes, GameLoader.NAMESPACE + ".Items.Machines.TeleportPad.RegisterMachines")]
        public static void RegisterMachines(Dictionary<string, ItemTypesServer.ItemTypeRaw> items)
        {
            MachineManager.MachineRemoved += MachineManager_MachineRemoved;
            MachineManager.RegisterMachineType(nameof(TeleportPad), new MachineManager.MachineSettings(nameof(TeleportPad), Item.ItemIndex, Repair, Refuel, Reload, DoWork, 10, 4, 5, 10));
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

                    if (_paired.ContainsKey(machineState.Position) &&
                        GetPadAt(_paired[machineState.Position], out var ms))
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

                if (_paired.ContainsKey(machineState.Position))
                    GetPadAt(_paired[machineState.Position], out paired);

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
            if (!Configuration.TeleportPadsRequireMachinists)
                return;
            
            if ((!player.IsConnected && Configuration.OfflineColonies) || player.IsConnected)
            {
                if (_paired.ContainsKey(machineState.Position) &&
                GetPadAt(_paired[machineState.Position], out var ms) &&
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

            JSONNode categories = new JSONNode(NodeType.Array);
            categories.AddToArray(new JSONNode("machine"));
            TeleportPadNode.SetAs("categories", categories);

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

        public static bool GetPadAt(Vector3Int pos, out MachineState state)
        {
            try
            {
                if (pos != null)
                    lock (MachineManager.Machines)
                        foreach (var p in MachineManager.Machines)
                        if (p.Value.ContainsKey(pos))
                            if (p.Value[pos].MachineType == nameof(TeleportPad))
                            {
                                state = p.Value[pos];
                                return true;
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
            JSONNode n = null;

            if (File.Exists(MachineManager.MACHINE_JSON))
                JSON.Deserialize(MachineManager.MACHINE_JSON, out n);

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

            using (StreamWriter writer = File.CreateText(MachineManager.MACHINE_JSON))
                n.Serialize(writer, 1, 1);
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterWorldLoad, GameLoader.NAMESPACE + ".Items.Machines.Teleportpad.AfterWorldLoad")]
        public static void AfterWorldLoad()
        {
            if (File.Exists(MachineManager.MACHINE_JSON) &&
                JSON.Deserialize(MachineManager.MACHINE_JSON, out var n) &&
                n.TryGetChild(GameLoader.NAMESPACE + ".Teleportpads", out var teleportPads))
                    foreach (var pad in teleportPads.LoopArray())
                        _paired[(Vector3Int)pad.GetAs<JSONNode>("Key")] = (Vector3Int)pad.GetAs<JSONNode>("Value");
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnPlayerMoved, GameLoader.NAMESPACE + ".Items.Machines.Teleportpad.OnPlayerMoved")]
        public static void OnPlayerMoved(Players.Player p)
        {
            var posBelow = new Vector3Int(p.Position);

            if (GetPadAt(posBelow, out var machineState) &&
                _paired.ContainsKey(machineState.Position) &&
                GetPadAt(_paired[machineState.Position], out var paired))
            {
                var startInt = Pipliz.Time.SecondsSinceStartInt;

                if (!_cooldown.ContainsKey(p))
                    _cooldown.Add(p, 0);

                if (_cooldown[p] <= startInt)
                {
                    ChatCommands.Implementations.Teleport.TeleportTo(p, paired.Position.Vector);

                    ServerManager.SendAudio(machineState.Position.Vector, GameLoader.NAMESPACE + ".TeleportPadMachineAudio");
                    ServerManager.SendAudio(paired.Position.Vector, GameLoader.NAMESPACE + ".TeleportPadMachineAudio");

                    _cooldown[p] = Configuration.GetorDefault("TeleportPadCooldown", 15) + startInt;
                }
            }
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnTryChangeBlock, GameLoader.NAMESPACE + ".Items.Machines.Teleportpad.OnTryChangeBlockUser")]
        public static void OnTryChangeBlockUser(ModLoader.OnTryChangeBlockData d)
        {
            if (d.CallbackState == ModLoader.OnTryChangeBlockData.ECallbackState.Cancelled)
                return;

            if (d.TypeNew == Item.ItemIndex && d.TypeOld == BuiltinBlocks.Air)
            {
                var ps = PlayerState.GetPlayerState(d.RequestedByPlayer);
                var ms = new MachineState(d.Position, d.RequestedByPlayer, nameof(TeleportPad));

                if (ps.TeleporterPlaced == Vector3Int.invalidPos)
                {
                    ps.TeleporterPlaced = d.Position;
                    PandaChat.Send(d.RequestedByPlayer, $"Place one more teleportation pad to link to.", ChatColor.orange);
                }
                else
                {
                    if (GetPadAt(ps.TeleporterPlaced, out var machineState))
                    {
                        _paired[ms.Position] = machineState.Position;
                        _paired[machineState.Position] = ms.Position;
                        PandaChat.Send(d.RequestedByPlayer, $"Teleportation pads linked!", ChatColor.orange);
                        ps.TeleporterPlaced = Vector3Int.invalidPos;
                    }
                    else
                    {
                        ps.TeleporterPlaced = d.Position;
                        PandaChat.Send(d.RequestedByPlayer, $"Place one more teleportation pad to link to.", ChatColor.orange);
                    }
                }

                MachineManager.RegisterMachineState(d.RequestedByPlayer, ms);
            }
        }

        private static void MachineManager_MachineRemoved(object sender, EventArgs e)
        {
            var machineState = sender as MachineState;

            if (machineState != null &&
                machineState.MachineType == nameof(TeleportPad))
            {
                var ps = PlayerState.GetPlayerState(machineState.Owner);
                
                if (_paired.ContainsKey(machineState.Position) &&
                    GetPadAt(_paired[machineState.Position], out var paired))
                {
                    if (_paired.ContainsKey(machineState.Position))
                        _paired.Remove(machineState.Position);

                    if (_paired.ContainsKey(paired.Position))
                        _paired.Remove(paired.Position);

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
