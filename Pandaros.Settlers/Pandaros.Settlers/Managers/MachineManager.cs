using System;
using System.Collections.Generic;
using System.Linq;
using BlockTypes.Builtin;
using Pandaros.Settlers.Entities;
using Pandaros.Settlers.Items.Machines;
using Pandaros.Settlers.Jobs;
using Pipliz;
using Pipliz.JSON;
using Server;
using Shared;
using UnityEngine;
using Math = Pipliz.Math;
using Time = Pipliz.Time;

namespace Pandaros.Settlers.Managers
{
    [ModLoader.ModManagerAttribute]
    public static class MachineManager
    {
        private const int MACHINE_REFRESH = 1;
        public static string MACHINE_JSON = "";

        public static Dictionary<string, IMachineSettings> MachineCallbacks = new Dictionary<string, IMachineSettings>(StringComparer.OrdinalIgnoreCase);

        public static Dictionary<ushort, float> FuelValues = new Dictionary<ushort, float>();
        private static double _nextUpdate;

        public static Dictionary<Players.Player, Dictionary<Vector3Int, MachineState>> Machines { get; } = new Dictionary<Players.Player, Dictionary<Vector3Int, MachineState>>();

        public static event EventHandler MachineRemoved;

        public static void RegisterMachineType(IMachineSettings callback)
        {
            PandaLogger.Log(callback.Name + " Registered as a Machine!");
            MachineCallbacks[callback.Name] = callback;
        }

        public static IMachineSettings GetCallbacks(string machineName)
        {
            if (MachineCallbacks.ContainsKey(machineName))
                return MachineCallbacks[machineName];

            PandaLogger.Log($"Unknown machine {machineName}. Returning {nameof(Miner)}.");
            return MachineCallbacks[nameof(Miner)];
        }

        [ModLoader.ModCallbackAttribute(ModLoader.EModCallbackType.AfterItemTypesDefined,
            GameLoader.NAMESPACE + ".Items.Machines.MacineManager.SetFuelValues")]
        public static void SetFuelValues()
        {
            FuelValues[BuiltinBlocks.Coalore]         = .20f;
            FuelValues[BuiltinBlocks.Firewood]        = .10f;
            FuelValues[BuiltinBlocks.Straw]           = .05f;
            FuelValues[BuiltinBlocks.LeavesTemperate] = .02f;
            FuelValues[BuiltinBlocks.LeavesTaiga]     = .02f;
        }

        [ModLoader.ModCallbackAttribute(ModLoader.EModCallbackType.OnUpdate,
            GameLoader.NAMESPACE + ".Items.Machines.MacineManager.OnUpdate")]
        public static void OnUpdate()
        {
            if (GameLoader.WorldLoaded && _nextUpdate < Time.SecondsSinceStartDouble)
            {
                lock (Machines)
                {
                    foreach (var machine in Machines)
                        if (!machine.Key.IsConnected && Configuration.OfflineColonies || machine.Key.IsConnected)
                            foreach (var state in machine.Value)
                                try
                                {
                                    state.Value.MachineSettings.DoWork(machine.Key, state.Value);

                                    if (state.Value.Load <= 0)
                                        Indicator.SendIconIndicatorNear(state.Value.Position.Add(0, 1, 0).Vector,
                                                                        new IndicatorState(MACHINE_REFRESH,
                                                                                           GameLoader.Reload_Icon, true,
                                                                                           false));

                                    if (state.Value.Durability <= 0)
                                        Indicator.SendIconIndicatorNear(state.Value.Position.Add(0, 1, 0).Vector,
                                                                        new IndicatorState(MACHINE_REFRESH,
                                                                                           GameLoader.Repairing_Icon,
                                                                                           true, false));

                                    if (state.Value.Fuel <= 0)
                                        Indicator.SendIconIndicatorNear(state.Value.Position.Add(0, 1, 0).Vector,
                                                                        new IndicatorState(MACHINE_REFRESH,
                                                                                           GameLoader.Refuel_Icon, true,
                                                                                           false));
                                }
                                catch (Exception ex)
                                {
                                    PandaLogger.LogError(ex);
                                }
                }

                _nextUpdate = Time.SecondsSinceStartDouble + MACHINE_REFRESH;
            }
        }

        [ModLoader.ModCallbackAttribute(ModLoader.EModCallbackType.OnLoadingPlayer,
            GameLoader.NAMESPACE + ".Items.Machines.MachineManager.OnLoadingPlayer")]
        public static void OnLoadingPlayer(JSONNode n, Players.Player p)
        {
            if (n.TryGetChild(GameLoader.NAMESPACE + ".Machines", out var machinesNode))
                lock (Machines)
                {
                    foreach (var node in machinesNode.LoopArray())
                        RegisterMachineState(p, new MachineState(node, p));

                    if (Machines.ContainsKey(p))
                        PandaLogger.Log(ChatColor.lime,
                                        $"{Machines[p].Count} machines loaded from save for {p.ID.steamID.m_SteamID}!");
                    else
                        PandaLogger.Log(ChatColor.lime, $"No machines found in save for {p.ID.steamID.m_SteamID}.");
                }
            else
                PandaLogger.Log(ChatColor.lime, $"No machines found in save for {p.ID.steamID.m_SteamID}.");
        }

        [ModLoader.ModCallbackAttribute(ModLoader.EModCallbackType.OnSavingPlayer,
            GameLoader.NAMESPACE + ".Items.Machines.MachineManager.PatrolTool.OnSavingPlayer")]
        public static void OnSavingPlayer(JSONNode n, Players.Player p)
        {
            lock (Machines)
            {
                if (Machines.ContainsKey(p))
                {
                    if (n.HasChild(GameLoader.NAMESPACE + ".Machines"))
                        n.RemoveChild(GameLoader.NAMESPACE + ".Machines");

                    var machineNode = new JSONNode(NodeType.Array);

                    foreach (var node in Machines[p])
                        machineNode.AddToArray(node.Value.ToJsonNode());

                    n[GameLoader.NAMESPACE + ".Machines"] = machineNode;
                }
            }
        }

        [ModLoader.ModCallbackAttribute(ModLoader.EModCallbackType.OnTryChangeBlock,
            GameLoader.NAMESPACE + ".Items.Machines.MachineManager.OnTryChangeBlockUser")]
        public static void OnTryChangeBlockUser(ModLoader.OnTryChangeBlockData d)
        {
            if (d.CallbackState == ModLoader.OnTryChangeBlockData.ECallbackState.Cancelled)
                return;

            if (d.TypeNew == BuiltinBlocks.Air && d.RequestedByPlayer != null)
                RemoveMachine(d.RequestedByPlayer, d.Position);
        }

        public static void RemoveMachine(Players.Player p, Vector3Int pos, bool throwEvent = true)
        {
            lock (Machines)
            {
                if (!Machines.ContainsKey(p))
                    Machines.Add(p, new Dictionary<Vector3Int, MachineState>());


                if (Machines[p].ContainsKey(pos))
                {
                    var mach = Machines[p][pos];

                    Machines[p].Remove(pos);

                    if (throwEvent && MachineRemoved != null)
                        MachineRemoved(mach, new EventArgs());
                }
            }
        }

        public static void RegisterMachineState(Players.Player player, MachineState state)
        {
            lock (Machines)
            {
                if (!Machines.ContainsKey(player))
                    Machines.Add(player, new Dictionary<Vector3Int, MachineState>());

                Machines[player][state.Position] = state;
            }
        }

        public static List<Vector3Int> GetClosestMachines(Vector3Int position, Players.Player owner, int maxDistance)
        {
            var closest = int.MaxValue;
            var retVal  = new List<Vector3Int>();

            foreach (var machine in Machines[owner])
            {
                var dis = Math.RoundToInt(Vector3.Distance(machine.Key.Vector, position.Vector));

                if (dis <= maxDistance && dis <= closest)
                    retVal.Add(machine.Key);
            }

            return retVal;
        }

        public static ushort Refuel(Players.Player player, MachineState machineState)
        {
            if (!player.IsConnected && Configuration.OfflineColonies || player.IsConnected)
            {
                var ps = PlayerState.GetPlayerState(player);

                if (machineState.Fuel < .75f)
                {
                    if (!MachineState.MAX_FUEL.ContainsKey(player))
                        MachineState.MAX_FUEL[player] = MachineState.DEFAULT_MAX_FUEL;

                    var stockpile = Stockpile.GetStockPile(player);

                    foreach (var item in FuelValues)
                        while ((stockpile.AmountContained(item.Key) > 100 ||
                                item.Key == BuiltinBlocks.Firewood ||
                                item.Key == BuiltinBlocks.Coalore) &&
                               machineState.Fuel < MachineState.MAX_FUEL[player])
                        {
                            stockpile.TryRemove(item.Key);
                            machineState.Fuel += item.Value;
                        }

                    if (machineState.Fuel < MachineState.MAX_FUEL[player])
                        return FuelValues.First().Key;
                }
            }

            return GameLoader.Refuel_Icon;
        }


        public class MachineSettings : IMachineSettings
        {
            public MachineSettings()
            {
            }

            public MachineSettings(string                                     name, 
                                   ushort                                     itemIndex,
                                   Func<Players.Player, MachineState, ushort> repair,
                                   Func<Players.Player, MachineState, ushort> refuel,
                                   Func<Players.Player, MachineState, ushort> reload,
                                   Action<Players.Player, MachineState>       doWork, 
                                   float                                      repairTime,
                                   float                                      refuelTime,
                                   float                                      reloadTime, 
                                   float                                      workTime)
            {
                Name       = name;
                ItemIndex  = itemIndex;
                Repair     = repair;
                Refuel     = refuel;
                DoWork     = doWork;
                RepairTime = repairTime;
                RefuelTime = refuelTime;
                WorkTime   = workTime;
                Reload     = reload;
                ReloadTime = reloadTime;
            }

            public string Name { get; set; }

            public float RepairTime { get; set; }

            public float RefuelTime { get; set; }

            public float ReloadTime { get; set; }

            public float WorkTime { get; set; }

            public ushort ItemIndex { get; set; }

            public Func<Players.Player, MachineState, ushort> Repair { get; set; }
            public Func<Players.Player, MachineState, ushort> Refuel { get; set; }
            public Func<Players.Player, MachineState, ushort> Reload { get; set; }
            public Action<Players.Player, MachineState> DoWork { get; set; }
            public string MachineType { get; set; } = MachinistJob.MECHANICAL;
            public string RefuelAudioKey { get; set; } = GameLoader.NAMESPACE + ".ReloadingAudio";
            public string ReloadAudioKey { get; set; } = GameLoader.NAMESPACE + ".ReloadingAudio";
            public string RepairAudioKey { get; set; } = GameLoader.NAMESPACE + ".HammerAudio";
        }
    }
}