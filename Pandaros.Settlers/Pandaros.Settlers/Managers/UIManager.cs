using System;
using System.IO;
using System.Collections.Generic;

using Pipliz;
using Pipliz.JSON;
using NetworkUI;
using NetworkUI.Items;
using Pandaros.Settlers.Help;

namespace Pandaros.Settlers.Managers
{
    [ModLoader.ModManager]
    public static class UIManager
    {
        public static JSONNode LoadedMenus { get; private set; }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnAssemblyLoaded, GameLoader.NAMESPACE + ".Managers.OnAssemblyLoaded")]
        [ModLoader.ModCallbackDependsOn(GameLoader.NAMESPACE + ".OnAssemblyLoaded")]
        public static void OnAssemblyLoaded(string path)
        {
            if (GameLoader.ModInfo.TryGetAs(GameLoader.NAMESPACE + ".jsonFiles", out JSONNode jsonFilles))
            {
                foreach (var jsonNode in jsonFilles.LoopArray())
                {
                    if (jsonNode.TryGetAs("fileType", out string jsonFileType) && jsonFileType == GameLoader.NAMESPACE + ".MenuFile" && jsonNode.TryGetAs("relativePath", out string menuFilePath))
                    {
                        var newMenu = JSON.Deserialize(GameLoader.MOD_FOLDER + menuFilePath);

                        if (LoadedMenus == null)
                            LoadedMenus = newMenu;
                        else
                        {
                            MergeJsons(LoadedMenus, newMenu);
                        }

                        PandaLogger.Log("Loaded Menu: {0}", menuFilePath);
                    }
                }
            }
            else
                PandaLogger.Log(ChatColor.yellow, "Missing json files node from modinfo.json. Unable to load UI files.");
        }

        private static void MergeJsons(JSONNode oldNode, JSONNode newNode)
        {
            if (newNode.NodeType != NodeType.Array && oldNode.NodeType != NodeType.Array)
            {
                foreach(var node in newNode.LoopObject())
                {
                    if (oldNode.TryGetChild(node.Key, out JSONNode existingChild))
                        MergeJsons(existingChild, node.Value);
                    else
                        oldNode.SetAs(node.Key, node.Value);
                }
            }
        }

        public static void SendMenu(Players.Player player, string reference)
        {
            string url = reference;

            if (reference.Contains("_"))
                url = reference.Substring(reference.IndexOf("_") + 1);

            var splitUrl = url.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
            var uiNode = default(JSONNode);

            foreach (var entry in splitUrl)
            {
                if (!LoadedMenus.TryGetAs(entry, out uiNode))
                    break;
            }

            if (uiNode == default(JSONNode))
                PandaLogger.Log(ChatColor.red, "Unable to find menu {0}", reference);
            else
                SendMenu(player, uiNode);
        }

        public static void SendMenu(Players.Player player, JSONNode json)
        {
            NetworkMenu menu = new NetworkMenu();

            json.TryGetAsOrDefault("header", out string header, "Title");
            menu.LocalStorage.SetAs("header", header);

            if(json.HasChild("width"))
                menu.Width = json.GetAs<int>("width");

            if(json.HasChild("height"))
                menu.Height = json.GetAs<int>("height");


            foreach(JSONNode item in ( json.GetAs<JSONNode>("Items") ).LoopArray())
                menu.Items.Add(LoadItem(item, ref menu));

            NetworkMenuManager.SendServerPopup(player, menu);
        }

        //Ref menu is added for change LocalStorage -> avoid client error
        public static IItem LoadItem(JSONNode item, ref NetworkMenu menu)
        {
            string itemType = item.GetAs<string>("type").Trim().ToLower();

            //Log.Write(string.Format("<color=lime>ItemType: {0}</color>", itemType));
            IItem newItem = new EmptySpace();

            switch(itemType)
            {
                case "label":
                {
                    LabelData lbd = GetLabelData(item);

                    newItem = new Label(lbd);
                }
                break;

                case "space":
                {
                    item.TryGetAsOrDefault<int>("height", out int height, 4);

                    newItem = new EmptySpace(height);
                }
                break;

                case "line":
                {
                    UnityEngine.Color color = UnityEngine.Color.white;

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
                        Log.Write("<color=red>Dropdown without ID defined, default: dropdown</color>");
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
                        Log.Write(string.Format("<color=red>dropdown {0} without options</color>", id));
                    }

                    item.TryGetAsOrDefault<int>("height", out int height, 30);
                    item.TryGetAsOrDefault<int>("marginHorizontal", out int marginHorizontal, 4);
                    item.TryGetAsOrDefault<int>("marginVertical", out int marginVertical, 2);

                    // if label dropdown else dropdownNOLABEL
                    if(item.TryGetChild("label", out JSONNode labelj))
                    {
                        LabelData label = GetLabelData(labelj);
                        //newItem = new DropDown(label.text, id, options, height, marginHorizontal, marginVertical);
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
                        Log.Write("<color=red>Toggle without ID defined, default: toggle</color>");
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
                        Log.Write("<color=red>Button without ID defined, default: button</color>");
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
                        Log.Write(string.Format("<color=red>Button {0} without label</color>", id));
                        newItem = new ButtonCallback(id, new LabelData("Key label not defined"), width, height);
                    }
                }
                break;

                case "link":
                {
                    string url;

                    if(item.HasChild("url"))
                    {
                        url = "ui.link_" + item.GetAs<string>("url");
                    }
                    else
                    {
                        Log.Write("<color=red>Link without URL defined</color>");
                        return new EmptySpace();
                    }

                    //Log.Write("<color=red>"+ url + "</color>");

                    item.TryGetAsOrDefault<int>("width", out int width, -1);
                    item.TryGetAsOrDefault<int>("height", out int height, 25);

                    if(item.TryGetChild("label", out JSONNode labelj))
                    {
                        LabelData label = GetLabelData(labelj);
                        newItem = new ButtonCallback(url, label, width, height);
                    }
                    else
                    {
                        Log.Write(string.Format("<color=red>Link {0} without label</color>", url));
                        newItem = new ButtonCallback(url, new LabelData("Key label not defined"), width, height);
                    }
                }
                break;

                case "table":
                {
                    item.TryGetAsOrDefault<int>("row_height", out int height, 30);
                    item.TryGetAsOrDefault<int>("col_width", out int width, 100);

                    foreach(JSONNode rows in ( item.GetAs<JSONNode>("rows") ).LoopArray())
                    {
                        List<IItem> items = new List<IItem>();

                        foreach(JSONNode row in rows.LoopArray())
                        {
                            items.Add(LoadItem(row, ref menu));
                        }

                        if(item.HasChild("position"))
                            menu.Items.Add(new HorizontalSplit(new EmptySpace(), new HorizontalGrid(items, width, height), 0, 0, HorizontalSplit.ESplitType.Relative, 0, item.GetAs<int>("position")));
                        else
                            menu.Items.Add(new HorizontalGrid(items, width, height));
                    }

                }
                break;

                default:
                {
                    Log.Write(string.Format("<color=red>It doesn't exist an item of type: {0}</color>", itemType));
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

        public static LabelData GetLabelData(JSONNode json)
        {
            json.TryGetAsOrDefault<string>("text", out string text, "Text key not found");

            UnityEngine.Color color = UnityEngine.Color.white;
            UnityEngine.TextAnchor alignement = UnityEngine.TextAnchor.MiddleLeft;

            if(json.HasChild("color"))
                color = GetColor(json.GetAs<string>("color"));

            if(json.HasChild("alignement"))
                alignement = GetAlignement(json.GetAs<string>("alignement"));

            json.TryGetAsOrDefault<int>("fontsize", out int fontSize, 18);

            return new LabelData(text, color, alignement, fontSize);
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
                return UnityEngine.Color.white;
            }
        }

        public static UnityEngine.TextAnchor GetAlignement(string alignement)
        {
            switch(alignement.Trim().ToLower())
            {
                case "left":
                default:
                return UnityEngine.TextAnchor.MiddleLeft;
                    
                case "center":
                return UnityEngine.TextAnchor.MiddleCenter;
                    
                case "right":
                return UnityEngine.TextAnchor.MiddleRight;
            }
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnPlayerPushedNetworkUIButton, "Khanx.UIManager.PressLink")]
        public static void PressButton(ButtonPressCallbackData data)
        {
            if(data.ButtonIdentifier.StartsWith("ui.link_"))
            {
                string url = data.ButtonIdentifier.Substring(data.ButtonIdentifier.IndexOf("_") + 1);

                SendMenu(data.Player, url);
            }
        }
    }
}
