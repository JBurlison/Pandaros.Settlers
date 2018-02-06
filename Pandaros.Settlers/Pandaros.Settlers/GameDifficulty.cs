using ChatCommands;
using Pandaros.Settlers.AI;
using Pandaros.Settlers.Entities;
using Pandaros.Settlers.Managers;
using Pipliz.JSON;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Pandaros.Settlers
{
    [ModLoader.ModManager]
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
            Normal = new GameDifficulty("Normal", 0f, 0f, 0f, 0f) { Rank = 0 };
            Easy = new GameDifficulty("Easy", 1.0f, 1f, 0.10f, 10f) { Rank = 1 };
            Medium = new GameDifficulty("Medium", 1.25f, 0f, 0.35f, 50f) { Rank = 2 };
            Hard = new GameDifficulty("Hard", 1.50f, -0.1f, 0.60f, 70f) { Rank = 3 };
            new GameDifficulty("Insane", 2f, -0.2f, .80f, 80f) { Rank = 4 };
        }

        public string Name { get; set; }
        public int Rank { get; set; }

        public float FoodMultiplier { get; set; }

        public float MachineThreashHold { get; set; } = 0;

        public float MonsterDamageReduction { get; set; }

        public float MonsterDamage { get; set; }

        public GameDifficulty() { }

        public GameDifficulty(JSONNode node)
        {
            if (node.TryGetAs(nameof(Name), out string name))
            {
                Name = name;

                if (node.TryGetAs(nameof(Rank), out int rank))
                    Rank = rank;

                if (node.TryGetAs(nameof(FoodMultiplier), out int foodMultiplier))
                    FoodMultiplier = foodMultiplier;

                if (node.TryGetAs(nameof(MachineThreashHold), out int machineThreashHold))
                    MachineThreashHold = machineThreashHold;

                if (node.TryGetAs(nameof(MonsterDamageReduction), out int monsterDamageReduction))
                    MonsterDamageReduction = monsterDamageReduction;

                GameDifficulties[Name] = this;
            }
        }

        public GameDifficulty(string name, float foodMultiplier, float machineThreashHold, float monsterDr, float monsterDamage)
        {
            Name = name;
            FoodMultiplier = foodMultiplier;
            GameDifficulties[name] = this;
            MachineThreashHold = machineThreashHold;
            MonsterDamageReduction = monsterDr;
            MonsterDamage = monsterDamage;
        }

        public JSONNode ToJson()
        {
            JSONNode node = new JSONNode()
                .SetAs(nameof(Name), Name)
                .SetAs(nameof(Rank), Rank)
                .SetAs(nameof(FoodMultiplier), FoodMultiplier)
                .SetAs(nameof(MachineThreashHold), MachineThreashHold)
                .SetAs(nameof(MonsterDamageReduction), MonsterDamageReduction)
                .SetAs(nameof(MonsterDamage), MonsterDamage);

            return node;
        }

        public override string ToString()
        {
            return Name;
        }
        
        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterWorldLoad, GameLoader.NAMESPACE + ".GameDifficulty.AfterWorldLoad")]
        public static void AfterWorldLoad()
        {
            foreach (var player in Players.PlayerDatabase.ValuesAsList)
            {
                PlayerState ps = PlayerState.GetPlayerState(player);

                if (ps != null && ps.Difficulty.Rank < Configuration.MinDifficulty.Rank)
                    ps.Difficulty = Configuration.MinDifficulty;
            }
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
                PandaChat.Send(player, "Settlers! Mod difficulty set to {0}.", ChatColor.green, state.Difficulty.Name);
                return true;
            }

            if (array.Length < 2)
            {
                UnknownCommand(player, chat);
                return true;
            }

            if (array.Length == 2 && Configuration.DifficutlyCanBeChanged)
            {
                if (!GameDifficulty.GameDifficulties.ContainsKey(array[1].Trim()))
                {
                    UnknownCommand(player, array[1].Trim());
                    return true;
                }

                var newDiff = GameDifficulty.GameDifficulties[array[1].Trim()];

                if (newDiff.Rank >= Configuration.MinDifficulty.Rank)
                {
                    state.Difficulty = newDiff;
                    Managers.SettlerManager.UpdateFoodUse(player);
                }
                else
                    PandaChat.Send(player, "The server administrator had disabled setting your difficulty below {0}.", ChatColor.green, Configuration.MinDifficulty.Name);

            }

            if (!Configuration.DifficutlyCanBeChanged)
                PandaChat.Send(player, "The server administrator had disabled the changing of game difficulty.", ChatColor.green);

            PandaChat.Send(player, "Settlers! Mod difficulty set to {0}.", ChatColor.green, state.Difficulty.Name);

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