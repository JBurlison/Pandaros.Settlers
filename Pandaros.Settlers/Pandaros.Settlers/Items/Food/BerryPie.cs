using Pandaros.Settlers.Extender;
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
        public Dictionary<string, int> Requirements => new Dictionary<string, int>()
        {
            { "flour", 4 },
            { "berry", 4 },
            { "firewood", 1 }
        };

        public Dictionary<string, int> Results => new Dictionary<string, int>()
        {
            { Name, 2 }
        };

        public CraftPriority Priority => CraftPriority.High;

        public bool IsOptional => false;

        public int DefautLimit => 50;

        public string Job => ItemFactory.JOB_BAKER;
    }
}