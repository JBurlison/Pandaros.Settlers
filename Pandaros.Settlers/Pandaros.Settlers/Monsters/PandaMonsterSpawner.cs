using System.Diagnostics;
using System.Reflection;
using NPC;
using Pandaros.Settlers.Entities;
using Pandaros.Settlers.Managers;
using Pandaros.Settlers.Monsters.Bosses;
using Pipliz;
using Server.AI;
using Server.Monsters;
using Server.NPCs;

namespace Pandaros.Settlers.Monsters
{
    [ModLoader.ModManagerAttribute]
    public class PandaMonsterSpawner : IMonsterSpawner
    {
        private static readonly double siegeModeCooldown = 3.0;
        private readonly Stopwatch _maxTimePerTick = new Stopwatch();
        public static MonsterSpawner.Variables MonsterVariables { get; } = new MonsterSpawner.Variables();
        public static PandaMonsterSpawner Instance { get; private set; }
        public static MonsterSpawner MonsterSpawnerInstance { get; private set; }

        public void Update()
        {
            if (!World.Initialized ||
                !ServerManager.WorldSettings.ZombiesEnabled ||
                AIManager.IsBusy() ||
                MonsterManager.BossActive)
                return;

            var worldSettings = ServerManager.WorldSettings;

            var flag = worldSettings.ZombiesEnabled &&
                                          (TimeCycle.ShouldSpawnMonsters || worldSettings.MonstersDayTime);

            var num                     = !worldSettings.MonstersDoubled ? 1 : 2;
            var secondsSinceStartDouble = Time.SecondsSinceStartDouble;
            var banners                 = BannerTracker.GetCount();

            _maxTimePerTick.Reset();
            _maxTimePerTick.Start();

            for (var i = 0; i < banners; i++)
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

        [ModLoader.ModCallbackAttribute(ModLoader.EModCallbackType.AfterWorldLoad,
            GameLoader.NAMESPACE + ".Monsters.PandaMonsterSpawner.AfterWorldLoad")]
        [ModLoader.ModCallbackDependsOnAttribute("pipliz.server.monsterspawner.register")]
        private static void AfterWorldLoad()
        {
            Instance                      = new PandaMonsterSpawner();
            MonsterSpawnerInstance        = MonsterTracker.MonsterSpawner as MonsterSpawner;
            MonsterTracker.MonsterSpawner = Instance;
            PandaLogger.Log("PandaMonsterSpawner Initialized!");
        }

        public void SpawnForBanner(Banner valueAtIndex, bool       flag, float num, double secondsSinceStartDouble,
                                   bool   force,        IPandaBoss boss)
        {
            if (valueAtIndex != null && valueAtIndex.KeyLocation.IsValid)
            {
                var colony = Colony.Get(valueAtIndex.Owner);

                if (flag)
                {
                    var num2 = MonsterSpawnerInstance.GetMaxZombieCount(colony.FollowerCount) * num;

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
            var typeToSpawn          = MonsterSpawner.GetTypeToSpawn(colony.FollowerCount);
            var maxSpawnWalkDistance = 500f;

            if (force || TimeCycle.ShouldSpawnMonsters)
                if (typeToSpawn.TryGetSettings(out NPCTypeMonsterSettings nPCTypeMonsterSettings))
                {
                    if (force)
                        maxSpawnWalkDistance =
                            TimeCycle.NightLengthInRealSeconds * (nPCTypeMonsterSettings.movementSpeed * 0.85f) +
                            bannerGoal.SafeRadius;
                    else
                        maxSpawnWalkDistance =
                            TimeCycle.NightLengthLeftInRealSeconds * (nPCTypeMonsterSettings.movementSpeed * 0.85f) +
                            bannerGoal.SafeRadius;
                }

            switch (MonsterSpawner.TryGetSpawnLocation(bannerGoal, maxSpawnWalkDistance, out var start))
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

        public static void SpawnPandaZombie(Colony     colony, Banner bannerGoal, IPandaBoss boss, NPCType typeToSpawn,
                                            Vector3Int start)
        {
            if (AIManager.ZombiePathFinder.TryFindPath(start, bannerGoal.KeyLocation, out var path, 2000000000) ==
                EPathFindingResult.Success)
            {
                var monster = new Zombie(typeToSpawn, path, bannerGoal.Owner);

                if (boss != null)
                    if (boss.ZombieHPBonus != 0)
                    {
                        var fi = monster
                                .GetType().GetField("health",
                                                    BindingFlags.GetField | BindingFlags.NonPublic |
                                                    BindingFlags.Instance);

                        fi.SetValue(monster, (float) fi.GetValue(monster) + boss.ZombieHPBonus);
                    }

                if (colony.FollowerCount > 499)
                {
                    var fi = monster
                            .GetType().GetField("health",
                                                BindingFlags.GetField | BindingFlags.NonPublic | BindingFlags.Instance);

                    fi.SetValue(monster, (float) fi.GetValue(monster) + colony.FollowerCount * .05f);
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