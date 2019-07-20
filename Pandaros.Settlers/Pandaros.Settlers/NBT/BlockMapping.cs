﻿using Pandaros.API;
using Pipliz.JSON;
using System;
using System.Collections.Generic;
using System.IO;

namespace Pandaros.Settlers.NBT
{
    public class MappingBlock
    {
        private ushort _index = ushort.MaxValue;

        public int Type { get; set; }
        public int Meta { get; set; }
        public string Name { get; set; }
        public string TextType { get; set; }
        public string CSType { get; set; }
        public ushort CSIndex
        {
            get
            {
                if (_index == ushort.MaxValue)
                {
                    var newType = ColonyBuiltIn.ItemTypes.AIR.Id;
                    
                    if (!string.IsNullOrWhiteSpace(CSType))
                    {
                        if (ItemTypes.IndexLookup.TryGetIndex(CSType, out ushort index))
                            newType = index;
                        else
                        {
                            SettlersLogger.Log(ChatColor.yellow, "Unable to find CSType {0} from the itemType table for block {1} from mapping the file. This item will be mapped to air.", CSType, Name);
                            _index = ColonyBuiltIn.ItemTypes.AIR.Id;
                        }
                    }
                    else
                    { 
                        SettlersLogger.Log(ChatColor.yellow, "Item {0} from mapping file has a blank cstype. This item will be mapped to air.", Name);
                        _index = ColonyBuiltIn.ItemTypes.AIR.Id;
                    }

                    _index = newType;
                }

                return _index;
            }
        }
    }

    public static class BlockMapping
    {
        private const string ERROR_MESSAGE = "Schematic builders may not function properly. Undable to deserialize {0}";
        public static readonly string WorldPath = GameLoader.SAVE_LOC + "MCtoCSMapping.json";
        public static readonly string ModPath = GameLoader.MOD_FOLDER + "/MCtoCSMapping.json";

        public static Dictionary<string, MappingBlock> MCtoCSMappings { get; set; } = new Dictionary<string, MappingBlock>();
        public static Dictionary<string, List<MappingBlock>> CStoMCMappings { get; set; } = new Dictionary<string, List<MappingBlock>>();
        static BlockMapping()
        {
            LoadMappingFile(ModPath);
            LoadMappingFile(WorldPath);
        }

        public static void LoadMappingFile(string file)
        {
            if (File.Exists(file))
            {
                try
                {
                    if (JSON.Deserialize(file, out var json))
                    {
                        foreach (var node in json.LoopArray())
                        {
                            MappingBlock newBlock = new MappingBlock();

                            if (node.TryGetAs("type", out int type))
                                newBlock.Type = type;

                            if (node.TryGetAs("meta", out int meta))
                                newBlock.Meta = meta;

                            if (node.TryGetAs("name", out string name))
                                newBlock.Name = name;

                            if (node.TryGetAs("text_type", out string textType))
                                newBlock.TextType = textType;

                            if (node.TryGetAs("cs_type", out string csType))
                            {
                                newBlock.CSType = csType;

                                if (!CStoMCMappings.ContainsKey(csType))
                                    CStoMCMappings.Add(csType, new List<MappingBlock>());

                                CStoMCMappings[csType].Add(newBlock);
                            }
                            else
                                SettlersLogger.Log(ChatColor.yellow, "Unable to load item {0} from mapping file. This item will be mapped to air.", name);

                            if (newBlock.Meta > 0)
                                MCtoCSMappings[string.Format("{0}:{1}", newBlock.Type, newBlock.Meta)] = newBlock;
                            else
                                MCtoCSMappings[newBlock.Type.ToString()] = newBlock;
                        }
                    }
                    else
                        SettlersLogger.Log(ChatColor.red, ERROR_MESSAGE, file);
                }
                catch (Exception ex)
                {
                    SettlersLogger.Log(ChatColor.red, ERROR_MESSAGE, file);
                    SettlersLogger.LogError(ex);
                }
            }
        }

    }
}
