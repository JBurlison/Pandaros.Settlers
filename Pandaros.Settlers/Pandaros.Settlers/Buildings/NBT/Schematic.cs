using Pipliz;

namespace Pandaros.Settlers.Buildings.NBT
{
    public class Schematic
    {
        public string Name { get; set; }
        public int XMax { get; set; }
        public int YMax { get; set; }
        public int ZMax { get; set; }
        /// <summary>Contains all usual blocks</summary>
        public SchematicBlock[,,] Blocks { get; set; }
        /// <summary>Contains TileEntities such as hoppers and chests</summary>
        public TileEntity[,,] TileEntities { get; set; }
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

        public Schematic(string name, int x, int y, int z, SchematicBlock[,,] blocks, TileEntity[,,] tileEntities, Vector3Int startPos) : this(name, x, y, z)
        {
            Blocks = blocks;
            TileEntities = tileEntities;
            StartPos = startPos;
        }
    }
}
