using Pandaros.Settlers.Extender;
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
        public override ReadOnlyCollection<string> categories => new ReadOnlyCollection<string>(new List<string>() { "food" });

        public Dictionary<string, int> Requirements => new Dictionary<string, int>()
        {
            { "flour", 3 },
            { "berry", 3 },
            { "firewood", 1 }
        };

        public Dictionary<string, int> Results => new Dictionary<string, int>()
        {
            { Name, 2 }
        };

        public CraftPriority Priority => CraftPriority.Medium;

        public bool IsOptional => false;

        public int DefautLimit => 50;

        public string Job => ItemFactory.JOB_BAKER;
    }
}