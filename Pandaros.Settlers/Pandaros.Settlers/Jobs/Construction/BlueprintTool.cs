using BlockTypes;
using NetworkUI;
using NetworkUI.Items;
using Pandaros.Settlers.Items;
using Pandaros.Settlers.Research;
using Pipliz;
using Shared;
using System.Collections.Generic;
using System.IO;

namespace Pandaros.Settlers.Jobs.Construction
{
    [ModLoader.ModManager]
    public class BlueprintMenu
    {
        private static readonly string Selected_Blueprint = GameLoader.NAMESPACE + ".SelectedBlueprint";

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnPlayerClicked, GameLoader.NAMESPACE + ".Jobs.Construction.BlueprintMenu.OpenMenu")]
        public static void OpenMenu(Players.Player player, Box<PlayerClickedData> boxedData)
        {
            if (player.ActiveColony == null)
            {
                PandaChat.Send(player, "Cannot open blueprint tool when not in range of a colony.", ChatColor.red);
                return;
            }

            if (ItemTypes.IndexLookup.TryGetIndex(BlueprintTool.NAME, out var blueprintItem) &&
                boxedData.item1.typeSelected == blueprintItem)
            {
                NetworkMenu menu = new NetworkMenu();
                menu.LocalStorage.SetAs("header", "Blueprint Menu");
                List<string> options = GetBlueprints(player);

                menu.Items.Add(new DropDown(GameLoader.NAMESPACE + ".Blueprint", "SelectedBlueprint", options));
                menu.Items.Add(new ButtonCallback(GameLoader.NAMESPACE + ".SetBuildArea", new LabelData("Build")));
                menu.LocalStorage.SetAs(Selected_Blueprint, 0);

                NetworkMenuManager.SendServerPopup(player, menu);
            }
        }

        public static List<string> GetBlueprints(Players.Player player)
        {
            var options = new List<string>();
            var colonyBlueprints = GameLoader.BLUEPRINT_SAVE_LOC + $"\\{player.ActiveColony.ColonyID}\\";

            if (!Directory.Exists(colonyBlueprints))
                Directory.CreateDirectory(colonyBlueprints);

            if (player.ActiveColony != null)
                foreach (var file in Directory.EnumerateFiles(colonyBlueprints))
                {
                    var fi = new FileInfo(file);
                    options.Add(fi.Name);
                }

            foreach (var file in Directory.EnumerateFiles(GameLoader.BLUEPRINT_DEFAULT_LOC))
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
        //"constructionType" : "GameLoader.NAMESPACE + ".BlueprintBuilder"

        //you call AreaJobTracker.SendData(player), then that triggers the OnSendAreaHighlights callback in which you can add any area you want. It'll show up for the player till the callback happens again
        //or you add an actual area to the areajobtracker & then call SendData(player), it'll automatically get the data from that area then
        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnPlayerPushedNetworkUIButton, GameLoader.NAMESPACE + ".Jobs.Construction.BlueprintMenu.PressButton")]
        public static void PressButton(ButtonPressCallbackData data)
        {
            if (data.ButtonIdentifier == GameLoader.NAMESPACE + ".SetBuildArea")
            {
                List<string> options = GetBlueprints(data.Player);
                var index = data.Storage.GetAs<int>(Selected_Blueprint);

                if (options.Count > index)
                {
                    var blueprint = options[index];
                }
            }
        }
    }

    public class BlueprintTool : CSType
    {
        public static string NAME = GameLoader.NAMESPACE + ".BlueprintTool";
        public override string Name => NAME;
        public override string icon => GameLoader.ICON_PATH + "Blueprints.png";
        public override bool? isPlaceable => false;
    }

    public class BlueprintToolResearch : IPandaResearch
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
