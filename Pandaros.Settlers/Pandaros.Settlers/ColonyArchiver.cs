using System;
using ChatCommands;
using Pandaros.Settlers.Managers;
using Permissions;

namespace Pandaros.Settlers
{
    public class ColonyArchiver : IChatCommand
    {
        public bool IsCommand(string chat)
        {
            return chat.StartsWith("/archive", StringComparison.OrdinalIgnoreCase);
        }

        public bool TryDoCommand(Players.Player player, string chat)
        {
            if (player == null || player.ID == NetworkID.Server ||
                !PermissionsManager.CheckAndWarnPermission(player,
                                                           new PermissionsManager.Permission(GameLoader.NAMESPACE +
                                                                                             ".Permissions.Archive")))
                return true;

            foreach (var p in Players.PlayerDatabase.Values)
                SettlerManager.SaveOffline(p);

            return true;
        }
    }
}