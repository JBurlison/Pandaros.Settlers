﻿using NetworkUI;
using NetworkUI.Items;
using Pandaros.API;
using Pandaros.API.Items;
using Pandaros.API.localization;
using Pandaros.API.Models;
using Pandaros.API.Research;
using Pandaros.Settlers.NBT;
using Pipliz;
using Pipliz.JSON;
using Recipes;
using Science;
using Shared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static AreaJobTracker;

namespace Pandaros.Settlers.Jobs.Construction
{
    public enum SchematicClickType
    {
        Build,
        Archetect
    }

    public class SchematicTool : CSType
    {
        public static string NAME = GameLoader.NAMESPACE + ".SchematicTool";
        public override string name => NAME;
        public override string icon => GameLoader.ICON_PATH + "Schematics.png";
        public override bool? isPlaceable => false;
        public override int? maxStackSize => 1;
        public override List<string> categories { get; set; } = new List<string>()
        {
            "essential",
            "aaa"
        };

        public override StaticItems.StaticItem StaticItemSettings => new StaticItems.StaticItem()
        {
            Name = GameLoader.NAMESPACE + ".SchematicTool",
            RequiredScience = SettlersBuiltIn.Research.ARCHITECT1
        };
    }

    public class SchematicToolResearch : IPandaResearch
    {
        public Dictionary<int, List<InventoryItem>> RequiredItems => new Dictionary<int, List<InventoryItem>>()
        {
            {
                0,
                new List<InventoryItem>()
                {
                    new InventoryItem(ColonyBuiltIn.ItemTypes.SCIENCEBAGCOLONY.Id),
                    new InventoryItem(ColonyBuiltIn.ItemTypes.SCIENCEBAGBASIC.Id, 3),
                    new InventoryItem(ColonyBuiltIn.ItemTypes.SCIENCEBAGADVANCED.Id)
                }
            }
        };
        public Dictionary<int, List<IResearchableCondition>> Conditions => new Dictionary<int, List<IResearchableCondition>>()
        {
            {
                0,
                new List<IResearchableCondition>()
                {
                    new HappinessCondition() { Threshold = 80 },
                    new ColonistCountCondition() { Threshold = 150 }
                }
            }
        };
        public int NumberOfLevels => 1;
        public float BaseValue => 0.05f;
        public Dictionary<int, List<string>> Dependancies => new Dictionary<int, List<string>>()
        {
            {
                0,
                new List<string>()
                {
                    ColonyBuiltIn.Research.CONSTRUCTIONBUILDER,
                    ColonyBuiltIn.Research.SCIENCEBAGADVANCED,
                    ColonyBuiltIn.Research.SCIENCEBAGCOLONY
                }
            }
        };

        public int BaseIterationCount => 300;
        public bool AddLevelToName => false;
        public string name => GameLoader.NAMESPACE + ".Architect";
        public string IconDirectory => GameLoader.ICON_PATH;

        public Dictionary<int, List<RecipeUnlock>> Unlocks => null;

        public void OnRegister()
        {

        }

        public void ResearchComplete(object sender, ResearchCompleteEventArgs e)
        {
            foreach (var p in e.Manager.Colony.Owners)
                StaticItems.AddStaticItemToStockpile(p);
        }
    }

    public class SchematicToolRecipe : ICSRecipe
    {
        public List<RecipeItem> requires => new List<RecipeItem>()
        {
            new RecipeItem(ColonyBuiltIn.ItemTypes.PLANKS.Id),
            new RecipeItem(ColonyBuiltIn.ItemTypes.SCIENCEBAGCOLONY.Id),
            new RecipeItem(ColonyBuiltIn.ItemTypes.SCIENCEBAGADVANCED.Id)
        };

        public List<RecipeResult> results => new List<RecipeResult>()
        {
            new RecipeResult(GameLoader.NAMESPACE + ".SchematicTool")
        };

        public CraftPriority defaultPriority => CraftPriority.Medium;

        public bool isOptional => true;

        public int defaultLimit => 1;

        public string Job => ColonyBuiltIn.NpcTypes.CRAFTER;

        public string name => GameLoader.NAMESPACE + ".SchematicTool";
    }

    [ModLoader.ModManager]
    public class SchematicMenu
    {
        private static readonly List<Schematic.Rotation> _rotation = new List<Schematic.Rotation>()
        {
            Schematic.Rotation.Front,
            Schematic.Rotation.Right,
            Schematic.Rotation.Back,
            Schematic.Rotation.Left
        };

        private static readonly string Selected_Schematic = GameLoader.NAMESPACE + ".SelectedSchematic";
        static readonly LocalizationHelper _localizationHelper = new LocalizationHelper(GameLoader.NAMESPACE, "buildertool");
        private static Dictionary<Players.Player, Tuple<SchematicClickType, string, Schematic.Rotation>> _awaitingClick = new Dictionary<Players.Player, Tuple<SchematicClickType, string, Schematic.Rotation>>();

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnPlayerClicked, GameLoader.NAMESPACE + ".Jobs.Construction.SchematicMenu.OpenMenu")]
        public static void OpenMenu(Players.Player player, PlayerClickedData playerClickData)
        {
            if (ItemTypes.IndexLookup.TryGetIndex(SchematicTool.NAME, out var schematicItem) &&
                playerClickData.TypeSelected == schematicItem)
            {
                if (player.ActiveColony == null)
                {
                    PandaChat.Send(player, _localizationHelper, "ErrorOpening", ChatColor.red);
                    return;
                }
                
                if (!_awaitingClick.ContainsKey(player))
                {
                    SendMainMenu(player);
                }
                else
                {
                    var tuple = _awaitingClick[player];
                    _awaitingClick.Remove(player);

                    switch (tuple.Item1)
                    {
                        case SchematicClickType.Build:
                            Vector3Int location = playerClickData.GetVoxelHit().BlockHit.Add(0, 1, 0);
                            var args = new JSONNode();
                            args.SetAs("constructionType", GameLoader.NAMESPACE + ".SchematicBuilder");
                            args.SetAs(SchematicBuilderLoader.NAME + ".SchematicName", tuple.Item2);
                            args.SetAs(SchematicBuilderLoader.NAME + ".Rotation", tuple.Item3);

                            if (SchematicReader.TryGetSchematic(tuple.Item2, player.ActiveColony.ColonyID, location, out var schematic))
                            {
                                if (tuple.Item3 >= Schematic.Rotation.Right)
                                    schematic.Rotate();

                                if (tuple.Item3 >= Schematic.Rotation.Back)
                                    schematic.Rotate();

                                if (tuple.Item3 >= Schematic.Rotation.Left)
                                    schematic.Rotate();

                                var maxSize = location.Add(schematic.XMax - 1, schematic.YMax - 1, schematic.ZMax - 1);
                                AreaJobTracker.CreateNewAreaJob("pipliz.constructionarea", args, player.ActiveColony, location, maxSize);
                                AreaJobTracker.SendData(player);
                            }

                            break;

                        case SchematicClickType.Archetect:

                            break;
                    }
                }
            }
        }

        private static void SendMainMenu(Players.Player player)
        {
            NetworkMenu menu = new NetworkMenu();
            menu.LocalStorage.SetAs("header", "Schematic Menu");
            List<FileInfo> options = SchematicReader.GetSchematics(player);

            menu.Items.Add(new DropDown(new LabelData(_localizationHelper.GetLocalizationKey("Schematic"), UnityEngine.Color.black), Selected_Schematic, options.Select(fi => fi.Name.Replace(".schematic", "")).ToList()));
            menu.Items.Add(new ButtonCallback(GameLoader.NAMESPACE + ".ShowBuildDetails", new LabelData(_localizationHelper.GetLocalizationKey("Details"), UnityEngine.Color.black, UnityEngine.TextAnchor.MiddleCenter)));
            menu.LocalStorage.SetAs(Selected_Schematic, 0);
            menu.Items.Add(new ButtonCallback(GameLoader.NAMESPACE + ".SetScemanticName", new LabelData(_localizationHelper.GetLocalizationKey("Save"), UnityEngine.Color.black, UnityEngine.TextAnchor.MiddleCenter)));

            NetworkMenuManager.SendServerPopup(player, menu);
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnSendAreaHighlights, GameLoader.NAMESPACE + ".Jobs.Construction.SchematicMenu.OnSendAreaHighlights")]
        static void OnSendAreaHighlights(Players.Player goal, List<AreaHighlight> list, List<ushort> showWhileHoldingTypes)
        {
            showWhileHoldingTypes.Add(ColonyBuiltIn.ItemTypes.BED.Id);

            if (ItemTypes.IndexLookup.StringLookupTable.TryGetItem(SchematicTool.NAME, out var item))
                showWhileHoldingTypes.Add(item.ItemIndex);
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnPlayerPushedNetworkUIButton, GameLoader.NAMESPACE + ".Jobs.Construction.SchematicMenu.PressButton")]
        public static void PressButton(ButtonPressCallbackData data)
        {
            switch (data.ButtonIdentifier)
            {
                case GameLoader.NAMESPACE + ".SetScemanticName":
                    NetworkMenu saveMenu = new NetworkMenu();
                    saveMenu.LocalStorage.SetAs("header", _localizationHelper.LocalizeOrDefault("SaveSchematic", data.Player));
                    saveMenu.Width = 600;
                    saveMenu.Height = 300;
                    saveMenu.ForceClosePopups = true;
                    saveMenu.Items.Add(new Label(new LabelData(_localizationHelper.GetLocalizationKey("SaveInstructions"), UnityEngine.Color.black)));
                    saveMenu.Items.Add(new InputField("Construction.SetArchitectArea"));
                    saveMenu.Items.Add(new ButtonCallback(GameLoader.NAMESPACE + ".SetArchitectArea", new LabelData(_localizationHelper.GetLocalizationKey("Start"), UnityEngine.Color.black)));

                    NetworkMenuManager.SendServerPopup(data.Player, saveMenu);
                    break;

                case GameLoader.NAMESPACE + ".SetArchitectArea":
                    NetworkMenuManager.CloseServerPopup(data.Player);
                    if (data.Storage.TryGetAs("Construction.SetArchitectArea", out string schematicName))
                    {
                        var colonySaves = GameLoader.Schematic_SAVE_LOC + $"\\{data.Player.ActiveColony.ColonyID}\\";

                        if (!Directory.Exists(colonySaves))
                            Directory.CreateDirectory(colonySaves);

                        var schematicFile = Path.Combine(colonySaves, schematicName + ".schematic");

                        if (File.Exists(schematicFile))
                            File.Delete(schematicFile);

                        var metaDataSave = Path.Combine(GameLoader.Schematic_SAVE_LOC, schematicName + ".schematic.metadata.json");

                        if (File.Exists(metaDataSave))
                            File.Delete(metaDataSave);

                        AreaJobTracker.StartCommandToolSelection(data.Player, new CommandToolTypeData()
                        {
                            AreaType = "pipliz.constructionarea",
                            LocaleEntry = _localizationHelper.LocalizeOrDefault("Architect", data.Player),
                            JSONData = new JSONNode().SetAs(ArchitectLoader.NAME + ".ArchitectSchematicName", schematicName).SetAs("constructionType", GameLoader.NAMESPACE + ".Architect"),
                            OneAreaOnly = true,
                            Maximum3DBlockCount = int.MaxValue,
                            Maximum2DBlockCount = int.MaxValue,
                            MaximumHeight = int.MaxValue,
                            MinimumHeight = 1,
                            Minimum2DBlockCount = 1,
                            Minimum3DBlockCount = 1
                        });
                    }

                    break;

                case GameLoader.NAMESPACE + ".ShowMainMenu":
                    SendMainMenu(data.Player);
                    break;

                case GameLoader.NAMESPACE + ".ShowBuildDetails":
                    List<FileInfo> options = SchematicReader.GetSchematics(data.Player);
                    var index = data.Storage.GetAs<int>(Selected_Schematic);

                    if (options.Count > index)
                    {
                        var selectedSchematic = options[index];

                        if (SchematicReader.TryGetSchematicMetadata(selectedSchematic.Name, data.Player.ActiveColony.ColonyID, out SchematicMetadata schematicMetadata))
                        {
                            if (schematicMetadata.Blocks.Count == 1 && schematicMetadata.Blocks.ContainsKey(ColonyBuiltIn.ItemTypes.AIR.Id))
                                PandaChat.Send(data.Player, _localizationHelper, "invlaidSchematic", ChatColor.red);
                            {
                                NetworkMenu menu = new NetworkMenu();
                                menu.Width = 800;
                                menu.Height = 600;
                                menu.LocalStorage.SetAs("header", selectedSchematic.Name.Replace(".schematic","") + " " + _localizationHelper.LocalizeOrDefault("Details", data.Player));

                                menu.Items.Add(new Label(new LabelData(_localizationHelper.LocalizeOrDefault("Height", data.Player) + ": " + schematicMetadata.MaxY, UnityEngine.Color.black)));
                                menu.Items.Add(new Label(new LabelData(_localizationHelper.LocalizeOrDefault("Width", data.Player) + ": " + schematicMetadata.MaxZ, UnityEngine.Color.black)));
                                menu.Items.Add(new Label(new LabelData(_localizationHelper.LocalizeOrDefault("Length", data.Player) + ": " + schematicMetadata.MaxX, UnityEngine.Color.black)));
                                menu.LocalStorage.SetAs(Selected_Schematic, selectedSchematic.Name);

                                List<ValueTuple<IItem, int>> headerItems = new List<ValueTuple<IItem, int>>();
                                headerItems.Add(ValueTuple.Create<IItem, int>(new Label(new LabelData("  ", UnityEngine.Color.black)), 200));
                                headerItems.Add(ValueTuple.Create<IItem, int>(new Label(new LabelData(_localizationHelper.LocalizeOrDefault("Item", data.Player), UnityEngine.Color.black)), 200));
                                headerItems.Add(ValueTuple.Create<IItem, int>(new Label(new LabelData(_localizationHelper.LocalizeOrDefault("Required", data.Player), UnityEngine.Color.black)), 200));
                                headerItems.Add(ValueTuple.Create<IItem, int>(new Label(new LabelData(_localizationHelper.LocalizeOrDefault("InStockpile", data.Player), UnityEngine.Color.black)), 200));
                                menu.Items.Add(new HorizontalRow(headerItems));

                                foreach (var kvp in schematicMetadata.Blocks)
                                {
                                    try
                                    {
                                        if (ItemTypes.TryGetType(kvp.Key, out ItemTypes.ItemType item))
                                        {
                                            var stockpileCount = 0;
                                            data.Player.ActiveColony.Stockpile.Items.TryGetValue(item.ItemIndex, out stockpileCount);

                                            List<ValueTuple<IItem, int>> items = new List<ValueTuple<IItem, int>>();
                                            items.Add(ValueTuple.Create<IItem, int>(new ItemIcon(kvp.Key), 200));
                                            items.Add(ValueTuple.Create<IItem, int>(new Label(new LabelData(item.Name, UnityEngine.Color.black, UnityEngine.TextAnchor.MiddleLeft, 18, LabelData.ELocalizationType.Type)), 200));
                                            items.Add(ValueTuple.Create<IItem, int>(new Label(new LabelData(" x " + kvp.Value.Count, UnityEngine.Color.black)), 200));
                                            items.Add(ValueTuple.Create<IItem, int>(new Label(new LabelData(" x " + stockpileCount, UnityEngine.Color.black)), 200));
                                            menu.Items.Add(new HorizontalRow(items));
                                        }
                                        else
                                            SettlersLogger.Log(ChatColor.orange, "Unknown item for schematic: {0}", kvp.Key);
                                    }
                                    catch (Exception ex)
                                    {
                                        SettlersLogger.LogError(ex);
                                    }
                                }

                                menu.Items.Add(new DropDown(new LabelData(_localizationHelper.GetLocalizationKey("Rotation"), UnityEngine.Color.black), Selected_Schematic + ".Rotation", _rotation.Select(r => r.ToString()).ToList()));
                                menu.Items.Add(new HorizontalSplit(new ButtonCallback(GameLoader.NAMESPACE + ".ShowMainMenu", new LabelData("Back", UnityEngine.Color.black, UnityEngine.TextAnchor.MiddleCenter)),
                                                                   new ButtonCallback(GameLoader.NAMESPACE + ".SetBuildArea", new LabelData("Build", UnityEngine.Color.black, UnityEngine.TextAnchor.MiddleCenter))));
                                menu.LocalStorage.SetAs(Selected_Schematic + ".Rotation", 0);

                                NetworkMenuManager.SendServerPopup(data.Player, menu);
                            }
                        }
                    }

                    break;

                case GameLoader.NAMESPACE + ".SetBuildArea":
                    var scem = data.Storage.GetAs<string>(Selected_Schematic);
                    var rotation = data.Storage.GetAs<int>(Selected_Schematic + ".Rotation");

                    SettlersLogger.Log("Schematic: {0}", scem);

                    if (SchematicReader.TryGetSchematicMetadata(scem, data.Player.ActiveColony.ColonyID, out SchematicMetadata metadata))
                    {
                        if (metadata.Blocks.Count == 1 && metadata.Blocks.ContainsKey(ColonyBuiltIn.ItemTypes.AIR.Id))
                            PandaChat.Send(data.Player, _localizationHelper, "invlaidSchematic", ChatColor.red);
                        {
                            _awaitingClick[data.Player] = Tuple.Create(SchematicClickType.Build, scem, _rotation[rotation]);
                            PandaChat.Send(data.Player, _localizationHelper, "instructions");
                            NetworkMenuManager.CloseServerPopup(data.Player);
                        }
                    }
        
                    break;
            }
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnSendAreaHighlights, GameLoader.NAMESPACE + ".Jobs.Construction.SchematicMenu.AreaHighlighted")]
        public static void AreaHighlighted(Players.Player player, List<AreaJobTracker.AreaHighlight> list, List<ushort> showWhileHoldingTypes)
        {
            if (ItemTypes.IndexLookup.TryGetIndex(SchematicTool.NAME, out var schematicItem))
                showWhileHoldingTypes.Add(schematicItem);
        }
    }
}
