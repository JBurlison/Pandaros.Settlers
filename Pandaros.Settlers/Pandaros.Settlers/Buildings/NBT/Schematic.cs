using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandaros.Settlers.Buildings.NBT
{
    public class Schematic
    {
        public string Name { get; set; }
        public int XMax { get; set; }
        public int YMax { get; set; }
        public int ZMax { get; set; }
        /// <summary>Contains all usual blocks</summary>
        public Block[,,] Blocks { get; set; }
        /// <summary>Contains TileEntities such as hoppers and chests</summary>
        public TileEntity[,,] TileEntities { get; set; }

        public Schematic()
        {
   
        }

        public Schematic(string name) : this()
        {
            Name = name;
        }

        public Schematic(string name, int x, int y, int z) : this(name)
        {
            this.XMax = x;
            this.YMax = y;
            this.ZMax = z;
        }

        public Schematic(string name, int x, int y, int z, Block[,,] blocks, TileEntity[,,] tileEntities) : this(name, x, y, z)
        {
            this.Blocks = blocks;
            this.TileEntities = tileEntities;
        }
    }
}
