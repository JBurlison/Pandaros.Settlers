using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pandaros.Settlers.Models;

namespace Pandaros.Settlers.Items.ConnectedBlocks
{
    public class PipeCalculationType : IConnectedBlockCalculationType
    {
        public List<BlockSide> AvailableBlockSides => new List<BlockSide>()
        {
            BlockSide.Xn,
            BlockSide.Xp,
            BlockSide.Zn,
            BlockSide.Zp,
            BlockSide.Yp,
            BlockSide.Yn
        };

        public string name => "Pipe";

        public int MaxConnections => 6;
    }
}
