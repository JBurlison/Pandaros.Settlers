namespace Pandaros.Settlers.Buildings.NBT
{
    public class Block
    {
        static Block()
        {
            Air = new Block();
        }

        public static Block Air { get; private set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int Z { get; set; }
        public int BlockID { get; set; }
        public int Data { get; set; }

        public MappingBlock MappedBlock
        {
            get
            {
                return BlockMapping.BlockMappings[ItemID];
            }
        }

        /// <summary>Returns ItemID:SubID</summary>
        public string ItemID
        {
            get
            {
                if (Data > 0)
                    return BlockID + ":" + Data;
                else
                    return BlockID.ToString();
            }
        }

        public override string ToString()
        {
            return string.Format("ID: {3}:{4}, X: {0}, Y: {1}, Z: {2}", X, Y, Z, BlockID, Data);
        }
    }
}
