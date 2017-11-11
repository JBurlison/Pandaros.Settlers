using BlockTypes.Builtin;
using NPC;
using Pandaros.Settlers.Entities;
using Pandaros.Settlers.Items.Machines;
using Pipliz;
using Pipliz.JSON;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Pandaros.Settlers.Managers
{
    [ModLoader.ModManager]
    public static class MachineManager
    {
        public class MachineSettings
        {
            public float RepairTime { get; set; }

            public float RefuelTime { get; set; }

            public float ReloadTime { get; set; }

            public float WorkTime { get; set; }

            public ushort ItemIndex { get; set; }

            public Func<Players.Player, MachineState, ushort> Repair { get; set; }
            public Func<Players.Player, MachineState, ushort> Refuel { get; set; }
            public Func<Players.Player, MachineState, ushort> Reload { get; set; }
            public Action<Players.Player, MachineState> DoWork { get; set; }

            public MachineSettings(ushort itemIndex, Func<Players.Player, MachineState, ushort> repair, Func<Players.Player, MachineState, ushort> refuel, Func<Players.Player, MachineState, ushort> reload,
                                    Action<Players.Player, MachineState> doWork, float repairTime, float refuelTime, float reloadTime, float workTime)
            {
                ItemIndex = itemIndex;
                Repair = repair;
                Refuel = refuel;
                DoWork = doWork;
                RepairTime = repairTime;
                RefuelTime = refuelTime;
                WorkTime = workTime;
                Reload = reload;
                ReloadTime = reloadTime;
            }
        }

        public static Dictionary<Players.Player, List<MachineState>> Machines { get; private set; } = new Dictionary<Players.Player, List<MachineState>>();
        public static double _nextUpdateTime;
        public static Dictionary<string, MachineSettings> _machineCallbacks = new Dictionary<string, MachineSettings>();
        public static Dictionary<ushort, float> FuelValues = new Dictionary<ushort, float>();
        static List<MachineState> _invalidMachines = new List<MachineState>();

        public static void RegisterMachineType(string machineType, MachineSettings callback)
        {
            PandaLogger.Log(machineType + " Registered as a Machine Type!");
            _machineCallbacks[machineType] = callback;
        }

        public static MachineSettings GetCallbacks(string machineType)
        {
            return _machineCallbacks[machineType];
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterItemTypesDefined, GameLoader.NAMESPACE + ".Items.Machines.MacineManager.SetFuelValues")]
        public static void SetFuelValues()
        {
            FuelValues[BuiltinBlocks.LeavesTemperate] = .02f;
            FuelValues[BuiltinBlocks.LeavesTaiga] = .02f;
            FuelValues[BuiltinBlocks.Leaves] = .02f;
            FuelValues[BuiltinBlocks.Coalore] = .20f;
            FuelValues[BuiltinBlocks.Firewood] = .10f;
            FuelValues[BuiltinBlocks.Straw] = .05f;
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnUpdate, GameLoader.NAMESPACE + ".Items.Machines.MacineManager.OnUpdate")]
        public static void OnUpdate()
        {
            if (GameLoader.WorldLoaded)
            {
                lock (Machines)
                    foreach (var machine in Machines)
                    {
                        _invalidMachines.Clear();

                        foreach (var state in machine.Value)
                            if (!state.PositionIsValid())
                                _invalidMachines.Add(state);
                            else
                                state.MachineSettings.DoWork(machine.Key, state);

                        foreach (var m in _invalidMachines)
                            machine.Value.Remove(m);
                    }
            }
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnLoadingPlayer, GameLoader.NAMESPACE + ".Items.Machines.MachineManager.OnLoadingPlayer")]
        public static void OnLoadingPlayer(JSONNode n, Players.Player p)
        {
            if (n.TryGetChild(GameLoader.NAMESPACE + ".Machines", out var machinesNode))
            {
                lock (Machines)
                {
                    foreach (var node in machinesNode.LoopArray())
                        RegisterMachineState(p, new MachineState(node, p));

                    if (Machines.ContainsKey(p))
                        PandaLogger.Log(ChatColor.lime, $"{Machines[p].Count} machines loaded from save for {p.ID.steamID.m_SteamID}!");
                    else
                        PandaLogger.Log(ChatColor.lime, $"No machines found in save for {p.ID.steamID.m_SteamID}.");
                }
            }
            else
                PandaLogger.Log(ChatColor.lime, $"No machines found in save for {p.ID.steamID.m_SteamID}.");
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnSavingPlayer, GameLoader.NAMESPACE + ".Items.Machines.MachineManager.PatrolTool.OnSavingPlayer")]
        public static void OnSavingPlayer(JSONNode n, Players.Player p)
        {
            lock (Machines)
                if (Machines.ContainsKey(p))
                {
                    if (n.HasChild(GameLoader.NAMESPACE + ".Machines"))
                        n.RemoveChild(GameLoader.NAMESPACE + ".Machines");

                    var machineNode = new JSONNode(NodeType.Array);

                    foreach (var node in Machines[p])
                            machineNode.AddToArray(node.ToJsonNode());

                    n[GameLoader.NAMESPACE + ".Machines"] = machineNode;
                }
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnTryChangeBlockUser, GameLoader.NAMESPACE + ".Items.Machines.MachineManager.OnTryChangeBlockUser")]
        public static bool OnTryChangeBlockUser(ModLoader.OnTryChangeBlockUserData d)
        {
            if (d.typeToBuild == BuiltinBlocks.Air)
            lock (Machines)
            {
                if (!Machines.ContainsKey(d.requestedBy))
                    Machines.Add(d.requestedBy, new List<MachineState>());

                    var mach = Machines[d.requestedBy].FirstOrDefault(m => m.Position == d.VoxelToChange);

                    if (mach != null)
                        Machines[d.requestedBy].Remove(mach);
            }

            return true;
        }


        public static void RegisterMachineState(Players.Player player, MachineState state)
        {
            lock (Machines)
            {
                if (!Machines.ContainsKey(player))
                    Machines.Add(player, new List<MachineState>());

                var existing = Machines[player].FirstOrDefault(m => m.Position == state.Position);

                if (existing != null)
                    Machines[player].Remove(existing);

                Machines[player].Add(state);
#if Debug
                PandaLogger.Log($"ADD {Machines[player].Count} known machines for Player {player.ID.steamID}");
#endif
            }
        }

        public static List<Vector3Int> GetClosestMachines(Vector3Int position, Players.Player owner, int maxDistance)
        {
            int closest = int.MaxValue;
            var retVal = new List<Vector3Int>();

            foreach (var machine in Machines[owner])
            {
                var dis = Pipliz.Math.RoundToInt(Vector3.Distance(machine.Position.Vector, position.Vector));

                if (dis <= maxDistance && dis <= closest)
                    retVal.Add(machine.Position);
            }

            return retVal;
        }

        public static ushort Refuel(Players.Player player, MachineState machineState)
        {
            if (machineState.Fuel < .75f)
            {
                if (!MachineState.MAX_FUEL.ContainsKey(player))
                    MachineState.MAX_FUEL[player] = MachineState.DEFAULT_MAX_FUEL;

                var stockpile = Stockpile.GetStockPile(player);

                foreach (var item in FuelValues)
                {
                    while ((stockpile.AmountContained(item.Key) > 100 ||
                            item.Key == BuiltinBlocks.Firewood ||
                            item.Key == BuiltinBlocks.Coalore)&& machineState.Fuel < MachineState.MAX_FUEL[player])
                    {
                        stockpile.TryRemove(item.Key);
                        machineState.Fuel += item.Value;
                    }
                }

                if (machineState.Fuel < MachineState.MAX_FUEL[player])
                    return FuelValues.First().Key;
            }

            return GameLoader.Refuel_Icon;
        }
    }
}
