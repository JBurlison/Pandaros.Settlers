﻿using Jobs;
using NPC;
using Pandaros.API;
using Pandaros.API.Items;
using Pandaros.API.Jobs.Roaming;
using Pandaros.API.Models;
using Pipliz;
using Recipes;
using System.Collections.Generic;
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
                printName = "Machinist Day",
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

        public virtual NPCType NPCType => _Settings;

        public virtual InventoryItem RecruitmentItem => new InventoryItem(ColonyBuiltIn.ItemTypes.COPPERTOOLS.Name);

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
        static NPCType _Settings;
        public override bool ToSleep => TimeCycle.IsDay;
        public override float NPCShopGameHourMinimum { get { return TimeCycle.Settings.SleepTimeEnd; } }
        public override float NPCShopGameHourMaximum { get { return TimeCycle.Settings.SleepTimeStart; } }

        public override ItemTypes.ItemType[] BlockTypes => new[]
        {
            ItemTypes.GetType(MachinistNight.JOB_ITEM_KEY)
        };

        static MachinistSettingsNight()
        {
            NPCType.AddSettings(new NPCTypeStandardSettings
            {
                keyName = MachinistNight.JOB_NAME,
                printName = "Machinist Night",
                maskColor1 = new Color32(127, 74, 24, 255),
                type = NPCTypeID.GetNextID(),
                inventoryCapacity = 1f
            });

            _Settings = NPCType.GetByKeyNameOrDefault(MachinistNight.JOB_NAME);
        }

        public override NPCType NPCType => _Settings;

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
        public override List<ItemId> OkStatus => new List<ItemId>
            {
                ItemId.GetItemId(GameLoader.NAMESPACE + ".Refuel"),
                ItemId.GetItemId(GameLoader.NAMESPACE + ".Reload"),
                ItemId.GetItemId(GameLoader.NAMESPACE + ".Repairing"),
                ItemId.GetItemId("Pandaros.API.Waiting")
            };
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
            return base.CalculateGoal(ref state, false);
        }
    }

    public class MachinistTexture : CSTextureMapping
    {
        public const string NAME = GameLoader.NAMESPACE + ".MachinistBenchTop";
        public override string name => NAME;
        public override string albedo => GameLoader.BLOCKS_ALBEDO_PATH + "MachinistBenchTop.png";
        public override string normal => GameLoader.BLOCKS_NORMAL_PATH + "MachinistBenchTop.png";
        public override string height => GameLoader.BLOCKS_HEIGHT_PATH + "MachinistBenchTop.png";
    }

    public class MachinistNightTexture : CSTextureMapping
    {
        public const string NAME = GameLoader.NAMESPACE + ".MachinistBenchTopNight";
        public override string name => NAME;
        public override string albedo => GameLoader.BLOCKS_ALBEDO_PATH + "MachinistBenchTopNight.png";
        public override string normal => GameLoader.BLOCKS_NORMAL_PATH + "MachinistBenchTop.png";
        public override string height => GameLoader.BLOCKS_HEIGHT_PATH + "MachinistBenchTop.png";
    }

    public class MachinistJobTypeNight : CSType
    {
        public override string name => MachinistNight.JOB_ITEM_KEY;
        public override string icon => GameLoader.ICON_PATH + "MachinistBenchNight.png";
        public override string onPlaceAudio => "stonePlace";
        public override string onRemoveAudio => "stoneDelete";
        public override string sideall => ColonyBuiltIn.ItemTypes.STONEBRICKS;
        public override string sideyp => MachinistNightTexture.NAME;
        public override List<string> categories => new List<string>() { "job", GameLoader.NAMESPACE };
    }

    public class MachinistJobType : CSType
    {
        public override string name => MachinistDay.JOB_ITEM_KEY;
        public override string icon => GameLoader.ICON_PATH + "MachinistBench.png";
        public override string onPlaceAudio => "stonePlace";
        public override string onRemoveAudio => "stoneDelete";
        public override string sideall => ColonyBuiltIn.ItemTypes.STONEBRICKS;
        public override string sideyp => MachinistTexture.NAME;
        public override List<string> categories => new List<string>() { "job", GameLoader.NAMESPACE };
    }

    public class MachinisNighttRecipe : ICSRecipe
    {
        public string name => MachinistNight.JOB_RECIPE;
        public List<RecipeItem> requires => new List<RecipeItem>()
        {
            { new RecipeItem(ColonyBuiltIn.ItemTypes.BRONZEINGOT.Name, 2) },
            { new RecipeItem(ColonyBuiltIn.ItemTypes.IRONWROUGHT.Name, 2) },
            { new RecipeItem(ColonyBuiltIn.ItemTypes.COPPERTOOLS.Name, 1) },
            { new RecipeItem(ColonyBuiltIn.ItemTypes.STONEBRICKS.Name, 4) }
        };

        public List<RecipeResult> results => new List<RecipeResult>()
        {
            { new RecipeResult(MachinistNight.JOB_ITEM_KEY, 1) }
        };

        public CraftPriority defaultPriority => CraftPriority.High;
        public bool isOptional => false;
        public int defaultLimit => 2;
        public string Job => AdvancedCrafterRegister.JOB_NAME;
    }

    public class MachinistRecipe : ICSRecipe
    {
        public string name => MachinistDay.JOB_RECIPE;

        public List<RecipeItem> requires => new List<RecipeItem>()
        {
            { new RecipeItem(ColonyBuiltIn.ItemTypes.BRONZEINGOT.Name, 2) },
            { new RecipeItem(ColonyBuiltIn.ItemTypes.IRONWROUGHT.Name, 2) },
            { new RecipeItem(ColonyBuiltIn.ItemTypes.COPPERTOOLS.Name, 1) },
            { new RecipeItem(ColonyBuiltIn.ItemTypes.STONEBRICKS.Name, 4) }
        };

        public List<RecipeResult> results => new List<RecipeResult>()
        {
            { new RecipeResult(MachinistDay.JOB_ITEM_KEY, 1) }
        };

        public CraftPriority defaultPriority => CraftPriority.High;
        public bool isOptional => false;
        public int defaultLimit => 2;
        public string Job => AdvancedCrafterRegister.JOB_NAME;
    }
}
