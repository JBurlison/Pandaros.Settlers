using ChatCommands;
using Pipliz.JSON;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Pandaros.Settlers
{
    [ModLoader.ModManager]
    public static class Configuration
    {
        private static string _saveFileName = $"{GameLoader.SAVE_LOC}/{GameLoader.NAMESPACE}.json";
        private static JSONNode _rootSettings = new JSONNode();

        public static GameDifficulty MinDifficulty
        {
            get
            {
                string diffStr = GetorDefault(nameof(MinDifficulty), GameDifficulty.Normal.Name);

                if (GameDifficulty.GameDifficulties.ContainsKey(diffStr))
                    return GameDifficulty.GameDifficulties[diffStr];
                else
                    return GameDifficulty.Normal;
            }
            private set
            {
                SetValue(nameof(MinDifficulty), value);
            }
        }

        public static bool HasSetting(string setting)
        {
            return _rootSettings.HasChild(setting);
        }

        public static GameDifficulty DefaultDifficulty
        {
            get
            {
                string diffStr = GetorDefault(nameof(DefaultDifficulty), GameDifficulty.Medium.Name);

                if (GameDifficulty.GameDifficulties.ContainsKey(diffStr))
                    return GameDifficulty.GameDifficulties[diffStr];
                else
                    return GameDifficulty.Medium;
            }
            private set
            {
                SetValue(nameof(DefaultDifficulty), value);
            }
        }

        public static bool DifficutlyCanBeChanged
        {
            get
            {
                return GetorDefault(nameof(DifficutlyCanBeChanged), true);
            }
            private set
            {
                SetValue(nameof(DifficutlyCanBeChanged), value);
            }
        }

        public static bool OfflineColonies
        {
            get
            {
                return GetorDefault(nameof(OfflineColonies), true);
            }
            private set
            {
                SetValue(nameof(OfflineColonies), value);
            }
        }

        public static bool TeleportPadsRequireMachinists
        {
            get
            {
                return GetorDefault(nameof(TeleportPadsRequireMachinists), false);
            }
            private set
            {
                SetValue(nameof(TeleportPadsRequireMachinists), value);
            }
        }
       

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterSelectedWorld, GameLoader.NAMESPACE + ".Configuration.AfterSelectedWorld"),
            ModLoader.ModCallbackDependsOn(GameLoader.NAMESPACE + ".AfterSelectedWorld")]
        public static void AfterSelectedWorld()
        {
            Reload();
            GetorDefault("BossesCanBeDisabled", true);
            GetorDefault("MonstersCanBeDisabled", true);
            GetorDefault("MaxSettlersToggle", 4);
            GetorDefault("SettlersEnabled", true);
            GetorDefault("ColonistsRecruitment", true);
            GetorDefault("CompoundingFoodRecruitmentCost", 5);
            Save();
        }

        public static void Reload()
        {
            if (File.Exists(_saveFileName) && JSON.Deserialize(_saveFileName, out var config))
            {
                _rootSettings = config;

                if (config.TryGetAs("GameDifficulties", out JSONNode diffs))
                {
                    foreach (var diff in diffs.LoopArray())
                    {
                        var newDiff = new GameDifficulty(diff);
                        GameDifficulty.GameDifficulties[newDiff.Name] = newDiff;
                    }
                }
            }
        }

        public static void Save()
        {
            JSONNode diffs = new JSONNode(NodeType.Array);

            foreach (var diff in GameDifficulty.GameDifficulties.Values)
                diffs.AddToArray(diff.ToJson());

            _rootSettings.SetAs("GameDifficulties", diffs);

            JSON.Serialize(_saveFileName, _rootSettings);
        }

        public static T GetorDefault<T>(string key, T defaultVal)
        {
            if (!_rootSettings.HasChild(key))
                SetValue(key, defaultVal);

            return _rootSettings.GetAs<T>(key);
        }

        public static void SetValue<T>(string key, T val)
        {
            _rootSettings.SetAs(key, val);
            Save();
        }
    }

    public class ConfigurationChatCommand : IChatCommand
    {
        public bool IsCommand(string chat)
        {
            return chat.StartsWith("/settlersconfig", StringComparison.OrdinalIgnoreCase);
        }

        public bool TryDoCommand(Players.Player player, string chat)
        {
            if (Permissions.PermissionsManager.CheckAndWarnPermission(player, new Permissions.PermissionsManager.Permission(GameLoader.NAMESPACE + ".Permissions.Config")))
            {
                string[] array = CommandManager.SplitCommand(chat);

                if (array.Length == 3)
                {
                    if (Configuration.HasSetting(array[1]))
                    {
                        if (int.TryParse(array[2], out var set))
                            Configuration.SetValue(array[1], set);
                        else if (float.TryParse(array[2], out var fset))
                            Configuration.SetValue(array[1], fset);
                        else if (bool.TryParse(array[2], out var bset))
                            Configuration.SetValue(array[1], bset);
                        else
                            Configuration.SetValue(array[1], array[2]);
                    }
                    else
                        PandaChat.Send(player, $"The configuration {array[1]} does not exist.", ChatColor.red);
                }
                else
                    Configuration.Reload();
            }

            return true;
        }
    }
}
