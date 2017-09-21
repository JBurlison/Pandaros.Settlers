using System;
using System.Collections.Generic;
using System.Linq;
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
        public int MaxPerSpawn
        {
            get
            {
                var max = SettlerManager.MIN_PERSPAWN;

                if (ColonistCount > SettlerManager.MAX_BUYABLE)
                {
                    var minAdd = (int)Math.Ceiling(ColonistCount / (decimal)10);
                    var maxAdd = (int)Math.Ceiling(ColonistCount / (decimal)7);

                    max += Rand.Next(minAdd, maxAdd);
                }

                return max;
            }
        }

        public PlayerState()
        {
            Rand = new Random(); 
        }
    }
}
