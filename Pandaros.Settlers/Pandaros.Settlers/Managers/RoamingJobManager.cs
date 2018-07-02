using BlockTypes.Builtin;
using Pandaros.Settlers.Items.Machines;
using Pandaros.Settlers.Jobs.Roaming;
using Pipliz;
using Pipliz.JSON;
using Server;
using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using Math = Pipliz.Math;
using Time = Pipliz.Time;

namespace Pandaros.Settlers.Managers
{
    [ModLoader.ModManager]
    public static class RoamingJobManager
    {
        private const int OBJECTIVE_REFRESH = 1;
        public static string MACHINE_JSON = "";

        public static Dictionary<string, IRoamingJobObjective> ObjectiveCallbacks = new Dictionary<string, IRoamingJobObjective>(StringComparer.OrdinalIgnoreCase);

        
        private static double _nextUpdate;

        public static Dictionary<Players.Player, Dictionary<Vector3Int, RoamingJobState>> Objectives { get; } = new Dictionary<Players.Player, Dictionary<Vector3Int, RoamingJobState>>();

        public static event EventHandler ObjectiveRemoved;

        public static void RegisterObjectiveType(IRoamingJobObjective objective)
        {
            ObjectiveCallbacks[objective.Name] = objective;
        }

        public static IRoamingJobObjective GetCallbacks(string objectiveName)
        {
            if (ObjectiveCallbacks.ContainsKey(objectiveName))
                return ObjectiveCallbacks[objectiveName];

            PandaLogger.Log($"Unknown objective {objectiveName}. Returning {nameof(Miner)}.");
            return ObjectiveCallbacks[nameof(Miner)];
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnUpdate, GameLoader.NAMESPACE + ".Managers.RoamingJobManager.OnUpdate")]
        public static void OnUpdate()
        {
            if (GameLoader.WorldLoaded && _nextUpdate < Time.SecondsSinceStartDouble)
            {
                lock (Objectives)
                {
                    foreach (var machine in Objectives)
                        if (!machine.Key.IsConnected && Configuration.OfflineColonies || machine.Key.IsConnected)
                            foreach (var state in machine.Value)
                                try
                                {
                                    state.Value.RoamingJobSettings.DoWork(machine.Key, state.Value);

                                    foreach (var objectiveLoad in state.Value.ActionEnergy)
                                    {
                                        if (objectiveLoad.Value <= 0)
                                            Indicator.SendIconIndicatorNear(state.Value.Position.Add(0, 1, 0).Vector, 
                                                                            new IndicatorState(OBJECTIVE_REFRESH,
                                                                            state.Value.RoamingJobSettings.ActionCallbacks[objectiveLoad.Key].ObjectiveLoadEmptyIcon, 
                                                                            true,
                                                                            false));
                                    }
                                }
                                catch (Exception ex)
                                {
                                    PandaLogger.LogError(ex);
                                }
                }

                _nextUpdate = Time.SecondsSinceStartDouble + OBJECTIVE_REFRESH;
            }
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnLoadingPlayer, GameLoader.NAMESPACE + ".Managers.RoamingJobManager.OnLoadingPlayer")]
        public static void OnLoadingPlayer(JSONNode n, Players.Player p)
        {
            if (p.ID == null || p.ID.steamID == null || p.ID.type == NetworkID.IDType.Server)
                return;

            if (n.TryGetChild(GameLoader.NAMESPACE + ".Objectives", out var objectivesNode) || n.TryGetChild(GameLoader.NAMESPACE + ".Machines", out objectivesNode))
                lock (Objectives)
                {
                    foreach (var node in objectivesNode.LoopArray())
                        RegisterRoamingJobState(p, new RoamingJobState(node, p));

                    if (Objectives.ContainsKey(p))
                        PandaLogger.Log(ChatColor.lime, $"{Objectives[p].Count} objectives loaded from save for {p.ID.steamID.m_SteamID}!");
                    else
                        PandaLogger.Log(ChatColor.lime, $"No objectives found in save for {p.ID.steamID.m_SteamID}.");
                }
            else
                PandaLogger.Log(ChatColor.lime, $"No objectives found in save for {p.ID.steamID.m_SteamID}.");
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnSavingPlayer, GameLoader.NAMESPACE + ".Managers.RoamingJobManager.PatrolTool.OnSavingPlayer")]
        public static void OnSavingPlayer(JSONNode n, Players.Player p)
        {
            lock (Objectives)
            {
                if (Objectives.ContainsKey(p))
                {
                    if (n.HasChild(GameLoader.NAMESPACE + ".Objectives"))
                        n.RemoveChild(GameLoader.NAMESPACE + ".Objectives");

                    var objectiveNode = new JSONNode(NodeType.Array);

                    foreach (var node in Objectives[p])
                        objectiveNode.AddToArray(node.Value.ToJsonNode());

                    n[GameLoader.NAMESPACE + ".Objectives"] = objectiveNode;
                }
            }
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnTryChangeBlock,  GameLoader.NAMESPACE + ".Managers.RoamingJobManager.OnTryChangeBlockUser")]
        public static void OnTryChangeBlockUser(ModLoader.OnTryChangeBlockData d)
        {
            if (d.CallbackState == ModLoader.OnTryChangeBlockData.ECallbackState.Cancelled)
                return;

            if (d.TypeNew == BuiltinBlocks.Air && d.RequestedByPlayer != null)
                RemoveObjective(d.RequestedByPlayer, d.Position);
        }

        public static void RemoveObjective(Players.Player p, Vector3Int pos, bool throwEvent = true)
        {
            lock (Objectives)
            {
                if (!Objectives.ContainsKey(p))
                    Objectives.Add(p, new Dictionary<Vector3Int, RoamingJobState>());


                if (Objectives[p].ContainsKey(pos))
                {
                    var mach = Objectives[p][pos];

                    Objectives[p].Remove(pos);

                    if (throwEvent && ObjectiveRemoved != null)
                        ObjectiveRemoved(mach, new EventArgs());
                }
            }
        }

        public static void RegisterRoamingJobState(Players.Player player, RoamingJobState state)
        {
            lock (Objectives)
            {
                if (!Objectives.ContainsKey(player))
                    Objectives.Add(player, new Dictionary<Vector3Int, RoamingJobState>());

                Objectives[player][state.Position] = state;
            }
        }

        public static List<Vector3Int> GetClosestObjective(Vector3Int position, Players.Player owner, int maxDistance, string category)
        {
            var closest = int.MaxValue;
            var retVal  = new List<Vector3Int>();

            foreach (var machine in Objectives[owner].Where(o => o.Value.RoamingJobSettings.ObjectiveCategory == category))
            {
                var dis = Math.RoundToInt(UnityEngine.Vector3.Distance(machine.Key.Vector, position.Vector));

                if (dis <= maxDistance && dis <= closest)
                    retVal.Add(machine.Key);
            }

            return retVal;
        }
    }
}