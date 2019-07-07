using BlockTypes;
using Pandaros.Settlers.Items.Machines;
using Pandaros.Settlers.Models;
using Pipliz;
using Pipliz.JSON;
using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using Math = Pipliz.Math;
using Time = Pipliz.Time;

namespace Pandaros.Settlers.Jobs.Roaming
{
    [ModLoader.ModManager]
    public static class RoamingJobManager
    {
        private const int OBJECTIVE_REFRESH = 1;
        public static string MACHINE_JSON = "";

        public static Dictionary<string, IRoamingJobObjective> ObjectiveCallbacks = new Dictionary<string, IRoamingJobObjective>(StringComparer.OrdinalIgnoreCase);


        private static double _nextUpdate;

        public static Dictionary<Colony, Dictionary<string, Dictionary<Vector3Int, RoamingJobState>>> Objectives { get; } = new Dictionary<Colony, Dictionary<string, Dictionary<Vector3Int, RoamingJobState>>>();

        public static event EventHandler<RoamingJobState> ObjectiveRemoved;

        public static void RegisterObjectiveType(IRoamingJobObjective objective)
        {
            ObjectiveCallbacks[objective.ItemIndex] = objective;
        }

        public static IRoamingJobObjective GetCallbacks(string objectiveName)
        {
            if (ObjectiveCallbacks.ContainsKey(objectiveName))
                return ObjectiveCallbacks[objectiveName];

            PandaLogger.Log($"Unknown objective {objectiveName}. Returning {GameLoader.NAMESPACE + ".Miner"}.");
            return ObjectiveCallbacks[GameLoader.NAMESPACE + ".Miner"];
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnUpdate, GameLoader.NAMESPACE + ".Managers.RoamingJobManager.OnUpdate")]
        public static void OnUpdate()
        {
            if (GameLoader.WorldLoaded && _nextUpdate < Time.SecondsSinceStartDouble)
            {
                lock (Objectives)
                {
                    foreach (var machine in Objectives)
                        foreach (var category in machine.Value)
                        {
                            var invalidKeys = new List<Vector3Int>();

                            foreach (var state in category.Value)
                                try
                                {
                                    if (!state.Value.PositionIsValid())
                                        invalidKeys.Add(state.Key);
                                    else
                                    {
                                        state.Value.RoamingJobSettings.DoWork(machine.Key, state.Value);

                                        foreach (var objectiveLoad in state.Value.ActionEnergy)
                                        {
                                            if (objectiveLoad.Value <= 0 &&
                                                state.Value.RoamingJobSettings.ActionCallbacks.TryGetValue(objectiveLoad.Key, out var objectiveAction))
                                                Indicator.SendIconIndicatorNear(state.Value.Position.Add(0, 1, 0).Vector,
                                                                                new IndicatorState(OBJECTIVE_REFRESH,
                                                                                objectiveAction.ObjectiveLoadEmptyIcon.Id,
                                                                                true,
                                                                                false));
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    PandaLogger.LogError(ex);
                                }

                            foreach (var key in invalidKeys)
                                category.Value.Remove(key);
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
                            foreach (var catNode in node.Value)
                            objectiveNode.AddToArray(catNode.Value.ToJsonNode());

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

            IRoamingJobObjective roamingJobObjective = null;

            if (d.TypeNew.ItemIndex == ColonyBuiltIn.ItemTypes.AIR.Id)
                RemoveObjective(d.RequestOrigin.AsPlayer.ActiveColony, d.Position);
            else if (ItemTypes.TryGetType(d.TypeNew.ItemIndex, out ItemTypes.ItemType item))
            {
                if (ObjectiveCallbacks.TryGetValue(item.Name, out roamingJobObjective))
                    RegisterRoamingJobState(d.RequestOrigin.AsPlayer.ActiveColony, new RoamingJobState(d.Position, d.RequestOrigin.AsPlayer.ActiveColony, roamingJobObjective.ItemIndex));
                else if (!string.IsNullOrEmpty(item.ParentType) && ObjectiveCallbacks.TryGetValue(item.ParentType, out roamingJobObjective))
                    RegisterRoamingJobState(d.RequestOrigin.AsPlayer.ActiveColony, new RoamingJobState(d.Position, d.RequestOrigin.AsPlayer.ActiveColony, roamingJobObjective.ItemIndex));
            }
        }

        public static void RemoveObjective(Colony c, Vector3Int pos, bool throwEvent = true)
        {
            lock (Objectives)
            {
                if (!Objectives.ContainsKey(c))
                    Objectives.Add(c, new Dictionary<string, Dictionary<Vector3Int, RoamingJobState>>());

                foreach (var item in Objectives[c])
                    if (item.Value.TryGetValue(pos, out var state))
                    {
                        item.Value.Remove(pos);

                        if (throwEvent && ObjectiveRemoved != null)
                            ObjectiveRemoved(null, state);

                        break;
                    }
            }
        }

        public static void RegisterRoamingJobState(Colony colony, RoamingJobState state)
        {
            if (colony != null && state != null)
                lock (Objectives)
                {
                    if (!Objectives.ContainsKey(colony))
                        Objectives.Add(colony, new Dictionary<string, Dictionary<Vector3Int, RoamingJobState>>());

                    if (!Objectives[colony].ContainsKey(state.RoamingJobSettings.ObjectiveCategory))
                        Objectives[colony].Add(state.RoamingJobSettings.ObjectiveCategory, new Dictionary<Vector3Int, RoamingJobState>());

                    if (state.Position != Vector3Int.invalidPos)
                        Objectives[colony][state.RoamingJobSettings.ObjectiveCategory][state.Position] = state;
                }
        }

        public static List<Vector3Int> GetClosestObjective(Vector3Int position, Colony owner, int maxDistance, string category)
        {
            var closest = int.MaxValue;
            var retVal  = new List<Vector3Int>();

            if (Objectives.ContainsKey(owner) && Objectives[owner].ContainsKey(category))
                foreach (var machine in Objectives[owner][category])
                {
                    var dis = Math.RoundToInt(UnityEngine.Vector3.Distance(machine.Key.Vector, position.Vector));

                    if (dis <= maxDistance && dis <= closest)
                        retVal.Add(machine.Key);
                }

            return retVal;
        }
    }
}