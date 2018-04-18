using ChatCommands;
using ICSharpCode.SharpZipLib.Zip;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;

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
            if (player == null || player.ID == NetworkID.Server || !Permissions.PermissionsManager.CheckAndWarnPermission(player, new Permissions.PermissionsManager.Permission(GameLoader.NAMESPACE + ".Permissions.Archive")))
                    return true;

            foreach (var p in Players.PlayerDatabase.ValuesAsList)
                Managers.SettlerManager.SaveOffline(p);
 
            return true;
        }
    }
   
}
