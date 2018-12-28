using BlockTypes;
using NetworkUI;
using NetworkUI.Items;
using Pandaros.Settlers.Entities;
using Pandaros.Settlers.Models;
using Pandaros.Settlers.Research;
using Pipliz;
using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Pandaros.Settlers.Items.StaticItems;

namespace Pandaros.Settlers.Items
{
    public class BackpackResearch : IPandaResearch
    {
        public Dictionary<ushort, int> RequiredItems => new Dictionary<ushort, int>()
        {
            { BuiltinBlocks.ScienceBagBasic, 1 },
        };

        public int NumberOfLevels => 1;
        public float BaseValue => 0.05f;
        public List<string> Dependancies => new List<string>()
            {
                ColonyBuiltIn.Research.ScienceBagBasic
            };

        public int BaseIterationCount => 50;
        public bool AddLevelToName => true;
        public string Name => "Backpack";

        public void OnRegister()
        {

        }

        public void ResearchComplete(object sender, ResearchCompleteEventArgs e)
        {
            if (ItemTypes.IndexLookup.IndexLookupTable.TryGetItem(Backpack.NAME, out var item) &&
                !e.Manager.Colony.Stockpile.Contains(item.ItemIndex))
                e.Manager.Colony.Stockpile.Add(item.ItemIndex);
        }
    }

    public class Backpack : CSType
    {
        public static string NAME = GameLoader.NAMESPACE + ".Backpack";
        public override string Name => NAME;
        public override string icon => GameLoader.ICON_PATH + "Backpack.png";
        public override bool? isPlaceable => false;
        public override int? maxStackSize => 1;
        public override List<string> categories { get; set; } = new List<string>()
        {
            "essential",
            "aaa"
        };

        public override StaticItem StaticItemSettings => new StaticItem()
        {
            Name = NAME,
            RequiredScience = NAME + 1
        };
    }

    [ModLoader.ModManager]
    public class BackpackCallbacks
    {
        static readonly Pandaros.Settlers.localization.LocalizationHelper _localizationHelper = new localization.LocalizationHelper("backpack");

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnPlayerClicked, GameLoader.NAMESPACE + ".Items.Backpack.OpenMenu")]
        public static void OpenMenu(Players.Player player, Box<PlayerClickedData> boxedData)
        {
            //Only launch on RIGHT click
            if (player == null || boxedData.item1.clickType != PlayerClickedData.ClickType.Right)
                return;

            if (ItemTypes.IndexLookup.TryGetIndex(GameLoader.NAMESPACE + ".Backpack", out var backpackItem) &&
                boxedData.item1.typeSelected == backpackItem)
            {
                NetworkMenuManager.SendServerPopup(player, MainMenu(player));
            }
        }

        public static NetworkMenu MainMenu(Players.Player player)
        {
            var ps = PlayerState.GetPlayerState(player);

            NetworkUI.NetworkMenu menu = new NetworkMenu();
            menu.LocalStorage.SetAs("header", _localizationHelper.LocalizeOrDefault("Backpack", player));
            menu.Width = 1000;
            menu.Height = 600;

            if (player.ActiveColony != null)
                menu.Items.Add(new HorizontalSplit(new ButtonCallback("Backpack.GetItemsFromStockpile", new LabelData(_localizationHelper.GetLocalizationKey("GetItemsFromStockpile"), UnityEngine.Color.black)),
                                                   new ButtonCallback("Backpack.GetItemsFromToolbar", new LabelData(_localizationHelper.GetLocalizationKey("GetItemsFromToolbar"), UnityEngine.Color.black))));
            else
                menu.Items.Add(new ButtonCallback("Backpack.GetItemsFromToolbar", new LabelData(_localizationHelper.GetLocalizationKey("GetItemsFromToolbar"), UnityEngine.Color.black)));

            List<IItem> headerItems = new List<IItem>();
            headerItems.Add(new Label(new LabelData(_localizationHelper.GetLocalizationKey("Numberofitems"), UnityEngine.Color.black)));
            headerItems.Add(new InputField("Backpack.NumberOfItems"));

            if (player.ActiveColony != null)
                headerItems.Add(new ButtonCallback("Backpack.MoveItemsToStockpile", new LabelData(_localizationHelper.GetLocalizationKey("MoveItemsToStockpile"), UnityEngine.Color.black)));

            headerItems.Add(new ButtonCallback("Backpack.MoveItemsToToolbar", new LabelData(_localizationHelper.GetLocalizationKey("MoveItemsToToolbar"), UnityEngine.Color.black)));
           
            menu.Items.Add(new HorizontalGrid(headerItems, 250));

            foreach (var itemKvp in ps.Backpack)
            {
                List<IItem> items = new List<IItem>();
                items.Add(new ItemIcon(itemKvp.Key));
                items.Add(new Label(new LabelData(ItemId.GetItemId(itemKvp.Key), UnityEngine.Color.black, UnityEngine.TextAnchor.MiddleLeft, 18, LabelData.ELocalizationType.Type)));
                items.Add(new Label(new LabelData(_localizationHelper.LocalizeOrDefault("Backpack", player) + ": " + itemKvp.Value.ToString(), UnityEngine.Color.black)));
                items.Add(new Toggle(new LabelData(_localizationHelper.LocalizeOrDefault("Select", player), UnityEngine.Color.black), "Backpack." + itemKvp.Key + ".ItemSelected"));
                menu.Items.Add(new HorizontalGrid(items, 250));
            }

            return menu;
        }
    }
}
