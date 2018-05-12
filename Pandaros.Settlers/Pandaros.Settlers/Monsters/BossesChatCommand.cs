using System;
using ChatCommands;
using Pandaros.Settlers.Entities;

namespace Pandaros.Settlers.Monsters
{
    public class BossesChatCommand : IChatCommand
    {
        public bool IsCommand(string chat)
        {
            return chat.StartsWith("/bosses", StringComparison.OrdinalIgnoreCase);
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
                PandaChat.Send(player, "Settlers! Bosses are {0}.", ChatColor.green,
                               state.BossesEnabled ? "on" : "off");

                return true;
            }

            if (array.Length == 2 && Configuration.GetorDefault("BossesCanBeDisabled", true))
            {
                if (array[1].ToLower().Trim() == "on" || array[1].ToLower().Trim() == "true")
                {
                    state.BossesEnabled = true;
                    PandaChat.Send(player, "Settlers! Mod Bosses are now on.", ChatColor.green);
                }
                else
                {
                    state.BossesEnabled = false;
                    PandaChat.Send(player, "Settlers! Mod Bosses are now off.", ChatColor.green);
                }
            }

            if (!Configuration.GetorDefault("BossesCanBeDisabled", true))
                PandaChat.Send(player, "The server administrator had disabled the changing of bosses.",
                               ChatColor.green);

            return true;
        }
    }
}