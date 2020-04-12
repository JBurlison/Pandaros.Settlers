using Jobs;
using NPC;
using Pandaros.API;
using Pandaros.API.Models;
using Pandaros.API.Research;
using Recipes;
using Science;
using System.Collections.Generic;
using UnityEngine;

namespace Pandaros.Settlers.Jobs
{
    [ModLoader.ModManager]
    public static class DecorBuilderRegister
    {
        public static string JOB_NAME = GameLoader.NAMESPACE + ".DecorBuilder";
        public static string JOB_ITEM_KEY = GameLoader.NAMESPACE + ".DecorBuilderTable";

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterItemTypesDefined, GameLoader.NAMESPACE + ".DecorBuilderRegister.RegisterJobs")]
        [ModLoader.ModCallbackProvidesFor("create_savemanager")]
        public static void RegisterJobs()
        {
            NPCType.AddSettings(new NPCTypeStandardSettings
            {
                keyName = JOB_NAME,
                printName = "DecorBuilder",
                maskColor1 = new Color32(9, 0, 115, 255),
                type = NPCTypeID.GetNextID()
            });

            ServerManager.BlockEntityCallbacks.RegisterEntityManager(new BlockJobManager<CraftingJobInstance>(new CraftingJobSettings(JOB_ITEM_KEY, JOB_NAME)));
        }
    }

    public class DecorBuilderTexture : CSTextureMapping
    {
        public override string name => GameLoader.NAMESPACE + ".DecorBuilderTableTop";
        public override string albedo => GameLoader.BLOCKS_ALBEDO_PATH + "DecorBuilderTableTop.png";
        public override string normal => GameLoader.BLOCKS_NORMAL_PATH + "DecorBuilderTableTop.png";
    }

    public class DecorBuilderJobItem : CSType
    {
        public override string icon => GameLoader.ICON_PATH + "DecorBuilderTable.png";
        public override string onPlaceAudio => "woodPlace";
        public override string onRemoveAudio => "woodDeleteLight";
        public override string sideall => "planks";
        public override string sideyp => GameLoader.NAMESPACE + ".DecorBuilderTableTop";
        public override List<string> categories => new List<string>() { "job", GameLoader.NAMESPACE };
        public override string name => DecorBuilderRegister.JOB_ITEM_KEY;
    }

    public class DecorBuilderRecipe : ICSRecipe
    {
        public DecorBuilderRecipe()
        {
            requires.Add(new RecipeItem(ColonyBuiltIn.ItemTypes.COPPERNAILS.Name, 20));
            requires.Add(new RecipeItem(ColonyBuiltIn.ItemTypes.COPPERTOOLS.Name, 1));
            requires.Add(new RecipeItem(ColonyBuiltIn.ItemTypes.PLANKS.Name, 6));
            results.Add(new RecipeResult(DecorBuilderRegister.JOB_ITEM_KEY, 1));
        }

        public List<RecipeItem> requires { get; private set; } = new List<RecipeItem>();
        public List<RecipeResult> results { get; private set; } = new List<RecipeResult>();
        public CraftPriority defaultPriority => CraftPriority.Medium;
        public bool isOptional => false;
        public int defaultLimit => 5;
        public string Job => ColonyBuiltIn.NpcTypes.CRAFTER;
        public string name => DecorBuilderRegister.JOB_ITEM_KEY;
    }
}