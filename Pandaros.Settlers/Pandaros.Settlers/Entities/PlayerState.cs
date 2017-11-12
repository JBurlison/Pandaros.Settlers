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

        public bool CallToArmsEnabled { get; set; }

        public string DifficultyStr
        {
            get
            {
                if (Difficulty == null)
                    Difficulty = GameDifficulty.Medium;

                return Difficulty.Name;
            }

            set
            {
                if (!GameDifficulty.GameDifficulties.ContainsKey(value))
                    Difficulty = GameDifficulty.Medium;
                else
                    Difficulty = GameDifficulty.GameDifficulties[value];
            }
        }


        public GameDifficulty Difficulty { get; set; }

        public Players.Player Player { get; private set; }

        public List<Vector3Int> FlagsPlaced { get; set; } = new List<Vector3Int>();

        public Dictionary<Items.Armor.ArmorSlot, ArmorState> Armor { get; set; } = new Dictionary<Items.Armor.ArmorSlot, ArmorState>();


        public ArmorState Weapon { get; set; } = new ArmorState();

        public PlayerState(Players.Player p)
        {
            Difficulty = GameDifficulty.Medium;
            Player = p;
            Rand = new System.Random();
            SetupArmor();
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

                if (stateNode.TryGetAs("Weapon", out JSONNode wepNode))
                    _playerStates[p].Weapon = new ArmorState(wepNode);

                if (stateNode.TryGetAs("Difficulty", out string diff))
                    _playerStates[p].DifficultyStr = diff;
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

                foreach (var armor in _playerStates[p].Armor)
                    armorNode.SetAs(armor.Key.ToString(), armor.Value.ToJsonNode());

                foreach (var flag in _playerStates[p].FlagsPlaced)
                    flagsPlaced.AddToArray((JSONNode)flag);

                node.SetAs("Armor", armorNode);
                node.SetAs("Weapon", _playerStates[p].Weapon.ToJsonNode());
                node.SetAs("FlagsPlaced", flagsPlaced);
                node.SetAs("Difficulty", _playerStates[p].DifficultyStr);

                n.SetAs(GameLoader.NAMESPACE + ".PlayerState", node);
            }
        }
    }
}
