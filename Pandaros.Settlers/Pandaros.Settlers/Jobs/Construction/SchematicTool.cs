using BlockTypes;
using NetworkUI;
using NetworkUI.Items;
using Pandaros.Settlers.Buildings.NBT;
using Pandaros.Settlers.Items;
using Pandaros.Settlers.Research;
using Pipliz;
using Shared;
using System.Collections.Generic;
using System.IO;

namespace Pandaros.Settlers.Jobs.Construction
{
    [ModLoader.ModManager]
    public class SchematicMenu
    {
        private static readonly string Selected_Schematic = GameLoader.NAMESPACE + ".SelectedSchematic";

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
                NetworkMenu menu = new NetworkMenu();
                menu.LocalStorage.SetAs("header", "Schematic Menu");
                List<string> options = GetSchematics(player);

                menu.Items.Add(new DropDown(GameLoader.NAMESPACE + ".Schematic", "SelectedSchematic", options));
                menu.Items.Add(new ButtonCallback(GameLoader.NAMESPACE + ".SetBuildArea", new LabelData("Build")));
                menu.LocalStorage.SetAs(Selected_Schematic, 0);

                NetworkMenuManager.SendServerPopup(player, menu);
            }
        }

        public static List<string> GetSchematics(Players.Player player)
        {
            var options = new List<string>();
            var colonySchematics = GameLoader.Schematic_SAVE_LOC + $"\\{player.ActiveColony.ColonyID}\\";

            if (!Directory.Exists(colonySchematics))
                Directory.CreateDirectory(colonySchematics);

            if (player.ActiveColony != null)
                foreach (var file in Directory.EnumerateFiles(colonySchematics))
                {
                    var fi = new FileInfo(file);
                    options.Add(fi.Name);
                }

            foreach (var file in Directory.EnumerateFiles(GameLoader.Schematic_DEFAULT_LOC))
            {
                var fi = new FileInfo(file);
                options.Add(fi.Name);
            }

            return options;
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
            if (data.ButtonIdentifier == GameLoader.NAMESPACE + ".SetBuildArea")
            {
                List<string> options = GetSchematics(data.Player);
                var index = data.Storage.GetAs<int>(Selected_Schematic);

                if (options.Count > index)
                {
                    var Schematic = options[index];
                    AreaJobTracker.SendData(data.Player);
                }
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
