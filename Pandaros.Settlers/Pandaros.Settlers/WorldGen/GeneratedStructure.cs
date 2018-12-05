using fNbt;
using Pandaros.Settlers.NBT;
using Pipliz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerrainGeneration;

namespace Pandaros.Settlers.WorldGen
{
    public class GeneratedStructure 
    {
        public string Name { get; set; }
        public NbtFile File { get; set; }
        public Vector2Int LastPlaced { get; set; }
        public RawSchematicSize SchematicSize { get; set; }
        public short Ymin { get; set; }
        public int DistanceBetweenOtherStructuresMax { get; set; } = -1;
        public int DistanceBetweenOtherStructuresMin { get; set; } = 200;
        public int NumberOfPlacements { get; set; } = 1;
        public float SpawnChance { get; set; } = .05f;

        public GeneratedStructure(NbtFile file)
        {
            SchematicSize = SchematicReader.LoadRawSize(file);
            File = file;
        }

        public StructureBlock GetBlock(int x, int y, int z)
        {
            StructureBlock sb = new StructureBlock();

            if (x >= SchematicSize.XMax ||
                y >= SchematicSize.YMax ||
                z >= SchematicSize.ZMax)
                return sb;


            if (File.RootTag.TryGet("CSBlocks", out var csBlocksTag))
            {
                NbtList csBlocks = csBlocksTag as NbtList;

                if (csBlocks != null)
                {
                    foreach (NbtCompound compTag in csBlocks)
                    {
                        var xTag = compTag["x"].IntValue;
                        var yTag = compTag["y"].IntValue;
                        var zTag = compTag["z"].IntValue;

                        if (xTag == x && yTag == y && zTag == z)
                        {
                            NbtTag idTag = compTag["id"];
                            sb = new StructureBlock(xTag, yTag, zTag, ItemTypes.GetType(idTag.StringValue).ItemIndex);
                            break;
                        }
                    }
                }
            }
            else if (File.RootTag.TryGet("Blocks", out var blocksTag) && File.RootTag.TryGet("Data", out var dataTag))
            {
                var blocks = blocksTag.ByteArrayValue;
                var data = dataTag.ByteArrayValue;

                int index = (y * SchematicSize.ZMax + z) * SchematicSize.XMax + x;
                SchematicBlock block = new SchematicBlock();
                block.BlockID = ((int)blocks[index]).ToString();
                block.Data = data[index];
                block.X = x;
                block.Y = y;
                block.Z = z;

                sb = new StructureBlock(x, y, z, block.MappedBlock.CSIndex);
            }

            return sb;
        }
    }
}
