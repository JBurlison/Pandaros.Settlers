using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Pipliz;
using Pipliz.JSON;
using NetworkUI;
using NetworkUI.Items;
using Pandaros.Settlers.Help;
using Pandaros.Settlers.Items;
using Shared;
using Pandaros.Settlers.Models;

namespace Pandaros.Settlers.Help
{
    [ModLoader.ModManager]
    public static class UIManager
    {
        public static JSONNode LoadedMenus { get; private set; } = new JSONNode();
        private static localization.LocalizationHelper _localizationHelper = new localization.LocalizationHelper("Wiki");
        public static List<OpenMenuSettings> OpenMenuItems { get; private set; } = new List<OpenMenuSettings>();


        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnPlayerClicked, GameLoader.NAMESPACE + ".Help.HelpMenuItem.OpenMenu")]
        public static void OpenMenu(Players.Player player, Box<PlayerClickedData> boxedData)
        {
            if (player == null)
                return;

            foreach (var item in OpenMenuItems)
            {
                if (boxedData.item1.clickType != item.ActivateClickType)
                    continue;

                if (ItemTypes.IndexLookup.TryGetIndex(item.ItemName, out var menuItem) &&
                    boxedData.item1.typeSelected == menuItem)
                {
                    SendMenu(player, item.UIUrl);
                }
            }
        }
        

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnAssemblyLoaded, GameLoader.NAMESPACE + ".Managers.OnAssemblyLoaded")]
        [ModLoader.ModCallbackDependsOn(GameLoader.NAMESPACE + ".OnAssemblyLoaded")]
        public static void OnAssemblyLoaded(string path)
        {
            var settings = GameLoader.GetJSONSettingPaths(GameLoader.NAMESPACE + ".MenuFile");

            foreach(var info in settings)
                foreach(var jsonNode in info.Value)
                {
                    var newMenu = JSON.Deserialize(info.Key + "\\" + jsonNode);
                    LoadedMenus.Merge(newMenu);
                }
        }

        public static Dictionary<ushort, Recipes.Recipe> ItemRecipe = new Dictionary<ushort, Recipes.Recipe>();

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterWorldLoad, GameLoader.NAMESPACE + ".Managers.LoadRecipes")]
        public static void LoadRecipes()
        {
            foreach(var recipe in ServerManager.RecipeStorage.Recipes.Values)
            {
                if(recipe != null && recipe.Results != null)
                    foreach(var results in recipe.Results)
                    {
                        if(!ItemRecipe.ContainsKey(results.Type))
                            ItemRecipe.Add(results.Type, recipe);
                    }
            }
        }

        public static void SendMenu(Players.Player player, string reference)
        {
            string url = reference;

            if (reference.Contains("_"))
                url = reference.Substring(reference.IndexOf("_") + 1);

            var splitUrl = url.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
            JSONNode uiNode = LoadedMenus;

            for (int i = 0; i < splitUrl.Length; i++)
            {
                if(!uiNode.TryGetAs(splitUrl[i], out uiNode))
                    break;
            }

            if (uiNode != LoadedMenus)
                SendMenu(player, uiNode);
        }

        public static void SendMenu(Players.Player player, JSONNode json)
        {
            NetworkMenu menu = new NetworkMenu();

            json.TryGetAsOrDefault("header", out string header, "Title");
            menu.LocalStorage.SetAs("header", header);

            if (json.HasChild("width"))
                menu.Width = json.GetAs<int>("width");
            else
                menu.Width = Configuration.GetorDefault("MenuWidth", 1000);

            if (json.HasChild("height"))
                menu.Height = json.GetAs<int>("height");
            else
                menu.Height = Configuration.GetorDefault("MenuHeight", 700);

            foreach(JSONNode item in ( json.GetAs<JSONNode>("Items") ).LoopArray())
                menu.Items.Add(LoadItem(item, ref menu, player));

            NetworkMenuManager.SendServerPopup(player, menu);
        }

        //Ref menu is added for change LocalStorage -> avoid client error
        public static IItem LoadItem(JSONNode item, ref NetworkMenu menu, Players.Player player)
        {
            string itemType = item.GetAs<string>("type").Trim().ToLower();

            //PandaLogger.Log(string.Format("<color=lime>ItemType: {0}", itemType));
            IItem newItem = new EmptySpace();

            switch(itemType)
            {
                case "label":
                    {
                        newItem = new Label(GetLabelData(item));
                    }
                    break;

                case "space":
                    {
                        item.TryGetAsOrDefault<int>("height", out int height, 10);
                        newItem = new EmptySpace(height);
                    }
                    break;

                case "line":
                    {
                        UnityEngine.Color color = UnityEngine.Color.black;

                        if(item.HasChild("color"))
                            color = GetColor(item.GetAs<string>("color"));

                        item.TryGetAsOrDefault<int>("height", out int height, 4);
                        item.TryGetAsOrDefault<int>("width", out int width, -1);

                        newItem = new Line(color, height, width);
                    }
                    break;

                case "icon":
                    {
                        item.TryGetAsOrDefault<string>("name", out string icon, "missingerror");
                        newItem = new ItemIcon(icon);
                    }
                    break;

                case "jobrecipies":
                    {
                        List<Recipes.Recipe> recipes = new List<Recipes.Recipe>();

                        if (item.TryGetAs("job", out string job))
                        {
                            if (ServerManager.RecipeStorage.DefaultRecipesPerLimitType.TryGetValue(job, out var recipesDefault))
                                recipes.AddRange(recipesDefault);

                            if (ServerManager.RecipeStorage.OptionalRecipesPerLimitType.TryGetValue(job, out var recipiesOptional))
                                recipes.AddRange(recipiesOptional);

                            foreach (var recipe in recipes.OrderBy(r => r.Name))
                                PrintRecipe(menu, player, recipe);
                        }

                    }
                    break;

                case "item":
                    {
                        if (!item.HasChild("name"))
                        {
                            PandaLogger.Log("Item: Not name defined");
                            return newItem;
                        }

                        item.TryGetAs<string>("name", out string name);

                        if (!ItemTypes.IndexLookup.TryGetIndex(name, out ushort index))
                        {
                            PandaLogger.Log("Item: Not item found with name: " + name);
                            return newItem;
                        }

                        if (Localization.TryGetType(player.LastKnownLocale, index, out string localeName) && Localization.TryGetTypeUse(player.LastKnownLocale, index, out var description))
                            menu.Items.Add(new HorizontalSplit(new ItemIcon(index), new Label(new LabelData(localeName + Environment.NewLine + description, UnityEngine.Color.black)), 30, .3f));

                        if (Localization.TryGetSentence(player.LastKnownLocale, _localizationHelper.GetLocalizationKey("ItemDetails." + name), out var extendedDetail))
                            menu.Items.Add(new Label(new LabelData(extendedDetail, UnityEngine.Color.black)));
                    }
                    break;

                case "itemrecipe":
                    {
                        if (!item.HasChild("name"))
                        {
                            PandaLogger.Log("ItemRecipe: Not name defined");
                            return newItem;
                        }

                        item.TryGetAs<string>("name", out string name);

                        if (!ItemTypes.IndexLookup.TryGetIndex(name, out ushort index))
                        {
                            PandaLogger.Log("ItemRecipe: Not item found with name: " + name );
                            return newItem;
                        }

                        if (!ItemRecipe.TryGetValue(index, out var recipe))
                        {
                            PandaLogger.Log("ItemRecipe: Not recipe found for: " + name );
                            return newItem;
                        }

                        if (Localization.TryGetType(player.LastKnownLocale, index, out string localeName))
                        {
                            menu.Items.Add(new Label(localeName + ":"));
                        }
                        else
                            menu.Items.Add(new Label(name + ":"));

                        PrintRecipe(menu, player, recipe);
                    }
                    break;

                case "dropdown":
                    {
                        string id;

                        if(item.HasChild("id"))
                        {
                            id = item.GetAs<string>("id");
                        }
                        else
                        {
                            id = "dropdown";
                            PandaLogger.Log("Dropdown without ID defined, default: dropdown");
                        }

                        List<string> options = new List<string>();

                        if(item.HasChild("options"))
                        {
                            JSONNode optionsj = item.GetAs<JSONNode>("options");

                            foreach(var option in optionsj.LoopArray())
                                options.Add(option.GetAs<string>());
                        }
                        else
                        {
                            options.Add("No options available");
                            PandaLogger.Log(string.Format("dropdown {0} without options", id));
                        }

                        item.TryGetAsOrDefault<int>("height", out int height, 30);
                        item.TryGetAsOrDefault<int>("marginHorizontal", out int marginHorizontal, 4);
                        item.TryGetAsOrDefault<int>("marginVertical", out int marginVertical, 2);

                        // if label dropdown else dropdownNOLABEL
                        if(item.TryGetChild("label", out JSONNode labelj))
                        {
                            LabelData label = GetLabelData(labelj);
                            newItem = new DropDown(label.text, id, options);
                        }
                        else
                        {
                            newItem = new DropDownNoLabel(id, options, height);
                        }

                        menu.LocalStorage.SetAs(id, 0);
                    }
                    break;

                case "toggle":
                    {
                        string id;

                        if(item.HasChild("id"))
                        {
                            id = item.GetAs<string>("id");
                        }
                        else
                        {
                            id = "toggle";
                            PandaLogger.Log("Toggle without ID defined, default: toggle");
                        }

                        item.TryGetAsOrDefault<int>("height", out int height, 25);
                        item.TryGetAsOrDefault<int>("toggleSize", out int toggleSize, 20);

                        // if label toggle else togglenolabel
                        if(item.TryGetChild("label", out JSONNode labelj))
                        {
                            LabelData label = GetLabelData(labelj);
                            newItem = new Toggle(label, id, height, toggleSize);
                        }
                        else
                        {
                            newItem = new ToggleNoLabel(id, toggleSize);
                        }

                        menu.LocalStorage.SetAs(id, false);
                    }
                    break;

                case "button":
                    {
                        string id;

                        if(item.HasChild("id"))
                        {
                            id = item.GetAs<string>("id");
                        }
                        else
                        {
                            id = "button";
                            PandaLogger.Log("Button without ID defined, default: button");
                        }

                        item.TryGetAsOrDefault<int>("width", out int width, -1);
                        item.TryGetAsOrDefault<int>("height", out int height, 25);

                        if(item.TryGetChild("label", out JSONNode labelj))
                        {
                            LabelData label = GetLabelData(labelj);
                            newItem = new ButtonCallback(id, label, width, height);
                        }
                        else
                        {
                            PandaLogger.Log(string.Format("Button {0} without label", id));
                            newItem = new ButtonCallback(id, new LabelData("Key label not defined"), width, height);
                        }
                    }
                    break;

                case "link":
                    {
                        string url;

                        if(item.HasChild("url"))
                        {
                            url = GameLoader.NAMESPACE + ".link_" + item.GetAs<string>("url");
                        }
                        else
                        {
                            PandaLogger.Log("Link without URL defined");
                            return new EmptySpace();
                        }

                        item.TryGetAsOrDefault<int>("width", out int width, -1);
                        item.TryGetAsOrDefault<int>("height", out int height, 25);

                        if(item.TryGetChild("label", out JSONNode labelj))
                        {
                            LabelData label = GetLabelData(labelj);
                            newItem = new ButtonCallback(url, label, width, height);
                        }
                        else
                        {
                            PandaLogger.Log(string.Format("Link {0} without label", url));
                            newItem = new ButtonCallback(url, new LabelData("Key label not defined"), width, height);
                        }
                    }
                    break;

                case "table":
                    {
                        item.TryGetAsOrDefault<int>("row_height", out int height, 30);

                        foreach(JSONNode rows in ( item.GetAs<JSONNode>("rows") ).LoopArray())
                        {
                            List<TupleStruct<IItem, int>> items = new List<TupleStruct<IItem, int>>();
                            var width = menu.Width / rows.ChildCount;

                            foreach(JSONNode row in rows.LoopArray())
                            {
                                if (row.TryGetAs("row_width", out int setWidth))
                                    items.Add(TupleStruct.Create<IItem, int>(LoadItem(row, ref menu, player), setWidth));
                                else
                                    items.Add(TupleStruct.Create<IItem, int>(LoadItem(row, ref menu, player), width));
                            }

                            menu.Items.Add(new HorizontalRow(items, height));
                        }

                    }
                    break;


                default:
                    {
                        PandaLogger.Log(string.Format("It doesn't exist an item of type: {0}", itemType));
                        newItem = new EmptySpace();
                    }
                    break;
            }

            if(item.HasChild("position"))
            {
                HorizontalSplit horizontalSplit = new HorizontalSplit(new EmptySpace(), newItem, 0, 0, HorizontalSplit.ESplitType.Relative, 0, item.GetAs<int>("position"));

                return horizontalSplit;
            }
            else
                return newItem;
        }

        private static void PrintRecipe(NetworkMenu menu, Players.Player player, Recipes.Recipe recipe)
        {
            menu.Items.Add(new Label(new LabelData(recipe.Name, UnityEngine.Color.black, UnityEngine.TextAnchor.MiddleCenter, 30, LabelData.ELocalizationType.Type)));
            menu.Items.Add(new Label(new LabelData(_localizationHelper.GetLocalizationKey("Requirements"), UnityEngine.Color.black, UnityEngine.TextAnchor.MiddleLeft, 24)));

            List<TupleStruct<IItem, int>> headerItems = new List<TupleStruct<IItem, int>>();

            headerItems.Add(TupleStruct.Create<IItem, int>(new Label(new LabelData("", UnityEngine.Color.black)), 70));
            headerItems.Add(TupleStruct.Create<IItem, int>(new Label(new LabelData(_localizationHelper.GetLocalizationKey("Item"), UnityEngine.Color.black)), 150));
            headerItems.Add(TupleStruct.Create<IItem, int>(new Label(new LabelData(_localizationHelper.GetLocalizationKey("Amount"), UnityEngine.Color.black)), 100));

            menu.Items.Add(new HorizontalRow(headerItems));

            foreach (var req in recipe.Requirements)
            {
                if (req == null)
                    continue;

                string reqName = ItemTypes.IndexLookup.GetName(req.Type);

                ItemIcon icon = new ItemIcon(reqName);
                if (Localization.TryGetType(player.LastKnownLocale, req.Type, out string localeReqName))
                    reqName = localeReqName;

                Label labelName = new Label(new LabelData(reqName, UnityEngine.Color.black));
                Label labelAmount = new Label(new LabelData(req.Amount.ToString(), UnityEngine.Color.black));

                List<TupleStruct<IItem, int>> items = new List<TupleStruct<IItem, int>>();
                items.Add(TupleStruct.Create<IItem, int>(icon, 70));
                items.Add(TupleStruct.Create<IItem, int>(labelName, 150));
                items.Add(TupleStruct.Create<IItem, int>(labelAmount, 100));

                menu.Items.Add(new HorizontalRow(items));
            }

            menu.Items.Add(new Label(new LabelData(_localizationHelper.GetLocalizationKey("Results"), UnityEngine.Color.black, UnityEngine.TextAnchor.MiddleLeft, 24)));

            headerItems = new List<TupleStruct<IItem, int>>();
            headerItems.Add(TupleStruct.Create<IItem, int>(new Label(new LabelData("", UnityEngine.Color.black)), 70));
            headerItems.Add(TupleStruct.Create<IItem, int>(new Label(new LabelData(_localizationHelper.GetLocalizationKey("Item"), UnityEngine.Color.black)), 150));
            headerItems.Add(TupleStruct.Create<IItem, int>(new Label(new LabelData(_localizationHelper.GetLocalizationKey("Amount"), UnityEngine.Color.black)), 100));
            headerItems.Add(TupleStruct.Create<IItem, int>(new Label(new LabelData(_localizationHelper.GetLocalizationKey("Chance"), UnityEngine.Color.black)), 100));

            menu.Items.Add(new HorizontalRow(headerItems));

            foreach (var req in recipe.Results)
            {
                string reqName = ItemTypes.IndexLookup.GetName(req.Type);

                ItemIcon icon = new ItemIcon(reqName);
                if (Localization.TryGetType(player.LastKnownLocale, req.Type, out string localeReqName))
                    reqName = localeReqName;

                Label labelName = new Label(new LabelData(reqName, UnityEngine.Color.black));
                Label labelAmount = new Label(new LabelData(req.Amount.ToString(), UnityEngine.Color.black));
                Label chance = new Label(new LabelData(req.chance * 100 + "%", UnityEngine.Color.black));
                List<TupleStruct<IItem, int>> items = new List<TupleStruct<IItem, int>>();
                items.Add(TupleStruct.Create<IItem, int>(icon, 70));
                items.Add(TupleStruct.Create<IItem, int>(labelName, 150));
                items.Add(TupleStruct.Create<IItem, int>(labelAmount, 100));
                items.Add(TupleStruct.Create<IItem, int>(chance, 100));

                menu.Items.Add(new HorizontalRow(items));

                if (Localization.TryGetTypeUse(player.LastKnownLocale, req.Type, out var description))
                    menu.Items.Add(new Label(new LabelData(description, UnityEngine.Color.black)));

                if (Localization.TryGetSentence(player.LastKnownLocale, _localizationHelper.GetLocalizationKey("ItemDetails." + ItemId.GetItemId(req.Type).Name), out var extendedDetail))
                    menu.Items.Add(new Label(new LabelData(extendedDetail, UnityEngine.Color.black)));
            }

            menu.Items.Add(new Line(UnityEngine.Color.black, 1));
        }

        public static LabelData GetLabelData(JSONNode json)
        {
            json.TryGetAsOrDefault("text", out string text, "Text key not found");

            UnityEngine.Color color = UnityEngine.Color.black;
            UnityEngine.TextAnchor alignement = UnityEngine.TextAnchor.MiddleLeft;

            if(json.HasChild("color"))
                color = GetColor(json.GetAs<string>("color"));

            if (json.TryGetAs("alignement", out string alignementStr))
                Enum.TryParse(alignementStr, true, out alignement);

            json.TryGetAsOrDefault("fontsize", out int fontSize, 18);

            LabelData.ELocalizationType localizationType = LabelData.ELocalizationType.Sentence;

            if (json.TryGetAs("localizationType", out string localizationString))
                Enum.TryParse(localizationString, true, out localizationType);

            return new LabelData(text, color, alignement, fontSize, localizationType);
        }

        public static UnityEngine.Color GetColor(string color)
        {
            switch(color.Trim().ToLower())
            {
                case "cyan":
                return UnityEngine.Color.cyan;

                case "green":
                return UnityEngine.Color.green;

                case "red":
                return UnityEngine.Color.red;

                case "black":
                return UnityEngine.Color.black;

                case "yellow":
                return UnityEngine.Color.yellow;

                case "blue":
                return UnityEngine.Color.blue;

                case "magenta":
                return UnityEngine.Color.magenta;

                case "gray":
                return UnityEngine.Color.gray;

                case "white":
                return UnityEngine.Color.white;

                case "clear":
                return UnityEngine.Color.clear;

                case "grey":
                return UnityEngine.Color.grey;

                default:
                return UnityEngine.Color.black;
            }
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnPlayerPushedNetworkUIButton, GameLoader.NAMESPACE + ".UIManager.PressLink")]
        public static void PressButton(ButtonPressCallbackData data)
        {
            if(data.ButtonIdentifier.StartsWith(GameLoader.NAMESPACE + ".link_"))
            {
                string url = data.ButtonIdentifier.Substring(data.ButtonIdentifier.IndexOf("_") + 1);

                SendMenu(data.Player, url);
            }
        }
    }
}
