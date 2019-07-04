using Pandaros.Settlers.Models;
using Recipes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandaros.Settlers.Items.Reagents
{
    public class Adamantine : CSType
    {
        public static string NAME = "Pandaros.Settlers.AutoLoad.Adamantine";
        public override string name { get; set; } = NAME;
        public override int? maxStackSize => 600;
        public override bool? isPlaceable => false;
        public override string icon { get; set; } = GameLoader.ICON_PATH + "Adamantine.png";
        public override List<string> categories { get; set; } = new List<string>()
        {
            "ingredient",
            "Adamantine"
        };
    }

    public class AdamantineRecipe : ICSRecipe
    {
        public List<RecipeItem> requires => new List<RecipeItem>()
        {
            new RecipeItem(SettlersBuiltIn.ItemTypes.ADAMANTINENUGGET.Id, 6),
            new RecipeItem(ColonyBuiltIn.ItemTypes.CHARCOAL.Id, 5)
        };

        public List<RecipeResult> results => new List<RecipeResult>()
        {
            new RecipeResult(SettlersBuiltIn.ItemTypes.ADAMANTINE.Id)
        };

        public CraftPriority defaultPriority => CraftPriority.Medium;

        public bool isOptional => true;

        public int defaultLimit => 50;

        public string Job => ColonyBuiltIn.NpcTypes.SMELTER;

        public string name => SettlersBuiltIn.ItemTypes.ADAMANTINE;
    }
}
