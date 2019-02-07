using Chatting;
using System;
using System.Collections.Generic;

namespace Pandaros.Settlers.ColonyManagement
{
    public class ColonyArchiver : IChatCommand
    {
        public bool TryDoCommand(Players.Player player, string chat, List<string> split)
        {
            if (!chat.StartsWith("/archive", StringComparison.OrdinalIgnoreCase))
                return false;

            if (player == null || player.ID == NetworkID.Server ||
                !PermissionsManager.CheckAndWarnPermission(player,
                                                           new PermissionsManager.Permission(GameLoader.NAMESPACE +
                                                                                             ".Permissions.Archive")))
                return true;

            foreach (var c in ServerManager.ColonyTracker.ColoniesByID.Values)
                ColonyArchive.SaveOffline(c);

            return true;
        }
    }
}