using AI;
using BlockEntities.Implementations;
using NPC;
using Pandaros.Settlers.Entities;
using Pandaros.Settlers.Managers;
using Pandaros.Settlers.Monsters.Bosses;
using Pipliz;
using System.Reflection;
using static BlockEntities.Implementations.BannerTracker;

namespace Pandaros.Settlers.Monsters
{
    [ModLoader.ModManager]
    public class PandaMonsterSpawner : MonsterSpawner
    {
        private static double siegeModeCooldown = 3.0;
        private const double MONSTERS_DELAY_THRESHOLD_SECONDS = 1.0;
        public static PandaMonsterSpawner Instance { get; private set; }
        public Variables SpawnerVariables { get { return variables; } }

        public override void Update()
        {
            if (!World.Initialized ||
                AIManager.IsBusy())
                return;

            CalculateColoniesThatRequireMonsters();

            maxTimePerTick.Reset();
            maxTimePerTick.Start();
            
            while (coloniesRequiringZombies.Count > 0 && maxTimePerTick.Elapsed.TotalMilliseconds < variables.MSPerTick)
            {
                int i = 0;

                while (i < coloniesRequiringZombies.Count && maxTimePerTick.Elapsed.TotalMilliseconds < variables.MSPerTick)
                {
                    var tuple = coloniesRequiringZombies[i];
                    Colony colony = tuple.Banners;
                    Banner banner = tuple.item2;

                    IDifficultySetting difficulty = colony.Owner.DifficultySetting;
                    float cooldown = difficulty.GetZombieSpawnCooldown(colony);

                    if (SpawnForBanner(banner, difficulty, colony, cooldown, null))
                    {
                        double nextZombieSpawn = NextZombieSpawnTimes.GetValueOrDefault(colony, 0.0) + cooldown;
                        NextZombieSpawnTimes[colony] = nextZombieSpawn;

                        if (nextZombieSpawn > Pipliz.Time.SecondsSinceStartDoubleThisFrame)
                            coloniesRequiringZombies.RemoveAt(i);
                    }
                    else if (colony.InSiegeMode)
                    {
                        coloniesRequiringZombies.RemoveAt(i); // only test getting out of siege mode once per time
                    }
                }
            }

            maxTimePerTick.Stop();
        }

        public void CalculateColoniesThatRequireMonsters()
        {
            int bannerCount = BannerTracker.GetCount();
            coloniesRequiringZombies.Clear();

            for (int i = 0; i < bannerCount; i++)
            {
                Banner banner;

                if (!BannerTracker.TryGetAtIndex(i, out banner) || !banner.KeyLocation.IsValid)
                    continue;

                Colony colony = Colony.Get(banner.Owner);

                if (colony.FollowerCount == 0)
                {
                    colony.OnZombieSpawn(true);
                    continue;
                }

                var ps = PlayerState.GetPlayerState(banner.Owner);
                IDifficultySetting difficultyColony = colony.Owner.DifficultySetting;

                if (!MonsterManager.BossActive)
                if (!ps.MonstersEnabled || !difficultyColony.ShouldSpawnZombies(colony))
                {
                    colony.OnZombieSpawn(true);
                    continue;
                }
                
                double nextZombieSpawnTime = NextZombieSpawnTimes.GetValueOrDefault(colony, 0.0);

                if (nextZombieSpawnTime > Pipliz.Time.SecondsSinceStartDoubleThisFrame)
                    continue;

                if (colony.InSiegeMode)
                {
                    if (Pipliz.Time.SecondsSinceStartDoubleThisFrame - colony.LastSiegeModeSpawn < siegeModeCooldown)
                        continue;
                    else
                        colony.LastSiegeModeSpawn = Pipliz.Time.SecondsSinceStartDoubleThisFrame;
                }

                // lagging behind, or no cooldown set: teleport to current time
                if (Pipliz.Time.SecondsSinceStartDoubleThisFrame - nextZombieSpawnTime > MONSTERS_DELAY_THRESHOLD_SECONDS)
                    NextZombieSpawnTimes[colony] = Pipliz.Time.SecondsSinceStartDoubleThisFrame;

                coloniesRequiringZombies.Add(TupleStruct.Create(colony, banner));
            }
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterWorldLoad,
            GameLoader.NAMESPACE + ".Monsters.PandaMonsterSpawner.AfterWorldLoad")]
        [ModLoader.ModCallbackDependsOn("pipliz.server.monsterspawner.register")]
        private static void AfterWorldLoad()
        {
            Instance                      = new PandaMonsterSpawner();
            MonsterTracker.MonsterSpawner = Instance;
            PandaLogger.Log("PandaMonsterSpawner Initialized!");
        }

        public bool SpawnForBanner(Banner banner, IDifficultySetting difficulty, Colony colony, float cooldown, IPandaBoss boss)
        {
            int recycleFrequency = MonsterSpawner.GetPathRecycleFrequency(1f / cooldown);
            NPCType zombieTypeToSpawn = difficulty.GetZombieTypeToSpawn(colony);
            float maxPathDistance = difficulty.GetMaxPathDistance(colony, zombieTypeToSpawn, banner.SafeRadius);
            return CaclulateZombie(banner, colony, zombieTypeToSpawn, recycleFrequency, maxPathDistance);
        }

        public static bool CaclulateZombie(Banner banner, Colony colony, NPCType typeToSpawn, int RecycleFrequency = 1, float maxSpawnWalkDistance = 500f, IPandaBoss boss = null)
        {
            Path path;
            if (RecycleFrequency > 1 && MonsterSpawner.PathCache.TryGetPath(RecycleFrequency, banner.KeyLocation, maxSpawnWalkDistance, out path))
            {
                SpawnPandaZombie(typeToSpawn, path, colony, boss);
                return true;
            }
            Vector3Int positionFinal;
            switch (MonsterSpawner.TryGetSpawnLocation(banner, maxSpawnWalkDistance, out positionFinal))
            {
                case MonsterSpawner.ESpawnResult.Success:
                    if (AIManager.ZombiePathFinder.TryFindPath(positionFinal, banner.KeyLocation, out path, 2000000000) == EPathFindingResult.Success)
                    {
                        if (RecycleFrequency > 1)
                            MonsterSpawner.PathCache.AddCopy(path);
                        SpawnPandaZombie(typeToSpawn, path, colony, boss);
                        return true;
                    }
                    colony.OnZombieSpawn(false);
                    return false;
                case MonsterSpawner.ESpawnResult.NotLoaded:
                case MonsterSpawner.ESpawnResult.Impossible:
                    colony.OnZombieSpawn(true);
                    return true;
                case MonsterSpawner.ESpawnResult.Fail:
                    colony.OnZombieSpawn(false);
                    return true;
                default:
                    return false;
            }
        }

        public static void SpawnPandaZombie(NPCType typeToSpawn, Path path, Colony colony, IPandaBoss boss)
        {
            var monster = new Zombie(typeToSpawn, path, colony.Owner);

            if (boss != null)
                if (boss.ZombieHPBonus != 0)
                {
                    var fi = monster
                            .GetType().GetField("health",
                                                BindingFlags.GetField | BindingFlags.NonPublic |
                                                BindingFlags.Instance);

                    fi.SetValue(monster, (float) fi.GetValue(monster) + boss.ZombieHPBonus);
                }

            if (colony.FollowerCount > Configuration.GetorDefault("MinColonistsCountForBosses", 50))
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
    }
}