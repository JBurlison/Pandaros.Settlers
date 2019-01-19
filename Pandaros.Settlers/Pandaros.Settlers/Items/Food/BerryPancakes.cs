using Pandaros.Settlers.Extender;
using Pandaros.Settlers.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Pandaros.Settlers.Items.Food
{
    public class BerryPancakesRecipe : ICSRecipe
    {
        public Dictionary<ItemId, int> Requirements => new Dictionary<ItemId, int>()
        {
            { ItemId.GetItemId(ColonyBuiltIn.ItemTypes.FLOUR), 3 },
            { ItemId.GetItemId(ColonyBuiltIn.ItemTypes.BERRY), 3 },
            { ItemId.GetItemId(ColonyBuiltIn.ItemTypes.FIREWOOD), 1 }
        };

        public Dictionary<ItemId, int> Results => new Dictionary<ItemId, int>()
        {
            { ItemId.GetItemId(Name), 2 }
        };

        public CraftPriority Priority => CraftPriority.Medium;

        public bool IsOptional => false;

        public int DefautLimit => 50;

        public string Job => ColonyBuiltIn.NpcTypes.BAKER;

        public string Name => GameLoader.NAMESPACE + ".BerryPancakes";
    }

    public class BerryPancakes : CSType
    {
        public override string Name => GameLoader.NAMESPACE + ".BerryPancakes";
        public override string icon => GameLoader.ICON_PATH + "BerryPancakes.png";
        public override bool? isPlaceable => false;
        public override float? foodValue => .3f;
        public override float? happiness => 10f;
        public override float? dailyFoodFractionOptimal => .5f;
        public override List<string> categories => new List<string>() { "food" };
    }
}