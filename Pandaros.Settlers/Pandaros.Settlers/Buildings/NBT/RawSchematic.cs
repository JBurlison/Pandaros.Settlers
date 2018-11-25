using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandaros.Settlers.Buildings.NBT
{
    public class RawSchematicSize
    {
        public int XMax { get; set; }
        public int YMax { get; set; }
        public int ZMax { get; set; }

        public override string ToString()
        {
            return $"Max Bounds: [{XMax}, {YMax}, {ZMax}]";
        }
    }

    public class RawSchematic : RawSchematicSize
    {
        public string Materials { get; set; }
        public byte[] Blocks { get; set; }
        public byte[] Data { get; set; }
        public SchematicBlock[,,] CSBlocks { get; set; }
        public TileEntity[,,] TileEntities { get; set; }

        public override string ToString()
        {
            return $"Max Bounds: [{XMax}, {YMax}, {ZMax}] CSBlock Count: {CSBlocks.LongLength} Blocks Count: {Blocks.LongLength}";
        }
    }
}
