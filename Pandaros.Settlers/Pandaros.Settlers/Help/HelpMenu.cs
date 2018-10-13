using NetworkUI;
using Pandaros.Settlers.Items;
using Pipliz;
using Shared;
using System.Collections.Generic;
using System.Linq;

namespace Pandaros.Settlers.Help
{
    public enum MenuItemType
    {
        Button,
        Image,
        Text
    }

    public class HelpMenuActivator : CSType
    {
        public static string NAME = GameLoader.NAMESPACE + ".HelpMenu";
        public override string Name => NAME;
        public override string icon => GameLoader.ICON_PATH + "Help.png";
        public override bool? isPlaceable => false;
    }

    public class HelpMenuItem : IHelpMenuItem
    {
        public string MenuName { get; private set; }
        public IItem Item { get; private set; }
        public string Name { get; private set; }
    }

    [ModLoader.ModManager]
    public static class HelpMenu
    {
        public static readonly string NAMESPACE = GameLoader.NAMESPACE + ".HelpMenu.";
        public static readonly string MAIN_MENU_NAME = NAMESPACE + "MainMenu";

        public static Dictionary<string, List<HelpMenuItem>> Menus { get; } = new Dictionary<string, List<HelpMenuItem>>();


        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnPlayerConnectedLate, GameLoader.NAMESPACE + ".Help.HelpMenuItem.OnPlayerConnectedLate")]
        public static void OnPlayerConnectedLate(Players.Player p)
        {
            AddHelpMenuToolToStockpile(p);
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnPlayerRespawn, GameLoader.NAMESPACE + ".Help.HelpMenuItem.OnPlayerRespawn")]
        public static void OnPlayerRespawn(Players.Player p)
        {
            AddHelpMenuToolToStockpile(p);
        }

        private static void AddHelpMenuToolToStockpile(Players.Player p)
        {
            if (p != null && p.Colonies != null && p.Colonies.Length != 0)
                foreach (var c in p.Colonies)
                    if (ItemTypes.IndexLookup.TryGetIndex(HelpMenuActivator.NAME, out var helpMenuitem) && !c.Stockpile.Contains(helpMenuitem))
                    {
                        c.Stockpile.Add(helpMenuitem);
                    }
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnPlayerClicked, GameLoader.NAMESPACE + ".Help.HelpMenuItem.OpenMenu")]
        public static void OpenMenu(Players.Player player, Box<PlayerClickedData> boxedData)
        {
            if (ItemTypes.IndexLookup.TryGetIndex(HelpMenuActivator.NAME, out var helpMenuitem) &&
                boxedData.item1.typeSelected == helpMenuitem)
            {
                var nm = new NetworkMenu();
                nm.Height = 600;
                nm.Width = 1000;
                nm.LocalStorage.SetAs("header", NAMESPACE + "title");
                nm.Identifier = GameLoader.NAMESPACE + ".Help.HelpMenu";

                foreach (var item in Menus[MAIN_MENU_NAME].OrderBy(i => i.Name))
                {
                    // TODO  
                }

                NetworkMenuManager.SendServerPopup(player, nm);
            }
        }
    }
}
