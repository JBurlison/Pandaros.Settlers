using Chatting;
using Pandaros.API;
using Pandaros.Settlers.Server;
using Pipliz.JSON;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Reflection;

namespace Pandaros.Settlers
{
    [ModLoader.ModManager]
    public static class GameLoader
    {
        public static readonly ReadOnlyCollection<string> BLOCK_ROTATIONS = new ReadOnlyCollection<string>(new List<string>() { "x+", "x-", "z+", "z-" });

        public const string MESHTYPE = ".obj";
        public const string ICONTYPE = ".png";
        public const string NAMESPACE = "Pandaros.Settlers";
        public const string TYPEPREFIX = NAMESPACE + ".Types.";
        public const string SETTLER_INV = "Pandaros.Settlers.Inventory";
        public const string ALL_SKILLS = "Pandaros.Settlers.ALLSKILLS";
        public static string MESH_PATH = "Meshes/";
        public static string AUDIO_PATH = "Audio/";
        public static string ICON_PATH = "icons/";
        public static string BLOCKS_ALBEDO_PATH = "Textures/albedo/";
        public static string BLOCKS_EMISSIVE_PATH = "Textures/emissive/";
        public static string BLOCKS_HEIGHT_PATH = "Textures/height/";
        public static string BLOCKS_NORMAL_PATH = "Textures/normal/";
        public static string BLOCKS_NPC_PATH = "Textures/npc/";
        public static string TEXTURE_FOLDER_PANDA = "Textures";
        public static string NPC_PATH = "materials/npc/";
        public static string MOD_FOLDER = @"";
        public static string MODS_FOLDER = @"";
        public static string GAMEDATA_FOLDER = @"";
        public static string GAME_ROOT = @"";
        public static string SAVE_LOC = "";
        public static string MACHINE_JSON = "";
        public static string Schematic_SAVE_LOC = "";
        public static string Schematic_DEFAULT_LOC = "";
        
        public static readonly Version MOD_VER = new Version(0, 8, 3, 5);
        public static bool RUNNING { get; private set; }
        public static bool WorldLoaded { get; private set; }
        public static Colony StubColony { get; private set; }
        public static JSONNode ModInfo { get; private set; }
        public static Dictionary<string, JSONNode> AllModInfos { get; private set; } = new Dictionary<string, JSONNode>(StringComparer.InvariantCultureIgnoreCase);
        public static bool FileWasCopied { get; set; }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterSelectedWorld, NAMESPACE + ".AfterSelectedWorld")]
        public static void AfterSelectedWorld()
        {
            WorldLoaded                 = true;
            SAVE_LOC                    = GAMEDATA_FOLDER + "savegames/" + ServerManager.WorldName + "/";
            MACHINE_JSON = $"{SAVE_LOC}/{NAMESPACE}.Machines.json";
            Schematic_SAVE_LOC = $"{SAVE_LOC}/Schematics/";

            if (!Directory.Exists(Schematic_SAVE_LOC))
                Directory.CreateDirectory(Schematic_SAVE_LOC);

            StubColony = Colony.CreateStub(-99998);
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnCreatedColony, NAMESPACE + ".OnAssemblyLoaded")]
        [ModLoader.ModCallbackDependsOn(GameInitializer.NAMESPACE + ".Entities.ColonyState.OnLoadingColony")]
        public static void OnCreatedColony(Colony c)
        {
            var state = Pandaros.API.Entities.ColonyState.GetColonyState(c);
            state.Difficulty = GameDifficulty.Medium;
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnAssemblyLoaded, NAMESPACE + ".OnAssemblyLoaded")]
        public static void OnAssemblyLoaded(string path)
        {
            MOD_FOLDER = Path.GetDirectoryName(path).Replace("\\", "/");
            Schematic_DEFAULT_LOC = $"{MOD_FOLDER}/Schematics/";
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;

            if (!Directory.Exists(Schematic_DEFAULT_LOC))
                Directory.CreateDirectory(Schematic_DEFAULT_LOC);

            GAME_ROOT = Path.GetDirectoryName(Assembly.GetCallingAssembly().Location) + @"/../../";
            GAMEDATA_FOLDER = Path.Combine(GAME_ROOT, "gamedata").Replace("\\", "/") + "/";      
            MODS_FOLDER = MOD_FOLDER + @"/../../";

            ICON_PATH = Path.Combine(MOD_FOLDER, "icons").Replace("\\", "/") + "/";
            MESH_PATH = Path.Combine(MOD_FOLDER, "Meshes").Replace("\\", "/") + "/";
            AUDIO_PATH = Path.Combine(MOD_FOLDER, "Audio").Replace("\\", "/") + "/";
            TEXTURE_FOLDER_PANDA = Path.Combine(MOD_FOLDER, "Textures").Replace("\\", "/") + "/";
            BLOCKS_ALBEDO_PATH = Path.Combine(TEXTURE_FOLDER_PANDA, "albedo").Replace("\\", "/") + "/";
            BLOCKS_EMISSIVE_PATH = Path.Combine(TEXTURE_FOLDER_PANDA, "emissive").Replace("\\", "/") + "/";
            BLOCKS_HEIGHT_PATH = Path.Combine(TEXTURE_FOLDER_PANDA, "height").Replace("\\", "/") + "/";
            BLOCKS_NORMAL_PATH = Path.Combine(TEXTURE_FOLDER_PANDA, "normal").Replace("\\", "/") + "/";
            BLOCKS_NPC_PATH = Path.Combine(TEXTURE_FOLDER_PANDA, "npc").Replace("\\", "/") + "/";

            ModInfo = JSON.Deserialize(MOD_FOLDER + "/modInfo.json")[0];

            SettlersLogger.Log("Found mod in {0}", MOD_FOLDER);
            SettlersLogger.Log("GAME_ROOT in {0}", GAME_ROOT);
            SettlersLogger.Log("GAMEDATA_FOLDER in {0}", GAMEDATA_FOLDER);
            SettlersLogger.Log("MODS_FOLDER in {0}", MODS_FOLDER);

            List<string> allinfos = new List<string>();
            DirSearch(MODS_FOLDER, "*modInfo.json", allinfos);

            foreach (var info in allinfos)
            {
                var modJson = JSON.Deserialize(info)[0];

                if (modJson.TryGetAs("enabled", out bool isEnabled) && isEnabled)
                {
                    SettlersLogger.Log("ModInfo Found: {0}", info);
                    AllModInfos[new FileInfo(info).Directory.FullName] = modJson;
                }
            }

            if (!File.Exists(GAME_ROOT + "/colonyserver.exe.config"))
            {
                File.Copy(MOD_FOLDER + "/App.config", GAME_ROOT + "/colonyserver.exe.config");
                FileWasCopied = true;
            }

            GenerateSettlersBuiltin();

            if (FileWasCopied)
                SettlersLogger.Log(ChatColor.red, "For settlers mod to fully be installed the Colony Survival surver needs to be restarted.");
        }

        private static void GenerateSettlersBuiltin()
        {
            if (File.Exists(MOD_FOLDER + "/SettlersBuiltin.cs"))
                File.Delete(MOD_FOLDER + "/SettlersBuiltin.cs");
            
            using (var fs = File.OpenWrite(MOD_FOLDER + "/SettlersBuiltin.cs"))
            using (var sr = new StreamWriter(fs))
            {
                sr.WriteLine("using Pandaros.Settlers.Models;");
                sr.WriteLine();
                sr.WriteLine("namespace Pandaros.Settlers");
                sr.WriteLine("{");
                sr.WriteLine("  public static class SettlersBuiltIn");
                sr.WriteLine("  {");
                sr.WriteLine("      public static class Research");
                sr.WriteLine("      {");

                foreach (var node in JSON.Deserialize(MOD_FOLDER + "/localization/en-US/science/en-US.json")["sentences"]["Pandaros"]["Settlers"].LoopObject())
                        sr.WriteLine($"          public const string {node.Key.ToUpper()} = \"{GameLoader.NAMESPACE}.{node.Key}\";");

                foreach (var node in JSON.Deserialize(MOD_FOLDER + "/localization/en-US/science/en-US.json")["sentences"]["pipliz"].LoopObject())
                        sr.WriteLine($"          public const string {node.Key.ToUpper()} = \"{GameLoader.NAMESPACE}.pipliz.{node.Key}\";");

                sr.WriteLine("      }");
                sr.WriteLine();

                sr.WriteLine("      public static class ItemTypes");
                sr.WriteLine("      {");

                foreach (var node in JSON.Deserialize(MOD_FOLDER + "/localization/en-US/en-US.json")["types"].LoopObject())
                    sr.WriteLine($"          public static readonly ItemId {node.Key.Replace('+', 'p').Replace('-', 'n').Replace(GameLoader.NAMESPACE + ".AutoLoad.", "").Replace(GameLoader.NAMESPACE + ".", "").Replace(".", "_").ToUpper()} = ItemId.GetItemId(\"{node.Key}\");");

                sr.WriteLine("      }");
                sr.WriteLine("  }");
                sr.WriteLine("}");
            }
        }

        private static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            SettlersLogger.Log(args.Name);
            try
            {
                if (args.Name.Contains("System.Xml.Linq"))
                    return Assembly.LoadFile(MOD_FOLDER + "/System.Xml.Linq.dll");

                if (args.Name.Contains("System.ComponentModel.DataAnnotations"))
                    return Assembly.LoadFile(MOD_FOLDER + "/System.ComponentModel.DataAnnotations.dll");

                if (args.Name.Contains("System.Numerics"))
                    return Assembly.LoadFile(MOD_FOLDER + "/System.Numerics.dll");

                if (args.Name.Contains("System.Runtime.Serialization"))
                    return Assembly.LoadFile(MOD_FOLDER + "/System.Runtime.Serialization.dll");

                if (args.Name.Contains("System.Transactions"))
                    return Assembly.LoadFile(MOD_FOLDER + "/System.Transactions.dll");

                if (args.Name.Contains("System.Data.SQLite"))
                    return Assembly.LoadFile(MOD_FOLDER + "/System.Data.SQLite.dll");

                if (args.Name.Contains("System.Data"))
                    return Assembly.LoadFile(MOD_FOLDER + "/System.Data.dll");

                if (args.Name.Contains("System.Drawing"))
                    return Assembly.LoadFile(MOD_FOLDER + "/System.Drawing.Common.dll");
            }
            catch (Exception ex)
            {
                SettlersLogger.LogError(ex);
            }

            return null;
        }

        public static void DirSearch(string sDir, string searchPattern, List<string> found)
        {
            try
            {
                foreach (string d in Directory.GetDirectories(sDir))
                {
                    foreach (string f in Directory.GetFiles(d, searchPattern))
                        found.Add(f);
                  
                    DirSearch(d, searchPattern, found);
                }
            }
            catch (System.Exception excpt)
            {
                Console.WriteLine(excpt.Message);
            }
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterModsLoaded, NAMESPACE + ".AfterStartup")]
        public static void AfterStartup(List<ModLoader.ModDescription> modDescriptions)
        {
            RUNNING = true;
            CommandManager.RegisterCommand(new ChatHistory());
            CommandManager.RegisterCommand(new ConfigurationChatCommand());
            CommandManager.RegisterCommand(new SettlersChatCommand());
            CommandManager.RegisterCommand(new UnknownItemCleanup());
#if Debug
            ChatCommands.CommandManager.RegisterCommand(new Research.PandaResearchCommand());
#endif
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnQuit, NAMESPACE + ".OnQuitLate")]
        public static void OnQuitLate()
        {
            RUNNING     = false;
            WorldLoaded = false;
        }

        public static string GetUpdatableBlocksJSONPath()
        {
            return string.Format("gamedata/savegames/{0}/updatableblocks.json", ServerManager.WorldName);
        }

        public static Dictionary<string, List<JSONNode>> GetJSONSettings(string fileType)
        {
            Dictionary<string, List<JSONNode>> retval = new Dictionary<string, List<JSONNode>>();

            try
            {
                foreach (var info in GameLoader.AllModInfos)
                    if (info.Value.TryGetAs(GameLoader.NAMESPACE + ".jsonFiles", out JSONNode jsonFilles))
                    {
                        foreach (var jsonNode in jsonFilles.LoopArray())
                        {
                            if (jsonNode.TryGetAs("fileType", out string jsonFileType))
                            {
                                if (jsonFileType == fileType)
                                {
                                    if (!retval.ContainsKey(info.Key))
                                        retval.Add(info.Key, new List<JSONNode>());

                                    retval[info.Key].Add(jsonNode);
                                    SettlersLogger.LogToFile("Getting json configurations {0} from file {1}", fileType, info.Key);
                                }
                            }
                            else
                            {
                                SettlersLogger.Log(ChatColor.red, "Unable to read fileType from file {0}", info.Value);
                            }
                        }
                    }
            }
            catch (Exception ex)
            {
                SettlersLogger.LogError(ex);
            }

            return retval;
        }

        public static Dictionary<string, List<string>> GetJSONSettingPaths(string fileType)
        {
            Dictionary<string, List<string>> retval = new Dictionary<string, List<string>>();

            try
            {
                foreach (var info in GameLoader.AllModInfos)
                    if (info.Value.TryGetAs(GameLoader.NAMESPACE + ".jsonFiles", out JSONNode jsonFilles))
                    {
                        foreach (var jsonNode in jsonFilles.LoopArray())
                        {
                            if (jsonNode.TryGetAs("fileType", out string jsonFileType))
                            {
                                if (jsonFileType == fileType)
                                    if (jsonNode.TryGetAs("relativePath", out string itemsPath))
                                    {
                                        if (!retval.ContainsKey(info.Key))
                                            retval.Add(info.Key, new List<string>());

                                        retval[info.Key].Add(itemsPath);
                                        SettlersLogger.LogToFile("Getting json configurations {0} from file {1}", fileType, info.Key);
                                    }
                                    else
                                    {
                                        SettlersLogger.Log(ChatColor.red, "Unable to read relativePath for fileType {0} from file {1}", itemsPath, info.Key);
                                    }
                            }
                            else
                            {
                                SettlersLogger.Log(ChatColor.red, "Unable to read fileType from file {0}", info.Key);
                            }
                        }
                    }
            }
            catch (Exception ex)
            {
                SettlersLogger.LogError(ex);
            }

            return retval;
        }
    }
}
