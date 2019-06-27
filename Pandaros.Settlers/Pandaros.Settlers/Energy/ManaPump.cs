using Pandaros.Settlers.Items;
using Pandaros.Settlers.Jobs;
using Recipes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandaros.Settlers.Energy
{
    public class ManaPumpTextureMapping : CSTextureMapping
    {
        public override string name => GameLoader.NAMESPACE + ".ManaPump";
        public override string albedo => Path.Combine(GameLoader.BLOCKS_ALBEDO_PATH, "Manapump_Albedo.png");
        public override string emissive => Path.Combine(GameLoader.BLOCKS_EMISSIVE_PATH, "Manapump_Emissive.png");
        public override string normal => Path.Combine(GameLoader.BLOCKS_NORMAL_PATH, "Manapump_Normal.png");
    }

    public class ManaPumpRecipe : ICSRecipe
    {
        public List<RecipeItem> requires => new List<RecipeItem>()
        {
            new RecipeItem(SettlersBuiltIn.ItemTypes.ADAMANTINE.Id, 6),
            new RecipeItem(SettlersBuiltIn.ItemTypes.MANA.Id, 10),
            new RecipeItem(ColonyBuiltIn.ItemTypes.SAND.Id, 3),
            new RecipeItem(ColonyBuiltIn.ItemTypes.COATEDPLANKS.Id, 5),
            new RecipeItem(SettlersBuiltIn.ItemTypes.REFINEDEMERALD.Id, 3),
            new RecipeItem(SettlersBuiltIn.ItemTypes.REFINEDRUBY.Id, 3),
            new RecipeItem(SettlersBuiltIn.ItemTypes.REFINEDSAPPHIRE.Id, 3),
            new RecipeItem(SettlersBuiltIn.ItemTypes.MAGICWAND.Id)
        };

        public List<RecipeResult> results => new List<RecipeResult>()
        {
            new RecipeResult(GameLoader.NAMESPACE + ".ManaPump")
        };

        public CraftPriority defaultPriority => CraftPriority.Medium;

        public bool isOptional => true;

        public int defaultLimit => 1;

        public string Job => GameLoader.NAMESPACE + ".Artificer";

        public string name => GameLoader.NAMESPACE + ".ManaPump";
    }

    public class ManaPumpGenerate : CSGenerateType
    {
        public override string generateType { get; set; } = "rotateBlock";
        public override string typeName { get; set; } = GameLoader.NAMESPACE + ".ManaPump";
        public override ICSType baseType { get; set; } = new CSType()
        {
            categories = new List<string>()
            {
                "Mana",
                "Energy",
                "Machine"
            },
            mesh = Path.Combine(GameLoader.MESH_PATH, "Manapump.obj"),
            icon = Path.Combine(GameLoader.ICON_PATH, "ManaPump.png"),
            onPlaceAudio = "Pandaros.Settlers.Metal",
            onRemoveAudio = "Pandaros.Settlers.MetalRemove",
            maxStackSize  = 300,
            destructionTime = 500,
            sideall = GameLoader.NAMESPACE + ".ManaPump",
            meshRotationEuler = new MeshRotationEuler()
            {
                y = 90
            },
            ConnectedBlock = new ConnectedBlock()
            {
                BlockType = "ManaPipe",
                Connections = new List<Models.BlockSide>()
                {
                    Models.BlockSide.Xn,
                    Models.BlockSide.Xp,
                    Models.BlockSide.Yn,
                    Models.BlockSide.Yp,
                    Models.BlockSide.Zn,
                    Models.BlockSide.Zp
                },
                AutoChange = false
            }
        };
    }
}
