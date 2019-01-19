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
        
        public RoamingJobState(Vector3Int pos, Colony colony, string objectiveType, IRoamingJobObjective settings = null)
        {
            Position    = pos;
            RoamObjective = objectiveType;
            Colony       = colony;

            if (settings == null)
                RoamingJobSettings = RoamingJobManager.GetCallbacks(objectiveType);
            else
                RoamingJobSettings = settings;

            Initialize();
        }

        public RoamingJobState(JSONNode baseNode, Colony colony)
        {
            Position = (Vector3Int)baseNode[nameof(Position)];
            Colony = colony;

            if (baseNode.TryGetAs<string>(nameof(RoamObjective), out var ro))
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
        static Dictionary<string, Dictionary<Colony, Dictionary<string, float>>> _maxActionEnergy = new Dictionary<string, Dictionary<Colony, Dictionary<string, float>>>();

        public Dictionary<string, float> ActionEnergy { get; private set; } = new Dictionary<string, float>();
        public Vector3Int Position { get; } = Vector3Int.invalidPos;
        public string RoamObjective { get; }
        public Colony Colony { get; }
        public double NextTimeForWork { get; set; } = Time.SecondsSinceStartDouble + _rand.NextDouble(0, 5);
        public RoamingJob JobRef { get; set; }

        public IRoamingJobObjective RoamingJobSettings { get; }

        public BoxedDictionary TempValues { get; } = new BoxedDictionary();

        private void Initialize()
        {
            foreach (var roamAction in RoamingJobSettings.ActionCallbacks.Values)
                SetActionsMaxEnergy(roamAction.name, Colony, RoamingJobSettings.ObjectiveCategory, DEFAULT_MAX);
        }

        public void ResetActionToMaxLoad(string action)
        {
            SetActionEnergy(action, GetActionsMaxEnergy(action, Colony, RoamingJobSettings.ObjectiveCategory));
        }

        public float GetActionEnergy(string action)
        {
            if (!ActionEnergy.ContainsKey(action))
                ActionEnergy.Add(action, GetActionsMaxEnergy(action, Colony, RoamingJobSettings.ObjectiveCategory));

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
                ActionEnergy.Add(action, GetActionsMaxEnergy(action, Colony, RoamingJobSettings.ObjectiveCategory));

            ActionEnergy[action] += value;
        }

        public void SubtractFromActionEnergy(string action, float value)
        {
            if (!ActionEnergy.ContainsKey(action))
                ActionEnergy.Add(action, GetActionsMaxEnergy(action, Colony, RoamingJobSettings.ObjectiveCategory));

            ActionEnergy[action] -= value;

            if (ActionEnergy[action] < 0)
                ActionEnergy[action] = 0;
        }

        public static float GetActionsMaxEnergy(string actionName, Colony colony, string category)
        {
            if (!_maxActionEnergy.ContainsKey(actionName))
                _maxActionEnergy.Add(actionName, new Dictionary<Colony, Dictionary<string, float>>());

            if (!_maxActionEnergy[actionName].ContainsKey(colony))
                _maxActionEnergy[actionName].Add(colony, new Dictionary<string, float>());

            if (!_maxActionEnergy[actionName][colony].ContainsKey(category))
                _maxActionEnergy[actionName][colony].Add(category, DEFAULT_MAX);

            return _maxActionEnergy[actionName][colony][category];
        }

        public static void SetActionsMaxEnergy(string actionName, Colony colony, string category, float maxLoad)
        {
            if (!_maxActionEnergy.ContainsKey(actionName))
                _maxActionEnergy.Add(actionName, new Dictionary<Colony, Dictionary<string, float>>());

            if (!_maxActionEnergy[actionName].ContainsKey(colony))
                _maxActionEnergy[actionName].Add(colony, new Dictionary<string, float>());

            if (!_maxActionEnergy[actionName][colony].ContainsKey(category))
                _maxActionEnergy[actionName][colony].Add(category, DEFAULT_MAX);

            _maxActionEnergy[actionName][colony][category] = maxLoad;
        }

        public bool PositionIsValid()
        {
            if (Position != null && World.TryGetTypeAt(Position, out ushort objType))
                return objType == RoamingJobSettings.ItemIndex;

            return Position != null;
        }

        public virtual JSONNode ToJsonNode()
        {
            var baseNode = new JSONNode();
            var actionLoadNode = new JSONNode();

            baseNode.SetAs(nameof(Position), (JSONNode)Position);

            foreach (var kvp in ActionEnergy)
                actionLoadNode.SetAs(kvp.Key.ToString(), kvp.Value);

            baseNode.SetAs(nameof(ActionEnergy), actionLoadNode);
            baseNode.SetAs(nameof(RoamObjective), RoamObjective);

            return baseNode;
        }
    }
}