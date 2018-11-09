using Chatting;
using Pandaros.Settlers.Managers;
using System;
using System.Collections.Generic;

namespace Pandaros.Settlers
{
    public class ColonyArchiver : IChatCommand
    {
        public bool IsCommand(string chat)
        {
            return chat.StartsWith("/archive", StringComparison.OrdinalIgnoreCase);
        }

        public bool TryDoCommand(Players.Player player, string chat, List<string> split)
        {
            if (player == null || player.ID == NetworkID.Server ||
                !PermissionsManager.CheckAndWarnPermission(player,
                                                           new PermissionsManager.Permission(GameLoader.NAMESPACE +
                                                                                             ".Permissions.Archive")))
                return true;

            foreach (var c in ServerManager.ColonyTracker.ColoniesByID.Values)
                SettlerManager.SaveOffline(c);

            return true;
        }
    }
}