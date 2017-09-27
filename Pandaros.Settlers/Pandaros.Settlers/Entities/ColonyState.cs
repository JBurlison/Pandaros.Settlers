using Pandaros.Settlers.Research;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Serialization;

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
        public double NeedsABed { get; set; }

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

                    max += Rand.Next(ColonyInterface.Colony.Owner.GetTemporaryValueOrDefault<int>(PandaResearch.GetTempValueKey(PandaResearch.MinSettlers), 0), 
                                     maxAdd + ColonyInterface.Colony.Owner.GetTemporaryValueOrDefault<int>(PandaResearch.GetTempValueKey(PandaResearch.MaxSettlers), 0));
                }

                return max;
            }
        }

        public void SetupColonyRefrences(Colony c)
        {
            ColonyInterface = new PlayerColonyInterface(c);
        }

        public PlayerState()
        {
            Rand = new Random();
            KnownLaborers = new Dictionary<NPC.NPCBase, double>();
            NeedsABed = 0;
            Difficulty = GameDifficulty.Medium;
        }
    }
}
