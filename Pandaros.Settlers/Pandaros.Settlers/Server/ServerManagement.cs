using NetworkUI;
using Pandaros.Settlers.Items;
using Pipliz;
using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Pandaros.Settlers.Items.StaticItems;

namespace Pandaros.Settlers.Server
{
    public class ServerManagementTool : CSType
    {
        public static string NAME = GameLoader.NAMESPACE + ".ServerManagementTool";
        public override string name => NAME;
        public override string icon => GameLoader.ICON_PATH + "ServerManagementTool.png";
        public override bool? isPlaceable => false;
        public override int? maxStackSize => 1;
        public override List<string> categories { get; set; } = new List<string>()
        {
            "essential",
            "aaa"
        };

        public override StaticItem StaticItemSettings => new StaticItem()
        {
            Name = GameLoader.NAMESPACE + ".ServerManagementTool",
            RequiredPermission = GameLoader.NAMESPACE + ".ServerManagementTool"
        };
    }

    public static class ServerManagement
    {
        static readonly localization.LocalizationHelper _localizationHelper = new localization.LocalizationHelper("servermanager");

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnPlayerClicked, GameLoader.NAMESPACE + ".Managers.ServerManager.OpenMenu")]
        public static void OpenMenu(Players.Player player, Box<PlayerClickedData> boxedData)
        {
            //Only launch on RIGHT click
            if (player == null || boxedData.item1.clickType != PlayerClickedData.ClickType.Right || player.ActiveColony == null)
                return;

            if (ItemTypes.IndexLookup.TryGetIndex(GameLoader.NAMESPACE + ".ServerManagementTool", out var toolItem) &&
                boxedData.item1.typeSelected == toolItem)
            {
                NetworkMenuManager.SendServerPopup(player, MainMenu(player));
            }
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnPlayerPushedNetworkUIButton, GameLoader.NAMESPACE + ".Managers.ServerManager.PressButton")]
        public static void PressButton(ButtonPressCallbackData data)
        {
            if (data.ButtonIdentifier == GameLoader.NAMESPACE + ".ResetPlayer")
            {
            }
        }

        public static NetworkMenu MainMenu(Players.Player p)
        {
            NetworkMenu menu = new NetworkMenu();
            menu.LocalStorage.SetAs("header", _localizationHelper.LocalizeOrDefault("ServerManager", p));
            menu.Width = Configuration.GetorDefault("MenuWidths", 1000);
            menu.Height = Configuration.GetorDefault("MenuHeights", 600);


            return menu;
        }
    }
}
