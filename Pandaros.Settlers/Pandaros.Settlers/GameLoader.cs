using BlockTypes.Builtin;
using Pandaros.Settlers.Entities;
using Pipliz.JSON;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Pandaros.Settlers
{
    [ModLoader.ModManager]
    public static class GameLoader
    {
        public static string ICON_FOLDER_PANDA = @"gamedata/mods/Pandaros/settlers/icons";
        public static string LOCALIZATION_FOLDER_PANDA = @"gamedata/mods/Pandaros/settlers/localization";
        public static string MESH_FOLDER_PANDA = @"gamedata/mods/Pandaros/settlers/Meshes";
        public static string TEXTURE_FOLDER_PANDA = @"gamedata/mods/Pandaros/settlers/Textures";
        public static string MOD_FOLDER = @"gamedata/mods/Pandaros/settlers";
        public static string AUDIO_FOLDER_PANDA = @"gamedata/mods/Pandaros/settlers/Audio";
        public static string AUTOLOAD_FOLDER_PANDA = @"gamedata/mods/Pandaros/settlers/localization";

        public const string NAMESPACE = "Pandaros.Settlers";
        public const string SETTLER_INV = "Pandaros.Settlers.Inventory";
        public const string ALL_SKILLS = "Pandaros.Settlers.ALLSKILLS";

        public static readonly Version MOD_VER = new Version(0, 6, 3, 3);
        public static bool RUNNING { get; private set; }
        public static bool WorldLoaded { get; private set; }

        public static ushort MissingMonster_Icon { get; private set; }
        public static ushort Repairing_Icon { get; private set; }
        public static ushort Refuel_Icon { get; private set; }
        public static ushort Waiting_Icon { get; private set; }
        public static ushort Reload_Icon { get; private set; }
        public static ushort Broken_Icon { get; private set; }
        public static ushort Empty_Icon { get; private set; }
        public static ushort NOAMMO_Icon { get; private set; }

        private static ushort _itemSortIndex = 30000;

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterWorldLoad, GameLoader.NAMESPACE + ".AfterWorldLoad")]
        public static void AfterWorldLoad()
        {
            WorldLoaded = true;
            PandaLogger.Log(ChatColor.lime, "World load detected. Starting monitor...");
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnAssemblyLoaded, NAMESPACE + ".OnAssemblyLoaded")]
        public static void OnAssemblyLoaded(string path)
        {
            MOD_FOLDER = Path.GetDirectoryName(path);
            PandaLogger.Log("Found mod in {0}", MOD_FOLDER);
            LOCALIZATION_FOLDER_PANDA = Path.Combine(MOD_FOLDER, "localization").Replace("\\", "/");
            ICON_FOLDER_PANDA = Path.Combine(MOD_FOLDER, "icons").Replace("\\", "/");
            MESH_FOLDER_PANDA = Path.Combine(MOD_FOLDER, "Meshes").Replace("\\", "/");
            TEXTURE_FOLDER_PANDA = Path.Combine(MOD_FOLDER, "Textures").Replace("\\", "/");
            AUDIO_FOLDER_PANDA = Path.Combine(MOD_FOLDER, "Audio").Replace("\\", "/");
            AUTOLOAD_FOLDER_PANDA = Path.Combine(MOD_FOLDER, "AutoLoad").Replace("\\", "/");
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterAddingBaseTypes, NAMESPACE + ".addlittypes")]
        public static void AddLitTypes(Dictionary<string, ItemTypesServer.ItemTypeRaw> items)
        {
            var monsterNode = new JSONNode();
            monsterNode["icon"] = new JSONNode(ICON_FOLDER_PANDA + "/NoMonster.png");
            var monster = new ItemTypesServer.ItemTypeRaw(NAMESPACE + ".Monster", monsterNode);
            MissingMonster_Icon = monster.ItemIndex;
            
            items.Add(NAMESPACE + ".Monster", monster);

            var repairingNode = new JSONNode();
            repairingNode["icon"] = new JSONNode(ICON_FOLDER_PANDA + "/Repairing.png");
            var repairing = new ItemTypesServer.ItemTypeRaw(NAMESPACE + ".Repairing", repairingNode);
            Repairing_Icon = repairing.ItemIndex;

            items.Add(NAMESPACE + ".Repairing", repairing);

            var refuelNode = new JSONNode();
            refuelNode["icon"] = new JSONNode(ICON_FOLDER_PANDA + "/Refuel.png");
            var refuel = new ItemTypesServer.ItemTypeRaw(NAMESPACE + ".Refuel", refuelNode);
            Refuel_Icon = refuel.ItemIndex;

            items.Add(NAMESPACE + ".Refuel", refuel);

            var waitingNode = new JSONNode();
            waitingNode["icon"] = new JSONNode(ICON_FOLDER_PANDA + "/Waiting.png");
            var waiting = new ItemTypesServer.ItemTypeRaw(NAMESPACE + ".Waiting", waitingNode);
            Waiting_Icon = waiting.ItemIndex;

            items.Add(NAMESPACE + ".Waiting", waiting);

            var reloadNode = new JSONNode();
            reloadNode["icon"] = new JSONNode(ICON_FOLDER_PANDA + "/Reload.png");
            var reload = new ItemTypesServer.ItemTypeRaw(NAMESPACE + ".Reload", reloadNode);
            Reload_Icon = reload.ItemIndex;

            items.Add(NAMESPACE + ".Reload", reload);

            var brokenNode = new JSONNode();
            brokenNode["icon"] = new JSONNode(ICON_FOLDER_PANDA + "/Broken.png");
            var broken = new ItemTypesServer.ItemTypeRaw(NAMESPACE + ".Broken", brokenNode);
            Broken_Icon = broken.ItemIndex;

            items.Add(NAMESPACE + ".Broken", broken);

            var emptyNode = new JSONNode();
            emptyNode["icon"] = new JSONNode(ICON_FOLDER_PANDA + "/Empty.png");
            var empty = new ItemTypesServer.ItemTypeRaw(NAMESPACE + ".Empty", emptyNode);
            Empty_Icon = empty.ItemIndex;

            items.Add(NAMESPACE + ".Empty", empty);

            var noAmmoNode = new JSONNode();
            noAmmoNode["icon"] = new JSONNode(ICON_FOLDER_PANDA + "/NoAmmo.png");
            var noAmmo = new ItemTypesServer.ItemTypeRaw(NAMESPACE + ".NoAmmo", emptyNode);
            NOAMMO_Icon = noAmmo.ItemIndex;

            items.Add(NAMESPACE + ".NoAmmo", noAmmo);

            Jobs.MachinistJob.OkStatus = new List<uint>()
            {
                Refuel_Icon,
                Reload_Icon,
                Repairing_Icon,
                Waiting_Icon
            };
        }
        
        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterStartup, GameLoader.NAMESPACE + ".AfterStartup")]
        public static void AfterStartup()
        {
            PandaLogger.Log(ChatColor.lime, "Active. Version {0}", MOD_VER);
            RUNNING = true;
            ChatCommands.CommandManager.RegisterCommand(new GameDifficultyChatCommand());
            ChatCommands.CommandManager.RegisterCommand(new AI.CalltoArms());
            ChatCommands.CommandManager.RegisterCommand(new Items.ArmorCommand());
#if Debug
            ChatCommands.CommandManager.RegisterCommand(new Research.PandaResearchCommand());
#endif
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnQuitLate, GameLoader.NAMESPACE + ".OnQuitLate")]
        public static void OnQuitLate()
        {
            RUNNING = false;
            WorldLoaded = false;
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterWorldLoad, NAMESPACE + ".Localize")]
        [ModLoader.ModCallbackDependsOn("pipliz.server.localization.waitforloading")]
        [ModLoader.ModCallbackProvidesFor("pipliz.server.localization.convert")]
        public static void Localize()
        {
            PandaLogger.Log("Localization directory: {0}", LOCALIZATION_FOLDER_PANDA);
            try
            {
                string[] array = new string[]
                {
                    "translation.json"
                };
                for (int i = 0; i < array.Length; i++)
                {
                    string text = array[i];
                    string[] files = Directory.GetFiles(LOCALIZATION_FOLDER_PANDA, text, SearchOption.AllDirectories);
                    string[] array2 = files;
                    for (int j = 0; j < array2.Length; j++)
                    {
                        string text2 = array2[j];
                        try
                        {
                            JSONNode jsonFromMod;
                            if (JSON.Deserialize(text2, out jsonFromMod, false))
                            {
                                string name = Directory.GetParent(text2).Name;

                                if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(text))
                                {
                                    PandaLogger.Log("Found mod localization file for '{0}' localization", name);
                                    localize(name, text, jsonFromMod);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            PandaLogger.Log("Exception reading localization from {0}; {1}", text2, ex.Message);
                        }
                    }
                }
            }
            catch (DirectoryNotFoundException)
            {
                PandaLogger.Log("Localization directory not found at {0}", LOCALIZATION_FOLDER_PANDA);
            }
        }

        public static string GetUpdatableBlocksJSONPath()
        {
            return string.Format("gamedata/savegames/{0}/updatableblocks.json", ServerManager.WorldName);
        }

        public static void AddSoundFile(string key, List<string> fileNames)
        {
            var node = new JSONNode();
            node.SetAs("clipCollectionName", key);

            var fileListNode = new JSONNode(NodeType.Array);

            foreach (var fileName in fileNames)
            {
                var audoFileNode = new JSONNode()
                    .SetAs("path", fileName)
                    .SetAs("audioGroup", "Effects");

                fileListNode.AddToArray(audoFileNode);
            }
            
            node.SetAs("fileList", fileListNode);

            ItemTypesServer.AudioFilesJSON.AddToArray(node);
        }

        public static void localize(string locName, string locFilename, JSONNode jsonFromMod)
        {
            try
            { 
                if (Server.Localization.Localization.LoadedTranslation == null)
                {
                    PandaLogger.Log("Unable to localize. Server.Localization.Localization.LoadedTranslation is null.");
                }
                else
                {
                    if (Server.Localization.Localization.LoadedTranslation.TryGetValue(locName, out JSONNode jsn))
                    {
                        if (jsn != null)
                        {
                            foreach (KeyValuePair<string, JSONNode> modNode in jsonFromMod.LoopObject())
                            {
                                PandaLogger.Log("Adding localization for '{0}' from '{1}'.", modNode.Key, Path.Combine(locName, locFilename));
                                AddRecursive(jsn, modNode);
                            }
                        }
                        else
                            PandaLogger.Log("Unable to localize. Localization '{0}' not found and is null.", locName);
                    }
                    else
                        PandaLogger.Log("Localization '{0}' not supported", locName);
                }
                
                PandaLogger.Log("Patched mod localization file '{0}/{1}'", locName, locFilename);
                
            }
            catch (Exception ex)
            {
                PandaLogger.LogError(ex, "Exception while localizing {0}", Path.Combine(locName, locFilename));
            }
        }

        private static void AddRecursive(JSONNode gameJson, KeyValuePair<string, JSONNode> modNode)
        {
            int childCount = 0;

            try
            {
                childCount = modNode.Value.ChildCount;
            }
            catch { }

            if (childCount != 0)
            {
                if (gameJson.HasChild(modNode.Key))
                {
                    foreach (var child in modNode.Value.LoopObject())
                        AddRecursive(gameJson[modNode.Key], child);
                }
                else
                {
                    gameJson[modNode.Key] = modNode.Value;
                }
            }
            else
            {
                gameJson[modNode.Key] = modNode.Value;
            }
        }

        //public static ushort GetNextItemSortIndex()
        //{
        //    return _itemSortIndex++;
        //}
    }
}
