using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandaros.Settlers.Buildings.NBT
{
    internal class RawSchematic
    {
        public int XMax;
        public int YMax;
        public int ZMax;
        public string Materials; //ignored later
        public byte[] Blocks;
        public byte[] Data;
        public TileEntity[,,] TileEntities;
    }
}
