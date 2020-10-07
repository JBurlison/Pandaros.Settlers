using Jobs;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using NPC;
using Pandaros.API;
using Pandaros.API.Items;
using Pandaros.API.Jobs.Roaming;
using Pandaros.API.Models;
using Pandaros.Settlers.Energy;
using Recipes;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using UnityEngine;

namespace Pandaros.Settlers.Items.Transportation
{
    public class GoldCubeObjective : IRoamingJobObjective
    {
        public float WorkTime => 6;
        public float WatchArea => 21;
        public ItemId ItemIndex => GameLoader.NAMESPACE + ".GoldCube";
        private double _nextWorkTime;

        public Dictionary<string, IRoamingJobObjectiveAction> ActionCallbacks => new Dictionary<string, IRoamingJobObjectiveAction>()
        {
            {
                GameLoader.NAMESPACE + ".ManaMachineRepair",
                new ManaMachineRepairingAction()
            }
        };

        public string ObjectiveCategory => "ManaMachine";

        public string name => GameLoader.NAMESPACE + ".GoldCube";

        public void DoWork(Colony colony, RoamingJobState state)
        {
            if (_nextWorkTime < TimeCycle.TotalHours)
            {
                var fuel = state.GetActionEnergy(GameLoader.NAMESPACE + ".ManaTankRefill");
                var durability = state.GetActionEnergy(GameLoader.NAMESPACE + ".ManaMachineRepair");

                if (!ColonyManagement.DecorHappiness.DecorBonuses.ContainsKey(colony))
                    ColonyManagement.DecorHappiness.DecorBonuses.Add(colony, new Dictionary<string, float>());

                if (fuel > 0 && durability > 0)
                {
                    ColonyManagement.DecorHappiness.DecorBonuses[colony][nameof(GoldCube)] = 10f;

                    state.SubtractFromActionEnergy(GameLoader.NAMESPACE + ".ManaTankRefill", .2f);
                    state.SubtractFromActionEnergy(GameLoader.NAMESPACE + ".ManaMachineRepair", .05f);

                    if (World.TryGetTypeAt(state.Position, out ItemTypes.ItemType itemType) && itemType.Name == GameLoader.NAMESPACE + ".GoldCubeUnlit")
                        ServerManager.TryChangeBlock(state.Position, ItemId.GetItemId(GameLoader.NAMESPACE + ".GoldCube").Id);
                }
                else
                {
                    if (ColonyManagement.DecorHappiness.DecorBonuses[colony].ContainsKey(nameof(GoldCube)))
                        ColonyManagement.DecorHappiness.DecorBonuses[colony].Remove(nameof(GoldCube));

                    if (World.TryGetTypeAt(state.Position, out ItemTypes.ItemType itemType) && itemType.Name == GameLoader.NAMESPACE + ".GoldCube")
                        ServerManager.TryChangeBlock(state.Position, ItemId.GetItemId(GameLoader.NAMESPACE + ".GoldCubeUnlit").Id);
                }

                _nextWorkTime = TimeCycle.TotalHours + 1;
            }
        }
    }

    public class GoldCubeRecipe : ICSRecipe
    {
        public List<RecipeItem> requires => new List<RecipeItem>()
        {
            new RecipeItem(SettlersBuiltIn.ItemTypes.ADAMANTINE.Id, 40),
            new RecipeItem(SettlersBuiltIn.ItemTypes.MANA.Id, 50),
            new RecipeItem(ColonyBuiltIn.ItemTypes.GOLDINGOT.Id, 15),
            new RecipeItem(SettlersBuiltIn.ItemTypes.REFINEDEMERALD.Id, 15),
            new RecipeItem(SettlersBuiltIn.ItemTypes.REFINEDRUBY.Id, 15),
            new RecipeItem(SettlersBuiltIn.ItemTypes.REFINEDSAPPHIRE.Id, 15),
            new RecipeItem(SettlersBuiltIn.ItemTypes.MAGICWAND.Id)
        };

        public List<RecipeResult> results => new List<RecipeResult>()
        {
            new RecipeResult(GameLoader.NAMESPACE + ".GoldCube")
        };

        public CraftPriority defaultPriority => CraftPriority.Medium;

        public bool isOptional => true;

        public int defaultLimit => 1;

        public string Job => GameLoader.NAMESPACE + ".AdvancedCrafter";

        public string name => GameLoader.NAMESPACE + ".GoldCube";

        public List<string> JobBlock => new List<string>();
    }

    public class GoldCubeTextureMapping : CSTextureMapping
    {
        public override string name => GameLoader.NAMESPACE + ".GoldCube";
        public override string albedo => Path.Combine(GameLoader.BLOCKS_ALBEDO_PATH, "GoldCube.png");
        public override string emissive => Path.Combine(GameLoader.BLOCKS_EMISSIVE_PATH, "GoldCube.png");
        public override string height => Path.Combine(GameLoader.BLOCKS_HEIGHT_PATH, "GoldCube.png");
        public override string normal => Path.Combine(GameLoader.BLOCKS_NORMAL_PATH, "GoldCube.png");
    }

    public class GoldCubeUnlitTextureMapping : CSTextureMapping
    {
        public override string name => GameLoader.NAMESPACE + ".GoldCubeUnlit";
        public override string albedo => Path.Combine(GameLoader.BLOCKS_ALBEDO_PATH, "GoldCube.png");
        public override string height => Path.Combine(GameLoader.BLOCKS_HEIGHT_PATH, "GoldCube.png");
        public override string normal => Path.Combine(GameLoader.BLOCKS_NORMAL_PATH, "GoldCube.png");
    }


    public class GoldCube : CSType
    {
        public override string name { get; set; } = GameLoader.NAMESPACE + ".GoldCube";
        public override string icon { get; set; } = Path.Combine(GameLoader.ICON_PATH, "GoldCube.png");
        public override string onPlaceAudio { get; set; } = "Pandaros.Settlers.Metal";
        public override string onRemoveAudio { get; set; } = "Pandaros.Settlers.MetalRemove";
        public override int? maxStackSize { get; set; } = 300;
        public override int? destructionTime { get; set; } = 500;
        public override string sideall { get; set; } = GameLoader.NAMESPACE + ".GoldCube";
        public override JObject customData { get; set; } =  JsonConvert.DeserializeObject<JObject>("{ \"torches\": { \"a\": { \"color\": \"#FFD700\", \"intensity\": 12.5, \"range\": 3, \"volume\": 0.5 } } }");
        public override ConnectedBlock ConnectedBlock { get; set; } = new ConnectedBlock()
        {
            BlockType = "ManaPipe",
            AutoChange = false,
            CalculationType = "Pipe",
            Connections = new List<BlockSide>()
            {
                BlockSide.Xn,
                BlockSide.Xp,
                BlockSide.Zn,
                BlockSide.Zp,
                BlockSide.Yn,
                BlockSide.Yp
            }
        };
        public override List<string> categories { get; set; } = new List<string>()
            {
                "Mana",
                "Energy",
                "Machine",
                "GoldCube"
            };
    }

    public class GoldCubeUnlit : GoldCube
    {
        public override string name { get; set; } = GameLoader.NAMESPACE + ".GoldCubeUnlit";
        public override List<OnRemove> onRemove { get; set; } = new List<OnRemove>()
        {
            new OnRemove(1,1, GameLoader.NAMESPACE + ".GoldCube")
        };
        public override string sideall { get; set; } = GameLoader.NAMESPACE + ".GoldCubeUnlit";
    }
}
