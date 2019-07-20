﻿using NetworkUI;
using NetworkUI.Items;
using Pandaros.API;
using Pandaros.API.Entities;
using Pandaros.API.Items;
using Pandaros.API.localization;
using Pandaros.API.Models;
using Pandaros.API.Research;
using Recipes;
using Science;
using Shared;
using System;
using System.Collections.Generic;

namespace Pandaros.Settlers.Items
{
    public class BackpackResearch : IPandaResearch
    {
        public Dictionary<int, List<InventoryItem>> RequiredItems => null;

        public int NumberOfLevels => 1;
        public float BaseValue => 0.05f;
        public Dictionary<int, List<string>> Dependancies => null;

        public int BaseIterationCount => 50;
        public bool AddLevelToName => true;
        public string name => GameLoader.NAMESPACE + ".Backpack";

        public string IconDirectory => GameLoader.ICON_PATH;

        public Dictionary<int, List<IResearchableCondition>> Conditions => new Dictionary<int, List<IResearchableCondition>>()
        {
            {
                0,
                new List<IResearchableCondition>()
                {
                    new HappinessCondition() { Threshold = 50 },
                    new ColonistCountCondition() { Threshold = 50 }
                }
            }
        };

        public Dictionary<int, List<RecipeUnlock>> Unlocks =>  new Dictionary<int, List<RecipeUnlock>>()
        {
            {
                1,
                new List<RecipeUnlock>()
                {
                    new RecipeUnlock(SettlersBuiltIn.ItemTypes.BACKPACK, ERecipeUnlockType.Recipe)
                }
            }
        };

        public void OnRegister()
        {

        }

        public void ResearchComplete(object sender, ResearchCompleteEventArgs e)
        {
            foreach (var p in e.Manager.Colony.Owners)
                StaticItems.AddStaticItemToStockpile(p);
        }
    }

    public class BackpackRecipe : ICSRecipe
    {
        public List<RecipeItem> requires => new List<RecipeItem>()
        {
            new RecipeItem(ColonyBuiltIn.ItemTypes.LINEN.Id, 6)
        };

        public List<RecipeResult> results => new List<RecipeResult>()
        {
            new RecipeResult(SettlersBuiltIn.ItemTypes.BACKPACK.Id)
        };

        public CraftPriority defaultPriority => CraftPriority.Low;

        public bool isOptional => true;

        public int defaultLimit => 1;

        public string Job => ColonyBuiltIn.NpcTypes.CRAFTER;

        public string name => SettlersBuiltIn.ItemTypes.BACKPACK;
    }

    public class Backpack : CSType
    {
        public static string NAME = GameLoader.NAMESPACE + ".Backpack";
        public override string name => NAME;
        public override string icon => GameLoader.ICON_PATH + "Backpack.png";
        public override bool? isPlaceable => false;
        public override int? maxStackSize => 1;
        public override List<string> categories { get; set; } = new List<string>()
        {
            "essential",
            "aaa"
        };
    }

    [ModLoader.ModManager]
    public class BackpackCallbacks
    {
        static readonly LocalizationHelper _localizationHelper = new LocalizationHelper(GameLoader.NAMESPACE, "backpack");

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnPlayerClicked, GameLoader.NAMESPACE + ".Items.Backpack.OpenMenu")]
        public static void OpenMenu(Players.Player player, PlayerClickedData playerClickData)
        {
            //Only launch on RIGHT click
            if (player == null || playerClickData.ClickType != PlayerClickedData.EClickType.Right)
                return;

            if (ItemTypes.IndexLookup.TryGetIndex(GameLoader.NAMESPACE + ".Backpack", out var backpackItem) &&
                playerClickData.TypeSelected == backpackItem)
            {
                NetworkMenuManager.SendServerPopup(player, MainMenu(player));
            }
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnPlayerPushedNetworkUIButton, GameLoader.NAMESPACE + ".Items.Backpack.PressButton")]
        public static void PressButton(ButtonPressCallbackData data)
        {
            if (data.ButtonIdentifier == "Backpack.GetItemsFromStockpile")
            {
                NetworkMenu menu = StockpileMenu(data);
                NetworkMenuManager.SendServerPopup(data.Player, menu);
            }
            else if (data.ButtonIdentifier == "Backpack.MainMenu")
            {
                NetworkMenuManager.SendServerPopup(data.Player, MainMenu(data.Player));
            }
            else if (data.ButtonIdentifier == "Backpack.GetItemsFromToolbar")
            {
                NetworkMenuManager.SendServerPopup(data.Player, ToolbarMenu(data));
            }
            else if (data.ButtonIdentifier == "Backpack.SelectAllInBackpack")
            {
                NetworkMenuManager.SendServerPopup(data.Player, StockpileMenu(data, false, true));
            }
            else if (data.ButtonIdentifier == "Backpack.SelectNoneInBackpack")
            {
                NetworkMenuManager.SendServerPopup(data.Player, StockpileMenu(data, false, false));
            }
            else if (data.ButtonIdentifier == "Backpack.SelectAllInToolbar")
            {
                NetworkMenuManager.SendServerPopup(data.Player, ToolbarMenu(data, false, true));
            }
            else if (data.ButtonIdentifier == "Backpack.SelectNoneInToolbar")
            {
                NetworkMenuManager.SendServerPopup(data.Player, ToolbarMenu(data, false, false));
            }
            else if (data.ButtonIdentifier == "Backpack.SelectNoneInBackpackMain")
            {
                NetworkMenuManager.SendServerPopup(data.Player, MainMenu(data.Player, false, false));
            }
            else if (data.ButtonIdentifier == "Backpack.SelectAllInBackpackMain")
            {
                NetworkMenuManager.SendServerPopup(data.Player, MainMenu(data.Player, false, true));
            }
            else if (data.ButtonIdentifier == "Backpack.SelectNoneInBackpackToolbar")
            {
                NetworkMenuManager.SendServerPopup(data.Player, ToolbarMenu(data, false, false));
            }
            else if (data.ButtonIdentifier == "Backpack.SelectAllInBackpackToolbar")
            {
                NetworkMenuManager.SendServerPopup(data.Player, ToolbarMenu(data, false, true));
            }
            else if (data.ButtonIdentifier == "Backpack.MoveItemsToStockpile")
            {
                if (data.Storage.TryGetAs("Backpack.NumberOfItems", out string strNumItems) && int.TryParse(strNumItems, out int numItems))
                {
                    Dictionary<ushort, int> removeItems = new Dictionary<ushort, int>();
                    var ps = PlayerState.GetPlayerState(data.Player);

                    foreach (var itemKvp in ps.Backpack)
                    {
                        if (data.Storage.TryGetAs("Backpack." + itemKvp.Key + ".ItemSelected", out bool selected) && selected)
                        {
                            int takeNum = System.Math.Min(numItems, itemKvp.Value);
                            removeItems[itemKvp.Key] = takeNum;
                        }
                    }

                    foreach (var item in removeItems)
                    {
                        ps.Backpack[item.Key] -= item.Value;

                        if (ps.Backpack[item.Key] <= 0)
                            ps.Backpack.Remove(item.Key);

                        data.Player.ActiveColony.Stockpile.Add(item.Key, item.Value);
                    }

                    NetworkMenuManager.SendServerPopup(data.Player, MainMenu(data.Player));
                }
                else
                    NetworkMenuManager.SendServerPopup(data.Player, MainMenu(data.Player, true));

            }
            else if (data.ButtonIdentifier == "Backpack.MoveItemsToToolbar")
            {
                if (data.Storage.TryGetAs("Backpack.NumberOfItems", out string strNumItems) && int.TryParse(strNumItems, out int numItems))
                {
                    Dictionary<ushort, int> removeItems = new Dictionary<ushort, int>();
                    var ps = PlayerState.GetPlayerState(data.Player);
                    var invRef = data.Player.Inventory;

                    foreach (var itemKvp in ps.Backpack)
                    {
                        if (data.Storage.TryGetAs("Backpack." + itemKvp.Key + ".ItemSelected", out bool selected) && selected)
                        {
                            int takeNum = System.Math.Min(numItems, itemKvp.Value);
                            removeItems[itemKvp.Key] = takeNum;
                        }
                    }

                    foreach (var item in removeItems)
                    {
                        if (invRef.TryAdd(item.Key, item.Value))
                        {
                            ps.Backpack[item.Key] -= item.Value;

                            if (ps.Backpack[item.Key] <= 0)
                                ps.Backpack.Remove(item.Key);
                        }
                        else
                            break;
                    }

                    NetworkMenuManager.SendServerPopup(data.Player, MainMenu(data.Player));
                }
                else
                    NetworkMenuManager.SendServerPopup(data.Player, MainMenu(data.Player, true));
            }
            else if (data.ButtonIdentifier == "Backpack.MoveItemsToBackpackFromStockpile")
            {
                if (data.Storage.TryGetAs("Backpack.NumberOfItems", out string strNumItems) && int.TryParse(strNumItems, out int numItems))
                {
                    Dictionary<ushort, int> removeItems = new Dictionary<ushort, int>();
                    var ps = PlayerState.GetPlayerState(data.Player);
                    var backpackID = ItemId.GetItemId(Backpack.NAME);

                    foreach (var itemKvp in data.Player.ActiveColony.Stockpile.Items)
                    {
                        if (itemKvp.Key != backpackID && data.Storage.TryGetAs("Backpack." + itemKvp.Key + ".ItemSelected", out bool selected) && selected)
                        {
                            int takeNum = System.Math.Min(numItems, itemKvp.Value);
                            removeItems[itemKvp.Key] = takeNum;
                        }
                    }

                    foreach (var item in removeItems)
                    {
                        data.Player.ActiveColony.Stockpile.TryRemove(item.Key, item.Value);

                        if (!ps.Backpack.ContainsKey(item.Key))
                            ps.Backpack.Add(item.Key, item.Value);
                        else
                            ps.Backpack[item.Key] += item.Value;
                    }

                    NetworkMenuManager.SendServerPopup(data.Player, MainMenu(data.Player));
                }
                else
                    NetworkMenuManager.SendServerPopup(data.Player, MainMenu(data.Player, true));
            }
            else if (data.ButtonIdentifier == "Backpack.MoveItemsToBackpackFromToolbar")
            {
                if (data.Storage.TryGetAs("Backpack.NumberOfItems", out string strNumItems) && int.TryParse(strNumItems, out int numItems))
                {
                    Dictionary<ushort, int> removeItems = new Dictionary<ushort, int>();
                    var ps = PlayerState.GetPlayerState(data.Player);
                    var invRef = data.Player.Inventory;
                    var backpackID = ItemId.GetItemId(Backpack.NAME);

                    foreach (var itemKvp in invRef.Items)
                    {
                        if (itemKvp.Type != backpackID && data.Storage.TryGetAs("Backpack." + itemKvp.Type + ".ItemSelected", out bool selected) && selected)
                        {
                            int takeNum = System.Math.Min(numItems, itemKvp.Amount);
                            removeItems[itemKvp.Type] = takeNum;
                        }
                    }

                    foreach (var item in removeItems)
                    {
                        if (invRef.TryRemove(item.Key, item.Value))
                        {
                            if (!ps.Backpack.ContainsKey(item.Key))
                                ps.Backpack.Add(item.Key, item.Value);
                            else
                                ps.Backpack[item.Key] += item.Value;
                        }
                    }

                    NetworkMenuManager.SendServerPopup(data.Player, MainMenu(data.Player));
                }
                else
                    NetworkMenuManager.SendServerPopup(data.Player, MainMenu(data.Player, true));
            }
        }

        private static NetworkMenu StockpileMenu(ButtonPressCallbackData data, bool error = false, bool? selectAll = null)
        {
            NetworkMenu menu = new NetworkMenu();
            menu.LocalStorage.SetAs("header", _localizationHelper.LocalizeOrDefault("MoveItemsToBackpack", data.Player));
            menu.Width = 1000;
            menu.Height = 600;

            try
            {
                if (error)
                    menu.Items.Add(new Label(new LabelData(_localizationHelper.GetLocalizationKey("invalidNumber"), UnityEngine.Color.red)));

                List<ValueTuple<IItem, int>> headerItems = new List<ValueTuple<IItem, int>>();
                headerItems.Add(ValueTuple.Create<IItem, int>(new Label(new LabelData(_localizationHelper.GetLocalizationKey("Numberofitems"), UnityEngine.Color.black)), 333));
                headerItems.Add(ValueTuple.Create<IItem, int>(new InputField("Backpack.NumberOfItems"), 333));
                headerItems.Add(ValueTuple.Create<IItem, int>(new ButtonCallback("Backpack.MoveItemsToBackpackFromStockpile", new LabelData(_localizationHelper.GetLocalizationKey("MoveItemsToBackpack"), UnityEngine.Color.black)), 333));
                menu.Items.Add(new HorizontalRow(headerItems));
                menu.Items.Add(new Line(UnityEngine.Color.black));

                List<ValueTuple<IItem, int>> items = new List<ValueTuple<IItem, int>>();
                items.Add(ValueTuple.Create<IItem, int>(new ButtonCallback("Backpack.MainMenu", new LabelData(_localizationHelper.GetLocalizationKey("Back"), UnityEngine.Color.black)), 250));
                items.Add(ValueTuple.Create<IItem, int>(new EmptySpace(), 250));
                items.Add(ValueTuple.Create<IItem, int>(new EmptySpace(), 250));

                bool selected = false;

                if (selectAll == true)
                    selected = true;
                else if (selectAll == false)
                    selected = false;

                if (selected)
                    items.Add(ValueTuple.Create<IItem, int>(new ButtonCallback("Backpack.SelectNoneInBackpack", new LabelData(_localizationHelper.GetLocalizationKey("SelectNone"), UnityEngine.Color.black)), 250));
                else
                    items.Add(ValueTuple.Create<IItem, int>(new ButtonCallback("Backpack.SelectAllInBackpack", new LabelData(_localizationHelper.GetLocalizationKey("SelectAll"), UnityEngine.Color.black)), 250));

                menu.Items.Add(new HorizontalRow(items));
                menu.Items.Add(new Line(UnityEngine.Color.black));
                var backpackID = ItemId.GetItemId(Backpack.NAME);

                foreach (var itemKvp in data.Player.ActiveColony.Stockpile.Items)
                {
                    if (itemKvp.Key != backpackID)
                    {
                        items = new List<ValueTuple<IItem, int>>();
                        items.Add(ValueTuple.Create<IItem, int>(new ItemIcon(itemKvp.Key), 250));
                        items.Add(ValueTuple.Create<IItem, int>(new Label(new LabelData(ItemId.GetItemId(itemKvp.Key), UnityEngine.Color.black, UnityEngine.TextAnchor.MiddleLeft, 18, LabelData.ELocalizationType.Type)), 250));
                        items.Add(ValueTuple.Create<IItem, int>(new Label(new LabelData(_localizationHelper.LocalizeOrDefault("Stockpile", data.Player) + ": " + itemKvp.Value.ToString(), UnityEngine.Color.black)), 250));
                        items.Add(ValueTuple.Create<IItem, int>(new Toggle(new LabelData(_localizationHelper.LocalizeOrDefault("Select", data.Player), UnityEngine.Color.black), "Backpack." + itemKvp.Key + ".ItemSelected"), 250));

                        if (selectAll == null)
                            menu.LocalStorage.TryGetAs("Backpack." + itemKvp.Key + ".ItemSelected", out selected);

                        menu.LocalStorage.SetAs("Backpack." + itemKvp.Key + ".ItemSelected", selected);
                        menu.Items.Add(new HorizontalRow(items));
                    }
                }
            }
            catch (Exception ex)
            {
                SettlersLogger.LogError(ex);
            }

            return menu;
        }

        public static NetworkMenu ToolbarMenu(ButtonPressCallbackData data, bool error = false, bool? selectAll = null)
        {
            NetworkMenu menu = new NetworkMenu();
            menu.LocalStorage.SetAs("header", _localizationHelper.LocalizeOrDefault("MoveItemsToBackpack", data.Player));
            menu.Width = 1000;
            menu.Height = 600;

            try
            {
                if (error)
                    menu.Items.Add(new Label(new LabelData(_localizationHelper.GetLocalizationKey("invalidNumber"), UnityEngine.Color.red)));

                List<ValueTuple<IItem, int>> headerItems = new List<ValueTuple<IItem, int>>();
                headerItems.Add(ValueTuple.Create<IItem, int>(new Label(new LabelData(_localizationHelper.GetLocalizationKey("Numberofitems"), UnityEngine.Color.black)), 333));
                headerItems.Add(ValueTuple.Create<IItem, int>(new InputField("Backpack.NumberOfItems"), 333));
                headerItems.Add(ValueTuple.Create<IItem, int>(new ButtonCallback("Backpack.MoveItemsToBackpackFromToolbar", new LabelData(_localizationHelper.GetLocalizationKey("MoveItemsToBackpack"), UnityEngine.Color.black)), 333));
                menu.Items.Add(new HorizontalRow(headerItems));
                menu.Items.Add(new Line(UnityEngine.Color.black));

                List<ValueTuple<IItem, int>> items = new List<ValueTuple<IItem, int>>();
                items.Add(ValueTuple.Create<IItem, int>(new ButtonCallback("Backpack.MainMenu", new LabelData(_localizationHelper.GetLocalizationKey("Back"), UnityEngine.Color.black)), 250));
                items.Add(ValueTuple.Create<IItem, int>(new EmptySpace(), 250));
                items.Add(ValueTuple.Create<IItem, int>(new EmptySpace(), 250));

                bool selected = false;

                if (selectAll == true)
                    selected = true;
                else if (selectAll == false)
                    selected = false;

                if (selected)
                    items.Add(ValueTuple.Create<IItem, int>(new ButtonCallback("Backpack.SelectNoneInBackpackToolbar", new LabelData(_localizationHelper.GetLocalizationKey("SelectNone"), UnityEngine.Color.black)), 250));
                else
                    items.Add(ValueTuple.Create<IItem, int>(new ButtonCallback("Backpack.SelectAllInBackpackToolbar", new LabelData(_localizationHelper.GetLocalizationKey("SelectAll"), UnityEngine.Color.black)), 250));

                menu.Items.Add(new HorizontalRow(items));
                menu.Items.Add(new Line(UnityEngine.Color.black));
                var invRef = data.Player.Inventory;
                var backpackID = ItemId.GetItemId(Backpack.NAME);

                foreach (var itemKvp in invRef.Items)
                {
                    if (itemKvp.Type != ColonyBuiltIn.ItemTypes.AIR.Id && itemKvp.Type != backpackID)
                    {
                        items = new List<ValueTuple<IItem, int>>();
                        items.Add(ValueTuple.Create<IItem, int>(new ItemIcon(itemKvp.Type), 250));
                        items.Add(ValueTuple.Create<IItem, int>(new Label(new LabelData(ItemId.GetItemId(itemKvp.Type), UnityEngine.Color.black, UnityEngine.TextAnchor.MiddleLeft, 18, LabelData.ELocalizationType.Type)), 250));
                        items.Add(ValueTuple.Create<IItem, int>(new Label(new LabelData(_localizationHelper.LocalizeOrDefault("Toolbar", data.Player) + ": " + itemKvp.Amount.ToString(), UnityEngine.Color.black)), 250));
                        items.Add(ValueTuple.Create<IItem, int>(new Toggle(new LabelData(_localizationHelper.LocalizeOrDefault("Select", data.Player), UnityEngine.Color.black), "Backpack." + itemKvp.Type + ".ItemSelected"), 250));

                        if (selectAll == null)
                            menu.LocalStorage.TryGetAs("Backpack." + itemKvp.Type + ".ItemSelected", out selected);

                        menu.LocalStorage.SetAs("Backpack." + itemKvp.Type + ".ItemSelected", selected);
                        menu.Items.Add(new HorizontalRow(items));
                    }
                }
            }
            catch (Exception ex)
            {
                SettlersLogger.LogError(ex);
            }

            return menu;
        }

        public static NetworkMenu MainMenu(Players.Player player, bool error = false, bool? selectAll = null)
        {
            var ps = PlayerState.GetPlayerState(player);

            NetworkMenu menu = new NetworkMenu();
            menu.LocalStorage.SetAs("header", _localizationHelper.LocalizeOrDefault("Backpack", player));
            menu.Width = 1000;
            menu.Height = 600;

            if (error)
                menu.Items.Add(new Label(new LabelData(_localizationHelper.GetLocalizationKey("invalidNumber"), UnityEngine.Color.red)));

            if (player.ActiveColony != null)
                menu.Items.Add(new HorizontalSplit(new ButtonCallback("Backpack.GetItemsFromStockpile", new LabelData(_localizationHelper.GetLocalizationKey("GetItemsFromStockpile"), UnityEngine.Color.black)),
                                                   new ButtonCallback("Backpack.GetItemsFromToolbar", new LabelData(_localizationHelper.GetLocalizationKey("GetItemsFromToolbar"), UnityEngine.Color.black))));
            else
                menu.Items.Add(new ButtonCallback("Backpack.GetItemsFromToolbar", new LabelData(_localizationHelper.GetLocalizationKey("GetItemsFromToolbar"), UnityEngine.Color.black)));

            menu.Items.Add(new Line(UnityEngine.Color.black));

            List<ValueTuple<IItem, int>> headerItems = new List<ValueTuple<IItem, int>>();
            headerItems.Add(ValueTuple.Create<IItem, int>(new Label(new LabelData(_localizationHelper.GetLocalizationKey("Numberofitems"), UnityEngine.Color.black)), 250));
            headerItems.Add(ValueTuple.Create<IItem, int>(new InputField("Backpack.NumberOfItems"), 250));

            if (player.ActiveColony != null)
                headerItems.Add(ValueTuple.Create<IItem, int>(new ButtonCallback("Backpack.MoveItemsToStockpile", new LabelData(_localizationHelper.GetLocalizationKey("MoveItemsToStockpile"), UnityEngine.Color.black)), 250));

            headerItems.Add(ValueTuple.Create<IItem, int>(new ButtonCallback("Backpack.MoveItemsToToolbar", new LabelData(_localizationHelper.GetLocalizationKey("MoveItemsToToolbar"), UnityEngine.Color.black)), 250));
           
            menu.Items.Add(new HorizontalRow(headerItems));
            menu.Items.Add(new Line(UnityEngine.Color.black));

            List<ValueTuple<IItem, int>> items = new List<ValueTuple<IItem, int>>();
            items.Add(ValueTuple.Create<IItem, int>(new EmptySpace(), 250));
            items.Add(ValueTuple.Create<IItem, int>(new EmptySpace(), 250));
            items.Add(ValueTuple.Create<IItem, int>(new EmptySpace(), 250));

            bool selected = false;

            if (selectAll == true)
                selected = true;
            else if (selectAll == false)
                selected = false;

            if (selected)
                items.Add(ValueTuple.Create<IItem, int>(new ButtonCallback("Backpack.SelectNoneInBackpackMain", new LabelData(_localizationHelper.GetLocalizationKey("SelectNone"), UnityEngine.Color.black)), 250));
            else
                items.Add(ValueTuple.Create<IItem, int>(new ButtonCallback("Backpack.SelectAllInBackpackMain", new LabelData(_localizationHelper.GetLocalizationKey("SelectAll"), UnityEngine.Color.black)), 250));

            menu.Items.Add(new HorizontalRow(items));
            menu.Items.Add(new Line(UnityEngine.Color.black));


            foreach (var itemKvp in ps.Backpack)
            {
                items = new List<ValueTuple<IItem, int>>();
                items.Add(ValueTuple.Create<IItem, int>(new ItemIcon(itemKvp.Key), 250));
                items.Add(ValueTuple.Create<IItem, int>(new Label(new LabelData(ItemId.GetItemId(itemKvp.Key), UnityEngine.Color.black, UnityEngine.TextAnchor.MiddleLeft, 18, LabelData.ELocalizationType.Type)), 250));
                items.Add(ValueTuple.Create<IItem, int>(new Label(new LabelData(_localizationHelper.LocalizeOrDefault("Backpack", player) + ": " + itemKvp.Value.ToString(), UnityEngine.Color.black)), 250));
                items.Add(ValueTuple.Create<IItem, int>(new Toggle(new LabelData(_localizationHelper.LocalizeOrDefault("Select", player), UnityEngine.Color.black), "Backpack." + itemKvp.Key + ".ItemSelected"), 250));

                if (selectAll == null)
                    menu.LocalStorage.TryGetAs("Backpack." + itemKvp.Key + ".ItemSelected", out selected);

                menu.LocalStorage.SetAs("Backpack." + itemKvp.Key + ".ItemSelected", selected);
                menu.Items.Add(new HorizontalRow(items));
            }

            return menu;
        }
    }
}
