using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandaros.Settlers.Buildings.NBT
{
    internal class RawSchematic
    {
        public int XMax { get; set; }
        public int YMax { get; set; }
        public int ZMax { get; set; }
        public string Materials { get; set; }
        public byte[] Blocks { get; set; }
        public byte[] Data { get; set; }
        public TileEntity[,,] TileEntities { get; set; }
    }
}
