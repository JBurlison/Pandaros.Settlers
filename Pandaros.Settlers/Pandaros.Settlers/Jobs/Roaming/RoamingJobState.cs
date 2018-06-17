using System.Collections.Generic;
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

            if (baseNode.TryGetAs(nameof(ActionEnergy), out JSONNode ItemsRemovedNode) && ItemsRemovedNode.NodeType == NodeType.Object)
                foreach (var aNode in ItemsRemovedNode.LoopObject())
                    ActionEnergy.Add(aNode.Key, aNode.Value.GetAs<int>());

            RoamingJobSettings = RoamingJobManager.GetCallbacks(RoamObjective);
            Initialize();
        }


        /// <summary>
        ///     Key: IRoamingJobAction.Name
        ///     Key: Players.Player
        ///     Key: Category
        ///     Value: Max Action Energy
        /// </summary>
        static Dictionary<string, Dictionary<Players.Player, Dictionary<string, float>>> _maxActionEnergy = new Dictionary<string, Dictionary<Players.Player, Dictionary<string, float>>>();

        public Dictionary<string, float> ActionEnergy { get; private set; } = new Dictionary<string, float>();
        public Vector3Int Position { get; } = Vector3Int.invalidPos;
        public string RoamObjective { get; }
        public Players.Player Owner { get; }
        public double NextTimeForWork { get; set; } = Time.SecondsSinceStartDouble + _rand.NextDouble(0, 5);
        public RoamingJob JobRef { get; set; }

        public IRoamingJobObjective RoamingJobSettings { get; }

        public BoxedDictionary TempValues { get; } = new BoxedDictionary();

        private void Initialize()
        {
            foreach (var roamAction in RoamingJobSettings.ActionCallbacks.Values)
                SetActionsMaxEnergy(roamAction.Name, Owner, RoamingJobSettings.ObjectiveCategory, DEFAULT_MAX);
        }

        public void ResetActionToMaxLoad(string action)
        {
            SetActionEnergy(action, GetActionsMaxEnergy(action, Owner, RoamingJobSettings.ObjectiveCategory));
        }

        public float GetActionEnergy(string action)
        {
            if (!ActionEnergy.ContainsKey(action))
                ActionEnergy.Add(action, GetActionsMaxEnergy(action, Owner, RoamingJobSettings.ObjectiveCategory));

            return ActionEnergy[action];
        }

        public void SetActionEnergy(string action, float value)
        {
            ActionEnergy[action] = value;

            if (ActionEnergy[action] < 0)
                ActionEnergy[action] = 0;
        }

        public void AddToActionEmergy(string action, float value)
        {
            if (!ActionEnergy.ContainsKey(action))
                ActionEnergy.Add(action, GetActionsMaxEnergy(action, Owner, RoamingJobSettings.ObjectiveCategory));

            ActionEnergy[action] += value;
        }

        public void SubtractFromActionEnergy(string action, float value)
        {
            if (!ActionEnergy.ContainsKey(action))
                ActionEnergy.Add(action, GetActionsMaxEnergy(action, Owner, RoamingJobSettings.ObjectiveCategory));

            ActionEnergy[action] -= value;

            if (ActionEnergy[action] < 0)
                ActionEnergy[action] = 0;
        }

        public static float GetActionsMaxEnergy(string actionName, Players.Player player, string category)
        {
            if (!_maxActionEnergy.ContainsKey(actionName))
                _maxActionEnergy.Add(actionName, new Dictionary<Players.Player, Dictionary<string, float>>());

            if (!_maxActionEnergy[actionName].ContainsKey(player))
                _maxActionEnergy[actionName].Add(player, new Dictionary<string, float>());

            if (!_maxActionEnergy[actionName][player].ContainsKey(category))
                _maxActionEnergy[actionName][player].Add(category, DEFAULT_MAX);

            return _maxActionEnergy[actionName][player][category];
        }

        public static void SetActionsMaxEnergy(string actionName, Players.Player player, string category, float maxLoad)
        {
            if (!_maxActionEnergy.ContainsKey(actionName))
                _maxActionEnergy.Add(actionName, new Dictionary<Players.Player, Dictionary<string, float>>());

            if (!_maxActionEnergy[actionName].ContainsKey(player))
                _maxActionEnergy[actionName].Add(player, new Dictionary<string, float>());

            if (!_maxActionEnergy[actionName][player].ContainsKey(category))
                _maxActionEnergy[actionName][player].Add(category, DEFAULT_MAX);

            _maxActionEnergy[actionName][player][category] = maxLoad;
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

            foreach (var kvp in ActionEnergy)
                actionLoadNode.SetAs(kvp.Key.ToString(), kvp.Value);

            baseNode.SetAs(nameof(ActionEnergy), actionLoadNode);
            baseNode.SetAs(nameof(RoamObjective), RoamObjective); //MachineType

            return baseNode;
        }
    }
}