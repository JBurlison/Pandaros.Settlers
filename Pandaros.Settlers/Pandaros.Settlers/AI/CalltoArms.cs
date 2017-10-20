using ChatCommands;
using Pandaros.Settlers.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pandaros.Settlers.AI
{
    public class CalltoArms : IChatCommand
    {
        public bool IsCommand(string chat)
        {
            return chat.StartsWith("/arms", StringComparison.OrdinalIgnoreCase);
        }

        public bool TryDoCommand(Players.Player player, string chat)
        {
            if (player == null || player.ID == NetworkID.Server)
                return true;

            string[] array = CommandManager.SplitCommand(chat);
            Colony colony = Colony.Get(player);
            PlayerState state = SettlerManager.GetPlayerState(player, colony);
            state.CallToArmsEnabled = !state.CallToArmsEnabled;

            if (state.CallToArmsEnabled)
            {

            }
            else
            {

            }

            return true;
        }
    }
}
