using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandaros.Settlers.Models
{
    public class TrainConfiguration
    {
        public SerializableVector3 TrainBounds { get; set; }
        public SerializableVector3 playerSeatOffset { get; set; }
        public bool AllowPlayerToEditBlocksWhileRiding { get; set; }
        public int IdealHeightFromTrack { get; set; }
        public int MoveTimePerBlockMs { get; set; }
    }
}
