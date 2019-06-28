using Pandaros.Settlers.Items;
using Pandaros.Settlers.Jobs.Roaming;
using Pandaros.Settlers.Models;
using Recipes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandaros.Settlers.Energy
{
    public class ManaTankObjective : IRoamingJobObjective
    {
        public float WorkTime => 6;

        public ItemId ItemIndex => SettlersBuiltIn.ItemTypes.MANATANK;

        public Dictionary<string, IRoamingJobObjectiveAction> ActionCallbacks => new Dictionary<string, IRoamingJobObjectiveAction>()
        {
            {
                GameLoader.NAMESPACE + ".ManaMachineRepair",
                new ManaMachineRepairingAction()
            }
        };

        public string ObjectiveCategory => "Mana";

        public string name => SettlersBuiltIn.ItemTypes.MANATANK;

        public void DoWork(Colony colony, RoamingJobState state)
        {
            
        }
    }

    public class ManaTankRecipe : ICSRecipe
    {
        public List<RecipeItem> requires => new List<RecipeItem>()
        {
            new RecipeItem(SettlersBuiltIn.ItemTypes.ADAMANTINE.Id, 6),
            new RecipeItem(SettlersBuiltIn.ItemTypes.MANA.Id, 10),
            new RecipeItem(ColonyBuiltIn.ItemTypes.SAND.Id, 20),
            new RecipeItem(ColonyBuiltIn.ItemTypes.COATEDPLANKS.Id, 5),
            new RecipeItem(SettlersBuiltIn.ItemTypes.REFINEDEMERALD.Id, 3),
            new RecipeItem(SettlersBuiltIn.ItemTypes.REFINEDRUBY.Id, 3),
            new RecipeItem(SettlersBuiltIn.ItemTypes.REFINEDSAPPHIRE.Id, 3),
            new RecipeItem(SettlersBuiltIn.ItemTypes.MAGICWAND.Id)
        };

        public List<RecipeResult> results => new List<RecipeResult>()
        {
            new RecipeResult(GameLoader.NAMESPACE + ".ManaTank")
        };

        public CraftPriority defaultPriority => CraftPriority.Medium;

        public bool isOptional => true;

        public int defaultLimit => 1;

        public string Job => GameLoader.NAMESPACE + ".Artificer";

        public string name => GameLoader.NAMESPACE + ".ManaTank";
    }

    public class ManaTankTextureMapping : CSTextureMapping
    {
        public override string name => GameLoader.NAMESPACE + ".ManaTank";
        public override string albedo => Path.Combine(GameLoader.BLOCKS_ALBEDO_PATH, "Tank_Albedo.png");
        public override string emissive => Path.Combine(GameLoader.BLOCKS_EMISSIVE_PATH, "Tank_Emissive.png");
        public override string normal => Path.Combine(GameLoader.BLOCKS_NORMAL_PATH, "Tank_Normal.png");
    }


    public class ManaTankBase : CSType
    {
        public override string icon { get; set; } = Path.Combine(GameLoader.ICON_PATH, "ManaTank.png");
        public override string onPlaceAudio { get; set; } = "Pandaros.Settlers.Metal";
        public override string onRemoveAudio { get; set; } = "Pandaros.Settlers.MetalRemove";
        public override int? maxStackSize { get; set; } = 300;
        public override int? destructionTime { get; set; } = 500;
        public override string sideall { get; set; } = GameLoader.NAMESPACE + ".ManaTank";
        public override List<OnRemove> onRemove { get; set; } = new List<OnRemove>()
        {
            new OnRemove(1, 1, GameLoader.NAMESPACE + ".ManaTank")
        };
    }


    public class ManaTankGenerate : ManaTankBase
    {
        public override string name { get; set; } = GameLoader.NAMESPACE + ".TankFull";
        public override string mesh { get; set; } = Path.Combine(GameLoader.MESH_PATH, "Tank_Full.obj");
    }

    public class ManaTankEmptyGenerate : ManaTankBase
    {
        public override string name { get; set; } = GameLoader.NAMESPACE + ".ManaTank";
        public override string mesh { get; set; } = Path.Combine(GameLoader.MESH_PATH, "Tank_empty.obj");
        public override List<string> categories { get; set; } = new List<string>()
            {
                "Mana",
                "Energy",
                "Machine"
            };
    }

    public class ManaTankQuarterGenerate : ManaTankBase
    {
        public override string name { get; set; } = GameLoader.NAMESPACE + ".TankQuarter";
        public override string mesh { get; set; } = Path.Combine(GameLoader.MESH_PATH, "Tank_quarter.obj");
    }

    public class ManaTankThreeQuarterGenerate : ManaTankBase
    {
        public override string name { get; set; } = GameLoader.NAMESPACE + ".TankThreeQuarter";
        public override string mesh { get; set; } = Path.Combine(GameLoader.MESH_PATH, "Tank_3quarter.obj");
    }

    public class ManaTankHalfGenerate : ManaTankBase
    {
        public override string name { get; set; } = GameLoader.NAMESPACE + ".TankHalf";
        public override string mesh { get; set; } = Path.Combine(GameLoader.MESH_PATH, "Tank_half.obj");
    }
}
