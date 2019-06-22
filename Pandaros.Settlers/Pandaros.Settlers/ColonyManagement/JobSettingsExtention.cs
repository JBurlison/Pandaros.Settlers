using Jobs;
using Pandaros.Settlers.Research;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandaros.Settlers
{
    public static class JobSettingsExtention
    {
        public static bool TryGetNPCCraftSettings(this NPC.NPCBase npc, out CraftingJobSettings settings)
        {
            return TryGetNPCCraftSettings(npc.Job, out settings);
        }

        public static bool TryGetNPCCraftSettings(this IJob job, out CraftingJobSettings settings)
        {
            if (job != null && job is CraftingJobInstance craftingJob)
            {
                settings = (CraftingJobSettings)craftingJob.Settings;
                return true;
            }

            settings = null;
            return false;
        }

        public static bool TryGetNPCCraftDefaultSettings(this NPC.NPCBase npc, out CraftingJobSettings settings)
        {
            return TryGetNPCCraftDefaultSettings(npc.Job, out settings);
        }

        public static bool TryGetNPCCraftDefaultSettings(this IJob job, out CraftingJobSettings settings)
        {
            if (job != null && job is CraftingJobInstance craftingJob)
            {
                settings = (CraftingJobSettings)craftingJob.Settings;

                if (ServerManager.BlockEntityCallbacks.TryGetCraftJobSettings(settings.NPCTypeKey, out settings))
                    return true;
            }

            settings = null;
            return false;
        }

        public static bool TryGetNPCGuardSettings(this NPC.NPCBase npc, out GuardJobSettings settings)
        {
            return TryGetNPCGuardSettings(npc.Job, out settings);
        }

        public static bool TryGetNPCGuardSettings(this IJob job, out GuardJobSettings settings)
        {
            if (job != null && job is GuardJobInstance guardJob)
            {
                settings = (GuardJobSettings)guardJob.Settings;
                return true;
            }

            settings = null;
            return false;
        }

        public static bool TryGetNPCGuardDefaultSettings(this NPC.NPCBase npc, out GuardJobSettings settings)
        {
            return TryGetNPCGuardDefaultSettings(npc.Job, out settings);
        }

        public static bool TryGetNPCGuardDefaultSettings(this IJob job, out GuardJobSettings settings)
        {
            if (job != null && job is GuardJobInstance guardJob)
            {
                settings = (GuardJobSettings)guardJob.Settings;

                if (ServerManager.BlockEntityCallbacks.TryGetGuardJobSettings(settings.NPCTypeKey, out settings))
                    return true;
            }

            settings = null;
            return false;
        }

        public static bool TryGetGuardJobSettings(this BlockEntities.BlockEntityCallbacks callbacks, string name, out GuardJobSettings guardJobSettings)
        {
            guardJobSettings = null;

            var guardJobInstance = callbacks.AutoLoadedInstances.Where(o => o is BlockJobManager<GuardJobInstance> manager && manager.Settings is GuardJobSettings set && set.NPCTypeKey == name).FirstOrDefault() as BlockJobManager<GuardJobInstance>;

            if (guardJobInstance == null)
                PandaLogger.Log(ChatColor.yellow, "Unable to find guard job settings for {0}", name);
            else
                guardJobSettings = guardJobInstance.Settings as GuardJobSettings;

            return guardJobSettings != null;
        }

        public static bool TryGetCraftJobSettings(this BlockEntities.BlockEntityCallbacks callbacks, string name, out CraftingJobSettings craftingJobSettings)
        {
            craftingJobSettings = null;

            var craftJobInstance = callbacks.AutoLoadedInstances.FirstOrDefault(o => o is BlockJobManager<CraftingJobInstance> manager && manager.Settings is CraftingJobSettings set && set.NPCTypeKey == name) as BlockJobManager<CraftingJobInstance>;

            if (craftJobInstance == null)
                PandaLogger.Log(ChatColor.yellow, "Unable to find craft job settings for {0}", name);
            else
                craftingJobSettings = craftJobInstance.Settings as CraftingJobSettings;

            return craftingJobSettings != null;
        }

        public static void ApplyJobResearch(this NPC.NPCBase npc)
        {
            ApplyJobResearch(npc?.Job);
        }

        public static void ApplyJobResearch(this IJob job)
        {
            if (job == null || !job.IsValid)
                return;

            job.Owner.TemporaryData.TryGetAs(GameLoader.NAMESPACE + ".MasterOfAll", out float masterOfAll);

            if (job.TryGetNPCGuardSettings(out var guardSettings) && job.TryGetNPCGuardDefaultSettings(out var defaultGuardSettings))
            {
                var key = guardSettings.NPCTypeKey.Replace("day", "").Replace("night", "");
                job.Owner.TemporaryData.TryGetAs(key, out float cooldownReduction);
                var total = masterOfAll + cooldownReduction;

                if (total != 0)
                {
                    guardSettings.CooldownShot = defaultGuardSettings.CooldownShot - (defaultGuardSettings.CooldownShot * total);
                    var maxCooldown = defaultGuardSettings.CooldownShot / 2;

                    if (guardSettings.CooldownShot < maxCooldown)
                        guardSettings.CooldownShot = maxCooldown;
                }
            }
            else if (job.TryGetNPCCraftSettings(out var craftSettings) && job.TryGetNPCCraftDefaultSettings(out var craftingJobDefaultSettings))
            {
                job.Owner.TemporaryData.TryGetAs(craftSettings.NPCTypeKey, out float cooldownReduction);
                var total = masterOfAll + cooldownReduction;

                if (total != 0)
                {
                    craftSettings.CraftingCooldown = craftingJobDefaultSettings.CraftingCooldown - (craftingJobDefaultSettings.CraftingCooldown * total);
                    var maxCooldown = craftingJobDefaultSettings.CraftingCooldown / 2;

                    if (craftSettings.CraftingCooldown < maxCooldown)
                        craftSettings.CraftingCooldown = maxCooldown;
                }
            }
        }
    }
}
