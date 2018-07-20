using Pandaros.Settlers.Extender;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Pandaros.Settlers.Jobs.Farmer.Animals
{
    public class CowTextureMap : CSTextureMapping
    {
        public const string NAME = GameLoader.NAMESPACE + ".Cow";
        public override string Name => NAME;
        public override string albedo => GameLoader.BLOCKS_ALBEDO_PATH + "cow.png";
    }

    public class Cow : CSType, ICSRecipe, AI.IAnimal
    {
        public override string Name => GameLoader.NAMESPACE + ".Cow";
        public override string icon => GameLoader.ICON_PATH + "cow.png";
        public override ReadOnlyCollection<string> categories => new ReadOnlyCollection<string>(new List<string>() { "Animal" });

        public Dictionary<string, int> Requirements => new Dictionary<string, int>()
        {
            { "goldcoin", 500 }
        };

        public Dictionary<string, int> Results => new Dictionary<string, int>()
        {
            { Name, 1 }
        };

        public CraftPriority Priority => CraftPriority.Medium;
        public bool IsOptional => false;
        public int DefautLimit => 50;
        public override string sideall => "SELF";
        public override string mesh => GameLoader.MESH_PATH + "cow.ply";
        public string Job => "pipliz.merchant";

        public double RoamUpdate => 0;

        public int RoamRange => 3;
    }
}
