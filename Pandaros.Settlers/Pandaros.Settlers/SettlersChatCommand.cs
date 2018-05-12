using System;
using ChatCommands;
using Pandaros.Settlers.Entities;

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

            var array          = CommandManager.SplitCommand(chat);
            var colony         = Colony.Get(player);
            var state          = PlayerState.GetPlayerState(player);
            var maxToggleTimes = Configuration.GetorDefault("MaxSettlersToggle", 4);

            if (maxToggleTimes == 0 && !Configuration.GetorDefault("SettlersEnabled", true))
            {
                PandaChat.Send(player, "The server administrator had disabled the changing of Settlers.",
                               ChatColor.red);

                return true;
            }

            if (state.SettlersToggledTimes >= maxToggleTimes)
            {
                PandaChat.Send(player,
                               $"To limit abuse of the /settlers command you can no longer toggle settlers on or off. You have used your alloted {maxToggleTimes} times.",
                               ChatColor.red);

                return true;
            }

            if (array.Length == 1)
            {
                PandaChat.Send(player, "Settlers! Settlers are {0}. You have toggled this {1} out of {2} times.",
                               ChatColor.green, state.SettlersEnabled ? "on" : "off",
                               state.SettlersToggledTimes.ToString(), maxToggleTimes.ToString());

                return true;
            }

            if (array.Length == 2 && state.SettlersToggledTimes <= maxToggleTimes)
            {
                if (array[1].ToLower().Trim() == "on" || array[1].ToLower().Trim() == "true")
                {
                    if (!state.SettlersEnabled)
                        state.SettlersToggledTimes++;

                    state.SettlersEnabled = true;

                    PandaChat.Send(player,
                                   $"Settlers! Mod Settlers are now on. You have toggled this {state.SettlersToggledTimes} out of {maxToggleTimes} times.",
                                   ChatColor.green);
                }
                else
                {
                    if (state.SettlersEnabled)
                        state.SettlersToggledTimes++;

                    state.SettlersEnabled = false;

                    PandaChat.Send(player,
                                   $"Settlers! Mod Settlers are now off. You have toggled this {state.SettlersToggledTimes} out of {maxToggleTimes} times.",
                                   ChatColor.green);
                }
            }

            return true;
        }
    }
}