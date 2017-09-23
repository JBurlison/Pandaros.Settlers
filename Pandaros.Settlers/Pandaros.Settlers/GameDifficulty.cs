using ChatCommands;
using Pandaros.Settlers.Entities;
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

        public static GameDifficulty Easy { get; private set; }
        public static GameDifficulty Medium { get; private set; }
        public static GameDifficulty Hard { get; private set; }

        static GameDifficulty()
        {
            GameDifficulties = new Dictionary<string, GameDifficulty>(StringComparer.OrdinalIgnoreCase);
            Easy = new GameDifficulty("Easy", 0.75f, 0.5f);
            Medium = new GameDifficulty("Medium", 1f, 0f);
            Hard = new GameDifficulty("Hard", 1.25f, -0.2f);
        }

        [XmlElement]
        public string Name { get; set; }

        [XmlElement]
        public float FoodMultiplier { get; set; }

        [XmlElement]
        public float AdditionalChance { get; set; }

        public GameDifficulty() { }

        public GameDifficulty(string name, float foodMultiplier, float chance)
        {
            Name = name;
            FoodMultiplier = foodMultiplier;
            AdditionalChance = chance;
            GameDifficulties[name] = this;
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
            return chat.StartsWith("/difficulty");
        }

        public bool TryDoCommand(Players.Player player, string chat)
        {
            if (player == null || player.ID == NetworkID.Server)
                return true;
            
            string[] array = CommandManager.SplitCommand(chat);

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

            Colony colony = Colony.Get(player);
            PlayerState state = SettlerManager.GetPlayerState(player, colony);

            state.Difficulty = GameDifficulty.GameDifficulties[array[1].Trim()];

            PandaChat.Send(player, "Settelers! Mod difficulty to {0}.", ChatColor.green, state.Difficulty.Name);
            SettlerManager.Update(Colony.Get(player));

            return true;
        }

        private static void UnknownCommand(Players.Player player, string command)
        {
            PandaChat.Send(player, "Unknown command {0}", ChatColor.white, command);
            PossibleCommands(player, ChatColor.white);
        }

        public static void PossibleCommands(Players.Player player, ChatColor color)
        {
            PandaChat.Send(player, "Possible commands:", color);

            foreach (var diff in GameDifficulty.GameDifficulties)
                PandaChat.Send(player, "/difficulty " + diff.Key, color);
        }
    }

}
