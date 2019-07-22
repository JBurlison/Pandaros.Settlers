using Chatting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Pandaros.API;
using Pandaros.API.Entities;
using Pandaros.API.localization;
using Pipliz.JSON;
using System;
using System.Collections.Generic;
using System.IO;

namespace Pandaros.Settlers
{
    [ModLoader.ModManager]
    public static class SettlersConfiguration
    {
        public static CSModConfiguration Configuration { get; set; } = new CSModConfiguration(GameLoader.NAMESPACE);

        public static bool TeleportPadsRequireMachinists
        {
            get => GetorDefault(nameof(TeleportPadsRequireMachinists), false);
            private set => SetValue(nameof(TeleportPadsRequireMachinists), value);
        }

        public static bool HasSetting(string setting)
        {
            return Configuration.HasSetting(setting);
        }


        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterSelectedWorld, GameLoader.NAMESPACE + ".Configuration.AfterSelectedWorld")]
        [ModLoader.ModCallbackDependsOn(GameLoader.NAMESPACE + ".AfterSelectedWorld")]
        public static void AfterSelectedWorld()
        {
            try
            {
                Configuration.Reload();
                GetorDefault("BossesCanBeDisabled", true);
                GetorDefault("MaxSettlersToggle", 4);
                GetorDefault("SettlersEnabled", true);
                GetorDefault("ColonistsRecruitment", true);
                GetorDefault("AllowPlayerToResetThemself", true);
                GetorDefault("CompoundingFoodRecruitmentCost", 5);

                GameDifficulty.Easy.GetorDefault("ZombieQueenTargetTeleportHp", 100);
                GameDifficulty.Easy.GetorDefault("BossHPPerColonist", 50);
                GameDifficulty.Easy.GetorDefault("ZombieQueenTargetTeleportCooldownSeconds", 5);
                GameDifficulty.Easy.GetorDefault("AdditionalChance ", .4);
                GameDifficulty.Easy.GetorDefault("UnhappinessPerColonistDeath", 1);
                GameDifficulty.Easy.GetorDefault("UnhappyGuardsMultiplyRate", .5);
                GameDifficulty.Easy.GetorDefault("MonsterHPPerColonist", .2);
                GameDifficulty.Easy.GetorDefault("UnhappyColonistsBought", -1);
                GameDifficulty.Easy.GetorDefault("FoodMultiplier", 1);

                GameDifficulty.Medium.GetorDefault("ZombieQueenTargetTeleportHp", 300);
                GameDifficulty.Medium.GetorDefault("BossHPPerColonist", 70);
                GameDifficulty.Medium.GetorDefault("ZombieQueenTargetTeleportCooldownSeconds", 4);
                GameDifficulty.Medium.GetorDefault("AdditionalChance ", 0);
                GameDifficulty.Medium.GetorDefault("UnhappinessPerColonistDeath", 2);
                GameDifficulty.Medium.GetorDefault("UnhappyGuardsMultiplyRate", 1);
                GameDifficulty.Medium.GetorDefault("MonsterHPPerColonist", .5);
                GameDifficulty.Medium.GetorDefault("UnhappyColonistsBought", -2);
                GameDifficulty.Medium.GetorDefault("FoodMultiplier", 1.25);

                GameDifficulty.Hard.GetorDefault("ZombieQueenTargetTeleportHp", 500);
                GameDifficulty.Hard.GetorDefault("BossHPPerColonist", 80);
                GameDifficulty.Hard.GetorDefault("ZombieQueenTargetTeleportCooldownSeconds", 3);
                GameDifficulty.Hard.GetorDefault("AdditionalChance ", -.2);
                GameDifficulty.Hard.GetorDefault("UnhappinessPerColonistDeath", 3);
                GameDifficulty.Hard.GetorDefault("UnhappyGuardsMultiplyRate", 1.5);
                GameDifficulty.Hard.GetorDefault("MonsterHPPerColonist", 1);
                GameDifficulty.Hard.GetorDefault("UnhappyColonistsBought", -3);
                GameDifficulty.Hard.GetorDefault("FoodMultiplier", 1.5);

                GameDifficulty.Insane.GetorDefault("ZombieQueenTargetTeleportHp", 500);
                GameDifficulty.Insane.GetorDefault("BossHPPerColonist", 100);
                GameDifficulty.Insane.GetorDefault("ZombieQueenTargetTeleportCooldownSeconds", 3);
                GameDifficulty.Insane.GetorDefault("AdditionalChance ", -.4);
                GameDifficulty.Insane.GetorDefault("UnhappinessPerColonistDeath", 4);
                GameDifficulty.Insane.GetorDefault("UnhappyGuardsMultiplyRate", 2);
                GameDifficulty.Insane.GetorDefault("MonsterHPPerColonist", 2);
                GameDifficulty.Insane.GetorDefault("UnhappyColonistsBought", -5);
                GameDifficulty.Insane.GetorDefault("FoodMultiplier", 2);

                Configuration.Save();
            }
            catch (Exception ex)
            {
                SettlersLogger.LogError(ex);
            }
        }

        public static T GetorDefault<T>(string key, T defaultVal)
        {
            return Configuration.GetorDefault(key, defaultVal);
        }

        public static void SetValue<T>(string key, T val)
        {
            Configuration.SetValue(key, val);
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterWorldLoad, GameLoader.NAMESPACE + ".GameDifficulty.AfterWorldLoad")]
        public static void AfterWorldLoad()
        {
            foreach (var colony in ServerManager.ColonyTracker.ColoniesByID.Values)
            {
                var cs = ColonyState.GetColonyState(colony);

                if (cs != null && cs.Difficulty.Rank < APIConfiguration.MinDifficulty.Rank)
                    cs.Difficulty = APIConfiguration.MinDifficulty;
            }
        }
    }


    public class ConfigurationChatCommand : IChatCommand
    {

        private static LocalizationHelper _localizationHelper = new LocalizationHelper(GameLoader.NAMESPACE, "Configuration");

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
                        PandaChat.Send(player, _localizationHelper, "UnknownConfiguration", ChatColor.red, array[1]);
                    }
                }
                else
                {
                    SettlersConfiguration.Configuration.Reload();
                }
            }

            return true;
        }
    }
}