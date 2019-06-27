using BlockTypes;
using Pandaros.Settlers.Jobs;
using Pipliz.JSON;
using Recipes;
using System.Collections.Generic;

namespace Pandaros.Settlers.Items
{
    public class EarthStone : CSType
    {
        public override string name { get; set; } = GameLoader.NAMESPACE + ".EarthStone";
        public override string icon { get; set; } = GameLoader.ICON_PATH + "Earthstone.png";
        public override bool? isPlaceable { get; set; } = false;
        public override List<string> categories { get; set; } = new List<string>()
        {
            "ingredient",
            "magic",
            "stone"
        };
    }

    public class EarthStoneRecipe : ICSRecipe
    {
        public List<RecipeItem> requires => new List<RecipeItem>()
        {
            new RecipeItem(Elementium.Item.ItemIndex, 1),
            new RecipeItem(ColonyBuiltIn.ItemTypes.STONEBRICKS.Name, 50)
        };

        public List<RecipeResult> results => new List<RecipeResult>()
        {
            new RecipeResult(SettlersBuiltIn.ItemTypes.EARTHSTONE.Id, 1)
        };

        public CraftPriority defaultPriority =>  CraftPriority.Medium;

        public bool isOptional => true;

        public int defaultLimit => 6;

        public string Job => ApothecaryRegister.JOB_NAME;

        public string name => SettlersBuiltIn.ItemTypes.EARTHSTONE;
    }
}