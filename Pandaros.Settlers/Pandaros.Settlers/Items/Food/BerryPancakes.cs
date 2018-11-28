using Pandaros.Settlers.Extender;
using Pandaros.Settlers.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Pandaros.Settlers.Items.Food
{
    public class BerryPancakes : CSType, ICSRecipe
    {
        public override string Name => GameLoader.NAMESPACE + ".BerryPancakes";
        public override string icon => GameLoader.ICON_PATH + "BerryPancakes.png";
        public override bool? isPlaceable => false;
        public override float? nutritionalValue => 4f;
        public override List<string> categories => new List<string>() { "food" };

        public Dictionary<ItemId, int> Requirements => new Dictionary<ItemId, int>()
        {
            { ItemId.GetItemId("flour"), 3 },
            { ItemId.GetItemId("berry"), 3 },
            { ItemId.GetItemId("firewood"), 1 }
        };

        public Dictionary<ItemId, int> Results => new Dictionary<ItemId, int>()
        {
            { ItemId.GetItemId(Name), 2 }
        };

        public CraftPriority Priority => CraftPriority.Medium;

        public bool IsOptional => false;

        public int DefautLimit => 50;

        public string Job => ItemFactory.JOB_BAKER;
    }
}