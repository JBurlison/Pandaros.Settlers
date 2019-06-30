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
    public class PropulsionPlatformRecipe : ICSRecipe
    {
        public List<RecipeItem> requires => new List<RecipeItem>()
        {
            new RecipeItem(SettlersBuiltIn.ItemTypes.ADAMANTINE.Id, 50),
            new RecipeItem(SettlersBuiltIn.ItemTypes.MANA.Id, 20),
            new RecipeItem(ColonyBuiltIn.ItemTypes.SAND.Id, 20),
            new RecipeItem(ColonyBuiltIn.ItemTypes.COATEDPLANKS.Id, 10),
            new RecipeItem(SettlersBuiltIn.ItemTypes.REFINEDEMERALD.Id, 10),
            new RecipeItem(SettlersBuiltIn.ItemTypes.REFINEDRUBY.Id, 10),
            new RecipeItem(SettlersBuiltIn.ItemTypes.REFINEDSAPPHIRE.Id, 10)
        };

        public List<RecipeResult> results => new List<RecipeResult>()
        {
            new RecipeResult(GameLoader.NAMESPACE + ".PropulsionPlatform")
        };

        public CraftPriority defaultPriority => CraftPriority.Medium;

        public bool isOptional => true;

        public int defaultLimit => 5;

        public string Job => GameLoader.NAMESPACE + ".AdvancedCrafter";

        public string name => GameLoader.NAMESPACE + ".PropulsionPlatform";
    }

    public class PropulsionTextureMapping : CSTextureMapping
    {
        public override string name => GameLoader.NAMESPACE + ".PropulsionPlatform";
        public override string albedo => Path.Combine(GameLoader.BLOCKS_ALBEDO_PATH, "PropulsionPlatform.png");
        public override string emissive => Path.Combine(GameLoader.BLOCKS_EMISSIVE_PATH, "PropulsionPlatform.png");
        public override string normal => Path.Combine(GameLoader.BLOCKS_NORMAL_PATH, "PropulsionPlatform.png");
        public override string height => Path.Combine(GameLoader.BLOCKS_HEIGHT_PATH, "PropulsionPlatform.png");
    }

    public class PropulsionPlatform : CSType
    {
        public override string name { get; set; } = GameLoader.NAMESPACE + ".PropulsionPlatform";
        public override string sideall { get; set; } = GameLoader.NAMESPACE + ".PropulsionPlatform";
        public override string onPlaceAudio { get; set; } = "Pandaros.Settlers.Metal";
        public override string onRemoveAudio { get; set; } = "Pandaros.Settlers.MetalRemove";
        public override int? maxStackSize { get; set; } = 300;
        public override int? destructionTime { get; set; } = 500;
        public override string icon { get; set; } = Path.Combine(GameLoader.ICON_PATH, "PropulsionPlatform.png");
        public override string mesh { get; set; } = Path.Combine(GameLoader.MESH_PATH, "PropulsionPlatform.obj");
        public override bool? isPlaceable { get; set; } = false;
        public override List<string> categories { get; set; } = new List<string>()
            {
                "Mana",
                "Energy",
                "Machine"
            };
        public override Colliders colliders { get; set; } = new Colliders(true, true, new List<Colliders.Boxes>()
        {
            new Colliders.Boxes(new List<float>() { -2f, -1f, -2f }, new List<float>() { 2, .5f, 2 })
        });
        public override ConnectedBlock ConnectedBlock { get; set; } = new ConnectedBlock()
        {
            BlockType = "Monorail"
        };
        public override TrainConfiguration TrainConfiguration { get; set; } = new TrainConfiguration()
        {
            AllowPlayerToEditBlocksWhileRiding = false,
            playerSeatOffset = new SerializableVector3(0, 2, 0),
            TrainBounds = new SerializableVector3(3, 2, 3),
            IdealHeightFromTrack = 3,
            MoveTimePerBlockMs = 500,
            ManaCostPerBlock = 0.001f
        };
    }
}
