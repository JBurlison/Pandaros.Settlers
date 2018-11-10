using BlockTypes;
using Jobs;
using NPC;
using Pandaros.Settlers.Items;
using Pandaros.Settlers.Jobs.Roaming;
using Pipliz;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

namespace Pandaros.Settlers.Jobs
{
    [ModLoader.ModManager]
    public static class MachinistModEntries
    {
        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterItemTypesDefined, GameLoader.NAMESPACE + ".Jobs.MachinistModEntries")]
        [ModLoader.ModCallbackProvidesFor("create_savemanager")]
        public static void AfterDefiningNPCTypes()
        {
            ServerManager.BlockEntityCallbacks.RegisterEntityManager(
                new BlockJobManager<MachinistDay>(
                    new MachinistSettingsDay(),
                    (setting, pos, type, bytedata) => new MachinistDay(setting, pos, type, bytedata),
                    (setting, pos, type, colony) => new MachinistDay(setting, pos, type, colony)
                )
            );

            ServerManager.BlockEntityCallbacks.RegisterEntityManager(
                new BlockJobManager<MachinistNight>(
                    new MachinistSettingsNight(),
                    (setting, pos, type, bytedata) => new MachinistNight(setting, pos, type, bytedata),
                    (setting, pos, type, colony) => new MachinistNight(setting, pos, type, colony)
                )
            );
        }
    }

    public class MachinistSettingsDay : IBlockJobSettings
    {
        static NPCType _Settings;
        public virtual float NPCShopGameHourMinimum { get { return TimeCycle.Settings.SleepTimeEnd; } }
        public virtual float NPCShopGameHourMaximum { get { return TimeCycle.Settings.SleepTimeStart; } }

        static MachinistSettingsDay()
        {
            NPCType.AddSettings(new NPCTypeStandardSettings
            {
                keyName = MachinistDay.JOB_NAME,
                printName = "Machinist",
                maskColor1 = new Color32(242, 132, 29, 255),
                type = NPCTypeID.GetNextID(),
                inventoryCapacity = 1f
            });

            _Settings = NPCType.GetByKeyNameOrDefault(MachinistDay.JOB_NAME);
        }

        public virtual ItemTypes.ItemType[] BlockTypes => new[]
        {
            ItemTypes.GetType(MachinistDay.JOB_ITEM_KEY)
        };

        public NPCType NPCType => _Settings;

        public virtual InventoryItem RecruitmentItem => new InventoryItem(BuiltinBlocks.CopperTools);

        public virtual bool ToSleep => !TimeCycle.IsDay;

        public Pipliz.Vector3Int GetJobLocation(BlockJobInstance instance)
        {
            if (instance is RoamingJob roamingJob)
                return roamingJob.OriginalPosition;

            return Pipliz.Vector3Int.invalidPos;
        }

        public void OnGoalChanged(BlockJobInstance instance, NPCBase.NPCGoal goalOld, NPCBase.NPCGoal goalNew)
        {
            
        }

        public void OnNPCAtJob(BlockJobInstance instance, ref NPCBase.NPCState state)
        {
            instance.OnNPCAtJob(ref state);
        }

        public void OnNPCAtStockpile(BlockJobInstance instance, ref NPCBase.NPCState state)
        {
            
        }
    }

    public class MachinistSettingsNight : MachinistSettingsDay
    {
        public override bool ToSleep => TimeCycle.IsDay;

        public override ItemTypes.ItemType[] BlockTypes => new[]
        {
            ItemTypes.GetType(MachinistNight.JOB_ITEM_KEY)
        };
    }

    public class MachinistDay : RoamingJob
    {
        public static string JOB_NAME = GameLoader.NAMESPACE + ".Machinist";
        public static string JOB_ITEM_KEY = GameLoader.NAMESPACE + ".MachinistBench";
        public static string JOB_RECIPE = JOB_ITEM_KEY + ".recipe";

        public MachinistDay(IBlockJobSettings settings, Pipliz.Vector3Int position, ItemTypes.ItemType type, ByteReader reader) :
            base(settings, position, type, reader)
        {
        }

        public MachinistDay(IBlockJobSettings settings, Pipliz.Vector3Int position, ItemTypes.ItemType type, Colony colony) :
            base(settings, position, type, colony)
        {
        }


        public override List<string> ObjectiveCategories => new List<string>() { Items.Machines.MachineConstants.MECHANICAL };
        public override string JobItemKey => JOB_ITEM_KEY;
        public override List<uint> OkStatus => new List<uint>
            {
                GameLoader.Refuel_Icon,
                GameLoader.Reload_Icon,
                GameLoader.Repairing_Icon,
                GameLoader.Waiting_Icon
            };

        public override NPCBase.NPCGoal CalculateGoal(ref NPCBase.NPCState state)
        {
            if (TimeCycle.IsDay)
                return NPCBase.NPCGoal.Job;
            else
                return NPCBase.NPCGoal.Bed;
        }
    }

    public class MachinistNight : MachinistDay
    {
        public new static string JOB_NAME = GameLoader.NAMESPACE + ".NightMachinist";
        public new static string JOB_ITEM_KEY = GameLoader.NAMESPACE + ".NightMachinistBench";
        public new static string JOB_RECIPE = JOB_ITEM_KEY + ".recipe";

        public override string JobItemKey => JOB_ITEM_KEY;

        public MachinistNight(IBlockJobSettings settings, Pipliz.Vector3Int position, ItemTypes.ItemType type, ByteReader reader) :
            base(settings, position, type, reader)
        {
        }

        public MachinistNight(IBlockJobSettings settings, Pipliz.Vector3Int position, ItemTypes.ItemType type, Colony colony) :
            base(settings, position, type, colony)
        {
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
        public override string Name => MachinistDay.JOB_ITEM_KEY;
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
        public virtual string Name => MachinistDay.JOB_RECIPE;

        public Dictionary<string, int> Requirements => new Dictionary<string, int>()
        {
            { "bronzeingot", 2 },
            { "ironwrought", 2 },
            { "coppertools", 1 },
            { "stonebricks", 4 }
        };

        public Dictionary<string, int> Results => new Dictionary<string, int>()
        {
            { MachinistDay.JOB_ITEM_KEY, 1 }
        };

        public CraftPriority Priority => CraftPriority.Medium;
        public bool IsOptional => true;
        public int DefautLimit => 2;
        public string Job => ItemFactory.JOB_CRAFTER;
    }
}
