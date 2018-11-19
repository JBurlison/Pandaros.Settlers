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
        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnPlayerClicked, GameLoader.NAMESPACE + ".Jobs.Construction.BlueprintMenu.OpenMenu")]
        public static void OpenMenu(Players.Player player, Box<PlayerClickedData> boxedData)
        {
            if (ItemTypes.IndexLookup.TryGetIndex(BlueprintTool.NAME, out var blueprintItem) &&
                boxedData.item1.typeSelected == blueprintItem)
            {
                NetworkMenu menu = new NetworkMenu();
                menu.LocalStorage.SetAs("header", "Blueprint Menu");

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

                menu.Items.Add(new DropDown("Blueprint", "SelectedBlueprint", options));
                menu.LocalStorage.SetAs("SelectedBlueprint", 0);

                NetworkMenuManager.SendServerPopup(player, menu);
            }
        }


        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnPlayerPushedNetworkUIButton, GameLoader.NAMESPACE + ".Jobs.Construction.BlueprintMenu.PressButton")]
        public static void PressButton(ButtonPressCallbackData data)
        {
            if (data.ButtonIdentifier.StartsWith("ui.link_"))
            {
                string url = data.ButtonIdentifier.Substring(data.ButtonIdentifier.IndexOf("_") + 1);


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
