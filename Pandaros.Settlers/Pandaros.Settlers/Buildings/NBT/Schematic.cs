using Pipliz;

namespace Pandaros.Settlers.Buildings.NBT
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
        //public TileEntity[,,] TileEntities { get; set; }
        public SchematicBlock[,,] CSBlocks { get; set; }
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

        public Schematic(string name, int x, int y, int z, SchematicBlock[,,] blocks, SchematicBlock[,,] scBlocks, Vector3Int startPos) : this(name, x, y, z)
        {
            Blocks = blocks;
            CSBlocks = scBlocks;
            // TileEntities = tileEntities;
            StartPos = startPos;
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

                        if (CSBlocks != null)
                            newBlocks[newZ, y, newX] = CSBlocks[z, y, x];
                        else
                            newBlocks[newZ, y, newX] = Blocks[z, y, x];
                    }
                }
            }

            if (CSBlocks != null)
                CSBlocks = newBlocks;
            else
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
