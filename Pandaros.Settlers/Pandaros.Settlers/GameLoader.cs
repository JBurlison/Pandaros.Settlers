using Chatting;
using Pandaros.Settlers.AI;
using Pandaros.Settlers.ColonyManagement;
using Pandaros.Settlers.Items.Armor;
using Pandaros.Settlers.Jobs.Roaming;
using Pandaros.Settlers.Monsters;
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

        public const string NAMESPACE = "Pandaros.Settlers";
        public const string SETTLER_INV = "Pandaros.Settlers.Inventory";
        public const string ALL_SKILLS = "Pandaros.Settlers.ALLSKILLS";
        public static string MESH_PATH = "gamedata/mods/Pandaros/Settlers/meshes/";
        public static string AUDIO_PATH = "gamedata/mods/Pandaros/Settlers/Audio/";
        public static string ICON_PATH = "gamedata/mods/Pandaros/Settlers/icons/";
        public static string BLOCKS_ALBEDO_PATH = "Textures/albedo/";
        public static string BLOCKS_EMISSIVE_PATH = "Textures/emissive/";
        public static string BLOCKS_HEIGHT_PATH = "Textures/height/";
        public static string BLOCKS_NORMAL_PATH = "Textures/normal/";
        public static string BLOCKS_NPC_PATH = "gamedata/mods/Pandaros/Settlers/Textures/npc/";
        public static string TEXTURE_FOLDER_PANDA = "Textures";
        public static string NPC_PATH = "gamedata/textures/materials/npc/";
        public static string MOD_FOLDER = @"gamedata/mods/Pandaros/Settlers";
        public static string MODS_FOLDER = @"";
        public static string GAMEDATA_FOLDER = @"";
        public static string GAME_ROOT = @"";
        public static string SAVE_LOC = "";
        public static string Schematic_SAVE_LOC = "";
        public static string Schematic_DEFAULT_LOC = "";

        public static readonly Version MOD_VER = new Version(0, 8, 2, 0);
        public static bool RUNNING { get; private set; }
        public static bool WorldLoaded { get; private set; }
        public static Colony StubColony { get; private set; }
        public static ushort MissingMonster_Icon { get; private set; }
        public static ushort Repairing_Icon { get; private set; }
        public static ushort Refuel_Icon { get; private set; }
        public static ushort Waiting_Icon { get; private set; }
        public static ushort Reload_Icon { get; private set; }
        public static ushort Broken_Icon { get; private set; }
        public static ushort Empty_Icon { get; private set; }
        public static ushort NOAMMO_Icon { get; private set; }
        public static ushort Poisoned_Icon { get; private set; }
        public static ushort Bow_Icon { get; private set; }
        public static JSONNode ModInfo { get; private set; }
        public static Dictionary<string, JSONNode> AllModInfos { get; private set; } = new Dictionary<string, JSONNode>();
        public static bool FileWasCopied { get; set; }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterSelectedWorld, NAMESPACE + ".AfterSelectedWorld")]
        public static void AfterSelectedWorld()
        {
            WorldLoaded                 = true;
            SAVE_LOC                    = GAMEDATA_FOLDER + "savegames/" + ServerManager.WorldName + "/";
            RoamingJobManager.MACHINE_JSON = $"{SAVE_LOC}/{NAMESPACE}.Machines.json";
            Schematic_SAVE_LOC = $"{SAVE_LOC}/Schematics/";

            if (!Directory.Exists(Schematic_SAVE_LOC))
                Directory.CreateDirectory(Schematic_SAVE_LOC);

            if (!File.Exists(SAVE_LOC + "pandaros.settlers.sqlite"))
                File.Copy(MOD_FOLDER + "/pandaros.settlers.sqlite", SAVE_LOC + "pandaros.settlers.sqlite");

            StubColony = Colony.CreateStub(-99998);
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnAssemblyLoaded, NAMESPACE + ".OnAssemblyLoaded")]
        public static void OnAssemblyLoaded(string path)
        {
            MOD_FOLDER = Path.GetDirectoryName(path);
            Schematic_DEFAULT_LOC = $"{MOD_FOLDER}/Schematics/";
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
            if (!Directory.Exists(Schematic_DEFAULT_LOC))
                Directory.CreateDirectory(Schematic_DEFAULT_LOC);

            PandaLogger.Log("Found mod in {0}", MOD_FOLDER);

            GAME_ROOT = path.Substring(0, path.IndexOf("gamedata")).Replace("\\", "/");
            GAMEDATA_FOLDER = path.Substring(0, path.IndexOf("gamedata") + "gamedata".Length).Replace("\\", "/") + "/";

            MODS_FOLDER = GAMEDATA_FOLDER + "mods/";
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

            List<string> allinfos = new List<string>();
            DirSearch(MODS_FOLDER, "*modInfo.json", allinfos);

            foreach (var info in allinfos)
            {
                var modJson = JSON.Deserialize(info)[0];

                if (modJson.TryGetAs("enabled", out bool isEnabled) && isEnabled)
                {
                    PandaLogger.Log("ModInfo Found: {0}", info);
                    AllModInfos[new FileInfo(info).Directory.FullName] = modJson;
                }
            }

            if (!File.Exists(GAME_ROOT + "/colonyserver.exe.config"))
            {
                File.Copy(MOD_FOLDER + "/App.config", GAME_ROOT + "/colonyserver.exe.config");
                FileWasCopied = true;
            }

            foreach (var file in Directory.GetFiles(MOD_FOLDER + "/ZipSupport"))
            {
                var destFile = GAME_ROOT + "colonyserver_Data/Managed/" + new FileInfo(file).Name;

                if (!File.Exists(destFile))
                {
                    FileWasCopied = true;
                    File.Copy(file, destFile);
                }
            }

            GenerateBuiltinBlocks();

            if (FileWasCopied)
                PandaLogger.Log(ChatColor.red, "For settlers mod to fully be installed the Colony Survival surver needs to be restarted.");
        }

        private static void GenerateBuiltinBlocks()
        {
            if (File.Exists(MOD_FOLDER + "/ColonyBuiltin.cs"))
                File.Delete(MOD_FOLDER + "/ColonyBuiltin.cs");

            using (var fs = File.OpenWrite(MOD_FOLDER + "/ColonyBuiltin.cs"))
            using (var sr = new StreamWriter(fs))
            {
                sr.WriteLine("using Pandaros.Settlers.Models;");
                sr.WriteLine();
                sr.WriteLine("namespace Pandaros.Settlers");
                sr.WriteLine("{");
                sr.WriteLine("  public static class ColonyBuiltIn");
                sr.WriteLine("  {");
                sr.WriteLine("      public static class Research");
                sr.WriteLine("      {");

                foreach (var node in JSON.Deserialize(GAMEDATA_FOLDER + "science.json").LoopArray())
                    if (node.TryGetAs("key", out string scienceKey))
                        sr.WriteLine($"          public const string {scienceKey.Substring(scienceKey.LastIndexOf('.') + 1).ToUpper()} = \"{scienceKey}\";");

                sr.WriteLine("      }");
                sr.WriteLine();

                sr.WriteLine("      public static class NpcTypes");
                sr.WriteLine("      {");

                foreach (var node in JSON.Deserialize(GAMEDATA_FOLDER + "npcTypes.json").LoopArray())
                    if (node.TryGetAs("keyName", out string npcType))
                        sr.WriteLine($"          public const string {npcType.Substring(npcType.LastIndexOf('.') + 1).ToUpper()} = \"{npcType}\";");

                sr.WriteLine("      }");
                sr.WriteLine();

                sr.WriteLine("      public static class ItemTypes");
                sr.WriteLine("      {");

                foreach (var node in JSON.Deserialize(GAMEDATA_FOLDER + "generateblocks.json").LoopArray())
                    if (node.TryGetAs("generateType", out string genType) && genType == "rotateBlock" && node.TryGetAs("typeName", out string itemName))
                    {
                        sr.WriteLine($"          public static readonly ItemId {itemName.Replace('+', 'p').Replace('-', 'n').ToUpper()} = ItemId.GetItemId(\"{itemName}\");");

                        foreach (var rotation in BLOCK_ROTATIONS)
                            sr.WriteLine($"          public static readonly ItemId {(itemName + rotation).Replace('+', 'p').Replace('-', 'n').ToUpper()} = ItemId.GetItemId(\"{(itemName + rotation)}\");");
                    }

                foreach (var node in JSON.Deserialize(GAMEDATA_FOLDER + "types.json").LoopObject())
                        sr.WriteLine($"          public static readonly ItemId {node.Key.Replace('+', 'p').Replace('-', 'n').ToUpper()} = ItemId.GetItemId(\"{node.Key}\");");

                sr.WriteLine("      }");
                sr.WriteLine("  }");
                sr.WriteLine("}");
            }
        }

        private static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            PandaLogger.Log(args.Name);
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
            }
            catch (Exception ex)
            {
                PandaLogger.LogError(ex);
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

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AddItemTypes, NAMESPACE + ".addlittypes")]
        public static void AddItemTypes(Dictionary<string, ItemTypesServer.ItemTypeRaw> items)
        {
            var monsterNode = new JSONNode();
            monsterNode["icon"] = new JSONNode(ICON_PATH + "NoMonster.png");
            var monster = new ItemTypesServer.ItemTypeRaw(NAMESPACE + ".Monster", monsterNode);
            MissingMonster_Icon = monster.ItemIndex;

            items.Add(NAMESPACE + ".Monster", monster);

            var repairingNode = new JSONNode();
            repairingNode["icon"] = new JSONNode(ICON_PATH + "Repairing.png");
            var repairing = new ItemTypesServer.ItemTypeRaw(NAMESPACE + ".Repairing", repairingNode);
            Repairing_Icon = repairing.ItemIndex;

            items.Add(NAMESPACE + ".Repairing", repairing);

            var refuelNode = new JSONNode();
            refuelNode["icon"] = new JSONNode(ICON_PATH + "Refuel.png");
            var refuel = new ItemTypesServer.ItemTypeRaw(NAMESPACE + ".Refuel", refuelNode);
            Refuel_Icon = refuel.ItemIndex;

            items.Add(NAMESPACE + ".Refuel", refuel);

            var waitingNode = new JSONNode();
            waitingNode["icon"] = new JSONNode(ICON_PATH + "Waiting.png");
            var waiting = new ItemTypesServer.ItemTypeRaw(NAMESPACE + ".Waiting", waitingNode);
            Waiting_Icon = waiting.ItemIndex;

            items.Add(NAMESPACE + ".Waiting", waiting);

            var reloadNode = new JSONNode();
            reloadNode["icon"] = new JSONNode(ICON_PATH + "Reload.png");
            var reload = new ItemTypesServer.ItemTypeRaw(NAMESPACE + ".Reload", reloadNode);
            Reload_Icon = reload.ItemIndex;

            items.Add(NAMESPACE + ".Reload", reload);

            var brokenNode = new JSONNode();
            brokenNode["icon"] = new JSONNode(ICON_PATH + "Broken.png");
            var broken = new ItemTypesServer.ItemTypeRaw(NAMESPACE + ".Broken", brokenNode);
            Broken_Icon = broken.ItemIndex;

            items.Add(NAMESPACE + ".Broken", broken);

            var emptyNode = new JSONNode();
            emptyNode["icon"] = new JSONNode(ICON_PATH + "Empty.png");
            var empty = new ItemTypesServer.ItemTypeRaw(NAMESPACE + ".Empty", emptyNode);
            Empty_Icon = empty.ItemIndex;

            items.Add(NAMESPACE + ".Empty", empty);

            var noAmmoNode = new JSONNode();
            noAmmoNode["icon"] = new JSONNode(ICON_PATH + "NoAmmo.png");
            var noAmmo = new ItemTypesServer.ItemTypeRaw(NAMESPACE + ".NoAmmo", emptyNode);
            NOAMMO_Icon = noAmmo.ItemIndex;

            items.Add(NAMESPACE + ".NoAmmo", noAmmo);

            var poisonedNode = new JSONNode();
            poisonedNode["icon"] = new JSONNode(ICON_PATH + "Poisoned.png");
            var poisoned = new ItemTypesServer.ItemTypeRaw(NAMESPACE + ".Poisoned", poisonedNode);
            Poisoned_Icon = poisoned.ItemIndex;

            items.Add(NAMESPACE + ".Poisoned", poisoned);

            var bowNode = new JSONNode();
            bowNode["icon"] = new JSONNode(ICON_PATH + "bow.png");
            var bow = new ItemTypesServer.ItemTypeRaw(NAMESPACE + ".BowIcon", bowNode);
            Bow_Icon = bow.ItemIndex;

            items.Add(NAMESPACE + ".BowIcon", bow);
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterStartup, NAMESPACE + ".AfterStartup")]
        public static void AfterStartup()
        {
            RUNNING = true;
            CommandManager.RegisterCommand(new GameDifficultyChatCommand());
            CommandManager.RegisterCommand(new CalltoArms());
            CommandManager.RegisterCommand(new ArmorCommand());
            CommandManager.RegisterCommand(new VersionChatCommand());
            CommandManager.RegisterCommand(new ColonyArchiver());
            CommandManager.RegisterCommand(new ConfigurationChatCommand());
            CommandManager.RegisterCommand(new BossesChatCommand());
            CommandManager.RegisterCommand(new SettlersChatCommand());

            VersionChecker.WriteVersionsToConsole();
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

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnTryChangeBlock, NAMESPACE + ".GameLoader.trychangeblock")]
        public static void OnTryChangeBlockUser(ModLoader.OnTryChangeBlockData userData)
        {
            if (userData.CallbackState == ModLoader.OnTryChangeBlockData.ECallbackState.Cancelled)
                return;

            var suffix = "bottom";
            var newType = userData.TypeNew;

            if (userData.CallbackOrigin == ModLoader.OnTryChangeBlockData.ECallbackOrigin.ClientPlayerManual)
            {
                var side    = userData.PlayerClickedData.GetVoxelHit().SideHit;

                switch (side)
                {
                    case VoxelSide.xPlus:
                        suffix = "right";
                        break;

                    case VoxelSide.xMin:
                        suffix = "left";
                        break;

                    case VoxelSide.yPlus:
                        suffix = "bottom";
                        break;

                    case VoxelSide.yMin:
                        suffix = "top";
                        break;

                    case VoxelSide.zPlus:
                        suffix = "front";
                        break;

                    case VoxelSide.zMin:
                        suffix = "back";
                        break;
                }

                
            }

            if (newType != userData.TypeOld && ItemTypes.IndexLookup.TryGetName(newType.ItemIndex, out var typename))
            {
                var otherTypename = typename + suffix;

                if (ItemTypes.IndexLookup.TryGetIndex(otherTypename, out var otherIndex))
                {
                    userData.TypeNew = ItemTypes.GetType(otherIndex);
                }
            }
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
                                    PandaLogger.LogToFile("Getting json configurations {0} from file {1}", fileType, info.Key);
                                }
                            }
                            else
                            {
                                PandaLogger.Log(ChatColor.red, "Unable to read fileType from file {0}", info.Value);
                            }
                        }
                    }
            }
            catch (Exception ex)
            {
                PandaLogger.LogError(ex);
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
                                        PandaLogger.LogToFile("Getting json configurations {0} from file {1}", fileType, info.Key);
                                    }
                                    else
                                    {
                                        PandaLogger.Log(ChatColor.red, "Unable to read relativePath for fileType {0} from file {1}", itemsPath, info.Key);
                                    }
                            }
                            else
                            {
                                PandaLogger.Log(ChatColor.red, "Unable to read fileType from file {0}", info.Key);
                            }
                        }
                    }
            }
            catch (Exception ex)
            {
                PandaLogger.LogError(ex);
            }

            return retval;
        }
    }
}
