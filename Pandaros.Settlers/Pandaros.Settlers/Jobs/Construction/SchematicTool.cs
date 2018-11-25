using BlockTypes;
using NetworkUI;
using NetworkUI.Items;
using Pandaros.Settlers.Buildings.NBT;
using Pandaros.Settlers.Items;
using Pandaros.Settlers.Research;
using Pipliz;
using Pipliz.JSON;
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

    [ModLoader.ModManager]
    public class SchematicMenu
    {
        private static readonly string Selected_Schematic = GameLoader.NAMESPACE + ".SelectedSchematic";

        private static Dictionary<Players.Player, Tuple<SchematicClickType, string>> _awaitingClick = new Dictionary<Players.Player, Tuple<SchematicClickType, string>>();

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnPlayerClicked, GameLoader.NAMESPACE + ".Jobs.Construction.SchematicMenu.OpenMenu")]
        public static void OpenMenu(Players.Player player, Box<PlayerClickedData> boxedData)
        {
            if (ItemTypes.IndexLookup.TryGetIndex(SchematicTool.NAME, out var schematicItem) &&
                boxedData.item1.typeSelected == schematicItem)
            {
                if (player.ActiveColony == null)
                {
                    PandaChat.Send(player, "Cannot open Schematic tool when not in range of a colony.", ChatColor.red);
                    return;
                }

                if (!_awaitingClick.ContainsKey(player))
                {
                    NetworkMenu menu = new NetworkMenu();
                    menu.LocalStorage.SetAs("header", "Schematic Menu");
                    List<FileInfo> options = SchematicReader.GetSchematics(player);
                    
                    menu.Items.Add(new DropDown("Schematic", Selected_Schematic, options.Select(fi => fi.Name).ToList()));
                    menu.Items.Add(new ButtonCallback(GameLoader.NAMESPACE + ".ShowBuildDetails", new LabelData("Details", UnityEngine.Color.black)));
                    menu.LocalStorage.SetAs(Selected_Schematic, 0);

                    NetworkMenuManager.SendServerPopup(player, menu);
                }
                else
                {
                    var tuple = _awaitingClick[player];
                    _awaitingClick.Remove(player);

                    switch (tuple.Item1)
                    {
                        case SchematicClickType.Build:
                            Vector3Int location = boxedData.item1.rayCastHit.voxelHit.Add(0, 1, 0);
                            var args = new JSONNode();
                            args.SetAs("constructionType", GameLoader.NAMESPACE + ".SchematicBuilder");
                            args.SetAs(SchematicBuilderLoader.NAME + ".SchematicName", tuple.Item2);

                            if (SchematicReader.TryGetSchematicSize(tuple.Item2, player.ActiveColony.ColonyID, out RawSchematicSize schematicSize))
                            {
                                var maxSize = location.Add(schematicSize.XMax, schematicSize.YMax, schematicSize.ZMax);
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

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnSendAreaHighlights, GameLoader.NAMESPACE + ".Jobs.Construction.SchematicMenu.OnSendAreaHighlights")]
        static void OnSendAreaHighlights(Players.Player goal, List<AreaHighlight> list, List<ushort> showWhileHoldingTypes)
        {
            showWhileHoldingTypes.Add(BuiltinBlocks.Bed);
        }

        //public static void CreateNewAreaJob (string identifier, Pipliz.JSON.JSONNode args, Colony colony, Vector3Int min, Vector3Int max)
        //on the areajobtracker
        //identifier = pipliz.constructionarea
        //then the args node has
        //"constructionType" : "GameLoader.NAMESPACE + ".SchematicBuilder"

        //you call AreaJobTracker.SendData(player), then that triggers the OnSendAreaHighlights callback in which you can add any area you want. It'll show up for the player till the callback happens again
        //or you add an actual area to the areajobtracker & then call SendData(player), it'll automatically get the data from that area then
        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnPlayerPushedNetworkUIButton, GameLoader.NAMESPACE + ".Jobs.Construction.SchematicMenu.PressButton")]
        public static void PressButton(ButtonPressCallbackData data)
        {
            switch (data.ButtonIdentifier)
            {
                case GameLoader.NAMESPACE + ".ShowBuildDetails":
                    List<FileInfo> options = SchematicReader.GetSchematics(data.Player);
                    var index = data.Storage.GetAs<int>(Selected_Schematic);

                    if (options.Count > index)
                    {
                        var selectedSchematic = options[index];

                        if (SchematicReader.TryGetSchematicMetadata(selectedSchematic.Name, data.Player.ActiveColony.ColonyID, out SchematicMetadata schematicMetadata))
                        {
                            if (schematicMetadata.Blocks.Count == 1 && schematicMetadata.Blocks.ContainsKey(BuiltinBlocks.Air))
                                PandaChat.Send(data.Player, "Unable to validate schematic. Schematic is all air. Cannot place area.", ChatColor.red);
                            {
                                NetworkMenu menu = new NetworkMenu();
                                menu.Width = 600;
                                menu.Height = 600;
                                menu.LocalStorage.SetAs("header", selectedSchematic.Name + " Details");

                                menu.Items.Add(new Label(new LabelData("Height: " + schematicMetadata.MaxY, UnityEngine.Color.black)));
                                menu.Items.Add(new Label(new LabelData("Width: " + schematicMetadata.MaxZ, UnityEngine.Color.black)));
                                menu.Items.Add(new Label(new LabelData("Length: " + schematicMetadata.MaxX, UnityEngine.Color.black)));
                                menu.LocalStorage.SetAs(Selected_Schematic, selectedSchematic.Name);

                                foreach (var kvp in schematicMetadata.Blocks)
                                {
                                    var item = ItemTypes.GetType(kvp.Key);

                                    List<IItem> items = new List<IItem>();
                                    items.Add(new ItemIcon(kvp.Key));
                                    items.Add(new Label(new LabelData(item.Name, UnityEngine.Color.black)));
                                    items.Add(new Label(new LabelData(" x " + kvp.Value.Count, UnityEngine.Color.black)));
                                    menu.Items.Add(new HorizontalGrid(items, 200));
                                }

                                menu.Items.Add(new ButtonCallback(GameLoader.NAMESPACE + ".SetBuildArea", new LabelData("Build", UnityEngine.Color.black)));

                                NetworkMenuManager.SendServerPopup(data.Player, menu);
                            }
                        }
                    }

                    break;

                case GameLoader.NAMESPACE + ".SetBuildArea":
                    var scem = data.Storage.GetAs<string>(Selected_Schematic);
                    PandaLogger.Log("Schematic: {0}", scem);

                    if (SchematicReader.TryGetSchematicMetadata(scem, data.Player.ActiveColony.ColonyID, out SchematicMetadata metadata))
                    {
                        if (metadata.Blocks.Count == 1 && metadata.Blocks.ContainsKey(BuiltinBlocks.Air))
                            PandaChat.Send(data.Player, "Unable to validate schematic. Schematic is all air. Cannot place area.", ChatColor.red);
                        {
                            _awaitingClick[data.Player] = Tuple.Create(SchematicClickType.Build, scem);
                            PandaChat.Send(data.Player, "Right click on the top of a block to place the scematic. This will be the front left corner.");
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

    public class SchematicTool : CSType
    {
        public static string NAME = GameLoader.NAMESPACE + ".SchematicTool";
        public override string Name => NAME;
        public override string icon => GameLoader.ICON_PATH + "Schematics.png";
        public override bool? isPlaceable => false;
    }

    public class SchematicToolResearch : IPandaResearch
    {
        public Dictionary<ushort, int> RequiredItems => new Dictionary<ushort, int>()
        {
            { BuiltinBlocks.ScienceBagColony, 1 },
            { BuiltinBlocks.ScienceBagBasic, 3 },
            { BuiltinBlocks.ScienceBagAdvanced, 1 }
        };

        public int NumberOfLevels => 1;
        public float BaseValue => 0.05f;
        public List<string> Dependancies => new List<string>()
            {
                ColonyBuiltIn.Research.Builder,
                ColonyBuiltIn.Research.ScienceBagAdvanced,
                ColonyBuiltIn.Research.ScienceBagColony
            };

        public int BaseIterationCount => 300;
        public bool AddLevelToName => false;
        public string Name => "Architect";

        public void OnRegister()
        {

        }

        public void ResearchComplete(object sender, ResearchCompleteEventArgs e)
        {

        }
    }
}
