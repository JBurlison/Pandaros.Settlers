using Jobs;
using NPC;
using Pandaros.API;
using Pandaros.API.Items;
using Pandaros.API.Jobs.Roaming;
using Pandaros.API.Models;
using Pandaros.Settlers.Energy;
using Recipes;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Pandaros.Settlers.Items.Transportation
{
    [ModLoader.ModManager]
    public static class ConductorRegister
    {
        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterItemTypesDefined, GameLoader.NAMESPACE + ".Items.Transportation.ConductorRegister")]
        [ModLoader.ModCallbackProvidesFor("create_savemanager")]
        [ModLoader.ModCallbackProvidesFor("pipliz.server.loadresearchables")]
        public static void RegisterJobs()
        {
            NPCType.AddSettings(new NPCTypeStandardSettings
            {
                keyName = GameLoader.NAMESPACE + ".Conductor",
                printName = "Conductor",
                maskColor1 = new Color32(245, 123, 66, 255),
                type = NPCTypeID.GetNextID()
            });

            ServerManager.BlockEntityCallbacks.RegisterEntityManager(new BlockJobManager<CraftingJobInstance>(new CraftingJobSettings(GameLoader.NAMESPACE + ".TrainStation", GameLoader.NAMESPACE + ".Conductor")));
        }
    }

    public class TrainStationObjective : IRoamingJobObjective
    {
        public float WorkTime => 6;
        public float WatchArea => 21;
        public ItemId ItemIndex => GameLoader.NAMESPACE + ".TrainStation";

        public Dictionary<string, IRoamingJobObjectiveAction> ActionCallbacks => new Dictionary<string, IRoamingJobObjectiveAction>()
        {
            {
                GameLoader.NAMESPACE + ".ManaMachineRepair",
                new ManaMachineRepairingAction()
            }
        };

        public string ObjectiveCategory => "ManaMachine";

        public string name => GameLoader.NAMESPACE + ".TrainStation";

        public void DoWork(Colony colony, RoamingJobState state)
        {
            state.GetActionEnergy(GameLoader.NAMESPACE + ".ManaTankRefill");
            state.GetActionEnergy(GameLoader.NAMESPACE + ".ManaMachineRepair");
        }
    }

    public class TrainStationRecipe : ICSRecipe
    {
        public List<RecipeItem> requires => new List<RecipeItem>()
        {
            new RecipeItem(SettlersBuiltIn.ItemTypes.ADAMANTINE.Id, 10),
            new RecipeItem(SettlersBuiltIn.ItemTypes.MANA.Id, 5),
            new RecipeItem(ColonyBuiltIn.ItemTypes.SAND.Id, 5),
            new RecipeItem(SettlersBuiltIn.ItemTypes.REFINEDEMERALD.Id, 5),
            new RecipeItem(SettlersBuiltIn.ItemTypes.REFINEDRUBY.Id, 5),
            new RecipeItem(SettlersBuiltIn.ItemTypes.REFINEDSAPPHIRE.Id, 5),
            new RecipeItem(SettlersBuiltIn.ItemTypes.MAGICWAND.Id)
        };

        public List<RecipeResult> results => new List<RecipeResult>()
        {
            new RecipeResult(GameLoader.NAMESPACE + ".TrainStation")
        };

        public CraftPriority defaultPriority => CraftPriority.Medium;

        public bool isOptional => true;

        public int defaultLimit => 1;

        public string Job => GameLoader.NAMESPACE + ".AdvancedCrafter";

        public string name => GameLoader.NAMESPACE + ".TrainStation";

        public List<string> JobBlock => new List<string>();
    }

    public class TrainStationTextureMapping : CSTextureMapping
    {
        public override string name => GameLoader.NAMESPACE + ".TrainStation";
        public override string albedo => Path.Combine(GameLoader.BLOCKS_ALBEDO_PATH, "TrainStation.png");
        public override string height => Path.Combine(GameLoader.BLOCKS_HEIGHT_PATH, "ironblock.png");
        public override string normal => Path.Combine(GameLoader.BLOCKS_NORMAL_PATH, "ironblock.png");
    }


    public class TrainStation : CSType
    {
        public override string name { get; set; } = GameLoader.NAMESPACE + ".TrainStation";
        public override string icon { get; set; } = Path.Combine(GameLoader.ICON_PATH, "TrainStation.png");
        public override string onPlaceAudio { get; set; } = "Pandaros.Settlers.Metal";
        public override string onRemoveAudio { get; set; } = "Pandaros.Settlers.MetalRemove";
        public override int? maxStackSize { get; set; } = 300;
        public override int? destructionTime { get; set; } = 500;
        public override string sideall { get; set; } = GameLoader.NAMESPACE + ".TrainStation";
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
        public override TrainStationSettings TrainStationSettings { get; set; } = new TrainStationSettings()
        {
            BlockType = "Monorail",
            ObjectiveCategory = "ManaMachine"
        };
        public override List<string> categories { get; set; } = new List<string>()
            {
                "Mana",
                "Energy",
                "Machine",
                "TrainStation"
            };
    }
}
