using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pandaros.Settlers.Models;

namespace Pandaros.Settlers.Models
{
    public class TransportSave
    {
        public SerializableVector3 position { get; set; }
        public SerializableVector3 rotation { get; set; }
        public SerializableVector3 prevPos { get; set; }
        public SerializableVector3 trackPos { get; set; }
        public int meshid { get; set; }
        public string player { get; set; }
        public string type { get; set; }
        public string BlockType { get; set; }
        public string itemName { get; set; }
        public float energy { get; set; }
    }
}
