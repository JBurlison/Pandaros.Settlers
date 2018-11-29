using BlockTypes;
using Jobs;
using NPC;
using Pandaros.Settlers.Items;
using Pandaros.Settlers.Models;
using Pipliz.JSON;
using Recipes;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

namespace Pandaros.Settlers.Jobs
{
    [ModLoader.ModManager]
    public static class ApothecaryRegister
    {
        public static string JOB_NAME = GameLoader.NAMESPACE + ".Apothecary";
        public static string JOB_ITEM_KEY = GameLoader.NAMESPACE + ".ApothecaryTable";
        public static string JOB_RECIPE = JOB_ITEM_KEY + ".recipe";

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterItemTypesDefined, GameLoader.NAMESPACE + ".Jobs.ApothecaryRegister.RegisterJobs")]
        public static void RegisterJobs()
        {
            NPCType.AddSettings(new NPCTypeStandardSettings
            {
                keyName = JOB_NAME,
                printName = "Apothecary",
                maskColor1 = new Color32(101, 121, 123, 255),
                type = NPCTypeID.GetNextID()
            });

            ServerManager.BlockEntityCallbacks.RegisterEntityManager(new BlockJobManager<CraftingJobInstance>(new CraftingJobSettings(JOB_ITEM_KEY, JOB_NAME)));
        }
    }

    public class ApothecaryTexture : CSTextureMapping
    {
        public override string Name => GameLoader.NAMESPACE + ".ApothecaryTable";
        public override string albedo => GameLoader.BLOCKS_ALBEDO_PATH + "ApothecaryTable.png";
        public override string height => GameLoader.BLOCKS_HEIGHT_PATH + "ApothecaryTable.png";
        public override string normal => GameLoader.BLOCKS_NORMAL_PATH + "ApothecaryTable.png";
    }

    public class ApothecaryJobItem : CSType
    {
        public override string icon => GameLoader.ICON_PATH + "ApothecaryTable.png";
        public override string onPlaceAudio => "woodPlace";
        public override string onRemoveAudio => "woodDeleteLight";
        public override string sideall => GameLoader.NAMESPACE + ".ApothecaryTable";
        public override List<string> categories => new List<string>() { "job" };
        public override string Name => ApothecaryRegister.JOB_ITEM_KEY;
    }

    public class ApothecaryRecipe : ICSRecipe
    {
        public ApothecaryRecipe()
        {
            Requirements.Add(ItemId.GetItemId(BuiltinBlocks.BronzeIngot), 2);
            Requirements.Add(ItemId.GetItemId(BuiltinBlocks.CopperTools), 1);
            Requirements.Add(ItemId.GetItemId(BuiltinBlocks.Planks), 4);
            Results.Add(ItemId.GetItemId(ApothecaryRegister.JOB_ITEM_KEY), 1);
        }

        public Dictionary<ItemId, int> Requirements { get; private set; } = new Dictionary<ItemId, int>();
        public Dictionary<ItemId, int> Results { get; private set; } = new Dictionary<ItemId, int>();
        public CraftPriority Priority => CraftPriority.Medium;
        public bool IsOptional => true;
        public int DefautLimit => 5;
        public string Job => ItemFactory.JOB_CRAFTER;
        public string Name => ApothecaryRegister.JOB_RECIPE;
    }
}