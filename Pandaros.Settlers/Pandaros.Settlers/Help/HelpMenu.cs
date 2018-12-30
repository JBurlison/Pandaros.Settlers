using Pandaros.Settlers.Items;
using Pandaros.Settlers.Managers;
using Pipliz;
using Shared;
using System.Collections.Generic;
using static Pandaros.Settlers.Items.StaticItems;

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
        public override int? maxStackSize => 1;
        public override List<string> categories { get; set; } = new List<string>()
        {
            "essential",
            "aaa"
        };
        public override StaticItem StaticItemSettings => new StaticItem() { Name = GameLoader.NAMESPACE + ".HelpMenu" };
    }

    [ModLoader.ModManager]
    public static class HelpMenu
    {
        public static readonly string NAMESPACE = GameLoader.NAMESPACE + ".HelpMenu.";
        public static readonly string MAIN_MENU_NAME = NAMESPACE + "MainMenu";

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnPlayerClicked, GameLoader.NAMESPACE + ".Help.HelpMenuItem.OpenMenu")]
        public static void OpenMenu(Players.Player player, Box<PlayerClickedData> boxedData)
        {
            //Only launch on RIGHT click
            if(player == null || boxedData.item1.clickType != PlayerClickedData.ClickType.Right)
                return;

            if (ItemTypes.IndexLookup.TryGetIndex(HelpMenuActivator.NAME, out var helpMenuitem) &&
                boxedData.item1.typeSelected == helpMenuitem)
            {
                UIManager.SendMenu(player, "Wiki.MainMenu");
            }
        }
    }
}
