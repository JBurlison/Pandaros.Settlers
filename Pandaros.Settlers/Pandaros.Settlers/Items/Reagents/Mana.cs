using BlockTypes;
using Pandaros.Settlers.Jobs;
using Pandaros.Settlers.Research;
using Pipliz.JSON;
using Recipes;
using Science;
using System.Collections.Generic;

namespace Pandaros.Settlers.Items
{
    public class Mana : CSType
    {
        public override string name { get; set; } = GameLoader.NAMESPACE + ".Mana";
        public override string icon { get; set; } = GameLoader.ICON_PATH + "bluebottle.png";
        public override bool? isPlaceable { get; set; } = false;
        public override List<string> categories { get; set; } = new List<string>()
        {
            "ingredient",
            "magic"
        };
    }

    public class ManaRecipe : ICSRecipe
    {
        public List<RecipeItem> requires => new List<RecipeItem>()
        {
            new RecipeItem(ColonyBuiltIn.ItemTypes.HOLLYHOCK.Name, 1),
            new RecipeItem(ColonyBuiltIn.ItemTypes.ALKANET.Name, 1),
            new RecipeItem(ColonyBuiltIn.ItemTypes.OLIVEOIL.Name, 1),
            new RecipeItem(ColonyBuiltIn.ItemTypes.WOLFSBANE.Name, 1),
            new RecipeItem(SettlersBuiltIn.ItemTypes.REFINEDSAPPHIRE.Id)
        };

        public List<RecipeResult> results => new List<RecipeResult>()
        {
            new RecipeResult(SettlersBuiltIn.ItemTypes.MANA.Name, 2),
            new RecipeResult(SettlersBuiltIn.ItemTypes.ESPER.Name, 1, 0.05f)
        };

        public CraftPriority defaultPriority => CraftPriority.Medium;

        public bool isOptional => true;

        public int defaultLimit => 50;

        public string Job => ApothecaryRegister.JOB_NAME;

        public string name => SettlersBuiltIn.ItemTypes.MANA.Name;
    }

    public class ManaResearch : IPandaResearch
    {
        public string IconDirectory => GameLoader.ICON_PATH;

        public Dictionary<int, List<InventoryItem>> RequiredItems => new Dictionary<int, List<InventoryItem>>()
        {
            {
                0,
                new List<InventoryItem>()
                {
                    new InventoryItem(ColonyBuiltIn.ItemTypes.ALKANET.Id, 10),
                    new InventoryItem(ColonyBuiltIn.ItemTypes.WOLFSBANE.Id, 10),
                    new InventoryItem(ColonyBuiltIn.ItemTypes.HOLLYHOCK.Id, 10),
                    new InventoryItem(ColonyBuiltIn.ItemTypes.CRYSTAL.Id, 10),
                    new InventoryItem(ColonyBuiltIn.ItemTypes.GYPSUM.Id, 10)
                }
            }
        };

        public Dictionary<int, List<IResearchableCondition>> Conditions => new Dictionary<int, List<IResearchableCondition>>()
        {
            {
                0,
                new List<IResearchableCondition>()
                {
                    new HappinessCondition() { Threshold = 60 }
                }
            }
        };

        public Dictionary<int, List<string>> Dependancies => new Dictionary<int, List<string>>()
        {
            {
                0,
                new List<string>()
                {
                    SettlersBuiltIn.Research.APOTHECARIES1
                }
            }
        };

        public Dictionary<int, List<RecipeUnlock>> Unlocks => new Dictionary<int, List<RecipeUnlock>>()
        {
            {
                1,
                new List<RecipeUnlock>()
                {
                    new RecipeUnlock(SettlersBuiltIn.ItemTypes.MANA.Name, ERecipeUnlockType.Recipe),
                    new RecipeUnlock(SettlersBuiltIn.ItemTypes.AETHER.Name, ERecipeUnlockType.Recipe)
                }
            }
        };

        public int NumberOfLevels => 1;

        public float BaseValue => 1f;

        public int BaseIterationCount => 50;

        public bool AddLevelToName => true;

        public string name => GameLoader.NAMESPACE + ".Mana";

        public void OnRegister()
        {

        }

        public void ResearchComplete(object sender, ResearchCompleteEventArgs e)
        {
        }
    }
}