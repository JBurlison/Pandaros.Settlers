using BlockTypes;
using Jobs;
using NPC;
using Pandaros.Settlers.Items;
using Pandaros.Settlers.Models;
using Pandaros.Settlers.Research;
using Pipliz.JSON;
using Recipes;
using System.Collections.Generic;
using UnityEngine;

namespace Pandaros.Settlers.Jobs
{
    public class SorcererResearch : IPandaResearch
    {
        public Dictionary<ushort, int> RequiredItems => new Dictionary<ushort, int>()
        {
            { BuiltinBlocks.ScienceBagColony, 1 },
            { BuiltinBlocks.ScienceBagBasic, 3 },
            { BuiltinBlocks.ScienceBagAdvanced, 1 }
        };

        public int NumberOfLevels => 1;
        public float BaseValue => 0.05f;
        public List<string> Dependancies => new List<string>()
            {
                PandaResearch.GetResearchKey(PandaResearch.Elementium + 1),
                ColonyBuiltIn.Research.ScienceBagAdvanced,
                ColonyBuiltIn.Research.ScienceBagColony
            };

        public int BaseIterationCount => 300;
        public bool AddLevelToName => false;
        public string Name => "Sorcerer";

        public void OnRegister()
        {

        }

        public void ResearchComplete(object sender, ResearchCompleteEventArgs e)
        {
            e.Manager.Colony.RecipeData.SetRecipeAvailability(new Recipes.RecipeKey(SorcererRegister.JOB_RECIPE), true);
        }
    }

    [ModLoader.ModManager]
    public static class SorcererRegister
    {
        public static string JOB_NAME = GameLoader.NAMESPACE + ".Sorcerer";
        public static string JOB_ITEM_KEY = GameLoader.NAMESPACE + ".SorcererTable";
        public static string JOB_RECIPE = JOB_ITEM_KEY + ".recipe";

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterItemTypesDefined, GameLoader.NAMESPACE + ".SorcererRegister.RegisterJobs")]
        [ModLoader.ModCallbackProvidesFor("create_savemanager")]
        public static void RegisterJobs()
        {
            NPCType.AddSettings(new NPCTypeStandardSettings
            {
                keyName = JOB_NAME,
                printName = "Sorcerer",
                maskColor1 = new Color32(9, 0, 115, 255),
                type = NPCTypeID.GetNextID()
            });

            ServerManager.BlockEntityCallbacks.RegisterEntityManager(new BlockJobManager<CraftingJobInstance>(new CraftingJobSettings(JOB_ITEM_KEY, JOB_NAME)));
        }
    }

    public class SorcererTexture : CSTextureMapping
    {
        public override string Name => GameLoader.NAMESPACE + ".SorcererTableTop";
        public override string albedo => GameLoader.BLOCKS_ALBEDO_PATH + "SorcererTableTop.png";
        public override string height => GameLoader.BLOCKS_HEIGHT_PATH + "SorcererTableTop.png";
        public override string normal => GameLoader.BLOCKS_NORMAL_PATH + "SorcererTableTop.png";
    }

    public class SorcererJobItem : CSType
    {
        public override string icon => GameLoader.ICON_PATH + "SorcererTable.png";
        public override string onPlaceAudio => "woodPlace";
        public override string onRemoveAudio => "woodDeleteLight";
        public override string sideall => "coatedplanks";
        public override string sideyp => GameLoader.NAMESPACE + ".SorcererTableTop";
        public override List<string> categories => new List<string>() { "job", GameLoader.NAMESPACE };
        public override string Name => SorcererRegister.JOB_ITEM_KEY;
    }

    public class SorcererRecipe : ICSRecipe
    {
        public SorcererRecipe()
        {
            Requirements.Add(ItemId.GetItemId(BuiltinBlocks.CopperNails), 60);
            Requirements.Add(ItemId.GetItemId(BuiltinBlocks.CopperTools), 6);
            Requirements.Add(ItemId.GetItemId(BuiltinBlocks.Planks), 6);
            Requirements.Add(ItemId.GetItemId("Pandaros.Settlers.AutoLoad.Adamantine"), 2);
            Results.Add(ItemId.GetItemId(SorcererRegister.JOB_ITEM_KEY), 1);
        }

        public Dictionary<ItemId, int> Requirements { get; private set; } = new Dictionary<ItemId, int>();
        public Dictionary<ItemId, int> Results { get; private set; } = new Dictionary<ItemId, int>();
        public CraftPriority Priority => CraftPriority.Medium;
        public bool IsOptional => true;
        public int DefautLimit => 5;
        public string Job => ItemFactory.JOB_CRAFTER;
        public string Name => SorcererRegister.JOB_RECIPE;
    }
}