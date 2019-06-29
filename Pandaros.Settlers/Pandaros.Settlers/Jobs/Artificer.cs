using Jobs;
using NPC;
using Pandaros.Settlers.Items;
using Pandaros.Settlers.Jobs.Roaming;
using Pandaros.Settlers.Models;
using Pandaros.Settlers.Research;
using Pipliz;
using Recipes;
using Science;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Pandaros.Settlers.Jobs
{
    [ModLoader.ModManager]
    public static class ArtificerModEntries
    {
        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterItemTypesDefined, GameLoader.NAMESPACE + ".Jobs.ArtificerModEntries")]
        [ModLoader.ModCallbackProvidesFor("create_savemanager")]
        public static void AfterDefiningNPCTypes()
        {
            ServerManager.BlockEntityCallbacks.RegisterEntityManager(
                new BlockJobManager<Artificer>(
                    new ArtificerSettings(),
                    (setting, pos, type, bytedata) => new Artificer(setting, pos, type, bytedata),
                    (setting, pos, type, colony) => new Artificer(setting, pos, type, colony)
                )
            );
        }
    }

    public class ArtificierResearch : PandaResearch
    {
        public override Dictionary<int, List<InventoryItem>> RequiredItems => new Dictionary<int, List<InventoryItem>>()
            {
                {
                    0,
                    new List<InventoryItem>()
                    {
                        new InventoryItem(SettlersBuiltIn.ItemTypes.ADAMANTINE.Id, 3),
                        new InventoryItem(SettlersBuiltIn.ItemTypes.MANA.Id, 2),
                        new InventoryItem(ColonyBuiltIn.ItemTypes.COATEDPLANKS.Id, 3),
                        new InventoryItem(SettlersBuiltIn.ItemTypes.REFINEDEMERALD.Id, 3),
                        new InventoryItem(SettlersBuiltIn.ItemTypes.REFINEDRUBY.Id, 3),
                        new InventoryItem(SettlersBuiltIn.ItemTypes.REFINEDSAPPHIRE.Id, 3)
                    }
                }
            };

        public override Dictionary<int, List<string>> Dependancies => new Dictionary<int, List<string>>()
            {
                {
                    0,
                    new List<string>()
                    {
                        SettlersBuiltIn.Research.SORCERER1,
                        SettlersBuiltIn.Research.MACHINES1
                    }
                }
            };

        public override Dictionary<int, List<RecipeUnlock>> Unlocks => new Dictionary<int, List<RecipeUnlock>>()
        {
            {
                1,
                new List<RecipeUnlock>()
                {
                    new RecipeUnlock(SettlersBuiltIn.ItemTypes.ARTIFICERBENCH, ERecipeUnlockType.Recipe),
                    new RecipeUnlock(SettlersBuiltIn.ItemTypes.MANAPIPE, ERecipeUnlockType.Recipe),
                    new RecipeUnlock(SettlersBuiltIn.ItemTypes.MANAPUMP, ERecipeUnlockType.Recipe),
                    new RecipeUnlock(SettlersBuiltIn.ItemTypes.MANATANK, ERecipeUnlockType.Recipe),
                    new RecipeUnlock(SettlersBuiltIn.ItemTypes.MAGICWAND, ERecipeUnlockType.Recipe)
                }
            }
        };

        public override int NumberOfLevels => 1;

        public override int BaseIterationCount => 250;

        public override string name => GameLoader.NAMESPACE + ".Artificer";

        public override string IconDirectory => GameLoader.ICON_PATH;
    }

    public class ArtificierBenchTexture : ICSTextureMapping
    {
        public string emissive => GameLoader.BLOCKS_EMISSIVE_PATH + "Artificer_emissive.png";

        public string albedo => GameLoader.BLOCKS_ALBEDO_PATH + "ArtificerTable.png";

        public string normal => GameLoader.BLOCKS_NORMAL_PATH + "Artificer_normal.png";

        public string height => GameLoader.BLOCKS_HEIGHT_PATH + "Artificer_height.png";

        public string name => GameLoader.NAMESPACE + ".ArtificerBench";
    }

    public class ArtificerBench : CSType
    {
        public override string name { get; set; } = GameLoader.NAMESPACE + ".ArtificerBench";
        public override string icon { get; set; } = GameLoader.ICON_PATH + "Artificer.png";
        public override string onPlaceAudio => "woodPlace";
        public override string onRemoveAudio => "woodDeleteLight";
        public override string sideall => "coatedplanks";
        public override string sideyp => GameLoader.NAMESPACE + ".ArtificerBench";
        public override List<string> categories => new List<string>() { "job", GameLoader.NAMESPACE };
    }

    public class ArtificerBenchRecipe : ICSRecipe
    {
        public List<RecipeItem> requires => new List<RecipeItem>()
        {
            new RecipeItem(SettlersBuiltIn.ItemTypes.ADAMANTINE.Id, 6),
            new RecipeItem(SettlersBuiltIn.ItemTypes.MANA.Id, 20),
            new RecipeItem(ColonyBuiltIn.ItemTypes.COATEDPLANKS.Id, 10),
            new RecipeItem(SettlersBuiltIn.ItemTypes.REFINEDEMERALD.Id, 3),
            new RecipeItem(SettlersBuiltIn.ItemTypes.REFINEDRUBY.Id, 3),
            new RecipeItem(SettlersBuiltIn.ItemTypes.REFINEDSAPPHIRE.Id, 3)
        };

        public List<RecipeResult> results => new List<RecipeResult>()
        {
            new RecipeResult(GameLoader.NAMESPACE + ".ArtificerBench")
        };

        public CraftPriority defaultPriority => CraftPriority.Medium;

        public bool isOptional => true;

        public int defaultLimit => 5;

        public string Job => SorcererRegister.JOB_NAME;

        public string name => GameLoader.NAMESPACE + ".ArtificerBench";
    }

    public class Artificer : RoamingJob
    {
        public static string JOB_NAME = GameLoader.NAMESPACE + ".Artificer";
        public static string JOB_ITEM_KEY = GameLoader.NAMESPACE + ".ArtificerBench";

        public Artificer(IBlockJobSettings settings, Pipliz.Vector3Int position, ItemTypes.ItemType type, ByteReader reader) :
            base(settings, position, type, reader)
        {
        }

        public Artificer(IBlockJobSettings settings, Pipliz.Vector3Int position, ItemTypes.ItemType type, Colony colony) :
            base(settings, position, type, colony)
        {
        }


        public override List<string> ObjectiveCategories => new List<string>() { "ManaMachine" };
        public override string JobItemKey => JOB_ITEM_KEY;
        public override List<ItemId> OkStatus => new List<ItemId>
            {
                ItemId.GetItemId(GameLoader.NAMESPACE + ".Refuel"),
                ItemId.GetItemId(GameLoader.NAMESPACE + ".Repairing"),
                ItemId.GetItemId(GameLoader.NAMESPACE + ".Waiting")
            };
    }

    public class ArtificerSettings : IBlockJobSettings
    { 
        static NPCType _Settings;

        static ArtificerSettings()
        {
            NPCType.AddSettings(new NPCTypeStandardSettings
            {
                keyName = Artificer.JOB_NAME,
                printName = "Artificer",
                maskColor1 = new Color32(81, 55, 102, 255),
                type = NPCTypeID.GetNextID(),
                inventoryCapacity = 1f
            });

            _Settings = NPCType.GetByKeyNameOrDefault(Artificer.JOB_NAME);
        }

        public ItemTypes.ItemType[] BlockTypes => new[]
        {
            ItemTypes.GetType(Artificer.JOB_ITEM_KEY)
        };

        public NPCType NPCType => _Settings;

        public InventoryItem RecruitmentItem => new InventoryItem(GameLoader.NAMESPACE + ".MagicWand");

        public bool ToSleep => !TimeCycle.IsDay;

        public virtual float NPCShopGameHourMinimum => TimeCycle.Settings.SleepTimeEnd;

        public virtual float NPCShopGameHourMaximum => TimeCycle.Settings.SleepTimeStart;

        public Pipliz.Vector3Int GetJobLocation(BlockJobInstance instance)
        {
            if (instance is RoamingJob roamingJob)
            {
                return roamingJob.OriginalPosition;
            }

            return Pipliz.Vector3Int.invalidPos;
        }

        public void OnNPCAtJob(BlockJobInstance instance, ref NPCBase.NPCState state)
        {
            if (instance is RoamingJob roamingJob)
                 roamingJob.OnNPCAtJob(ref state);
        }

        public void OnGoalChanged(BlockJobInstance instanceBlock, NPCBase.NPCGoal oldGoal, NPCBase.NPCGoal newGoal)
        {
            
        }

        public void OnNPCAtStockpile(BlockJobInstance instance, ref NPCBase.NPCState state)
        {
            
        }
    }

}
