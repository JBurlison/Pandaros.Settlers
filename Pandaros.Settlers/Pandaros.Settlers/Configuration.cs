using Chatting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Pipliz.JSON;
using System;
using System.Collections.Generic;
using System.IO;

namespace Pandaros.Settlers
{
    [ModLoader.ModManager]
    public static class SettlersConfiguration
    {
        private static CSModConfiguration _configuration = new CSModConfiguration(GameLoader.NAMESPACE);

        public static GameDifficulty MinDifficulty
        {
            get
            {
                var diffStr = GetorDefault(nameof(MinDifficulty), GameDifficulty.Normal.Name);

                if (GameDifficulty.GameDifficulties.ContainsKey(diffStr))
                    return GameDifficulty.GameDifficulties[diffStr];

                return GameDifficulty.Normal;
            }
            private set => SetValue(nameof(MinDifficulty), value);
        }

        public static GameDifficulty DefaultDifficulty
        {
            get
            {
                var diffStr = GetorDefault(nameof(DefaultDifficulty), GameDifficulty.Medium.Name);

                if (GameDifficulty.GameDifficulties.ContainsKey(diffStr))
                    return GameDifficulty.GameDifficulties[diffStr];

                return GameDifficulty.Medium;
            }
            private set => SetValue(nameof(DefaultDifficulty), value);
        }

        public static bool DifficutlyCanBeChanged
        {
            get => GetorDefault(nameof(DifficutlyCanBeChanged), true);
            private set => SetValue(nameof(DifficutlyCanBeChanged), value);
        }

        public static bool OfflineColonies
        {
            get => GetorDefault(nameof(OfflineColonies), true);
            private set => SetValue(nameof(OfflineColonies), value);
        }

        public static bool TeleportPadsRequireMachinists
        {
            get => GetorDefault(nameof(TeleportPadsRequireMachinists), false);
            private set => SetValue(nameof(TeleportPadsRequireMachinists), value);
        }

        public static bool HasSetting(string setting)
        {
            return _configuration.HasSetting(setting);
        }


        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterSelectedWorld, GameLoader.NAMESPACE + ".Configuration.AfterSelectedWorld")]
        [ModLoader.ModCallbackDependsOn(GameLoader.NAMESPACE + ".AfterSelectedWorld")]
        public static void AfterSelectedWorld()
        {
            try
            {
                Reload();
                GetorDefault("BossesCanBeDisabled", true);
                GetorDefault("MaxSettlersToggle", 4);
                GetorDefault("SettlersEnabled", true);
                GetorDefault("ColonistsRecruitment", true);
                GetorDefault("AllowPlayerToResetThemself", true);
                GetorDefault("CompoundingFoodRecruitmentCost", 5);
                Save();
            }
            catch (Exception ex)
            {
                PandaLogger.LogError(ex);
            }
        }

        public static void Reload()
        {
            if (File.Exists(_configuration.SaveFile) && JSON.Deserialize(_configuration.SaveFile, out var config))
            {
                _configuration.SettingsRoot = config;

                if (config.TryGetAs("GameDifficulties", out JSONNode diffs) && diffs.NodeType == NodeType.Array)
                    foreach (var diff in diffs.LoopArray())
                    {
                        var newDiff = diff.JsonDeerialize<GameDifficulty>();
                        GameDifficulty.GameDifficulties[newDiff.Name] = newDiff;
                    }
            }
        }

        public static void Save()
        {
            var diffs = new JSONNode(NodeType.Array);

            foreach (var diff in GameDifficulty.GameDifficulties.Values)
                diffs.AddToArray(diff.ToJson());

            _configuration.SettingsRoot.SetAs("GameDifficulties", diffs);

            _configuration.Save();
        }

        public static T GetorDefault<T>(string key, T defaultVal)
        {
            return _configuration.GetorDefault(key, defaultVal);
        }

        public static void SetValue<T>(string key, T val)
        {
            _configuration.SetValue(key, val);
        }
    }


    public class CSModConfiguration
    {
        public string SaveFile { get; }
        public JSONNode SettingsRoot { get; set; }

        public CSModConfiguration(string configurationFileName)
        {
            SaveFile = $"{GameLoader.SAVE_LOC}/{configurationFileName}.json";

            if (File.Exists(SaveFile))
                SettingsRoot = JSON.Deserialize(SaveFile);
            else
                SettingsRoot = new JSONNode();
        }

        public void Reload()
        {
            if (File.Exists(SaveFile) && JSON.Deserialize(SaveFile, out var config))
                SettingsRoot = config;
        }

        public void Save()
        {
            JSON.Serialize(SaveFile, SettingsRoot);
        }

        public bool HasSetting(string setting)
        {
            return SettingsRoot.HasChild(setting);
        }

        public T GetorDefault<T>(string key, T defaultVal)
        {
            if (!SettingsRoot.HasChild(key))
                SetValue(key, defaultVal);

            return SettingsRoot.GetAs<T>(key);
        }

        public void SetValue<T>(string key, T val)
        {
            SettingsRoot.SetAs<T>(key, val);
            Save();
        }
    }

    public class ConfigurationChatCommand : IChatCommand
    {
        public bool TryDoCommand(Players.Player player, string chat, List<string> split)
        {
            if (!chat.StartsWith("/settlersconfig", StringComparison.OrdinalIgnoreCase))
                return false;


            if (PermissionsManager.CheckAndWarnPermission(player, new PermissionsManager.Permission(GameLoader.NAMESPACE + ".Permissions.Config")))
            {
                var array = new List<string>();
                CommandManager.SplitCommand(chat, array);

                if (array.Count == 3)
                {
                    if (SettlersConfiguration.HasSetting(array[1]))
                    {
                        if (int.TryParse(array[2], out var set))
                            SettlersConfiguration.SetValue(array[1], set);
                        else if (float.TryParse(array[2], out var fset))
                            SettlersConfiguration.SetValue(array[1], fset);
                        else if (bool.TryParse(array[2], out var bset))
                            SettlersConfiguration.SetValue(array[1], bset);
                        else
                            SettlersConfiguration.SetValue(array[1], array[2]);
                    }
                    else
                    {
                        PandaChat.Send(player, $"The configuration {array[1]} does not exist.", ChatColor.red);
                    }
                }
                else
                {
                    SettlersConfiguration.Reload();
                }
            }

            return true;
        }
    }
}