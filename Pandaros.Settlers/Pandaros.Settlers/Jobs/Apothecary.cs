using Jobs;
using NPC;
using Pandaros.API;
using Pandaros.API.Items;
using Pandaros.API.Models;
using Pandaros.API.Research;
using Recipes;
using Science;
using Shared;
using System.Collections.Generic;
using UnityEngine;

namespace Pandaros.Settlers.Jobs
{
    public class ApothecaryResearch : IPandaResearch
    {
        public string IconDirectory => GameLoader.ICON_PATH;

        public Dictionary<int, List<InventoryItem>> RequiredItems => new Dictionary<int, List<InventoryItem>>()
        {
            {
                0,
                new List<InventoryItem>()
                {
                    new InventoryItem(ColonyBuiltIn.ItemTypes.SCIENCEBAGLIFE.Id, 4)
                }
            }
        };

        public Dictionary<int, List<IResearchableCondition>> Conditions => new Dictionary<int, List<IResearchableCondition>>()
        {
            {
                0,
                new List<IResearchableCondition>()
                {
                    new HappinessCondition() { Threshold = 50 }
                }
            }
        };

        public Dictionary<int, List<string>> Dependancies => new Dictionary<int, List<string>>()
        {
            {
                0,
                new List<string>()
                {
                    ColonyBuiltIn.Research.HERBFARMING,
                    ColonyBuiltIn.Research.FLAXFARMING,
                    ColonyBuiltIn.Research.SCIENCEBAGLIFE
                }
            }
        };

        public Dictionary<int, List<RecipeUnlock>> Unlocks => new Dictionary<int, List<RecipeUnlock>>()
        {
            {
                1,
                new List<RecipeUnlock>()
                {
                    new RecipeUnlock(ApothecaryRegister.JOB_ITEM_KEY, ERecipeUnlockType.Recipe),
                    new RecipeUnlock(SettlersBuiltIn.ItemTypes.BANDAGE, ERecipeUnlockType.Recipe)
                }
            }
        };

        public int NumberOfLevels => 1;

        public float BaseValue => 1f;

        public int BaseIterationCount => 10;

        public bool AddLevelToName => true;

        public string name => GameLoader.NAMESPACE + ".Apothecaries";

        public Dictionary<int, List<(string, RecipeUnlockClient.EType)>> AdditionalUnlocks => new Dictionary<int, List<(string, RecipeUnlockClient.EType)>>();

        public void BeforeRegister()
        {
            
        }

        public void OnRegister()
        {

        }

        public void ResearchComplete(object sender, ResearchCompleteEventArgs e)
        {
            //e.Manager.Colony.RecipeData.UnlockedOptionalRecipes.Add(new Recipes.RecipeKey(ApothecaryRegister.JOB_RECIPE));
        }
    }

    public class AdvancedApothecaryResearch : IPandaResearch
    {
        public string IconDirectory => GameLoader.ICON_PATH;

        public Dictionary<int, List<InventoryItem>> RequiredItems => new Dictionary<int, List<InventoryItem>>()
        {
            {
                0,
                new List<InventoryItem>()
                {
                    new InventoryItem(ColonyBuiltIn.ItemTypes.SCIENCEBAGLIFE.Id, 4),
                    new InventoryItem(ColonyBuiltIn.ItemTypes.SCIENCEBAGADVANCED.Id, 2)
                }
            }
        };

        public Dictionary<int, List<IResearchableCondition>> Conditions => new Dictionary<int, List<IResearchableCondition>>()
        {
            {
                0,
                new List<IResearchableCondition>()
                {
                    new HappinessCondition() { Threshold = 60 }
                }
            }
        };

        public Dictionary<int, List<string>> Dependancies => new Dictionary<int, List<string>>()
        {
            {
                0,
                new List<string>()
                {
                    SettlersBuiltIn.Research.APOTHECARIES1,
                    ColonyBuiltIn.Research.SCIENCEBAGADVANCED
                }
            }
        };

        public Dictionary<int, List<RecipeUnlock>> Unlocks => new Dictionary<int, List<RecipeUnlock>>()
        {
            {
                0,
                new List<RecipeUnlock>()
                {
                    new RecipeUnlock(SettlersBuiltIn.ItemTypes.ANITBIOTIC, ERecipeUnlockType.Recipe),
                    new RecipeUnlock(SettlersBuiltIn.ItemTypes.TREATEDBANDAGE, ERecipeUnlockType.Recipe)
                }
            }
        };

        public int NumberOfLevels => 1;

        public float BaseValue => 1f;

        public int BaseIterationCount => 10;

        public bool AddLevelToName => true;

        public string name => GameLoader.NAMESPACE + ".AdvancedApothecary";

        public Dictionary<int, List<(string, RecipeUnlockClient.EType)>> AdditionalUnlocks => new Dictionary<int, List<(string, RecipeUnlockClient.EType)>>();

        public void BeforeRegister()
        {
            
        }

        public void OnRegister()
        {

        }

        public void ResearchComplete(object sender, ResearchCompleteEventArgs e)
        {
            //e.Manager.Colony.RecipeData.UnlockedOptionalRecipes.Add(new Recipes.RecipeKey(Anitbiotic.Item.name));
            //e.Manager.Colony.RecipeData.UnlockedOptionalRecipes.Add(new Recipes.RecipeKey(TreatedBandage.Item.name));
        }
    }

    [ModLoader.ModManager]
    public static class ApothecaryRegister
    {
        public static string JOB_NAME = GameLoader.NAMESPACE + ".Apothecary";
        public static string JOB_ITEM_KEY = GameLoader.NAMESPACE + ".ApothecaryTable";

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterItemTypesDefined, GameLoader.NAMESPACE + ".Jobs.ApothecaryRegister.RegisterJobs")]
        [ModLoader.ModCallbackProvidesFor("pipliz.server.loadresearchables")]
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
            results.Add(new RecipeResult(ApothecaryRegister.JOB_ITEM_KEY, 1));
        }

        public List<RecipeItem> requires { get; private set; } = new List<RecipeItem>();
        public List<RecipeResult> results { get; private set; } = new List<RecipeResult>();
        public CraftPriority defaultPriority => CraftPriority.Medium;
        public bool isOptional => true;
        public int defaultLimit => 5;
        public string Job => ColonyBuiltIn.NpcTypes.CRAFTER;
        public string name => ApothecaryRegister.JOB_ITEM_KEY;
        public List<string> JobBlock => new List<string>()
        {
            ColonyBuiltIn.ItemTypes.WORKBENCH
        };
    }
}