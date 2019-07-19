using BlockTypes;
using fNbt;
using Newtonsoft.Json;
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
                if (TryGetScematicMetadataFilePath(name + METADATA_FILEEXT, out string savePath))
                {
                    var json = JSON.Deserialize(savePath);
                    metadata = JsonConvert.DeserializeObject<SchematicMetadata>(json.ToString());
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

        public static bool TryGetScematicMetadataFilePath(string name, out string colonySaves)
        {
            colonySaves = GameLoader.Schematic_SAVE_LOC;

            if (!Directory.Exists(colonySaves))
                Directory.CreateDirectory(colonySaves);

            if (File.Exists(colonySaves + name))
                colonySaves = colonySaves + name;
            else
                colonySaves = null;

            return !string.IsNullOrWhiteSpace(colonySaves);
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

        public static void SaveSchematic(Colony colony, Schematic schematic)
        {
            var colonySaves = GameLoader.Schematic_SAVE_LOC + $"\\{colony.ColonyID}\\";

            if (!Directory.Exists(colonySaves))
                Directory.CreateDirectory(colonySaves);

            List<NbtTag> tags = new List<NbtTag>();

            tags.Add(new NbtInt("Width", schematic.XMax));
            tags.Add(new NbtInt("Height", schematic.YMax));
            tags.Add(new NbtInt("Length", schematic.ZMax));

            List<NbtTag> blocks = new List<NbtTag>();

            for (int Y = 0; Y <= schematic.YMax; Y++)
            {
                for (int Z = 0; Z <= schematic.ZMax; Z++)
                {
                    for (int X = 0; X <= schematic.XMax; X++)
                    {
                        NbtCompound compTag = new NbtCompound();
                        compTag.Add(new NbtInt("x", X));
                        compTag.Add(new NbtInt("y", Y));
                        compTag.Add(new NbtInt("z", Z));
                        compTag.Add(new NbtString("id", schematic.Blocks[X,Y,Z].BlockID));
                        blocks.Add(compTag);
                    }
                }
            }

            NbtList nbtList = new NbtList("CSBlocks", blocks);
            tags.Add(nbtList);

            NbtFile nbtFile = new NbtFile(new NbtCompound("CompoundTag", tags));
            var fileSave = Path.Combine(colonySaves, schematic.Name + ".schematic");
            var metaDataSave = Path.Combine(GameLoader.Schematic_SAVE_LOC, schematic.Name + ".schematic.metadata.json");

            if (File.Exists(fileSave))
                File.Delete(fileSave);

            if (File.Exists(metaDataSave))
                File.Delete(metaDataSave);

            GenerateMetaData(metaDataSave, schematic.Name, schematic);
            nbtFile.SaveToFile(fileSave, NbtCompression.GZip);
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
                var colonySaves = GameLoader.Schematic_SAVE_LOC + $"\\{colonyId}\\";

                if (!Directory.Exists(colonySaves))
                    Directory.CreateDirectory(colonySaves);

                var metadataPath = Path.Combine(GameLoader.Schematic_SAVE_LOC, name + METADATA_FILEEXT);
                Schematic schematic = LoadSchematic(new NbtFile(path), Vector3Int.invalidPos);


                return GenerateMetaData(metadataPath, name.Substring(0, name.LastIndexOf('.')), schematic);
            }
            else
                return null;
        }

        private static SchematicMetadata GenerateMetaData(string metadataPath, string name, Schematic schematic)
        {
            var metadata = new SchematicMetadata();
            metadata.Name = name;

            for (int Y = 0; Y <= schematic.YMax; Y++)
            {
                for (int Z = 0; Z <= schematic.ZMax; Z++)
                {
                    for (int X = 0; X <= schematic.XMax; X++)
                    {
                        var block = schematic.Blocks[X, Y, Z].MappedBlock;

                        if (block.CSIndex != ColonyBuiltIn.ItemTypes.AIR.Id)
                        {
                            var buildType = ItemTypes.GetType(block.CSIndex);

                            if (!buildType.Name.Contains("bedend"))
                            {
                                var index = block.CSIndex;

                                if (!string.IsNullOrWhiteSpace(buildType.ParentType) && !buildType.Name.Contains("grass") && !buildType.Name.Contains("leaves"))
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
            }

            metadata.MaxX = schematic.XMax;
            metadata.MaxY = schematic.YMax;
            metadata.MaxZ = schematic.ZMax;

            JSON.Serialize(metadataPath, metadata.JsonSerialize());

            return metadata;
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
                        raw.XMax = tag.IntValue;
                        break;
                    case "Height": //Short
                        raw.YMax = tag.IntValue;
                        break;
                    case "Length": //Short
                        raw.ZMax = tag.IntValue;
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
                        raw.XMax = tag.IntValue + 1;
                        break;
                    case "Height": //Short
                        raw.YMax = tag.IntValue + 1;
                        break;
                    case "Length": //Short
                        raw.ZMax = tag.IntValue + 1;
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
                    case "Icon": //Compound
                        break; //Ignore
                    case "CSBlocks":
                        raw.CSBlocks = GetCSBlocks(tag, new SchematicBlock[raw.XMax + 1, raw.YMax + 1, raw.ZMax + 1]);
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

        public static SchematicBlock[,,] GetBlocks(RawSchematic rawSchematic)
        {
            //Sorted by height (bottom to top) then length then width -- the index of the block at X,Y,Z is (Y×length + Z)×width + X.
            SchematicBlock[,,] blocks = new SchematicBlock[rawSchematic.XMax + 1,rawSchematic.YMax + 1,rawSchematic.ZMax + 1];
            for (int Y = 0; Y <= rawSchematic.YMax; Y++)
            {
                for (int Z = 0; Z <= rawSchematic.ZMax; Z++)
                {
                    for (int X = 0; X <= rawSchematic.XMax; X++)
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
