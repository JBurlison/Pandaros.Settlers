﻿using System;
using NPC;
using Pandaros.Settlers.ColonyManagement;
using Pandaros.Settlers.Entities;
using Pipliz;

namespace Pandaros.Settlers.AI
{
    public static class SettlerEvaluation
    {
        public static float SpawnChance(ColonyState state)
        {
            var chance        = .3f;
            var remainingBeds = ServerManager.BlockEntityTracker.BedTracker.CalculateBedCount(state.ColonyRef) - state.ColonyRef.FollowerCount;

            if (remainingBeds < 1)
                chance -= 0.1f;

            if (remainingBeds >= state.MaxPerSpawn)
                chance += 0.3f;
            else if (remainingBeds > SettlerManager.MIN_PERSPAWN)
                chance += 0.15f;

            var jobCount = state.ColonyRef.JobFinder.OpenJobCount;

            if (jobCount > state.MaxPerSpawn)
                chance += 0.4f;
            else if (jobCount > SettlerManager.MIN_PERSPAWN)
                chance += 0.1f;
            else
                chance -= 0.2f;

            if (state.ColonyRef.HappinessData.CachedHappiness > 100)
                chance += 0.4f;
            else if (state.ColonyRef.HappinessData.CachedHappiness > 50)
                chance += 0.2f;
            else if (state.ColonyRef.HappinessData.CachedHappiness < -50)
                chance -= 0.4f;
            else if (state.ColonyRef.HappinessData.CachedHappiness < 0)
                chance -= 0.2f;

            if (state.Difficulty != GameDifficulty.Easy && state.Difficulty != GameDifficulty.Normal)
                if (state.ColonyRef.InSiegeMode ||
                    state.ColonyRef.LastSiegeModeSpawn != 0 &&
                    Time.SecondsSinceStartDouble - state.ColonyRef.LastSiegeModeSpawn > TimeSpan.FromMinutes(5).TotalSeconds)
                    chance -= 0.4f;

            return chance;
        }
    }
}