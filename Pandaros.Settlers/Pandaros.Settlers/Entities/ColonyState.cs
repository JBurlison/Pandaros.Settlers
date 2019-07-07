using NetworkUI;
using NetworkUI.Items;
using Pandaros.Settlers.ColonyManagement;
using Pandaros.Settlers.Models;
using Pandaros.Settlers.Research;
using Pipliz.JSON;
using System;
using System.Collections.Generic;

namespace Pandaros.Settlers.Entities
{
    [ModLoader.ModManager]
    public class ColonyState
    {
        private static readonly Dictionary<Colony, ColonyState> _colonyStates = new Dictionary<Colony, ColonyState>();
        static readonly Pandaros.Settlers.localization.LocalizationHelper _localizationHelper = new localization.LocalizationHelper("colonytool");

        public Colony ColonyRef { get; set; }
        public int FaiedBossSpawns { get; set; }
        public GameDifficulty Difficulty { get; set; } = SettlersConfiguration.DefaultDifficulty;
        public bool BossesEnabled { get; set; } = true;
        public SettlersState SettlersEnabled { get; set; }
        public double NeedsABed { get; set; }
        public int HighestColonistCount { get; set; }
        public double NextGenTime { get; set; }
        public int SettlersToggledTimes { get; set; }
        public float FoodPerHour { get; set; }
        public Dictionary<ushort, int> ItemsPlaced { get; set; } = new Dictionary<ushort, int>();
        public Dictionary<ushort, int> ItemsRemoved { get; set; } = new Dictionary<ushort, int>();
        public Dictionary<ushort, int> ItemsInWorld { get; set; } = new Dictionary<ushort, int>();
        public Dictionary<string, double> Stats { get; set; } = new Dictionary<string, double>();
        public DateTime CreationDate { get; set; } = DateTime.Now;
        public string DifficultyStr
        {
            get
            {
                if (Difficulty == null)
                    Difficulty = SettlersConfiguration.DefaultDifficulty;

                return Difficulty.Name;
            }

            set
            {
                if (value != null && !GameDifficulty.GameDifficulties.ContainsKey(value))
                    Difficulty = SettlersConfiguration.DefaultDifficulty;
                else
                    Difficulty = GameDifficulty.GameDifficulties[value];
            }
        }
        public int MaxPerSpawn
        {
            get
            {
                var max = SettlerManager.MIN_PERSPAWN;

                if (ColonyRef != null && ColonyRef.FollowerCount >= SettlerManager.MAX_BUYABLE)
                    max += Pipliz.Random.Next((int)ColonyRef.TemporaryData.GetAsOrDefault(GameLoader.NAMESPACE + ".MinSettlers", 0f),
                                                SettlerManager.ABSOLUTE_MAX_PERSPAWN + (int)ColonyRef.TemporaryData.GetAsOrDefault(GameLoader.NAMESPACE + ".MaxSettlers", 0f));

                return max;
            }
        }

        public ColonyState(Colony c)
        {
            ColonyRef = c;
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnConstructInventoryManageColonyUI, GameLoader.NAMESPACE + ".Entities.ColonyState.OnConstructInventoryManageColonyUI")]
        public static void OnConstructInventoryManageColonyUI(Players.Player player, NetworkMenu networkMenu)
        {
            var cs = GetColonyState(player.ActiveColony);

            if (cs != null)
            {
                networkMenu.Items.Add(new HorizontalSplit(new Label(new LabelData(GameLoader.NAMESPACE + ".inventory.ColonyCreationDate", UnityEngine.TextAnchor.MiddleLeft, 18, LabelData.ELocalizationType.Sentence)),
                                                    new Label(new LabelData(cs.CreationDate.ToString())), 30, 0.75f));
            }
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnLoadingColony, GameLoader.NAMESPACE + ".Entities.ColonyState.OnLoadingColony")]
        public static void OnLoadingColony(Colony c, JSONNode n)
        {
            if (!_colonyStates.ContainsKey(c))
                _colonyStates.Add(c, new ColonyState(c));

            if (n.TryGetChild(GameLoader.NAMESPACE + ".ColonyState", out var stateNode))
            {
                if (stateNode.TryGetAs(nameof(ItemsPlaced), out JSONNode ItemsPlacedNode))
                    foreach (var aNode in ItemsPlacedNode.LoopObject())
                        _colonyStates[c].ItemsPlaced.Add(ushort.Parse(aNode.Key), aNode.Value.GetAs<int>());

                if (stateNode.TryGetAs(nameof(ItemsRemoved), out JSONNode ItemsRemovedNode))
                    foreach (var aNode in ItemsRemovedNode.LoopObject())
                        _colonyStates[c].ItemsRemoved.Add(ushort.Parse(aNode.Key), aNode.Value.GetAs<int>());

                if (stateNode.TryGetAs(nameof(ItemsInWorld), out JSONNode ItemsInWorldNode))
                    foreach (var aNode in ItemsInWorldNode.LoopObject())
                        _colonyStates[c].ItemsInWorld.Add(ushort.Parse(aNode.Key), aNode.Value.GetAs<int>());

                if (stateNode.TryGetAs(nameof(Stats), out JSONNode itterations))
                    foreach (var skill in itterations.LoopObject())
                        _colonyStates[c].Stats[skill.Key] = skill.Value.GetAs<double>();

                if (stateNode.TryGetAs("Difficulty", out string diff))
                    _colonyStates[c].DifficultyStr = diff;

                if (stateNode.TryGetAs(nameof(BossesEnabled), out bool bosses))
                    _colonyStates[c].BossesEnabled = bosses;

                if (stateNode.TryGetAs(nameof(SettlersEnabled), out SettlersState notify))
                    _colonyStates[c].SettlersEnabled = notify;

                if (stateNode.TryGetAs(nameof(HighestColonistCount), out int hsc))
                    _colonyStates[c].HighestColonistCount = hsc;

                if (stateNode.TryGetAs(nameof(NeedsABed), out int nb))
                    _colonyStates[c].NeedsABed = nb;

                if (stateNode.TryGetAs(nameof(SettlersToggledTimes), out int stt))
                    _colonyStates[c].SettlersToggledTimes = stt;

                if (stateNode.TryGetAs(nameof(CreationDate), out string joindate) && DateTime.TryParse(joindate, out var parsedJoinDate))
                    _colonyStates[c].CreationDate = parsedJoinDate;
            }
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnSavingColony, GameLoader.NAMESPACE + ".Entities.ColonyState.OnSavingColony")]
        public static void OnSavingColony(Colony c, JSONNode n)
        {
            if (_colonyStates.ContainsKey(c))
            {
                var ItemsPlacedNode = new JSONNode();
                var ItemsRemovedNode = new JSONNode();
                var ItemsInWorldNode = new JSONNode();
                var node = new JSONNode();

                node.SetAs("Difficulty", _colonyStates[c].DifficultyStr);
                node.SetAs(nameof(BossesEnabled), _colonyStates[c].BossesEnabled);
                node.SetAs(nameof(SettlersEnabled), _colonyStates[c].SettlersEnabled);
                node.SetAs(nameof(HighestColonistCount), _colonyStates[c].HighestColonistCount);
                node.SetAs(nameof(SettlersToggledTimes), _colonyStates[c].SettlersToggledTimes);

                foreach (var kvp in _colonyStates[c].ItemsPlaced)
                    ItemsPlacedNode.SetAs(kvp.Key.ToString(), kvp.Value);

                foreach (var kvp in _colonyStates[c].ItemsRemoved)
                    ItemsRemovedNode.SetAs(kvp.Key.ToString(), kvp.Value);

                foreach (var kvp in _colonyStates[c].ItemsInWorld)
                    ItemsInWorldNode.SetAs(kvp.Key.ToString(), kvp.Value);

                var statsNode = new JSONNode();

                foreach (var job in _colonyStates[c].Stats)
                    statsNode[job.Key] = new JSONNode(job.Value);

                node.SetAs(nameof(Stats), statsNode);
                node.SetAs(nameof(ItemsPlaced), ItemsPlacedNode);
                node.SetAs(nameof(ItemsRemoved), ItemsRemovedNode);
                node.SetAs(nameof(ItemsInWorld), ItemsInWorldNode);
                node.SetAs(nameof(CreationDate), _colonyStates[c].CreationDate);

                n.SetAs(GameLoader.NAMESPACE + ".ColonyState", node);
            }
        }

        public static ColonyState GetColonyState(Colony c)
        {
            if (c != null)
            {
                if (!_colonyStates.ContainsKey(c))
                    _colonyStates.Add(c, new ColonyState(c));

                return _colonyStates[c];
            }

            return null;
        }
    }
}
