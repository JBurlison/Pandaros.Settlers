using System.Collections.Generic;
using System.Collections.ObjectModel;
using BlockTypes.Builtin;
using Pandaros.Settlers.Extender;
using Pipliz.JSON;

namespace Pandaros.Settlers.Items.Food
{
    public class BerryPancakes : CSType
    {
        public const string NAME = GameLoader.NAMESPACE + ".BerryPancakes";

        public override string Name => NAME;
        public override string icon => GameLoader.ICON_PATH + "BerryPancakes.png";
        public override bool? isPlaceable => false;
        public override float? nutritionalValue => 4f;
        public override ReadOnlyCollection<string> categories => new ReadOnlyCollection<string>(new List<string>() { "food" });
    }

    public class BerryPancakesRecipe : ICSRecipe
    {
        public string Name => BerryPancakes.NAME;

        public Dictionary<string, int> Requirements => new Dictionary<string, int>()
        {
            { "flour", 3 },
            { "berry", 3 },
            { "firewood", 1 }
        };

        public Dictionary<string, int> Results => new Dictionary<string, int>()
        {
            { BerryPancakes.NAME, 2 }
        };

        public CraftPriority Priority => CraftPriority.Medium;

        public bool IsOptional => false;

        public int DefautLimit => 50;

        public string Job => ItemFactory.JOB_BAKER;
    }
}