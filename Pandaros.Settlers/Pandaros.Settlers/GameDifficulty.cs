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
            Normal = new GameDifficulty("Normal", 0f, 0f, 0f);
            Easy = new GameDifficulty("Easy", 1.0f, 1f, 0.10f);
            Medium = new GameDifficulty("Medium", 1.25f, 0f, 0.25f);
            Hard = new GameDifficulty("Hard", 1.50f, -0.2f, 0.50f);
            new GameDifficulty("Insane", 2f, -0.3f, .75f);
        }

        public string Name { get; set; }

        public float FoodMultiplier { get; set; }

        public float MachineThreashHold { get; set; } = 0;

        public float MonsterDamageReduction { get; set; }

        public GameDifficulty() { }

        public GameDifficulty(string name, float foodMultiplier, float machineThreashHold, float monsterDr)
        {
            Name = name;
            FoodMultiplier = foodMultiplier;
            GameDifficulties[name] = this;
            MachineThreashHold = machineThreashHold;
            MonsterDamageReduction = monsterDr;
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