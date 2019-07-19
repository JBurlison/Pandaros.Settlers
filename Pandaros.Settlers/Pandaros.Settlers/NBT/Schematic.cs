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
                for (int z = 0; z < ZMax; z++)
                {
                    for (int x = 0; x < XMax; x++)
                    {
                        int newX = x;
                        int newZ = ZMax - (z + 1);

                        if (Blocks[x, y, z].ItemID.Contains("z+"))
                        {
                            Blocks[x, y, z].BlockID = Blocks[x, y, z].BlockID.Replace("z+", "x-");
                        }
                        else if (Blocks[x, y, z].ItemID.Contains("z-"))
                        {
                            Blocks[x, y, z].BlockID = Blocks[x, y, z].BlockID.Replace("z-", "x+");
                        }
                        else if (Blocks[x, y, z].ItemID.Contains("x+"))
                        {
                            Blocks[x, y, z].BlockID = Blocks[x, y, z].BlockID.Replace("x+", "z+");
                        }
                        else if (Blocks[x, y, z].ItemID.Contains("x-"))
                        {
                            Blocks[x, y, z].BlockID = Blocks[x, y, z].BlockID.Replace("x-", "z-");
                        }

                        newBlocks[newZ, y, newX] = Blocks[x, y, z];
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
