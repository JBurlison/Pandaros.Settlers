using BlockTypes.Builtin;
using Pandaros.Settlers.AI;
using Pandaros.Settlers.Managers;
using Pandaros.Settlers.Research;
using Pipliz;
using Pipliz.Collections;
using Pipliz.JSON;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Serialization;
using static Pandaros.Settlers.Entities.SettlerInventory;

namespace Pandaros.Settlers.Entities
{
    [ModLoader.ModManager]
    public class PlayerState
    {
        static Dictionary<Players.Player, PlayerState> _playerStates = new Dictionary<Players.Player, PlayerState>();

        public System.Random Rand { get; set; }

        public static List<HealingOverTimePC> HealingSpells { get; private set; } = new List<HealingOverTimePC>();

        public bool CallToArmsEnabled { get; set; }

        public string DifficultyStr
        {
            get
            {
                if (Difficulty == null)
                    Difficulty = Configuration.DefaultDifficulty;

                return Difficulty.Name;
            }

            set
            {
                if (value != null && !GameDifficulty.GameDifficulties.ContainsKey(value))
                    Difficulty = Configuration.DefaultDifficulty;
                else
                    Difficulty = GameDifficulty.GameDifficulties[value];
            }
        }


        public GameDifficulty Difficulty { get; set; }

        public Players.Player Player { get; private set; }

        public List<Vector3Int> FlagsPlaced { get; set; } = new List<Vector3Int>();
        public Vector3Int TeleporterPlaced { get; set; } = Vector3Int.invalidPos;

        public Dictionary<Items.Armor.ArmorSlot, ArmorState> Armor { get; set; } = new Dictionary<Items.Armor.ArmorSlot, ArmorState>();
        public bool BossesEnabled { get; set; } = true;
        public bool MonstersEnabled { get; set; } = true;
        public bool SettlersEnabled { get; set; } = true;
        public ArmorState Weapon { get; set; } = new ArmorState();

        public Items.BuildersWand.WandMode BuildersWandMode { get; set; }
        public int BuildersWandCharge { get; set; } = Items.BuildersWand.DURABILITY;
        public int BuildersWandMaxCharge { get; set; }
        public int SettlersToggledTimes { get; set; }
        public int HighestColonistCount { get; set; }

        public List<Vector3Int> BuildersWandPreview { get; set; } = new List<Vector3Int>();
        public ushort BuildersWandTarget { get; set; } = BuiltinBlocks.Air;
        public double NextGenTime { get; set; }
        public double NeedsABed { get; set; }

        public int MaxPerSpawn
        {
            get
            {
                var max = SettlerManager.MIN_PERSPAWN;
                var col = Colony.Get(Player);

                if (col.FollowerCount >= SettlerManager.MAX_BUYABLE)
                    max += Rand.Next((int)Player.GetTempValues(true).GetOrDefault(PandaResearch.GetResearchKey(PandaResearch.MinSettlers), 0f),
                               SettlerManager.ABSOLUTE_MAX_PERSPAWN + (int)Player.GetTempValues(true).GetOrDefault(PandaResearch.GetResearchKey(PandaResearch.MaxSettlers), 0f));

                return max;
            }
        }

        public PlayerState(Players.Player p)
        {
            Difficulty = Configuration.DefaultDifficulty;
            Player = p;
            Rand = new System.Random();
            SetupArmor();

            HealingOverTimePC.NewInstance += HealingOverTimePC_NewInstance;
        }

        private void HealingOverTimePC_NewInstance(object sender, EventArgs e)
        {
            var healing = sender as HealingOverTimePC;

            lock (HealingSpells)
                HealingSpells.Add(healing);

            healing.Complete += Healing_Complete;
        }

        private void Healing_Complete(object sender, EventArgs e)
        {
            var healing = sender as HealingOverTimePC;

            lock (HealingSpells)
                HealingSpells.Remove(healing);

            healing.Complete -= Healing_Complete;
        }

        private void SetupArmor()
        {
            Weapon = new ArmorState();

            foreach (Items.Armor.ArmorSlot armorType in Items.Armor.ArmorSlotEnum)
                Armor.Add(armorType, new ArmorState());
        }

        public static PlayerState GetPlayerState(Players.Player p)
        {
            if (p != null)
            {
                if (!_playerStates.ContainsKey(p))
                    _playerStates.Add(p, new PlayerState(p));

                return _playerStates[p];
            }

            return null;
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnLoadingPlayer, GameLoader.NAMESPACE + ".Entities.PlayerState.OnLoadingPlayer")]
        public static void OnLoadingPlayer(JSONNode n, Players.Player p)
        {
            if (!_playerStates.ContainsKey(p))
                _playerStates.Add(p, new PlayerState(p));

            if (n.TryGetChild(GameLoader.NAMESPACE + ".PlayerState", out var stateNode))
            {
                if (stateNode.TryGetAs("Armor", out JSONNode armorNode) && armorNode.NodeType == NodeType.Object)
                    foreach (var aNode in armorNode.LoopObject())
                        _playerStates[p].Armor[(Items.Armor.ArmorSlot)Enum.Parse(typeof(Items.Armor.ArmorSlot), aNode.Key)] = new ArmorState(aNode.Value);

                if (stateNode.TryGetAs("FlagsPlaced", out JSONNode flagsPlaced) && flagsPlaced.NodeType == NodeType.Array)
                    foreach (var aNode in flagsPlaced.LoopArray())
                        _playerStates[p].FlagsPlaced.Add((Vector3Int)aNode);

                if (stateNode.TryGetAs("TeleporterPlaced", out JSONNode teleporterPlaced))
                    _playerStates[p].TeleporterPlaced = (Vector3Int)teleporterPlaced;

                if (stateNode.TryGetAs("Weapon", out JSONNode wepNode))
                    _playerStates[p].Weapon = new ArmorState(wepNode);

                if (stateNode.TryGetAs("Difficulty", out string diff))
                    _playerStates[p].DifficultyStr = diff;

                if (stateNode.TryGetAs(nameof(BuildersWandMode), out string wandMode))
                    _playerStates[p].BuildersWandMode = (Items.BuildersWand.WandMode)Enum.Parse(typeof(Items.BuildersWand.WandMode), wandMode);

                if (stateNode.TryGetAs(nameof(BuildersWandCharge), out int wandCharge))
                    _playerStates[p].BuildersWandCharge = wandCharge;

                if (stateNode.TryGetAs(nameof(BuildersWandTarget), out ushort wandTarget))
                    _playerStates[p].BuildersWandTarget = wandTarget;

                if (stateNode.TryGetAs(nameof(BossesEnabled), out bool bosses))
                    _playerStates[p].BossesEnabled = bosses;

                if (stateNode.TryGetAs(nameof(MonstersEnabled), out bool monsters))
                    _playerStates[p].MonstersEnabled = monsters;

                if (stateNode.TryGetAs(nameof(SettlersEnabled), out bool settlers))
                    _playerStates[p].SettlersEnabled = settlers;

                if (stateNode.TryGetAs(nameof(SettlersToggledTimes), out int toggle))
                    _playerStates[p].SettlersToggledTimes = toggle;

                if (stateNode.TryGetAs(nameof(HighestColonistCount), out int hsc))
                    _playerStates[p].HighestColonistCount = hsc;

                if (stateNode.TryGetAs(nameof(NeedsABed), out int nb))
                    _playerStates[p].NeedsABed = nb;

                _playerStates[p].BuildersWandPreview.Clear();

                if (stateNode.TryGetAs(nameof(BuildersWandPreview), out JSONNode wandPreview))
                    foreach (var node in wandPreview.LoopArray())
                        _playerStates[p].BuildersWandPreview.Add((Vector3Int)node);
            }
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnSavingPlayer, GameLoader.NAMESPACE + ".Entities.PlayerState.OnSavingPlayer")]
        public static void OnSavingPlayer(JSONNode n, Players.Player p)
        {
            if (_playerStates.ContainsKey(p))
            {
                var node = new JSONNode();
                var armorNode = new JSONNode();
                var flagsPlaced = new JSONNode(NodeType.Array);
                var buildersWandPreview = new JSONNode(NodeType.Array);

                foreach (var armor in _playerStates[p].Armor)
                    armorNode.SetAs(armor.Key.ToString(), armor.Value.ToJsonNode());

                foreach (var flag in _playerStates[p].FlagsPlaced)
                    flagsPlaced.AddToArray((JSONNode)flag);

                foreach (var preview in _playerStates[p].BuildersWandPreview)
                    buildersWandPreview.AddToArray((JSONNode)preview);

                node.SetAs("Armor", armorNode);
                node.SetAs("Weapon", _playerStates[p].Weapon.ToJsonNode());
                node.SetAs("Difficulty", _playerStates[p].DifficultyStr);
                node.SetAs("FlagsPlaced", flagsPlaced);
                node.SetAs("TeleporterPlaced", (JSONNode)_playerStates[p].TeleporterPlaced);
                node.SetAs(nameof(BuildersWandPreview), buildersWandPreview);
                node.SetAs(nameof(BossesEnabled), _playerStates[p].BossesEnabled);
                node.SetAs(nameof(MonstersEnabled), _playerStates[p].MonstersEnabled);
                node.SetAs(nameof(SettlersEnabled), _playerStates[p].SettlersEnabled);
                node.SetAs(nameof(BuildersWandMode), _playerStates[p].BuildersWandMode.ToString());
                node.SetAs(nameof(BuildersWandCharge), _playerStates[p].BuildersWandCharge);
                node.SetAs(nameof(BuildersWandTarget), _playerStates[p].BuildersWandTarget);
                node.SetAs(nameof(SettlersToggledTimes), _playerStates[p].SettlersToggledTimes);
                node.SetAs(nameof(HighestColonistCount), _playerStates[p].HighestColonistCount);
                node.SetAs(nameof(NeedsABed), _playerStates[p].NeedsABed);

                n.SetAs(GameLoader.NAMESPACE + ".PlayerState", node);
            }
        }
    }
}
