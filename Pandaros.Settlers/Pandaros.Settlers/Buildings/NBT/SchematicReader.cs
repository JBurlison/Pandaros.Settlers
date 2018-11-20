using fNbt;
using Pipliz;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandaros.Settlers.Buildings.NBT
{
    public static class SchematicReader
    {
        static Dictionary<string, Dictionary<Vector3Int, Schematic>> _loadedSchematics = new Dictionary<string, Dictionary<Vector3Int, Schematic>>();

        public static bool TryGetSchematic(string name, int colonyId, Vector3Int location, out Schematic schematic)
        {
            if (TryGetScematicLocation(name, colonyId, out var scematic))
                schematic = LoadSchematic(scematic, location);
            else
                schematic = null;

            return schematic != null;
        }

        public static bool GetSchematicSize(string name, int colonyId, out RawSchematicSize size)
        {
            if (TryGetScematicLocation(name, colonyId, out var scematic))
                size = LoadRawSize(new NbtFile(scematic));
            else
                size = null;

            return size != null;
        }

        public static bool TryGetScematicLocation(string name, int colonyId, out string colonySaves)
        {
            colonySaves = GameLoader.Schematic_SAVE_LOC + $"\\{colonyId}\\";

            if (!Directory.Exists(colonySaves))
                Directory.CreateDirectory(colonySaves);

            if (File.Exists(colonySaves + name))
                colonySaves = colonySaves + name;
            else if (File.Exists(GameLoader.Schematic_DEFAULT_LOC + name))
                colonySaves = GameLoader.Schematic_DEFAULT_LOC + name;
            else
            {
                PandaLogger.Log(ChatColor.red, "Cannot find blueprint {0}!", name);
                colonySaves = null;
            }

            return !string.IsNullOrWhiteSpace(colonySaves);
        }

        private static Schematic LoadSchematic(string path, Vector3Int startPos)
        {
            if (_loadedSchematics.ContainsKey(path))
                if (_loadedSchematics[path].ContainsKey(startPos))
                    return _loadedSchematics[path][startPos];
                else
                {
                    var item = _loadedSchematics[path].FirstOrDefault();

                    if (item.Key != null)
                    {
                        _loadedSchematics[path][startPos] = item.Value;
                        return item.Value;
                    }
                }

            NbtFile file = new NbtFile(path);
            return LoadSchematic(file, startPos);
        }

        public static void UnloadSchematic(string path, Vector3Int startPos)
        {
            if (_loadedSchematics.ContainsKey(path))
            {
                if (_loadedSchematics[path].ContainsKey(startPos))
                    _loadedSchematics[path].Remove(startPos);

                if (_loadedSchematics[path].Count == 0)
                    _loadedSchematics.Remove(path);
            }
        }

        private static Schematic LoadSchematic(NbtFile nbtFile, Vector3Int startPos)
        {
            RawSchematic raw = LoadRaw(nbtFile);
            Block[,,] blocks = GetBlocks(raw);
            string name = Path.GetFileNameWithoutExtension(nbtFile.FileName);
            Schematic schematic = new Schematic(name, raw.XMax, raw.YMax, raw.ZMax, blocks, raw.TileEntities, startPos);

            if (!_loadedSchematics.ContainsKey(nbtFile.FileName))
                _loadedSchematics.Add(nbtFile.FileName, new Dictionary<Vector3Int, Schematic>());
  
            _loadedSchematics[nbtFile.FileName][startPos] = schematic;

            return schematic;
        }

        private static RawSchematicSize LoadRawSize(NbtFile nbtFile)
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

        private static RawSchematic LoadRaw(NbtFile nbtFile)
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
                    case "TileEntities": //List
                        TileEntity[,,] tileEntities = new TileEntity[raw.XMax, raw.YMax, raw.ZMax];
                        raw.TileEntities = GetTileEntities(tag, tileEntities);
                        break;
                    case "Icon": //Compound
                        break; //Ignore
                    case "SchematicaMapping": //Compound
                        tag.ToString();
                        break; //Ignore
                    default:
                        break;
                }
            }
            return raw;
        }

        private static TileEntity[,,] GetTileEntities(NbtTag tileEntitiesList, TileEntity[,,] list)
        {
            NbtList TileEntities = tileEntitiesList as NbtList;
            if (TileEntities != null)
            {
                foreach (NbtCompound compTag in TileEntities)
                {
                    NbtTag xTag = compTag["x"];
                    NbtTag yTag = compTag["y"];
                    NbtTag zTag = compTag["z"];
                    NbtTag idTag = compTag["id"];
                    TileEntity entity = new TileEntity(xTag.IntValue, yTag.IntValue, zTag.IntValue, idTag.StringValue);
                    list[xTag.IntValue, yTag.IntValue, zTag.IntValue] = entity;
                }
            }
            return list;
        }

        private static Block[,,] GetBlocks(RawSchematic rawSchematic)
        {
            //Sorted by height (bottom to top) then length then width -- the index of the block at X,Y,Z is (Y×length + Z)×width + X.
            Block[,,] blocks = new Block[rawSchematic.XMax,rawSchematic.YMax,rawSchematic.ZMax];
            for (int Y = 0; Y < rawSchematic.YMax; Y++)
            {
                for (int Z = 0; Z < rawSchematic.ZMax; Z++)
                {
                    for (int X = 0; X < rawSchematic.XMax; X++)
                    {
                        int index = (Y * rawSchematic.ZMax + Z) * rawSchematic.XMax + X;
                        Block block = new Block();
                        block.BlockID = rawSchematic.Blocks[index];
                        block.Data = rawSchematic.Data[index];
                        block.X = X;
                        block.Y = Y;
                        block.Z = Z;

                        blocks[X,Y,Z] = block;
                    }
                }
            }
            return blocks;
        }
    }
}
