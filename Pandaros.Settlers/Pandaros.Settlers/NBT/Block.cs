using System.Linq;

namespace Pandaros.Settlers.NBT
{
    public class SchematicBlock
    {
        static SchematicBlock()
        {
            Air = new SchematicBlock();
        }

        public static SchematicBlock Air { get; private set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int Z { get; set; }
        public string BlockID { get; set; }
        public int Data { get; set; }
        public bool CSBlock { get; set; }

        public MappingBlock MappedBlock
        {
            get
            {
                try
                {
                    if (CSBlock)
                    {
                        if (BlockMapping.CStoMCMappings.TryGetValue(BlockID, out var mappingBlocks))
                        {
                            var map = mappingBlocks.FirstOrDefault();

                            if (map == null)
                            {
                                map = new MappingBlock()
                                {
                                    CSType = BlockID
                                };

                                BlockMapping.CStoMCMappings[BlockID].Add(map);
                            }

                            return map;
                        }
                        else
                        {
                            MappingBlock mappingBlock = new MappingBlock()
                            {
                                CSType = BlockID
                            };

                            BlockMapping.CStoMCMappings.Add(BlockID, new System.Collections.Generic.List<MappingBlock>() { mappingBlock });
                            return mappingBlock;
                        }
                    }
                    else if (BlockMapping.MCtoCSMappings.TryGetValue(ItemID, out var mapping))
                        return mapping;
                    else
                        PandaLogger.Log(ChatColor.yellow, "1) Unable to find mapping for block {0}", ToString());
                }
                catch (System.Exception)
                {
                    PandaLogger.Log(ChatColor.yellow, "2) Unable to find mapping for block {0}", ToString());
                }

                return BlockMapping.MCtoCSMappings[BlockTypes.BuiltinBlocks.Air.ToString()];
            }
        }

        /// <summary>Returns ItemID:SubID</summary>
        public string ItemID
        {
            get
            {
                if (string.IsNullOrEmpty(BlockID))
                    BlockID = "0";

                if (Data > 0 && BlockID != "0")
                    return BlockID + ":" + Data;
                else
                    return BlockID;
            }
        }

        public override string ToString()
        {
            return string.Format("ID: {3}:{4}, X: {0}, Y: {1}, Z: {2}", X, Y, Z, BlockID, Data);
        }
    }
}
