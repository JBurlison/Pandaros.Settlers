using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pandaros.Settlers.Models;

namespace Pandaros.Settlers.Items.ConnectedBlocks
{
    public class TrackCalculationType : IConnectedBlockCalculationType
    {
        public List<BlockSide> AvailableBlockSides => new List<BlockSide>((BlockSide[])Enum.GetValues(typeof(BlockSide)));

        public string name => "Track";

        public int MaxConnections => 2;
    }
}
