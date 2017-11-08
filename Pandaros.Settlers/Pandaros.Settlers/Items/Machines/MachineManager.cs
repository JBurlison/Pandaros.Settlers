using BlockTypes.Builtin;
using NPC;
using Pipliz;
using Pipliz.JSON;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pandaros.Settlers.Items.Machines
{
    [ModLoader.ModManager]
    public static class MachineManager
    {
        public class MachineCallback
        {
            public Action<NPCBase.NPCState, Players.Player, MachineState> Repair { get; set; }
            public Action<NPCBase.NPCState, Players.Player, MachineState> Refuel { get; set; }
            public Action<Players.Player, MachineState> DoWork { get; set; }

            public MachineCallback(Action<NPCBase.NPCState, Players.Player, MachineState> repair, Action<NPCBase.NPCState, Players.Player, MachineState> refuel, Action<Players.Player, MachineState> doWork)
            {
                Repair = repair;
                Refuel = refuel;
                DoWork = doWork;
            }
        }

        public static Dictionary<Players.Player, Dictionary<Vector3Int, MachineState>> Machines { get; private set; } = new Dictionary<Players.Player, Dictionary<Vector3Int, MachineState>>();
        public static DateTime _nextUpdateTime;
        public static Dictionary<string, MachineCallback> _machineCallbacks = new Dictionary<string, MachineCallback>();


        public static void RegisterMachineType(string machineType, MachineCallback callback)
        {
            _machineCallbacks[machineType] = callback;
        }

        public static MachineCallback GetCallback(string machineType)
        {
            return _machineCallbacks[machineType];
        }
           

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnUpdate, GameLoader.NAMESPACE + ".Items.Machines.MacineManager.OnUpdate")]
        public static void OnUpdate()
        {
            if (GameLoader.WorldLoaded && DateTime.Now > _nextUpdateTime)
            {
                lock(Machines)
                    foreach(var machine in Machines)
                    {
                        foreach (var state in machine.Value)
                            state.Value.DoWork();
                    }

                _nextUpdateTime = DateTime.Now + TimeSpan.FromSeconds(1);
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
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnSavingPlayer, GameLoader.NAMESPACE + ".Items.Machines.Miner.PatrolTool.OnSavingPlayer")]
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

                    n[GameLoader.NAMESPACE + ".Miners"] = minersNode;
                }
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnTryChangeBlockUser, GameLoader.NAMESPACE + ".Items.Machines.MachineManager.OnTryChangeBlockUser")]
        public static bool OnTryChangeBlockUser(ModLoader.OnTryChangeBlockUserData d)
        {
            lock (Machines)
            {
                if (!Machines.ContainsKey(d.requestedBy))
                    Machines.Add(d.requestedBy, new Dictionary<Vector3Int, MachineState>());

                if (d.typeToBuild == BuiltinBlocks.Air && Machines[d.requestedBy].ContainsKey(d.voxelHit))
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
    }
}
