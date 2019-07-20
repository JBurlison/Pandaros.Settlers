using Pandaros.API;
using Pandaros.API.Models;
using Recipes;
using System.Collections.Generic;

namespace Pandaros.Settlers.Items.Reagents
{
    public class MagicWand : CSType
    {
        public override string name { get; set; } = GameLoader.NAMESPACE + ".MagicWand";
        public override bool? isPlaceable { get; set; } = false;
        public override string icon { get; set; } = GameLoader.ICON_PATH + "MagicWand.png";
        public override List<string> categories { get; set; } = new List<string>()
        {
            "Mana",
            "Reagent",
            GameLoader.NAMESPACE
        };
    }

    public class MagicWandRecipe : ICSRecipe
    {
        public List<RecipeItem> requires => new List<RecipeItem>()
        {
            new RecipeItem(ColonyBuiltIn.ItemTypes.PLANKS.Id),
            new RecipeItem(ColonyBuiltIn.ItemTypes.SAND.Id),
            new RecipeItem(SettlersBuiltIn.ItemTypes.ADAMANTINE.Id, 2),
            new RecipeItem(SettlersBuiltIn.ItemTypes.MANA.Id),
            new RecipeItem(SettlersBuiltIn.ItemTypes.REFINEDEMERALD.Id),
            new RecipeItem(SettlersBuiltIn.ItemTypes.REFINEDRUBY.Id),
            new RecipeItem(SettlersBuiltIn.ItemTypes.REFINEDSAPPHIRE.Id)
        };

        public List<RecipeResult> results => new List<RecipeResult>()
        {
            new RecipeResult(GameLoader.NAMESPACE + ".MagicWand")
        };

        public CraftPriority defaultPriority => CraftPriority.Medium;

        public bool isOptional => true;

        public int defaultLimit => 20;

        public string Job => GameLoader.NAMESPACE + ".Sorcerer";

        public string name => GameLoader.NAMESPACE + ".MagicWand";
    }
}
