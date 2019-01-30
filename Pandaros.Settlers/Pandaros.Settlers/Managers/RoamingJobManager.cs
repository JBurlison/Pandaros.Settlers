using BlockTypes;
using Pandaros.Settlers.Items.Machines;
using Pandaros.Settlers.Jobs.Roaming;
using Pipliz;
using Pipliz.JSON;
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
        public static Dictionary<ushort, IRoamingJobObjective> ObjectiveCallbacksIndex = new Dictionary<ushort, IRoamingJobObjective>();


        private static double _nextUpdate;

        public static Dictionary<Colony, Dictionary<Vector3Int, RoamingJobState>> Objectives { get; } = new Dictionary<Colony, Dictionary<Vector3Int, RoamingJobState>>();

        public static event EventHandler ObjectiveRemoved;

        public static void RegisterObjectiveType(IRoamingJobObjective objective)
        {
            ObjectiveCallbacks[objective.name] = objective;
            ObjectiveCallbacksIndex[objective.ItemIndex] = objective;
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
                        if (!machine.Key.OwnerIsOnline() && Configuration.OfflineColonies || machine.Key.OwnerIsOnline())
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

        static Dictionary<Colony, JSONNode> _loadedColonies = new Dictionary<Colony, JSONNode>();

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnLoadingColony, GameLoader.NAMESPACE + ".Managers.RoamingJobManager.OnLoadingColony")]
        public static void OnLoadingColony(Colony c, JSONNode n)
        {
            if (c.ColonyID <= 0)
                return;

            _loadedColonies[c] = n;
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterItemTypesDefined, GameLoader.NAMESPACE + ".Managers.RoamingJobManager.AfterItemTypesDefined")]
        [ModLoader.ModCallbackDependsOn(GameLoader.NAMESPACE + ".Extender.SettlersExtender.AfterItemTypesDefined")]
        public static void AfterItemTypesDefined()
        {
            PandaLogger.Log("{0} objective types loaded", ObjectiveCallbacks.Count);

            foreach (var cKvp in _loadedColonies)
            {
                var c = cKvp.Key;
                var n = cKvp.Value;

                if (n.TryGetChild(GameLoader.NAMESPACE + ".Objectives", out var objectivesNode))
                    lock (Objectives)
                    {
                        int countLoaded = 0;
                        foreach (var node in objectivesNode.LoopArray())
                            try
                            {
                                RegisterRoamingJobState(c, new RoamingJobState(node, c));
                                countLoaded++;
                            }
                            catch (Exception ex)
                            {
                                PandaLogger.LogError(ex, node.ToString());
                            }

                        if (Objectives.ContainsKey(c))
                            PandaLogger.Log(ChatColor.lime, $"{countLoaded} of {Objectives[c].Count} objectives loaded from save for {c.ColonyID}!");
                        else
                            PandaLogger.Log(ChatColor.lime, $"No objectives found in save for {c.ColonyID}.");
                    }
                else
                    PandaLogger.Log(ChatColor.lime, $"No objectives found in save for {c.ColonyID}.");
            }
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnSavingColony, GameLoader.NAMESPACE + ".Managers.RoamingJobManager.PatrolTool.OnSavingColony")]
        public static void OnSavingColony(Colony c, JSONNode n)
        {
            try
            {
                lock (Objectives)
                {
                    if (Objectives.ContainsKey(c))
                    {
                        if (n.HasChild(GameLoader.NAMESPACE + ".Objectives"))
                            n.RemoveChild(GameLoader.NAMESPACE + ".Objectives");

                        var objectiveNode = new JSONNode(NodeType.Array);

                        foreach (var node in Objectives[c])
                            objectiveNode.AddToArray(node.Value.ToJsonNode());

                        n[GameLoader.NAMESPACE + ".Objectives"] = objectiveNode;
                    }
                }
            }
            catch (Exception ex)
            {
                PandaLogger.LogError(ex);
            }
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnTryChangeBlock,  GameLoader.NAMESPACE + ".Managers.RoamingJobManager.OnTryChangeBlockUser")]
        public static void OnTryChangeBlockUser(ModLoader.OnTryChangeBlockData d)
        {
            if (d.CallbackState == ModLoader.OnTryChangeBlockData.ECallbackState.Cancelled ||
                d.RequestOrigin.AsPlayer == null ||
                d.RequestOrigin.AsPlayer.ID.type == NetworkID.IDType.Server ||
                d.RequestOrigin.AsPlayer.ID.type == NetworkID.IDType.Invalid ||
                d.RequestOrigin.AsPlayer.ActiveColony == null)
                    return;

            if (d.TypeNew.ItemIndex == ColonyBuiltIn.ItemTypes.AIR.Id)
                RemoveObjective(d.RequestOrigin.AsPlayer.ActiveColony, d.Position);
            else if (ObjectiveCallbacksIndex.TryGetValue(d.TypeNew.ItemIndex, out var objective))
                RegisterRoamingJobState(d.RequestOrigin.AsPlayer.ActiveColony, new RoamingJobState(d.Position, d.RequestOrigin.AsPlayer.ActiveColony, objective.name));
        }

        public static void RemoveObjective(Colony c, Vector3Int pos, bool throwEvent = true)
        {
            lock (Objectives)
            {
                if (!Objectives.ContainsKey(c))
                    Objectives.Add(c, new Dictionary<Vector3Int, RoamingJobState>());

                if (Objectives[c].ContainsKey(pos))
                {
                    var mach = Objectives[c][pos];

                    Objectives[c].Remove(pos);

                    if (throwEvent && ObjectiveRemoved != null)
                        ObjectiveRemoved(mach, new EventArgs());
                }
            }
        }

        public static void RegisterRoamingJobState(Colony colony, RoamingJobState state)
        {
            lock (Objectives)
            {
                if (!Objectives.ContainsKey(colony))
                    Objectives.Add(colony, new Dictionary<Vector3Int, RoamingJobState>());

                Objectives[colony][state.Position] = state;
            }
        }

        public static List<Vector3Int> GetClosestObjective(Vector3Int position, Colony owner, int maxDistance, string category)
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