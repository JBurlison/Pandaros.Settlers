using BlockTypes;
using NPC;
using Pandaros.Settlers.Items;
using Pandaros.Settlers.Jobs.Roaming;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

namespace Pandaros.Settlers.Jobs
{
    [ModLoader.ModManager]
    public class Machinist : RoamingJob
    {
        public static string JOB_NAME = GameLoader.NAMESPACE + ".Machinist";
        public static string JOB_ITEM_KEY = GameLoader.NAMESPACE + ".MachinistBench";
        public static string JOB_RECIPE = JOB_ITEM_KEY + ".recipe";

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterItemTypesDefined, GameLoader.NAMESPACE + ".Jobs.Machinist.RegisterJobs")]
        [ModLoader.ModCallbackProvidesFor("pipliz.apiprovider.jobs.resolvetypes")]
        public static void RegisterJobs()
        {
            BlockJobManagerTracker.Register<Machinist>(JOB_ITEM_KEY);
        }

        public override List<string> ObjectiveCategories => new List<string>() { Items.Machines.MachineConstants.MECHANICAL };
        public override string NPCTypeKey => JOB_NAME;
        public override string JobItemKey => JOB_ITEM_KEY;
        public override InventoryItem RecruitementItem => new InventoryItem(BuiltinBlocks.CopperTools, 1);
        public override bool ToSleep => !TimeCycle.IsDay;
        public override List<uint> OkStatus => new List<uint>
            {
                GameLoader.Refuel_Icon,
                GameLoader.Reload_Icon,
                GameLoader.Repairing_Icon,
                GameLoader.Waiting_Icon
            };

        public override NPCTypeStandardSettings GetNPCTypeDefinition()
        {
            return new NPCTypeStandardSettings
            {
                keyName = NPCTypeKey,
                printName = "Machinist",
                maskColor1 = new Color32(242, 132, 29, 255),
                type = NPCTypeID.GetNextID(),
                inventoryCapacity = 1f
            };
        }

        public override NPCBase.NPCGoal CalculateGoal(ref NPCBase.NPCState state)
        {
            if (TimeCycle.IsDay)
                return NPCBase.NPCGoal.Job;
            else
                return NPCBase.NPCGoal.Bed;
        }
    }

    [ModLoader.ModManager]
    public class MachinistNight : Machinist
    {
        public new static string JOB_NAME = GameLoader.NAMESPACE + ".NightMachinist";
        public new static string JOB_ITEM_KEY = GameLoader.NAMESPACE + ".NightMachinistBench";
        public new static string JOB_RECIPE = JOB_ITEM_KEY + ".recipe";

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterItemTypesDefined, GameLoader.NAMESPACE + ".Jobs.NightMachinist.RegisterJobs")]
        [ModLoader.ModCallbackProvidesFor("pipliz.apiprovider.jobs.resolvetypes")]
        public new static void RegisterJobs()
        {
            BlockJobManagerTracker.Register<MachinistNight>(JOB_ITEM_KEY);
        }
        
        public override string NPCTypeKey => JOB_NAME;
        public override string JobItemKey => JOB_ITEM_KEY;
        public override bool ToSleep => TimeCycle.IsDay;

        public override NPCTypeStandardSettings GetNPCTypeDefinition()
        {
            return new NPCTypeStandardSettings
            {
                keyName = NPCTypeKey,
                printName = "Night Machinist",
                maskColor1 = new Color32(242, 132, 29, 255),
                type = NPCTypeID.GetNextID(),
                inventoryCapacity = 1f
            };
        }

        public override NPCBase.NPCGoal CalculateGoal(ref NPCBase.NPCState state)
        {
            if (TimeCycle.IsDay)
                return NPCBase.NPCGoal.Bed;
            else
                return NPCBase.NPCGoal.Job;
        }
    }

    public class MachinistTexture : CSTextureMapping
    {
        public const string NAME = GameLoader.NAMESPACE + ".MachinistBenchTop";
        public override string Name => NAME;
        public override string albedo => GameLoader.BLOCKS_ALBEDO_PATH + "MachinistBenchTop.png";
        public override string normal => GameLoader.BLOCKS_NORMAL_PATH + "MachinistBenchTop.png";
        public override string height => GameLoader.BLOCKS_HEIGHT_PATH + "MachinistBenchTop.png";
    }

    public class MachinistNightTexture : CSTextureMapping
    {
        public const string NAME = GameLoader.NAMESPACE + ".MachinistBenchTopNight";
        public override string Name => NAME;
        public override string albedo => GameLoader.BLOCKS_ALBEDO_PATH + "MachinistBenchTopNight.png";
        public override string normal => GameLoader.BLOCKS_NORMAL_PATH + "MachinistBenchTop.png";
        public override string height => GameLoader.BLOCKS_HEIGHT_PATH + "MachinistBenchTop.png";
    }

    public class MachinistJobTypeNight : CSType
    {
        public override string Name => MachinistNight.JOB_ITEM_KEY;
        public override string icon => GameLoader.ICON_PATH + "MachinistBenchNight.png";
        public override string onPlaceAudio => "stonePlace";
        public override string onRemoveAudio => "stoneDelete";
        public override string sideall => "stonebricks";
        public override string sideyp => MachinistNightTexture.NAME;
        public override ReadOnlyCollection<string> categories => new ReadOnlyCollection<string>(new List<string>()
        {
            "job"
        });
    }

    public class MachinistJobType : CSType
    {
        public override string Name => Machinist.JOB_ITEM_KEY;
        public override string icon => GameLoader.ICON_PATH + "MachinistBench.png";
        public override string onPlaceAudio => "stonePlace";
        public override string onRemoveAudio => "stoneDelete";
        public override string sideall => "stonebricks";
        public override string sideyp => MachinistTexture.NAME;
        public override ReadOnlyCollection<string> categories => new ReadOnlyCollection<string>(new List<string>()
        {
            "job"
        });
    }

    public class MachinisNighttRecipe : MachinistRecipe
    {
        public override string Name => MachinistNight.JOB_RECIPE;
    }

    public class MachinistRecipe : ICSRecipe
    {
        public virtual string Name => Machinist.JOB_RECIPE;

        public Dictionary<string, int> Requirements => new Dictionary<string, int>()
        {
            { "bronzeingot", 2 },
            { "ironwrought", 2 },
            { "coppertools", 1 },
            { "stonebricks", 4 }
        };

        public Dictionary<string, int> Results => new Dictionary<string, int>()
        {
            { Machinist.JOB_ITEM_KEY, 1 }
        };

        public CraftPriority Priority => CraftPriority.Medium;
        public bool IsOptional => true;
        public int DefautLimit => 2;
        public string Job => ItemFactory.JOB_CRAFTER;
    }
}
