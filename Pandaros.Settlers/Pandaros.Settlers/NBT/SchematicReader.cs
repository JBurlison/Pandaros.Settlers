using BlockTypes;
using fNbt;
using Pipliz;
using Pipliz.JSON;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandaros.Settlers.NBT
{
    [ModLoader.ModManager]
    public static class SchematicReader
    {
        private const string METADATA_FILEEXT = ".metadata.json";

        public static bool TryGetSchematicMetadata(string name, int colonyId, out SchematicMetadata metadata)
        {
            try
            {
                if (TryGetScematicFilePath(name + METADATA_FILEEXT, colonyId, out string savePath))
                {
                    var json = JSON.Deserialize(savePath);
                    metadata = new SchematicMetadata(json);
                }
                else
                {
                    metadata = GenerateMetaData(name, colonyId);
                }
            }
            catch (Exception ex)
            {
                metadata = null;
                PandaLogger.LogError(ex, "error getting metadata for schematic {0}", name);
            }

            return metadata != null;
        }

        public static bool TryGetSchematic(string name, int colonyId, Vector3Int location, out Schematic schematic)
        {
            if (TryGetScematicFilePath(name, colonyId, out var scematic))
                schematic = LoadSchematic(scematic, location);
            else
                schematic = null;

            return schematic != null;
        }

        public static bool TryGetSchematicSize(string name, int colonyId, out RawSchematicSize size)
        {
            if (TryGetScematicFilePath(name, colonyId, out var scematic))
                size = LoadRawSize(new NbtFile(scematic));
            else
                size = null;

            return size != null;
        }

        public static bool TryGetScematicFilePath(string name, int colonyId, out string colonySaves)
        {
            colonySaves = GameLoader.Schematic_SAVE_LOC + $"\\{colonyId}\\";

            if (!Directory.Exists(colonySaves))
                Directory.CreateDirectory(colonySaves);

            if (File.Exists(colonySaves + name))
                colonySaves = colonySaves + name;
            else if (File.Exists(GameLoader.Schematic_DEFAULT_LOC + name))
                colonySaves = GameLoader.Schematic_DEFAULT_LOC + name;
            else
                colonySaves = null;

            return !string.IsNullOrWhiteSpace(colonySaves);
        }

        public static List<FileInfo> GetSchematics(Players.Player player)
        {
            var options = new List<FileInfo>();
            var colonySchematics = GameLoader.Schematic_SAVE_LOC + $"\\{player.ActiveColony.ColonyID}\\";

            if (!Directory.Exists(colonySchematics))
                Directory.CreateDirectory(colonySchematics);

            if (player.ActiveColony != null)
                foreach (var file in Directory.EnumerateFiles(colonySchematics, "*.schematic"))
                    options.Add(new FileInfo(file));

            foreach (var file in Directory.EnumerateFiles(GameLoader.Schematic_DEFAULT_LOC, "*.schematic"))
                options.Add(new FileInfo(file));

            return options.OrderBy(f => f.Name).ToList();
        }

        public static Schematic LoadSchematic(string path, Vector3Int startPos)
        {
            NbtFile file = new NbtFile(path);
            return LoadSchematic(file, startPos);
        }

        public static SchematicMetadata GenerateMetaData(string name, int colonyId)
        {
            if (TryGetScematicFilePath(name, colonyId, out string path))
            {
                var metadataPath = path + METADATA_FILEEXT;
                var metadata = new SchematicMetadata();
                metadata.Name = name.Substring(0, name.LastIndexOf('.'));
                Schematic schematic = LoadSchematic(new NbtFile(path), Vector3Int.invalidPos);

                for (int Y = 0; Y < schematic.YMax; Y++)
                {
                    for (int Z = 0; Z < schematic.ZMax; Z++)
                    {
                        for (int X = 0; X < schematic.XMax; X++)
                        {
                            var block = schematic.Blocks[X, Y, Z].MappedBlock;

                            if (block.CSIndex != BuiltinBlocks.Air)
                            {
                                var buildType = ItemTypes.GetType(block.CSIndex);
                                var index = block.CSIndex;

                                if (!string.IsNullOrWhiteSpace(buildType.ParentType))
                                    index = ItemTypes.GetType(buildType.ParentType).ItemIndex;


                                if (metadata.Blocks.TryGetValue(index, out var blockMeta))
                                {
                                    blockMeta.Count++;
                                }
                                else
                                {
                                    blockMeta = new SchematicBlockMetadata();
                                    blockMeta.Count++;
                                    blockMeta.ItemId = index;

                                    metadata.Blocks.Add(blockMeta.ItemId, blockMeta);
                                }
                            }
                        }
                    }
                }

                metadata.MaxX = schematic.XMax;
                metadata.MaxY = schematic.YMax;
                metadata.MaxZ = schematic.ZMax;

                JSON.Serialize(metadataPath, metadata.JsonSerialize());

                return metadata;
            }
            else
                return null;
        }

        public static Schematic LoadSchematic(NbtFile nbtFile, Vector3Int startPos)
        {
            RawSchematic raw = LoadRaw(nbtFile);
            SchematicBlock[,,] blocks = default(SchematicBlock[,,]);

            if (raw.CSBlocks != null && raw.CSBlocks.Length > 0)
                blocks = raw.CSBlocks;
            else
                blocks = GetBlocks(raw);

            string name = Path.GetFileNameWithoutExtension(nbtFile.FileName);
            Schematic schematic = new Schematic(name, raw.XMax, raw.YMax, raw.ZMax, blocks, startPos);

            return schematic;
        }

        public static RawSchematicSize LoadRawSize(NbtFile nbtFile)
        {
            RawSchematicSize raw = new RawSchematicSize();
            var rootTag = nbtFile.RootTag;

            foreach (NbtTag tag in rootTag.Tags)
            {
                switch (tag.Name)
                {
                    case "Width": //Short
                        raw.XMax = tag.ShortValue;
                        break;
                    case "Height": //Short
                        raw.YMax = tag.ShortValue;
                        break;
                    case "Length": //Short
                        raw.ZMax = tag.ShortValue;
                        break;
                    default:
                        break;
                }
            }
            return raw;
        }

        public static RawSchematic LoadRaw(NbtFile nbtFile)
        {
            RawSchematic raw = new RawSchematic();
            var rootTag = nbtFile.RootTag;

            foreach (NbtTag tag in rootTag.Tags)
            {
                switch (tag.Name)
                {
                    case "Width": //Short
                        raw.XMax = tag.ShortValue;
                        break;
                    case "Height": //Short
                        raw.YMax = tag.ShortValue;
                        break;
                    case "Length": //Short
                        raw.ZMax = tag.ShortValue;
                        break;
                    case "Materials": //String
                        raw.Materials = tag.StringValue;
                        break;
                    case "Blocks": //ByteArray
                        raw.Blocks = tag.ByteArrayValue;
                        break;
                    case "Data": //ByteArray
                        raw.Data = tag.ByteArrayValue;
                        break;
                    case "Entities": //List
                        break; //Ignore
                    //case "TileEntities": //List
                    //    TileEntity[,,] tileEntities = new TileEntity[raw.XMax, raw.YMax, raw.ZMax];
                    //    raw.TileEntities = GetTileEntities(tag, tileEntities);
                    //    break;
                    case "Icon": //Compound
                        break; //Ignore
                    case "CSBlocks":
                        raw.CSBlocks = GetCSBlocks(tag, new SchematicBlock[raw.XMax, raw.YMax, raw.ZMax]);
                        break;
                    case "SchematicaMapping": //Compound
                        tag.ToString();
                        break; //Ignore
                    default:
                        break;
                }
            }
            return raw;
        }

        public static SchematicBlock[,,] GetCSBlocks(NbtTag csBlockTag, SchematicBlock[,,] list)
        {
            NbtList csBlocks = csBlockTag as NbtList;

            if (csBlocks != null)
            {
                foreach (NbtCompound compTag in csBlocks)
                {
                    NbtTag xTag = compTag["x"];
                    NbtTag yTag = compTag["y"];
                    NbtTag zTag = compTag["z"];
                    NbtTag idTag = compTag["id"];
                    SchematicBlock block = new SchematicBlock()
                    {
                        X = xTag.IntValue,
                        Y = yTag.IntValue,
                        Z = zTag.IntValue,
                        BlockID = idTag.StringValue,
                        CSBlock = true
                    };

                    if (string.IsNullOrEmpty(block.BlockID))
                        block.BlockID = "0";

                    list[xTag.IntValue, yTag.IntValue, zTag.IntValue] = block;
                }
            }
            return list;
        }

        //private static TileEntity[,,] GetTileEntities(NbtTag tileEntitiesList, TileEntity[,,] list)
        //{
        //    NbtList TileEntities = tileEntitiesList as NbtList;
        //    if (TileEntities != null)
        //    {
        //        foreach (NbtCompound compTag in TileEntities)
        //        {
        //            NbtTag xTag = compTag["x"];
        //            NbtTag yTag = compTag["y"];
        //            NbtTag zTag = compTag["z"];
        //            NbtTag idTag = compTag["id"];
        //            TileEntity entity = new TileEntity(xTag.IntValue, yTag.IntValue, zTag.IntValue, idTag.StringValue);
        //            list[xTag.IntValue, yTag.IntValue, zTag.IntValue] = entity;
        //        }
        //    }
        //    return list;
        //}

        public static SchematicBlock[,,] GetBlocks(RawSchematic rawSchematic)
        {
            //Sorted by height (bottom to top) then length then width -- the index of the block at X,Y,Z is (Y×length + Z)×width + X.
            SchematicBlock[,,] blocks = new SchematicBlock[rawSchematic.XMax,rawSchematic.YMax,rawSchematic.ZMax];
            for (int Y = 0; Y < rawSchematic.YMax; Y++)
            {
                for (int Z = 0; Z < rawSchematic.ZMax; Z++)
                {
                    for (int X = 0; X < rawSchematic.XMax; X++)
                    {
                        int index = (Y * rawSchematic.ZMax + Z) * rawSchematic.XMax + X;
                        SchematicBlock block = new SchematicBlock();
                        block.BlockID = ((int)rawSchematic.Blocks[index]).ToString();
                        block.Data = rawSchematic.Data[index];
                        block.X = X;
                        block.Y = Y;
                        block.Z = Z;

                        if (string.IsNullOrEmpty(block.BlockID))
                            block.BlockID = "0";

                        blocks[X,Y,Z] = block;
                    }
                }
            }
            return blocks;
        }
    }
}
