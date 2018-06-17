using BlockTypes.Builtin;
using NPC;
using Pandaros.Settlers.Items;
using Pandaros.Settlers.Jobs.Roaming;
using Pandaros.Settlers.Extender;
using Pipliz.JSON;
using Pipliz.Mods.APIProvider.Jobs;
using Server.NPCs;
using System.Collections.Generic;
using UnityEngine;
using System.Collections.ObjectModel;

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
        public override bool ToSleep => false;

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
    }

    public class MachinistTexture : CSTextureMapping
    {
        public const string NAME = GameLoader.NAMESPACE + ".MachinistBenchTop";
        public override string Name => NAME;
        public override string albedo => GameLoader.BLOCKS_ALBEDO_PATH + "MachinistBenchTop.png";
        public override string normal => GameLoader.BLOCKS_NORMAL_PATH + "MachinistBenchTop.png";
        public override string height => GameLoader.BLOCKS_HEIGHT_PATH + "MachinistBenchTop.png";
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

    public class MachinistRecipe : ICSRecipe
    {
        public string Name => Machinist.JOB_RECIPE;

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
