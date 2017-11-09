using BlockTypes.Builtin;
using NPC;
using Pandaros.Settlers.Entities;
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

        public static Dictionary<Players.Player, Dictionary<Vector3Int, MachineState>> Machines { get; private set; } = new Dictionary<Players.Player, Dictionary<Vector3Int, MachineState>>();
        public static double _nextUpdateTime;
        public static Dictionary<string, MachineSettings> _machineCallbacks = new Dictionary<string, MachineSettings>();
        public static Dictionary<ushort, float> FuelValues = new Dictionary<ushort, float>();

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
            FuelValues[BuiltinBlocks.Coalore] = .20f;
            FuelValues[Items.ItemFactory.Firewood] = .10f;
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnUpdate, GameLoader.NAMESPACE + ".Items.Machines.MacineManager.OnUpdate")]
        public static void OnUpdate()
        {
            lock(Machines)
                foreach(var machine in Machines)
                {
                    foreach (var state in machine.Value)
                        state.Value.MachineSettings.DoWork(machine.Key, state.Value);
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
                }
            }
            else
                PandaLogger.Log("No Machines.");
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnSavingPlayer, GameLoader.NAMESPACE + ".Items.Machines.MachineManager.PatrolTool.OnSavingPlayer")]
        public static void OnSavingPlayer(JSONNode n, Players.Player p)
        {
            lock (Machines)
                if (Machines.ContainsKey(p))
                {
                    if (n.HasChild(GameLoader.NAMESPACE + ".Machines"))
                        n.RemoveChild(GameLoader.NAMESPACE + ".Machines");

                    var minersNode = new JSONNode(NodeType.Array);

                    foreach (var node in Machines[p])
                        minersNode.AddToArray(node.Value.ToJsonNode());

                    n[GameLoader.NAMESPACE + ".Machines"] = minersNode;
                }
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnTryChangeBlockUser, GameLoader.NAMESPACE + ".Items.Machines.MachineManager.OnTryChangeBlockUser")]
        public static bool OnTryChangeBlockUser(ModLoader.OnTryChangeBlockUserData d)
        {
            lock (Machines)
            {
                if (!Machines.ContainsKey(d.requestedBy))
                    Machines.Add(d.requestedBy, new Dictionary<Vector3Int, MachineState>());

                if (Machines[d.requestedBy].ContainsKey(d.voxelHit))
                    Machines[d.requestedBy].Remove(d.voxelHit);
            }

            return true;
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
            int closest = int.MaxValue;
            var retVal = new List<Vector3Int>();

            foreach (var machine in Machines[owner])
            {
                var dis = Pipliz.Math.RoundToInt(Vector3.Distance(machine.Key.Vector, position.Vector));

                if (dis <= maxDistance && dis <= closest)
                    retVal.Add(machine.Key);
            }

            return retVal;
        }

        public static ushort Refuel(Players.Player player, MachineState machineState)
        {
            if (machineState.Fuel < .75f)
            {
                var stockpile = Stockpile.GetStockPile(player);

                foreach (var item in MachineManager.FuelValues)
                {
                    while (stockpile.Contains(item.Key) && machineState.Fuel < MachineState.MAX_FUEL)
                    {
                        if (stockpile.TryRemove(item.Key))
                            machineState.Fuel += item.Value;
                    }

                    if (machineState.Fuel > MachineState.MAX_FUEL)
                        break;
                }

                if (machineState.Fuel < MachineState.MAX_FUEL)
                    return MachineManager.FuelValues.First().Key;
            }

            return GameLoader.Refuel_Icon;
        }
    }
}
