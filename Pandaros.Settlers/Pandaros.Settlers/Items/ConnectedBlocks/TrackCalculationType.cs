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
        public TrackCalculationType()
        {
            AvailableBlockSides = new List<BlockSide>((BlockSide[])Enum.GetValues(typeof(BlockSide)));
            AvailableBlockSides.Remove(BlockSide.Invalid);
            AvailableBlockSides.Remove(BlockSide.Yp);
            AvailableBlockSides.Remove(BlockSide.Yn);
        }

        public List<RotationAxis> AxisRotations => new List<RotationAxis>()
        {
            RotationAxis.Y
        };

        public List<BlockSide> AvailableBlockSides { get; }

        public string name => "Track";

        public int MaxConnections => 2;
    }
}
