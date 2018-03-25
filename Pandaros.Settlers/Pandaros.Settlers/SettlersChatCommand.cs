using ChatCommands;
using Pandaros.Settlers.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pandaros.Settlers
{
    public class SettlersChatCommand : IChatCommand
    {
        public bool IsCommand(string chat)
        {
            return chat.StartsWith("/settlers", StringComparison.OrdinalIgnoreCase);
        }

        public bool TryDoCommand(Players.Player player, string chat)
        {
            if (player == null || player.ID == NetworkID.Server)
                return true;

            string[] array = CommandManager.SplitCommand(chat);
            Colony colony = Colony.Get(player);
            PlayerState state = PlayerState.GetPlayerState(player);
            int maxToggleTimes = Configuration.GetorDefault("MaxSettlersToggle", 3);

            if (array.Length == 1)
            {
                PandaChat.Send(player, "Settlers! Settlers are {0}. You have toggled this {1} out of {2} times.", ChatColor.green, state.MonstersEnabled ? "on" : "off", state.SettlersToggledTimes.ToString(), maxToggleTimes.ToString());
                return true;
            }

            if (array.Length == 2 && state.SettlersToggledTimes <= maxToggleTimes)
            {
                if (array[1].ToLower().Trim() == "on" || array[1].ToLower().Trim() == "true")
                {
                    state.MonstersEnabled = true;
                    PandaChat.Send(player, $"Settlers! Mod Settlers are now on. You have toggled this {state.SettlersToggledTimes} out of {maxToggleTimes} times.", ChatColor.green);
                }
                else
                {
                    state.MonstersEnabled = false;
                    Server.Monsters.MonsterTracker.KillAllZombies(player);
                    PandaChat.Send(player, $"Settlers! Mod Settlers are now off. You have toggled this {state.SettlersToggledTimes} out of {maxToggleTimes} times.", ChatColor.green);
                }
            }

            if (state.SettlersToggledTimes >= maxToggleTimes)
                PandaChat.Send(player, $"To limit abuse of the /settlers command you can no longer toggle settlers on or off. You have used your alloted {maxToggleTimes} times.", ChatColor.red);

            if (maxToggleTimes == 0 && !Configuration.GetorDefault("SettlersEnabled", true))
                PandaChat.Send(player, "The server administrator had disabled the changing of Settlers.", ChatColor.red);

            return true;
        }
    }
}
