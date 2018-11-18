using Pandaros.Settlers.Extender;
using Pandaros.Settlers.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Pandaros.Settlers.Items.Food
{
    public class BerryPie : CSType, ICSRecipe
    {
        public override string Name => GameLoader.NAMESPACE + ".BerryPie";
        public override string icon => GameLoader.ICON_PATH + "BerryPie.png";
        public override bool? isPlaceable => false;
        public override float? nutritionalValue => 5.5f;
        public override ReadOnlyCollection<string> categories => new ReadOnlyCollection<string>(new List<string>() { "food" });
        public Dictionary<ItemId, int> Requirements => new Dictionary<ItemId, int>()
        {
            { ItemId.GetItemId("flour"), 4 },
            { ItemId.GetItemId("berry"), 4 },
            { ItemId.GetItemId("firewood"), 1 }
        };

        public Dictionary<ItemId, int> Results => new Dictionary<ItemId, int>()
        {
            { ItemId.GetItemId(Name), 2 }
        };

        public CraftPriority Priority => CraftPriority.High;

        public bool IsOptional => false;

        public int DefautLimit => 50;

        public string Job => ItemFactory.JOB_BAKER;
    }
}