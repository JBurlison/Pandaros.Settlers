using BlockTypes;
using Pandaros.Settlers.Items;
using Pandaros.Settlers.Research;
using System.Collections.Generic;

namespace Pandaros.Settlers.Jobs.Construction
{
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
