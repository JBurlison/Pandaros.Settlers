using Pandaros.Settlers.AI;
using Pandaros.Settlers.Managers;
using Pandaros.Settlers.Research;
using Pipliz;
using Pipliz.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Serialization;
using static Pandaros.Settlers.Entities.SettlerInventory;

namespace Pandaros.Settlers.Entities
{
    [Serializable]
    public class ColonyState
    {
        [XmlElement]
        public SerializableDictionary<string, PlayerState> PlayerStates { get; set; }
        
        public ColonyState()
        {
            PlayerStates = new SerializableDictionary<string, PlayerState>();
        }
    }

    public class PlayerState
    {
        [XmlIgnore]
        public System.Random Rand { get; set; }

        [XmlElement]
        public int ColonistCount { get; set; }

        [XmlElement]
        public int HighestColonistCount { get; set; }

        [XmlElement]
        public bool SettlersOn { get; set; } = false;

        [XmlElement]
        public List<int> BannersAwarded { get; set; } = new List<int>();

        [XmlIgnore]
        public bool CallToArmsEnabled { get; set; }

        [XmlElement]
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

        [XmlIgnore]
        public GameDifficulty Difficulty { get; set; }

        [XmlIgnore]
        public Players.Player Player { get; set; }

        [XmlIgnore]
        public List<Vector3Int> FlagsPlaced { get; set; } = new List<Vector3Int>();

        [XmlElement]
        public SerializableDictionary<Items.Armor.ArmorSlot, ArmorState> Armor { get; set; } = new SerializableDictionary<Items.Armor.ArmorSlot, ArmorState>();

        [XmlElement]
        public ArmorState Weapon { get; set; } = new ArmorState();

        public void SetupColonyRefrences(Colony c)
        {
            Player = c.Owner;
        }

        public PlayerState()
        {
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
            return GetPlayerState(p, Colony.Get(p));
        }

        public static PlayerState GetPlayerState(Players.Player p, Colony c)
        {
            var colony = SettlerManager.CurrentColonyState;

            if (colony != null && c != null && p != null)
            {
                string playerId = p.ID.ToString();

                if (!colony.PlayerStates.ContainsKey(playerId))
                    colony.PlayerStates.Add(playerId, new PlayerState());

                if (colony.PlayerStates[playerId].ColonistCount == 0 &&
                    c.FollowerCount != 0)
                    colony.PlayerStates[playerId].ColonistCount = c.FollowerCount;

                return colony.PlayerStates[playerId];
            }

            return null;
        }
    }
}
