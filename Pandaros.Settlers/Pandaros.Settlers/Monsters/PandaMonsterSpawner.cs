using General.Settings;
using NPC;
using Pandaros.Settlers.Entities;
using Pandaros.Settlers.Monsters.Bosses;
using Pipliz;
using Server.AI;
using Server.Monsters;
using Server.NPCs;
using Server.TerrainGeneration;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace Pandaros.Settlers.Monsters
{
    [ModLoader.ModManager]
    public class PandaMonsterSpawner : IMonsterSpawner
    {
        private Stopwatch _maxTimePerTick = new Stopwatch();
        private static double siegeModeCooldown = 3.0;
        public static MonsterSpawner.Variables MonsterVariables { get; private set; } = new MonsterSpawner.Variables();
        public static PandaMonsterSpawner Instance { get; private set; }
        public static MonsterSpawner MonsterSpawnerInstance { get; private set; }
        
        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterWorldLoad, GameLoader.NAMESPACE + ".Monsters.PandaMonsterSpawner.AfterWorldLoad"),
            ModLoader.ModCallbackDependsOn("pipliz.server.monsterspawner.register")]
        private static void AfterWorldLoad()
        {
            Instance = new PandaMonsterSpawner();
            MonsterSpawnerInstance = MonsterTracker.MonsterSpawner as MonsterSpawner;
            MonsterTracker.MonsterSpawner = Instance;
            PandaLogger.Log("PandaMonsterSpawner Initialized!");
        }

        public void Update()
        {
            if (!World.Initialized || 
                !ServerManager.WorldSettings.ZombiesEnabled || 
                Server.AI.AIManager.IsBusy() || 
                Managers.MonsterManager.BossActive)
                return;

            WorldSettings worldSettings = ServerManager.WorldSettings;

            bool flag = worldSettings.ZombiesEnabled && (TimeCycle.ShouldSpawnMonsters || worldSettings.MonstersDayTime);
            int num = (!worldSettings.MonstersDoubled) ? 1 : 2;
            double secondsSinceStartDouble = Pipliz.Time.SecondsSinceStartDouble;
            var banners = BannerTracker.GetCount();

            _maxTimePerTick.Reset();
            _maxTimePerTick.Start();

            for (int i = 0; i < banners; i++)
            {
                if (_maxTimePerTick.Elapsed.TotalMilliseconds > MonsterVariables.MSPerTick)
                    break;

                if (BannerTracker.TryGetAtIndex(i, out var banner))
                {
                    var ps = PlayerState.GetPlayerState(banner.Owner);
                    
                    if (ps.MonstersEnabled)
                        SpawnForBanner(banner, flag, num, secondsSinceStartDouble, false, null);
                }
            }

            _maxTimePerTick.Stop();
        }
        
        public void SpawnForBanner(Banner valueAtIndex, bool flag, float num, double secondsSinceStartDouble, bool force, IPandaBoss boss)
        {
            if (valueAtIndex != null && valueAtIndex.KeyLocation.IsValid)
            {
                Colony colony = Colony.Get(valueAtIndex.Owner);

                if (flag)
                {
                    float num2 = MonsterSpawnerInstance.GetMaxZombieCount(colony.FollowerCount) * num;

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

                            CaclulateZombie(colony, valueAtIndex, boss, force);
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

        public static void SpawnZombie(Colony colony, Banner bannerGoal)
        {
            CaclulateZombie(colony, bannerGoal, null);
        }

        public static void CaclulateZombie(Colony colony, Banner bannerGoal, IPandaBoss boss, bool force = false)
        {
            NPCType typeToSpawn = MonsterSpawner.GetTypeToSpawn((float)colony.FollowerCount);
            float maxSpawnWalkDistance = 500f;

            if (force || TimeCycle.ShouldSpawnMonsters)
            {
                if (typeToSpawn.TryGetSettings(out NPCTypeMonsterSettings nPCTypeMonsterSettings))
                {
                    if (force)
                        maxSpawnWalkDistance = TimeCycle.NightLengthInRealSeconds * (nPCTypeMonsterSettings.movementSpeed * 0.85f) + (float)bannerGoal.SafeRadius;
                    else
                        maxSpawnWalkDistance = TimeCycle.NightLengthLeftInRealSeconds * (nPCTypeMonsterSettings.movementSpeed * 0.85f) + (float)bannerGoal.SafeRadius;
                }
            }

            switch (MonsterSpawner.TryGetSpawnLocation(bannerGoal, maxSpawnWalkDistance, out Vector3Int start))
            {
                case MonsterSpawner.ESpawnResult.Success:
                    {
                        SpawnPandaZombie(colony, bannerGoal, boss, typeToSpawn, start);
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

        public static void SpawnPandaZombie(Colony colony, Banner bannerGoal, IPandaBoss boss, NPCType typeToSpawn, Vector3Int start)
        {
            if (AIManager.ZombiePathFinder.TryFindPath(start, bannerGoal.KeyLocation, out Path path, 2000000000) == EPathFindingResult.Success)
            {
                Zombie monster = new Zombie(typeToSpawn, path, bannerGoal.Owner);

                if (boss != null)
                {
                    if (boss.ZombieHPBonus != 0)
                    {
                        var fi = monster.GetType().GetField("health", BindingFlags.GetField | BindingFlags.NonPublic | BindingFlags.Instance);
                        fi.SetValue(monster, (float)fi.GetValue(monster) + boss.ZombieHPBonus);
                    }
                }

                if (colony.FollowerCount > 499)
                {
                    var fi = monster.GetType().GetField("health", BindingFlags.GetField | BindingFlags.NonPublic | BindingFlags.Instance);
                    fi.SetValue(monster, (float)((float)fi.GetValue(monster) + (colony.FollowerCount * .05f)));
                }

                ModLoader.TriggerCallbacks<IMonster>(ModLoader.EModCallbackType.OnMonsterSpawned, monster);
                MonsterTracker.Add(monster);
                colony.OnZombieSpawn(true);
            }
            else
            {
                colony.OnZombieSpawn(false);
            }
        }
    }
}
