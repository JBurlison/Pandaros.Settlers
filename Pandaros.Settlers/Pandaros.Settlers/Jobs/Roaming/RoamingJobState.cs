using System.Collections.Generic;
using Pandaros.Settlers.Jobs;
using Pandaros.Settlers.Managers;
using Pipliz;
using Pipliz.Collections;
using Pipliz.JSON;
using Random = System.Random;

namespace Pandaros.Settlers.Jobs.Roaming
{
    public class RoamingJobState
    {
        public const float DEFAULT_MAX = 1f;
        private static readonly Random _rand = new Random();

        public RoamingJobState(Vector3Int pos, Players.Player owner, string machineType, IRoamingJobObjective settings = null)
        {
            Position    = pos;
            RoamObjective = machineType;
            Owner       = owner;

            if (settings == null)
                RoamingJobSettings = RoamingJobManager.GetCallbacks(machineType);
            else
                RoamingJobSettings = settings;

            Initialize();
        }

        public RoamingJobState(JSONNode baseNode, Players.Player owner)
        {
            Position = (Vector3Int) baseNode[nameof(Position)];
            Owner = owner;

            if (baseNode.TryGetAs<string>(nameof(RoamObjective), out var ro) || baseNode.TryGetAs("MachineType", out ro))
                RoamObjective = ro; 

            if (baseNode.TryGetAs(nameof(ActionLoad), out JSONNode ItemsRemovedNode) && ItemsRemovedNode.NodeType == NodeType.Object)
                foreach (var aNode in ItemsRemovedNode.LoopObject())
                    ActionLoad.Add(aNode.Key, aNode.Value.GetAs<int>());

            RoamingJobSettings = RoamingJobManager.GetCallbacks(RoamObjective);
            Initialize();
        }


        /// <summary>
        ///     Key: IRoamingJobAction.Name
        ///     Key: Players.Player
        ///     Key: Category
        ///     Value: Max Action Load
        /// </summary>
        static Dictionary<string, Dictionary<Players.Player, Dictionary<string, float>>> _maxActionLoad = new Dictionary<string, Dictionary<Players.Player, Dictionary<string, float>>>();

        public static float GetMaxLoad(string actionName, Players.Player player, string category)
        {
            if (!_maxActionLoad.ContainsKey(actionName))
                _maxActionLoad.Add(actionName, new Dictionary<Players.Player, Dictionary<string, float>>());

            if (!_maxActionLoad[actionName].ContainsKey(player))
                _maxActionLoad[actionName].Add(player, new Dictionary<string, float>());

            if (!_maxActionLoad[actionName][player].ContainsKey(category))
                _maxActionLoad[actionName][player].Add(category, DEFAULT_MAX);

            return _maxActionLoad[actionName][player][category];
        }

        public static void SetMaxLoad(string actionName, Players.Player player, string category, float maxLoad)
        {
            if (!_maxActionLoad.ContainsKey(actionName))
                _maxActionLoad.Add(actionName, new Dictionary<Players.Player, Dictionary<string, float>>());

            if (!_maxActionLoad[actionName].ContainsKey(player))
                _maxActionLoad[actionName].Add(player, new Dictionary<string, float>());

            if (!_maxActionLoad[actionName][player].ContainsKey(category))
                _maxActionLoad[actionName][player].Add(category, DEFAULT_MAX);

            _maxActionLoad[actionName][player][category] = maxLoad;
        }

        public Vector3Int Position { get; } = Vector3Int.invalidPos;

        /// <summary>
        ///     Key: IRoamingJobAction.Name
        ///     Value: Current Action Load
        /// </summary>
        public Dictionary<string, float> ActionLoad { get; set; } = new Dictionary<string, float>();
        public string RoamObjective { get; }
        public Players.Player Owner { get; }
        public double NextTimeForWork { get; set; } = Time.SecondsSinceStartDouble + _rand.NextDouble(0, 5);
        public RoamingJob JobRef { get; set; }

        public IRoamingJobObjective RoamingJobSettings { get; }

        public BoxedDictionary TempValues { get; } = new BoxedDictionary();

        private void Initialize()
        {
            foreach (var roamAction in RoamingJobSettings.ActionCallbacks.Values)
                SetMaxLoad(roamAction.Name, Owner, RoamingJobSettings.ObjectiveCategory, DEFAULT_MAX);
        }

        public bool PositionIsValid()
        {
            if (Position != null && World.TryGetTypeAt(Position, out var objType))
            {
#if Debug
                PandaLogger.Log(ChatColor.lime, $"PositionIsValid {ItemTypes.IndexLookup.GetName(objType)}. POS {Position}");
#endif
                return objType == RoamingJobSettings.ItemIndex;
            }
#if Debug
                PandaLogger.Log(ChatColor.lime, $"PositionIsValid Trt Get Failed {Position == null}.");
#endif
            return Position != null;
        }

        public virtual JSONNode ToJsonNode()
        {
            var baseNode = new JSONNode();
            var actionLoadNode = new JSONNode();

            baseNode.SetAs(nameof(Position), (JSONNode) Position);

            foreach (var kvp in ActionLoad)
                actionLoadNode.SetAs(kvp.Key.ToString(), kvp.Value);

            baseNode.SetAs(nameof(RoamObjective), RoamObjective); //MachineType

            return baseNode;
        }
    }
}