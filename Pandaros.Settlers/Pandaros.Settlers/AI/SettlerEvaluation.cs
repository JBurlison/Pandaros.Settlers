using Pandaros.API;
using Pandaros.API.Entities;
using Pandaros.Settlers.ColonyManagement;
using Pipliz;
using System;
using System.Linq;

namespace Pandaros.Settlers.AI
{
    public static class SettlerEvaluation
    {
        public static float SpawnChance(ColonyState state)
        {
            var chance        = .3f;
            var remainingBeds = state.ColonyRef.BedTracker.CalculateTotalBedCount() - state.ColonyRef.FollowerCount;

            if (remainingBeds < 1)
                chance -= 0.1f;

            if (remainingBeds >= SettlerManager.MaxPerSpawn(state.ColonyRef))
                chance += 0.3f;
            else if (remainingBeds > SettlerManager.MIN_PERSPAWN)
                chance += 0.15f;

            var jobCount = state.ColonyRef.GetJobCounts().Select(kvp => kvp.Value.AvailableCount).Sum();

            if (jobCount > SettlerManager.MaxPerSpawn(state.ColonyRef))
                chance += 0.4f;
            else if (jobCount > SettlerManager.MIN_PERSPAWN)
                chance += 0.1f;
            else
                chance -= 0.2f;

            chance += SettlerChance.GetSettlerChance(state.ColonyRef);

            if (state.Difficulty != GameDifficulty.Normal)
                if (state.ColonyRef.InSiegeMode ||
                    state.ColonyRef.LastSiegeModeSpawn != 0 &&
                    Time.SecondsSinceStartDouble - state.ColonyRef.LastSiegeModeSpawn > TimeSpan.FromMinutes(5).TotalSeconds)
                    chance -= 0.4f;

            return chance;
        }
    }
}