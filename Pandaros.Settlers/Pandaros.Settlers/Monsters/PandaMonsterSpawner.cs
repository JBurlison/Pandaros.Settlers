using General.Settings;
using NPC;
using Pandaros.Settlers.Entities;
using Pipliz;
using Server.AI;
using Server.Monsters;
using Server.NPCs;
using Server.TerrainGeneration;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Pandaros.Settlers.Monsters
{
    [ModLoader.ModManager]
    public class PandaMonsterSpawner : MonsterSpawner
    {
        NPCType[] _monsterTypes = new NPCType[100];

        private Stopwatch maxTimePerTick = new Stopwatch();
        public static Variables MonsterVariables { get; private set; } = new Variables();
        private static double siegeModeCooldown = 3.0;
        public static PandaMonsterSpawner Instance { get; private set; }


        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterWorldLoad, GameLoader.NAMESPACE + ".Monsters.PandaMonsterSpawner.AfterWorldLoad"),
            ModLoader.ModCallbackDependsOn("pipliz.server.monsterspawner.register")]
        private static void AfterWorldLoad()
        {
            Instance = new PandaMonsterSpawner();
            MonsterTracker.MonsterSpawner = Instance;
            PandaLogger.Log("PandaMonsterSpawner Initialized!");
        }

        public new void Update()
        {
            if (!World.Initialized || !ServerManager.WorldSettings.ZombiesEnabled || Server.AI.AIManager.IsBusy() || Managers.MonsterManager.BossActive)
                return;

            WorldSettings worldSettings = ServerManager.WorldSettings;

            bool flag = worldSettings.ZombiesEnabled && (TimeCycle.ShouldSpawnMonsters || worldSettings.MonstersDayTime);
            int num = (!worldSettings.MonstersDoubled) ? 1 : 2;
            double secondsSinceStartDouble = Pipliz.Time.SecondsSinceStartDouble;
            var banners = BannerTracker.GetBanners();

            maxTimePerTick.Reset();
            maxTimePerTick.Start();

            for (int i = 0; i < banners.Count; i++)
            {
                if (maxTimePerTick.Elapsed.TotalMilliseconds > MonsterVariables.MSPerTick)
                    break;

                Banner valueAtIndex = banners.GetValueAtIndex(i);
                var ps = PlayerState.GetPlayerState(valueAtIndex.Owner);

                SpawnForBanner(valueAtIndex, flag, num, secondsSinceStartDouble, false);
            }

            maxTimePerTick.Stop();
        }
        
        public void SpawnForBanner(Banner valueAtIndex, bool flag, int num, double secondsSinceStartDouble, bool force)
        {
            if (valueAtIndex != null && valueAtIndex.KeyLocation.IsValid)
            {
                Colony colony = Colony.Get(valueAtIndex.Owner);

                if (flag)
                {
                    float num2 = GetMaxZombieCount(colony.FollowerCount) * num;

                    if (num2 > 0f)
                    {
                        if (MonsterTracker.MonstersPerPlayer(valueAtIndex.Owner) < num2)
                        {
                            if (colony.InSiegeMode)
                            {
                                if (secondsSinceStartDouble - colony.LastSiegeModeSpawn < siegeModeCooldown)
                                    return;

                                colony.LastSiegeModeSpawn = secondsSinceStartDouble;
                            }

                            SpawnPandaZombie(colony, valueAtIndex, force);
                        }
                    }
                    else
                    {
                        colony.OnZombieSpawn(true);
                    }
                }
                else
                {
                    colony.OnZombieSpawn(true);
                }
            }
        }

        new public static void SpawnZombie(Colony colony, Banner bannerGoal)
        {
            SpawnPandaZombie(colony, bannerGoal);
        }

        public static void SpawnPandaZombie(Colony colony, Banner bannerGoal, bool force = false)
        {
            NPCType typeToSpawn = GetTypeToSpawn((float)colony.FollowerCount);
            float maxSpawnWalkDistance = 500f;

            if (force || TimeCycle.ShouldSpawnMonsters)
            {
                NPCTypeMonsterSettings nPCTypeMonsterSettings;
                typeToSpawn.TryGetSettings(out nPCTypeMonsterSettings);


                if (force)
                    maxSpawnWalkDistance = TimeCycle.NightLengthInRealSeconds * (nPCTypeMonsterSettings.movementSpeed * 0.85f) + (float)bannerGoal.SafeRadius;
                else
                    maxSpawnWalkDistance = TimeCycle.NightLengthLeftInRealSeconds * (nPCTypeMonsterSettings.movementSpeed * 0.85f) + (float)bannerGoal.SafeRadius;
            }

            switch (MonsterSpawner.TryGetSpawnLocation(bannerGoal, maxSpawnWalkDistance, out Vector3Int start))
            {
                case MonsterSpawner.ESpawnResult.Success:
                    {
                        if (AIManager.ZombiePathFinder.TryFindPath(start, bannerGoal.KeyLocation, out Path path, 2000000000) == EPathFindingResult.Success)
                        {
                            IMonster monster = new Zombie(typeToSpawn, path, bannerGoal.Owner);
                            ModLoader.TriggerCallbacks<IMonster>(ModLoader.EModCallbackType.OnMonsterSpawned, monster);
                            MonsterTracker.Add(monster);
                            colony.OnZombieSpawn(true);
                        }
                        else
                        {
                            colony.OnZombieSpawn(false);
                        }
                        break;
                    }

                case MonsterSpawner.ESpawnResult.NotLoaded:
                    colony.OnZombieSpawn(true);
                    break;

                case MonsterSpawner.ESpawnResult.Fail:
                    colony.OnZombieSpawn(false);
                    break;
            }
        }
    }
}
