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

namespace Pandaros.Settlers.Jobs.Construction
{
    public enum SchematicClickType
    {
        Build,
        PlaceBuilder,
        Archetect,
        PlaceArchetect
    }

    [ModLoader.ModManager]
    public class SchematicMenu
    {
        private static readonly string Selected_Schematic = GameLoader.NAMESPACE + ".SelectedSchematic";
        private static Dictionary<Players.Player, Tuple<SchematicClickType, string>> _awaitingClick = new Dictionary<Players.Player, Tuple<SchematicClickType, string>>();

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnPlayerClicked, GameLoader.NAMESPACE + ".Jobs.Construction.SchematicMenu.OpenMenu")]
        public static void OpenMenu(Players.Player player, Box<PlayerClickedData> boxedData)
        {
            if (player.ActiveColony == null)
            {
                PandaChat.Send(player, "Cannot open Schematic tool when not in range of a colony.", ChatColor.red);
                return;
            }

            if (ItemTypes.IndexLookup.TryGetIndex(SchematicTool.NAME, out var SchematicItem) &&
                boxedData.item1.typeSelected == SchematicItem)
            {
                if (!_awaitingClick.ContainsKey(player))
                {
                    NetworkMenu menu = new NetworkMenu();
                    menu.LocalStorage.SetAs("header", "Schematic Menu");
                    List<FileInfo> options = SchematicReader.GetSchematics(player);

                    menu.Items.Add(new DropDown(GameLoader.NAMESPACE + ".Schematic", "SelectedSchematic", options.Select(fi => fi.Name).ToList()));
                    menu.Items.Add(new ButtonCallback(GameLoader.NAMESPACE + ".SetBuildArea", new LabelData("Build")));
                    menu.Items.Add(new ButtonCallback(GameLoader.NAMESPACE + ".PlaceBuilder", new LabelData("Place Builder")));
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
                                AreaJobTracker.CreateNewAreaJob("pipliz.constructionarea", args, player.ActiveColony, location, location.Add(schematicSize.XMax, schematicSize.YMax, schematicSize.ZMax));
                            }

                            break;

                        case SchematicClickType.PlaceBuilder:

                            break;

                        case SchematicClickType.Archetect:

                            break;

                        case SchematicClickType.PlaceArchetect:

                            break;
                    }
                }
            }
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
                case GameLoader.NAMESPACE + ".PlaceBuilder":
                    _awaitingClick.Add(data.Player, Tuple.Create(SchematicClickType.PlaceBuilder, string.Empty));
                    break;

                case GameLoader.NAMESPACE + ".SetBuildArea":
                    List<FileInfo> options = SchematicReader.GetSchematics(data.Player);
                    var index = data.Storage.GetAs<int>(Selected_Schematic);

                    if (options.Count > index)
                    {
                        var schematic = options[index];
                        _awaitingClick.Add(data.Player, Tuple.Create(SchematicClickType.Build, schematic.Name));
                        PandaChat.Send(data.Player, "Right click on the top of a block to place the scematic. This will be the front left corner.");
                    }

                    break;
            }
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnSendAreaHighlights, GameLoader.NAMESPACE + ".Jobs.Construction.SchematicMenu.AreaHighlighted")]
        public static void AreaHighlighted(Players.Player player, List<AreaJobTracker.AreaHighlight> list, List<ushort> showWhileHoldingTypes)
        {

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
