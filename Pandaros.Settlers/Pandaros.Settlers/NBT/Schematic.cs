using Pipliz;

namespace Pandaros.Settlers.NBT
{
    public class Schematic
    {
        public enum Rotation
        {
            Front,
            Right,
            Back,
            Left
        }

        public string Name { get; set; }
        public int XMax { get; set; }
        public int YMax { get; set; }
        public int ZMax { get; set; }
        /// <summary>Contains all usual blocks</summary>
        public SchematicBlock[,,] Blocks { get; set; }
        /// <summary>Contains TileEntities such as hoppers and chests</summary>
        public Vector3Int StartPos { get; set; }

        public Schematic()
        {
   
        }

        public Schematic(string name) : this()
        {
            Name = name;
        }

        public Schematic(string name, int x, int y, int z) : this(name)
        {
            XMax = x;
            YMax = y;
            ZMax = z;
        }

        public Schematic(string name, int x, int y, int z, SchematicBlock[,,] blocks, Vector3Int startPos) : this(name, x, y, z)
        {
            Blocks = blocks;
            StartPos = startPos;
        }

        public SchematicBlock GetBlock(int X, int Y, int Z)
        {
            SchematicBlock block = default(SchematicBlock);

            if (Y < YMax &&
                X < XMax &&
                Z < ZMax)
                block = Blocks[X, Y, Z];

            if (block == default(SchematicBlock))
                block = SchematicBlock.Air;

            return block;
        }

        public void Rotate()
        {
            SchematicBlock[,,] newBlocks = new SchematicBlock[ZMax, YMax, XMax];

            for (int y = 0; y < YMax; y++)
            {
                for (int x = 0; x < ZMax; x++)
                {
                    for (int z = 0; z < XMax; z++)
                    {
                        int newX = z;
                        int newZ = ZMax - (x + 1);

                        if (Blocks[z, y, x].CSBlock)
                            if (Blocks[z, y, x].ItemID.Contains("z+"))
                            { 
                                Blocks[z, y, x].BlockID.Replace("z+", "x+");
                            }
                            else if (Blocks[z, y, x].ItemID.Contains("z-"))
                            {
                                Blocks[z, y, x].BlockID.Replace("z-", "x-");
                            }
                            else if (Blocks[z, y, x].ItemID.Contains("x+"))
                            {
                                Blocks[z, y, x].BlockID.Replace("x+", "z-");
                            }
                            else if (Blocks[z, y, x].ItemID.Contains("x-"))
                            {
                                Blocks[z, y, x].BlockID.Replace("x-", "z+");
                            }

                        newBlocks[newZ, y, newX] = Blocks[z, y, x];

                       
                    }
                }
            }

            Blocks = newBlocks;
    
            int tmpSize = XMax;
            XMax = ZMax;
            ZMax = tmpSize;
        }

        public override string ToString()
        {
            return $"Name: {Name}  Max Bounds: [{XMax}, {YMax}, {ZMax}]";
        }
    }
}
