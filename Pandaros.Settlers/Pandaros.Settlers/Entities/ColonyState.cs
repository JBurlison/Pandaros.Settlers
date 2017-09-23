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
        public Random Rand { get; set; }

        [XmlElement]
        public int ColonistCount { get; set; }

        [XmlIgnore]
        public float CurrentFoodPerHour { get; set; }

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

                if (ColonistCount > SettlerManager.MAX_BUYABLE)
                {
                    var maxAdd = (int)Math.Ceiling(ColonistCount / (decimal)10);

                    if (maxAdd > SettlerManager.ABSOLUTE_MAX_PERSPAWN)
                        maxAdd = SettlerManager.ABSOLUTE_MAX_PERSPAWN;

                    max += Rand.Next(0, maxAdd);
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
        }
    }
}
