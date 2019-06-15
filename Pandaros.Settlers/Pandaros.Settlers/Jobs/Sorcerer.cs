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
        public Dictionary<ItemId, int> RequiredItems => new Dictionary<ItemId, int>()
        {
            { ItemId.GetItemId(Items.Reagents.Adamantine.NAME), 1 },
            { ColonyBuiltIn.ItemTypes.SCIENCEBAGCOLONY, 1 },
            { ColonyBuiltIn.ItemTypes.SCIENCEBAGBASIC, 3 },
            { ColonyBuiltIn.ItemTypes.SCIENCEBAGADVANCED, 1 }
        };

        public int NumberOfLevels => 1;
        public float BaseValue => 0.05f;
        public List<string> Dependancies => new List<string>()
            {
                PandaResearch.GetResearchKey(GeneralResearch.ArmorSmithing + 4),
                PandaResearch.GetResearchKey(GeneralResearch.SwordSmithing + 4),
                PandaResearch.GetResearchKey(GeneralResearch.Elementium + 1),
                ColonyBuiltIn.Research.SCIENCEBAGADVANCED,
                ColonyBuiltIn.Research.SCIENCEBAGCOLONY
            };

        public int BaseIterationCount => 300;
        public bool AddLevelToName => false;
        public string name => "Sorcerer";
        public string IconDirectory => GameLoader.ICON_PATH;
        public void OnRegister()
        {

        }

        public void ResearchComplete(object sender, ResearchCompleteEventArgs e)
        {
            e.Manager.Colony.RecipeData.UnlockedOptionalRecipes.Add(new Recipes.RecipeKey(SorcererRegister.JOB_NAME));
        }
    }

    [ModLoader.ModManager]
    public static class SorcererRegister
    {
        public static string JOB_NAME = GameLoader.NAMESPACE + ".Sorcerer";
        public static string JOB_ITEM_KEY = GameLoader.NAMESPACE + ".SorcererTable";

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
        public override string name => GameLoader.NAMESPACE + ".SorcererTableTop";
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
        public override string name => SorcererRegister.JOB_ITEM_KEY;
    }

    public class SorcererRecipe : ICSRecipe
    {
        public SorcererRecipe()
        {
            requires.Add(new RecipeItem(ColonyBuiltIn.ItemTypes.COPPERNAILS.Name, 60));
            requires.Add(new RecipeItem(ColonyBuiltIn.ItemTypes.COPPERTOOLS.Name, 6));
            requires.Add(new RecipeItem(ColonyBuiltIn.ItemTypes.PLANKS.Name, 6));
            requires.Add(new RecipeItem(Items.Reagents.Adamantine.NAME, 2));
            results.Add(new RecipeItem(SorcererRegister.JOB_ITEM_KEY, 1));
        }

        public List<RecipeItem> requires { get; private set; } = new List<RecipeItem>();
        public List<RecipeItem> results { get; private set; } = new List<RecipeItem>();
        public CraftPriority defaultPriority => CraftPriority.Medium;
        public bool isOptional => true;
        public int defaultLimit => 5;
        public string Job => ColonyBuiltIn.NpcTypes.CRAFTER;
        public string name => SorcererRegister.JOB_NAME;
    }
}