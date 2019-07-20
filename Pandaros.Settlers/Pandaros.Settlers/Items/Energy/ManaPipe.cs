using Pandaros.API;
using Pandaros.API.Items;
using Pandaros.API.Models;
using Recipes;
using System.Collections.Generic;
using System.IO;

// pipe fill color: 1058c7
namespace Pandaros.Settlers.Energy
{
    public class ManaPipeRecipe : ICSRecipe
    {
        public List<RecipeItem> requires => new List<RecipeItem>()
        {
            new RecipeItem(SettlersBuiltIn.ItemTypes.ADAMANTINE.Id, 2),
            new RecipeItem(SettlersBuiltIn.ItemTypes.MANA.Id, 1),
            new RecipeItem(ColonyBuiltIn.ItemTypes.SAND.Id, 3),
            new RecipeItem(ColonyBuiltIn.ItemTypes.COATEDPLANKS.Id, 1),
            new RecipeItem(SettlersBuiltIn.ItemTypes.REFINEDEMERALD.Id, 1),
            new RecipeItem(SettlersBuiltIn.ItemTypes.REFINEDRUBY.Id, 1),
            new RecipeItem(SettlersBuiltIn.ItemTypes.REFINEDSAPPHIRE.Id, 1)
        };

        public List<RecipeResult> results => new List<RecipeResult>()
        {
            new RecipeResult(GameLoader.NAMESPACE + ".ManaPipe")
        };

        public CraftPriority defaultPriority => CraftPriority.Medium;

        public bool isOptional => true;

        public int defaultLimit => 5;

        public string Job => GameLoader.NAMESPACE + ".AdvancedCrafter";

        public string name => GameLoader.NAMESPACE + ".ManaPipe";
    }

    public class ManaPipeTextureMapping : CSTextureMapping
    {
        public override string name => GameLoader.NAMESPACE + ".ManaPipe";
        public override string albedo => Path.Combine(GameLoader.BLOCKS_ALBEDO_PATH, "Pipe_Albedo.png");
        public override string emissive => Path.Combine(GameLoader.BLOCKS_EMISSIVE_PATH, "Pipe_Emissive.png");
        public override string normal => Path.Combine(GameLoader.BLOCKS_NORMAL_PATH, "Pipe_Normal.png");
        public override string height => Path.Combine(GameLoader.BLOCKS_HEIGHT_PATH, "Pipe_height.png");
    }

    public class ManaPipeBase : CSType
    {
        public const string MANA_PIPE = GameLoader.NAMESPACE + ".ManaPipe";
        public override string sideall { get; set; } = GameLoader.NAMESPACE + ".ManaPipe";
        public override string onPlaceAudio { get; set; } = "Pandaros.Settlers.Metal";
        public override string onRemoveAudio { get; set; } = "Pandaros.Settlers.MetalRemove";
        public override int? maxStackSize { get; set; } = 300;
        public override int? destructionTime { get; set; } = 500;
        public override List<OnRemove> onRemove { get; set; } = new List<OnRemove>()
        {
            new OnRemove(1, 1, MANA_PIPE)
        };
    }

    public class ManaPipe : ManaPipeBase
    {
        public override string name { get; set; } = MANA_PIPE;
        public override string icon { get; set; } = Path.Combine(GameLoader.ICON_PATH, "ManaPipe.png");
        public override string mesh { get; set; } = Path.Combine(GameLoader.MESH_PATH, "Pipe.obj");
        public override List<string> categories { get; set; } = new List<string>()
        {
            "Mana",
            "Energy",
            "Machine"
        };
        public override ConnectedBlock ConnectedBlock { get; set; } = new ConnectedBlock()
        {
            BlockType = "ManaPipe",
            Connections = new List<BlockSide>()
            {
                BlockSide.Zn,
                BlockSide.Zp
            },
            CalculationType = "Pipe"
        };
    }

    public class ManaPipeElbow : ManaPipeBase
    {
        public override string name { get; set; } = GameLoader.NAMESPACE + ".ManaPipeElbow";
        public override string mesh { get; set; } = Path.Combine(GameLoader.MESH_PATH, "Pipe_Elbow.obj");
        public override ConnectedBlock ConnectedBlock { get; set; } = new ConnectedBlock()
        {
            BlockType = "ManaPipe",
            Connections = new List<BlockSide>()
            {
                BlockSide.Zp,
                BlockSide.Xp
            },
            CalculationType = "Pipe"
        };
    }

    public class ManaPipeThreeWay : ManaPipeBase
    {
        public override string name { get; set; } = GameLoader.NAMESPACE + ".ManaPipeThreeWay";
        public override string mesh { get; set; } = Path.Combine(GameLoader.MESH_PATH, "Pipe_3way.obj");
        public override ConnectedBlock ConnectedBlock { get; set; } = new ConnectedBlock()
        {
            BlockType = "ManaPipe",
            Connections = new List<BlockSide>()
            {
                BlockSide.Yp,
                BlockSide.Zn,
                BlockSide.Zp
            },
            CalculationType = "Pipe"
        };
    }


    public class ManaPipeThreeCorner : ManaPipeBase
    {
        public override string name { get; set; } = GameLoader.NAMESPACE + ".ManaPipeThreeCorner";
        public override string mesh { get; set; } = Path.Combine(GameLoader.MESH_PATH, "Pipe_3way_Corner.obj");
        public override ConnectedBlock ConnectedBlock { get; set; } = new ConnectedBlock()
        {
            BlockType = "ManaPipe",
            Connections = new List<BlockSide>()
            {
                BlockSide.Yn,
                BlockSide.Zn,
                BlockSide.Xp
            },
            CalculationType = "Pipe"
        };
    }

    public class ManaPipeFourWay : ManaPipeBase
    {
        public override string name { get; set; } = GameLoader.NAMESPACE + ".ManaPipeFourWay";
        public override string mesh { get; set; } = Path.Combine(GameLoader.MESH_PATH, "Pipe_4way.obj");
        public override ConnectedBlock ConnectedBlock { get; set; } = new ConnectedBlock()
        {
            BlockType = "ManaPipe",
            Connections = new List<BlockSide>()
            {
                BlockSide.Zp,
                BlockSide.Zn,
                BlockSide.Yp,
                BlockSide.Yn
            },
            CalculationType = "Pipe"
        };
    }

    public class ManaPipeFourWayCorner : ManaPipeBase
    {
        public override string name { get; set; } = GameLoader.NAMESPACE + ".ManaPipeFourWayCorner";
        public override string mesh { get; set; } = Path.Combine(GameLoader.MESH_PATH, "Pipe_4way_potrude.obj");
        public override ConnectedBlock ConnectedBlock { get; set; } = new ConnectedBlock()
        {
            BlockType = "ManaPipe",
            Connections = new List<BlockSide>()
            {
                BlockSide.Zp,
                BlockSide.Zn,
                BlockSide.Xp,
                BlockSide.Yp
            },
            CalculationType = "Pipe"
        };
    }

    public class ManaPipeFiveWay : ManaPipeBase
    {
        public override string name { get; set; } = GameLoader.NAMESPACE + ".ManaPipeFiveWay";
        public override string mesh { get; set; } = Path.Combine(GameLoader.MESH_PATH, "Pipe_5way.obj");
        public override ConnectedBlock ConnectedBlock { get; set; } = new ConnectedBlock()
        {
            BlockType = "ManaPipe",
            Connections = new List<BlockSide>()
            {
                BlockSide.Zp,
                BlockSide.Zn,
                BlockSide.Xp,
                BlockSide.Yp,
                BlockSide.Yn
            },
            CalculationType = "Pipe"
        };
    }

    public class ManaPipeSixWay : ManaPipeBase
    {
        public override string name { get; set; } = GameLoader.NAMESPACE + ".ManaPipeSixWay";
        public override string mesh { get; set; } = Path.Combine(GameLoader.MESH_PATH, "Pipe_6way.obj");
        public override ConnectedBlock ConnectedBlock { get; set; } = new ConnectedBlock()
        {
            BlockType = "ManaPipe",
            Connections = new List<BlockSide>()
            {
                BlockSide.Zp,
                BlockSide.Zn,
                BlockSide.Xp,
                BlockSide.Xn,
                BlockSide.Yp,
                BlockSide.Yn
            },
            CalculationType = "Pipe"
        };
    }
}
