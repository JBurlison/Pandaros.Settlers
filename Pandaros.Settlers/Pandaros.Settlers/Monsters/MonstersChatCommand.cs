using System;
using ChatCommands;
using Pandaros.Settlers.Entities;
using Server.Monsters;

namespace Pandaros.Settlers.Monsters
{
    public class MonstersChatCommand : IChatCommand
    {
        public bool IsCommand(string chat)
        {
            return chat.StartsWith("/monsters", StringComparison.OrdinalIgnoreCase);
        }

        public bool TryDoCommand(Players.Player player, string chat)
        {
            if (player == null || player.ID == NetworkID.Server)
                return true;

            var array  = CommandManager.SplitCommand(chat);
            var colony = Colony.Get(player);
            var state  = PlayerState.GetPlayerState(player);

            if (array.Length == 1)
            {
                PandaChat.Send(player, "Settlers! Monsters are {0}.", ChatColor.green,
                               state.MonstersEnabled ? "on" : "off");

                return true;
            }

            if (array.Length == 2 && Configuration.GetorDefault("MonstersCanBeDisabled", true))
            {
                if (array[1].ToLower().Trim() == "on" || array[1].ToLower().Trim() == "true")
                {
                    state.MonstersEnabled = true;
                    PandaChat.Send(player, "Settlers! Mod Monsters are now on.", ChatColor.green);
                }
                else
                {
                    state.MonstersEnabled = false;
                    MonsterTracker.KillAllZombies(player);
                    PandaChat.Send(player, "Settlers! Mod Monsters are now off.", ChatColor.green);
                }
            }

            if (!Configuration.GetorDefault("MonstersCanBeDisabled", true))
                PandaChat.Send(player, "The server administrator had disabled the changing of Monsters.",
                               ChatColor.green);

            return true;
        }
    }
}