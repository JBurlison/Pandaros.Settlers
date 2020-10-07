using Jobs;
using NPC;
using Pandaros.API;
using Pandaros.API.Models;
using Pandaros.API.Research;
using Recipes;
using Science;
using Shared;
using System.Collections.Generic;
using UnityEngine;

namespace Pandaros.Settlers.Jobs
{
    public class SorcererResearch : IPandaResearch
    {
        public Dictionary<int, List<InventoryItem>> RequiredItems => new Dictionary<int, List<InventoryItem>>()
        {
            {
                0,
                new List<InventoryItem>()
                {
                    new InventoryItem(SettlersBuiltIn.ItemTypes.ADAMANTINENUGGET.Id, 4),
                    new InventoryItem(ColonyBuiltIn.ItemTypes.SCIENCEBAGCOLONY.Id),
                    new InventoryItem(ColonyBuiltIn.ItemTypes.SCIENCEBAGBASIC.Id, 3),
                    new InventoryItem(ColonyBuiltIn.ItemTypes.SCIENCEBAGADVANCED.Id)
                }
            }
        };
        public Dictionary<int, List<IResearchableCondition>> Conditions => new Dictionary<int, List<IResearchableCondition>>()
        {
            {
                0,
                new List<IResearchableCondition>()
                {
                    new HappinessCondition() { Threshold = 90 },
                    new ColonistCountCondition() { Threshold = 250 }
                }
            }
        };
        public int NumberOfLevels => 1;
        public float BaseValue => 0.05f;
        public Dictionary<int, List<string>> Dependancies => new Dictionary<int, List<string>>()
        {
            {
                0,
                new List<string>()
                {
                    SettlersBuiltIn.Research.ARMORSMITHING4,
                    SettlersBuiltIn.Research.SWORDSMITHING4,
                    SettlersBuiltIn.Research.ELEMENTIUM1,
                    ColonyBuiltIn.Research.SCIENCEBAGADVANCED,
                    ColonyBuiltIn.Research.SCIENCEBAGCOLONY
                }
            }
        };

        public int BaseIterationCount => 300;
        public bool AddLevelToName => true;
        public string name => GameLoader.NAMESPACE + ".Sorcerer";
        public string IconDirectory => GameLoader.ICON_PATH;

        public Dictionary<int, List<RecipeUnlock>> Unlocks => new Dictionary<int, List<RecipeUnlock>>()
        {
            {
                1,
                new List<RecipeUnlock>()
                {
                    new RecipeUnlock(SettlersBuiltIn.ItemTypes.SORCERERTABLE, ERecipeUnlockType.Recipe),
                    new RecipeUnlock(SettlersBuiltIn.ItemTypes.ADAMANTINE, ERecipeUnlockType.Recipe)
                }
            }
        };

        public Dictionary<int, List<(string, RecipeUnlockClient.EType)>> AdditionalUnlocks => new Dictionary<int, List<(string, RecipeUnlockClient.EType)>>();

        public void BeforeRegister()
        {
            
        }

        public void OnRegister()
        {

        }

        public void ResearchComplete(object sender, ResearchCompleteEventArgs e)
        {
            
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
            results.Add(new RecipeResult(SorcererRegister.JOB_ITEM_KEY, 1));
        }

        public List<RecipeItem> requires { get; private set; } = new List<RecipeItem>();
        public List<RecipeResult> results { get; private set; } = new List<RecipeResult>();
        public CraftPriority defaultPriority => CraftPriority.Medium;
        public bool isOptional => true;
        public int defaultLimit => 5;
        public string Job => ColonyBuiltIn.NpcTypes.CRAFTER;
        public string name => SorcererRegister.JOB_ITEM_KEY;

        public List<string> JobBlock => new List<string>();
    }
}