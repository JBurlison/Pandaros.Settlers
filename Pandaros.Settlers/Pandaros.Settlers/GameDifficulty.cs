using Chatting;
using Pandaros.Settlers.ColonyManagement;
using Pandaros.Settlers.Entities;
using Pipliz;
using Pipliz.JSON;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Pandaros.Settlers
{
    [ModLoader.ModManager]
    public class GameDifficulty
    {
        static GameDifficulty()
        {
            GameDifficulties = new Dictionary<string, GameDifficulty>(StringComparer.OrdinalIgnoreCase);

            Normal = new GameDifficulty("Normal", 0f, 0f, 0f, 0f)
            {
                Rank                                     = 0,
                ZombieQueenTargetTeleportHp              = 100,
                BossHPPerColonist                        = 50,
                ZombieQueenTargetTeleportCooldownSeconds = 30,
                AdditionalChance                         = 0f,
                TimeUnhappyAfterColonistDeathSeconds     = 0f,
                UnhappinessPerColonistDeath              = 0f,
                UnhappyGuardsMultiplyRate                = 0f,
                MonsterHPPerColonist                     = 0f
            };

            Easy = new GameDifficulty("Easy", 1.0f, 1f, 0.10f, 10f)
            {
                Rank                                     = 1,
                ZombieQueenTargetTeleportHp              = 100,
                BossHPPerColonist                        = 50,
                ZombieQueenTargetTeleportCooldownSeconds = 3,
                AdditionalChance                         = 0.4f,
                TimeUnhappyAfterColonistDeathSeconds     = 300,
                UnhappinessPerColonistDeath              = 1,
                UnhappyGuardsMultiplyRate                = 0.5f,
                MonsterHPPerColonist                     = .5f
            };

            Medium = new GameDifficulty("Medium", 1.25f, 0f, 0.35f, 50f)
            {
                Rank                                     = 2,
                ZombieQueenTargetTeleportHp              = 300,
                BossHPPerColonist                        = 70,
                ZombieQueenTargetTeleportCooldownSeconds = 15,
                AdditionalChance                         = 0f,
                TimeUnhappyAfterColonistDeathSeconds     = 900,
                UnhappinessPerColonistDeath              = 2,
                UnhappyGuardsMultiplyRate                = 1,
                MonsterHPPerColonist                     = 1f
            };

            Hard = new GameDifficulty("Hard", 1.50f, -0.1f, 0.60f, 70f)
            {
                Rank                                     = 3,
                ZombieQueenTargetTeleportHp              = 500,
                BossHPPerColonist                        = 80,
                ZombieQueenTargetTeleportCooldownSeconds = 10,
                AdditionalChance                         = -0.2f,
                TimeUnhappyAfterColonistDeathSeconds     = 900,
                UnhappinessPerColonistDeath              = 3,
                UnhappyGuardsMultiplyRate                = 1.5f,
                MonsterHPPerColonist                     = 3f
            };

            new GameDifficulty("Insane", 2f, -0.2f, .80f, 80f)
            {
                Rank                                     = 4,
                ZombieQueenTargetTeleportHp              = 500,
                BossHPPerColonist                        = 100,
                ZombieQueenTargetTeleportCooldownSeconds = 5,
                AdditionalChance                         = -0.4f,
                TimeUnhappyAfterColonistDeathSeconds     = 1800,
                UnhappinessPerColonistDeath              = 4,
                UnhappyGuardsMultiplyRate                = 2,
                MonsterHPPerColonist                     = 10f
            };
        }

        public GameDifficulty()
        {
        }

        public GameDifficulty(string name, float foodMultiplier, float roamingJobActionEnergy, float monsterDr,
                              float  monsterDamage)
        {
            Name                   = name;
            FoodMultiplier         = foodMultiplier;
            GameDifficulties[name] = this;
            RoamingJobActionEnergy = roamingJobActionEnergy;
            MonsterDamageReduction = monsterDr;
            MonsterDamage          = monsterDamage;
        }

        public static Dictionary<string, GameDifficulty> GameDifficulties { get; }

        public static GameDifficulty Normal { get; }
        public static GameDifficulty Easy { get; }
        public static GameDifficulty Medium { get; }
        public static GameDifficulty Hard { get; }

        public string Name { get; set; }
        public int Rank { get; set; }

        public float FoodMultiplier { get; set; }

        public float RoamingJobActionEnergy { get; set; }

        public float MonsterDamageReduction { get; set; }
        public float AdditionalChance { get; set; }
        public float MonsterDamage { get; set; }
        public float ZombieQueenTargetTeleportHp { get; set; } = 250;
        public float ZombieQueenTargetTeleportCooldownSeconds { get; set; } = 45;
        public float BossHPPerColonist { get; set; } = 30;
        public float MonsterHPPerColonist { get; set; } = 1;
        public double UnhappinessPerColonistDeath { get; set; } = 2;
        public double TimeUnhappyAfterColonistDeathSeconds { get; set; } = 900;
        public float UnhappyGuardsMultiplyRate = 1;

        public void Print(Players.Player player)
        {
            PandaChat.Send(player, $"FoodMultiplier: {FoodMultiplier}", ChatColor.green);
            PandaChat.Send(player, $"MonsterDamage: {MonsterDamage}", ChatColor.green);
            PandaChat.Send(player, $"MonsterDamageReduction: {MonsterDamageReduction}", ChatColor.green);
        }

        public JSONNode ToJson()
        {
            return this.JsonSerialize();
        }

        public override string ToString()
        {
            return Name;
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterWorldLoad, GameLoader.NAMESPACE + ".GameDifficulty.AfterWorldLoad")]
        public static void AfterWorldLoad()
        {
            foreach (var colony in ServerManager.ColonyTracker.ColoniesByID.Values)
            {
                var cs = ColonyState.GetColonyState(colony);

                if (cs != null && cs.Difficulty.Rank < Configuration.MinDifficulty.Rank)
                    cs.Difficulty = Configuration.MinDifficulty;
            }
        }
    }

    [ModLoader.ModManager]
    public class GameDifficultyChatCommand : IChatCommand
    {
        private static string _Difficulty = GameLoader.NAMESPACE + ".Difficulty";

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnConstructWorldSettingsUI, GameLoader.NAMESPACE + "Difficulty.AddSetting")]
        public static void AddSetting(Players.Player player, NetworkUI.NetworkMenu menu)
        {
            if (player.ActiveColony != null)
            {
                menu.Items.Add(new NetworkUI.Items.DropDown("Settlers Difficulty", _Difficulty, GameDifficulty.GameDifficulties.Keys.ToList()));
                var ps = ColonyState.GetColonyState(player.ActiveColony);
                menu.LocalStorage.SetAs(_Difficulty, ps.Difficulty.Rank);
            }
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnPlayerChangedNetworkUIStorage, GameLoader.NAMESPACE + "Difficulty.ChangedSetting")]
        public static void ChangedSetting(ValueTuple<Players.Player, JSONNode, string> data)
        {
            if (data.Item1.ActiveColony != null)
                switch (data.Item3)
                {
                    case "server_popup":
                        var ps = ColonyState.GetColonyState(data.Item1.ActiveColony);

                        if (ps != null && data.Item2.GetAsOrDefault(_Difficulty, ps.Difficulty.Rank) != ps.Difficulty.Rank)
                        {
                            var difficulty = GameDifficulty.GameDifficulties.FirstOrDefault(kvp => kvp.Value.Rank == data.Item2.GetAsOrDefault(_Difficulty, ps.Difficulty.Rank)).Key;

                            if (difficulty != null)
                                ChangeDifficulty(data.Item1, ps, difficulty);
                        }

                        break;
                }
        }

        public bool TryDoCommand(Players.Player player, string chat, List<string> split)
        {
            if (!chat.StartsWith("/difficulty", StringComparison.OrdinalIgnoreCase) ||
                   !chat.StartsWith("/dif", StringComparison.OrdinalIgnoreCase))
                return false;

            if (player == null || player.ID == NetworkID.Server || player.ActiveColony == null)
                return true;

            var array = new List<string>();
            CommandManager.SplitCommand(chat, array);

            var state = ColonyState.GetColonyState(player.ActiveColony);

            if (array.Count == 1)
            {
                PandaChat.Send(player, "Settlers! Mod difficulty set to {0}.", ChatColor.green, state.Difficulty.Name);
                return true;
            }

            if (array.Count < 2)
            {
                UnknownCommand(player, chat);
                return true;
            }

            if (array.Count == 2)
            {
                var difficulty = array[1].Trim();

                return ChangeDifficulty(player, state, difficulty);
            }

            if (!Configuration.DifficutlyCanBeChanged)
                PandaChat.Send(player, "The server administrator had disabled the changing of game difficulty.",
                               ChatColor.green);

            return true;
        }

        public static bool ChangeDifficulty(Players.Player player, ColonyState state, string difficulty)
        {
            if (Configuration.DifficutlyCanBeChanged)
            {
                if (!GameDifficulty.GameDifficulties.ContainsKey(difficulty))
                {
                    UnknownCommand(player, difficulty);
                    return true;
                }

                var newDiff = GameDifficulty.GameDifficulties[difficulty];

                if (newDiff.Rank >= Configuration.MinDifficulty.Rank)
                {
                    state.Difficulty = newDiff;
                    SettlerManager.UpdateFoodUse(state);
                    state.Difficulty.Print(player);

                    PandaChat.Send(player, "Settlers! Mod difficulty set to {0}.", ChatColor.green,
                                   state.Difficulty.Name);

                    NetworkUI.NetworkMenuManager.SendColonySettingsUI(player);
                    return true;
                }

                NetworkUI.NetworkMenuManager.SendColonySettingsUI(player);
                PandaChat.Send(player, "The server administrator had disabled setting your difficulty below {0}.",
                               ChatColor.green, Configuration.MinDifficulty.Name);
            }

            return true;
        }

        private static void UnknownCommand(Players.Player player, string command)
        {
            PandaChat.Send(player, "Unknown command {0}", ChatColor.white, command);
            PossibleCommands(player, ChatColor.white);
        }

        public static void PossibleCommands(Players.Player player, ChatColor color)
        {
            if (player.ActiveColony != null)
            {
                PandaChat.Send(player, "Current Difficulty: " + ColonyState.GetColonyState(player.ActiveColony).Difficulty.Name, color);
                PandaChat.Send(player, "Possible commands:", color);

                var diffs = string.Empty;

                foreach (var diff in GameDifficulty.GameDifficulties)
                    diffs += diff.Key + " | ";

                PandaChat.Send(player, "/difficulty " + diffs.Substring(0, diffs.Length - 2), color);
            }
        }
    }
}