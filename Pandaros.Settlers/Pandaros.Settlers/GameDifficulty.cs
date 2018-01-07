using ChatCommands;
using Pandaros.Settlers.AI;
using Pandaros.Settlers.Entities;
using Pandaros.Settlers.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Pandaros.Settlers
{
    [Serializable]
    public class GameDifficulty
    {
        public static Dictionary<string, GameDifficulty> GameDifficulties { get; private set; }

        public static GameDifficulty Normal { get; private set; }
        public static GameDifficulty Easy { get; private set; }
        public static GameDifficulty Medium { get; private set; }
        public static GameDifficulty Hard { get; private set; }

        static GameDifficulty()
        {
            GameDifficulties = new Dictionary<string, GameDifficulty>(StringComparer.OrdinalIgnoreCase);
            Normal = new GameDifficulty("Normal", 0f, 0f, 0f, 0f) { PosionChance = 0f, InfectionChance = 0f, MonsterDiff = .10f };
            Easy = new GameDifficulty("Easy", 1.0f, 1f, 0.10f, 10f) { PosionChance = 10f, InfectionChance = 10f, MonsterDiff = .10f };
            Medium = new GameDifficulty("Medium", 1.25f, 0f, 0.35f, 50f) { PosionChance = 35f, InfectionChance = 40f, MonsterDiff = .20f };
            Hard = new GameDifficulty("Hard", 1.50f, -0.1f, 0.60f, 70f) { PosionChance = 60f, InfectionChance = 60f, MonsterDiff = .30f };
            new GameDifficulty("Insane", 2f, -0.2f, .80f, 80f) { PosionChance = 80f, InfectionChance = 80f, MonsterDiff = .50f };
        }

        public string Name { get; set; }

        public float FoodMultiplier { get; set; }

        public float MachineThreashHold { get; set; } = 0;

        public float MonsterDamageReduction { get; set; }
        public float MonsterDamage { get; set; }

        public float PosionChance { get; set; }

        public float InfectionChance { get; set; }

        public float MonsterDiff { get; set; }

        public GameDifficulty() { }

        public GameDifficulty(string name, float foodMultiplier, float machineThreashHold, float monsterDr, float monsterDamage)
        {
            Name = name;
            FoodMultiplier = foodMultiplier;
            GameDifficulties[name] = this;
            MachineThreashHold = machineThreashHold;
            MonsterDamageReduction = monsterDr;
            MonsterDamage = monsterDamage;
        }

        public override string ToString()
        {
            return Name;
        }
    }

    public class GameDifficultyChatCommand : IChatCommand
    {
        public bool IsCommand(string chat)
        {
            return chat.StartsWith("/difficulty", StringComparison.OrdinalIgnoreCase) || chat.StartsWith("/dif", StringComparison.OrdinalIgnoreCase);
        }

        public bool TryDoCommand(Players.Player player, string chat)
        {
            if (player == null || player.ID == NetworkID.Server)
                return true;

            string[] array = CommandManager.SplitCommand(chat);
            Colony colony = Colony.Get(player);
            PlayerState state = PlayerState.GetPlayerState(player);

            if (array.Length == 1)
            {
                PandaChat.Send(player, "Settlers! Mod difficulty is set to {0}.", ChatColor.green, state.Difficulty.Name);
                return true;
            }

            if (array.Length < 2)
            {
                UnknownCommand(player, chat);
                return true;
            }

            if (!GameDifficulty.GameDifficulties.ContainsKey(array[1].Trim()))
            {
                UnknownCommand(player, array[1].Trim());
                return true;
            }

            state.Difficulty = GameDifficulty.GameDifficulties[array[1].Trim()];

            PandaChat.Send(player, "Settlers! Mod difficulty set to {0}.", ChatColor.green, state.Difficulty.Name);
            Managers.SettlerManager.UpdateFoodUse(player);

            return true;
        }

        private static void UnknownCommand(Players.Player player, string command)
        {
            PandaChat.Send(player, "Unknown command {0}", ChatColor.white, command);
            PossibleCommands(player, ChatColor.white);
        }

        public static void PossibleCommands(Players.Player player, ChatColor color)
        {
            PandaChat.Send(player, "Current Difficulty: " + PlayerState.GetPlayerState(player).Difficulty.Name, color);
            PandaChat.Send(player, "Possible commands:", color);

            string diffs = string.Empty;

            foreach (var diff in GameDifficulty.GameDifficulties)
                diffs += diff.Key + " | ";

            PandaChat.Send(player, "/difficulty " + diffs.Substring(0, diffs.Length - 2), color);
        }
    }
}