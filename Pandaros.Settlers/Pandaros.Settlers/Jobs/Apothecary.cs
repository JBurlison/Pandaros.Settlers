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
        public override string name => GameLoader.NAMESPACE + ".ApothecaryTable";
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
        public override List<string> categories => new List<string>() { "job", GameLoader.NAMESPACE };
        public override string name => ApothecaryRegister.JOB_ITEM_KEY;
    }

    public class ApothecaryRecipe : ICSRecipe
    {
        public ApothecaryRecipe()
        {
            requires.Add(new RecipeItem(ColonyBuiltIn.ItemTypes.BRONZEINGOT.Name, 2));
            requires.Add(new RecipeItem(ColonyBuiltIn.ItemTypes.COPPERTOOLS.Name, 1));
            requires.Add(new RecipeItem(ColonyBuiltIn.ItemTypes.PLANKS.Name, 4));
            results.Add(new RecipeItem(ApothecaryRegister.JOB_ITEM_KEY, 1));
        }

        public List<RecipeItem> requires { get; private set; } = new List<RecipeItem>();
        public List<RecipeItem> results { get; private set; } = new List<RecipeItem>();
        public CraftPriority defaultPriority => CraftPriority.Medium;
        public bool isOptional => true;
        public int defaultLimit => 5;
        public string Job => ColonyBuiltIn.NpcTypes.CRAFTER;
        public string name => ApothecaryRegister.JOB_RECIPE;
    }
}