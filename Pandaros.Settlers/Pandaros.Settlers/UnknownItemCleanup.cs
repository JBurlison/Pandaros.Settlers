using Chatting;
using Pandaros.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandaros.Settlers
{
    [ModLoader.ModManager]
    public class UnknownItemCleanup : IChatCommand
    {
        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterWorldLoad, GameLoader.NAMESPACE + ".Pandaros.Settlers.UnknownItemCleanup.AfterWorldLoad")]
        public static void AfterWorldLoad()
        {
            foreach (var c in ServerManager.ColonyTracker.ColoniesByID)
                ScrubColony(c.Value);
        }

        public bool TryDoCommand(Players.Player player, string chat, List<string> splits)
        {
            if (!chat.StartsWith("/clean", StringComparison.OrdinalIgnoreCase))
                return false;

            if (player == null || player.ID == NetworkID.Server || player.ActiveColony == null)
                return true;

            foreach (var c in player.Colonies)
                ScrubColony(c);

            for (int i=0; i < player.Inventory.Items.Length ;i++)
            {
                if (ItemTypes.TryGetType(player.Inventory.Items[i].Type, out var itemType))
                {
                    if (!string.IsNullOrEmpty(itemType.ParentType) && itemType.ParentType == "missingerror")
                        player.Inventory.Items[i] = new InventoryItem();
                }
                else
                    player.Inventory.Items[i] = new InventoryItem();
            }

            return true;
        }

        private static void ScrubColony(Colony c)
        {
            List<ushort> notFound = new List<ushort>();

            foreach (var item in c.Stockpile.Items)
            {
                if (ItemTypes.TryGetType(item.Key, out var itemType))
                {
                    if (!string.IsNullOrEmpty(itemType.ParentType) && itemType.ParentType == "missingerror")
                        notFound.Add(item.Key);
                }
                else
                    notFound.Add(item.Key);
            }

            foreach (var i in notFound)
                c.Stockpile.Items.Remove(i);
        }
    }
}
