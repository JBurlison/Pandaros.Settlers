using Pandaros.Settlers.Models;
using Recipes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandaros.Settlers.Items.Transportation
{
    public class ManaTankRecipe : ICSRecipe
    {
        public List<RecipeItem> requires => new List<RecipeItem>()
        {
            new RecipeItem(SettlersBuiltIn.ItemTypes.ADAMANTINE.Id, 2),
            new RecipeItem(SettlersBuiltIn.ItemTypes.MANA.Id, 5),
            new RecipeItem(ColonyBuiltIn.ItemTypes.SAND.Id, 5),
            new RecipeItem(SettlersBuiltIn.ItemTypes.REFINEDEMERALD.Id, 1),
            new RecipeItem(SettlersBuiltIn.ItemTypes.REFINEDRUBY.Id, 1),
            new RecipeItem(SettlersBuiltIn.ItemTypes.REFINEDSAPPHIRE.Id, 1),
            new RecipeItem(SettlersBuiltIn.ItemTypes.MAGICWAND.Id)
        };

        public List<RecipeResult> results => new List<RecipeResult>()
        {
            new RecipeResult(GameLoader.NAMESPACE + ".Monorail")
        };

        public CraftPriority defaultPriority => CraftPriority.Medium;

        public bool isOptional => true;

        public int defaultLimit => 1;

        public string Job => GameLoader.NAMESPACE + ".AdvancedCrafter";

        public string name => GameLoader.NAMESPACE + ".Monorail";
    }

    public class MonorailTextureMapping : CSTextureMapping
    {
        public override string name => GameLoader.NAMESPACE + ".Monorail";
        public override string albedo => Path.Combine(GameLoader.BLOCKS_ALBEDO_PATH, "Monorail.png");
        public override string emissive => Path.Combine(GameLoader.BLOCKS_EMISSIVE_PATH, "Monorail.png");
        public override string normal => Path.Combine(GameLoader.BLOCKS_NORMAL_PATH, "Monorail.png");
    }


    public class MonorailBase : CSType
    {
        public override string icon { get; set; } = Path.Combine(GameLoader.ICON_PATH, "Monorail.png");
        public override string onPlaceAudio { get; set; } = "Pandaros.Settlers.Metal";
        public override string onRemoveAudio { get; set; } = "Pandaros.Settlers.MetalRemove";
        public override int? maxStackSize { get; set; } = 300;
        public override int? destructionTime { get; set; } = 500;
        public override string sideall { get; set; } = GameLoader.NAMESPACE + ".Monorail";
        public override List<OnRemove> onRemove { get; set; } = new List<OnRemove>()
        {
            new OnRemove(1, 1, GameLoader.NAMESPACE + ".Monorail")
        };
    }

    public class Monorail : MonorailBase
    {
        public override string name { get; set; } = GameLoader.NAMESPACE + ".Monorail";
        public override string mesh { get; set; } = Path.Combine(GameLoader.MESH_PATH, "Monorail_Straight.obj");
        public override List<string> categories { get; set; } = new List<string>()
            {
                "Mana",
                "Energy",
                "Machine"
            };
        public override ConnectedBlock ConnectedBlock { get; set; } = new ConnectedBlock()
        {
            BlockType = "Monorail",
            Connections = new List<Models.BlockSide>()
            {
                Models.BlockSide.Zn,
                Models.BlockSide.Zp
            },
            CalculationType = "Track"
        };
    }

    public class MonorailElbow : MonorailBase
    {
        public override string name { get; set; } = GameLoader.NAMESPACE + ".MonorailElbow";
        public override string mesh { get; set; } = Path.Combine(GameLoader.MESH_PATH, "Monorail_Turn.obj");
        public override ConnectedBlock ConnectedBlock { get; set; } = new ConnectedBlock()
        {
            BlockType = "Monorail",
            Connections = new List<Models.BlockSide>()
            {
                Models.BlockSide.Zn,
                Models.BlockSide.Xn
            },
            CalculationType = "Track"
        };
    }

    public class MonorailRamp : MonorailBase
    {
        public override string name { get; set; } = GameLoader.NAMESPACE + ".MonorailRamp";
        public override string mesh { get; set; } = Path.Combine(GameLoader.MESH_PATH, "Monorail_Ramp.obj");
        public override ConnectedBlock ConnectedBlock { get; set; } = new ConnectedBlock()
        {
            BlockType = "Monorail",
            Connections = new List<Models.BlockSide>()
            {
                Models.BlockSide.ZpYp,
                Models.BlockSide.ZnYn
            },
            CalculationType = "Track"
        };
        public override Colliders colliders { get; set; } = new Colliders(true, true, new List<Colliders.Boxes>()
        {
            new Colliders.Boxes(new List<float>() { -.5f, -.5f, -.5f }, new List<float>() { .5f, 1.5f, .5f })
        });
    }
}
