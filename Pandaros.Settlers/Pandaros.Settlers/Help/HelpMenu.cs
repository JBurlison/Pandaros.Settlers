using NetworkUI;
using Pandaros.Settlers.Items;
using Pipliz;
using Pipliz.JSON;
using Shared;
using System.Collections.Generic;

using Pandaros.Settlers.Managers;
using System.IO;

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
        public static readonly string ModPath = GameLoader.MOD_FOLDER + "/help.json";

        public static Dictionary<string, JSONNode> Menus { get; } = new Dictionary<string, JSONNode>();

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterWorldLoad, ".Help.HelpMenuItem.AfterWorldLoad")]
        public static void AfterWorldLoad()
        {
            if(File.Exists(ModPath))
            {
                JSONNode jsonFile = JSON.Deserialize(ModPath);

                foreach(var child in jsonFile.LoopObject())
                {
                    Menus.Add(child.Key, child.Value);
                    Log.Write(string.Format("<color=lime>Adding: {0}</color>", child.Key));
                }
                Log.Write("<color=green>" + ModPath + "FOUND</color>");
            }
            else
                Log.Write("<color=red>" + ModPath + "NOT FOUND</color>");
        }

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
            //Only launch on RIGHT click
            if(player == null || boxedData.item1.clickType != PlayerClickedData.ClickType.Right)
                return;

            if (ItemTypes.IndexLookup.TryGetIndex(HelpMenuActivator.NAME, out var helpMenuitem) &&
                boxedData.item1.typeSelected == helpMenuitem)
            {
                UIManager.SendMenu(player, Menus["Wiki"]);
            }
        }
    }
}
