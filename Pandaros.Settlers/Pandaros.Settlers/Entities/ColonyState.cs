using Pandaros.Settlers.AI;
using Pandaros.Settlers.Managers;
using Pandaros.Settlers.Research;
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
        public Random Rand { get; set; }

        [XmlElement]
        public int ColonistCount { get; set; }

        [XmlElement]
        public int HighestColonistCount { get; set; }

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
        public int FoodDivider { get; set; }

        [XmlIgnore]
        public double NextGenTime { get; set; }

        [XmlIgnore]
        public Dictionary<NPC.NPCBase, double> KnownLaborers { get; set; }

        [XmlIgnore]
        public PlayerColonyInterface ColonyInterface { get; private set; }

        [XmlIgnore]
        public Players.Player Player { get; set; }

        [XmlIgnore]
        public double NeedsABed { get; set; }

        [XmlElement]
        public SerializableDictionary<Items.Armor.ArmorSlot, ItemState> Armor { get; set; } = new SerializableDictionary<Items.Armor.ArmorSlot, ItemState>();

        [XmlElement]
        public ItemState Weapon { get; set; }

        [XmlIgnore]
        public int MaxPerSpawn
        {
            get
            {
                var max = SettlerManager.MIN_PERSPAWN;

                if (ColonistCount >= SettlerManager.MAX_BUYABLE)
                {
                    var maxAdd = (int)Math.Ceiling(ColonistCount * 0.05f);

                    if (maxAdd > SettlerManager.ABSOLUTE_MAX_PERSPAWN)
                        maxAdd = SettlerManager.ABSOLUTE_MAX_PERSPAWN;

                    max += Rand.Next((int)Player.GetTempValues(true).GetOrDefault(PandaResearch.GetResearchKey(PandaResearch.MinSettlers), 0f),
                               maxAdd + (int)Player.GetTempValues(true).GetOrDefault(PandaResearch.GetResearchKey(PandaResearch.MaxSettlers), 0f));
                }

                return max;
            }
        }

        public void SetupColonyRefrences(Colony c)
        {
            ColonyInterface = new PlayerColonyInterface(c);
            Player = c.Owner;
        }

        public PlayerState()
        {
            Rand = new Random();
            KnownLaborers = new Dictionary<NPC.NPCBase, double>();
            NeedsABed = 0;
            Difficulty = GameDifficulty.Medium;
            SetupArmor();
        }

        private void SetupArmor()
        {
            Weapon = new ItemState();

            foreach (Items.Armor.ArmorSlot armorType in Items.Armor.ArmorSlotEnum)
                Armor.Add(armorType, new ItemState());
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

                if (colony.PlayerStates[playerId].ColonyInterface == null)
                    colony.PlayerStates[playerId].SetupColonyRefrences(c);

                if (colony.PlayerStates[playerId].ColonistCount == 0 &&
                    c.FollowerCount != 0)
                    colony.PlayerStates[playerId].ColonistCount = c.FollowerCount;

                return colony.PlayerStates[playerId];
            }

            return null;
        }
    }
}
