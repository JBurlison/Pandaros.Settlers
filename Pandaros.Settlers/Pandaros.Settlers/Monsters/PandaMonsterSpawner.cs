using AI;
using Difficulty;
using Monsters;
using NPC;
using Pandaros.Settlers.Monsters.Bosses;
using Pandaros.Settlers.Monsters.Normal;
using Pipliz;
using System.Collections.Generic;
using System.Reflection;
using static BlockEntities.Implementations.BannerTracker;

namespace Pandaros.Settlers.Monsters
{
    [ModLoader.ModManager]
    public class PandaMonsterSpawner : MonsterSpawner
    {
        private struct Data
        {
            public Dictionary<Colony, double> NextZombieSpawnTimes;
            public List<Colony> ColoniesRequiringZombies;
        }

        private static double siegeModeCooldown = 3.0;
        private const double MONSTERS_DELAY_THRESHOLD_SECONDS = 1.0;
        public static PandaMonsterSpawner Instance { get; private set; }

        public override void Update()
        {
            if (!World.Initialized ||
                AIManager.IsBusy())
                return;

            Data data;
            data.ColoniesRequiringZombies = coloniesRequiringZombies;
            data.NextZombieSpawnTimes = NextZombieSpawnTimes;
            ServerManager.BlockEntityTracker.BannerTracker.Foreach(ForeachBanner, ref data);

            maxTimePerTick.Reset();
            maxTimePerTick.Start();
            
            while (coloniesRequiringZombies.Count > 0 && maxTimePerTick.Elapsed.TotalMilliseconds < ServerManager.ServerSettings.Zombies.MSPerTick)
            {
                int i = 0;

                while (i < coloniesRequiringZombies.Count && maxTimePerTick.Elapsed.TotalMilliseconds < ServerManager.ServerSettings.Zombies.MSPerTick)
                {
                    Colony colony = coloniesRequiringZombies[i];
                    Banner banner = colony.GetRandomBanner();

                    IColonyDifficultySetting difficulty = colony.DifficultySetting;
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

        static void ForeachBanner(Vector3Int position, Banner banner, ref Data data)
        {
            Colony colony = banner.Colony;
            if (colony == null)
            {
                return;
            }

            if (colony.FollowerCount == 0)
            {
                colony.OnZombieSpawn(true);
                return;
            }

            IColonyDifficultySetting difficultyColony = colony.DifficultySetting;

            if (!difficultyColony.ShouldSpawnZombies(colony))
            {
                colony.OnZombieSpawn(true);
                return;
            }

            double nextZombieSpawnTime = data.NextZombieSpawnTimes.GetValueOrDefault(colony, 0.0);
            if (nextZombieSpawnTime > Pipliz.Time.SecondsSinceStartDoubleThisFrame)
            {
                return;
            }

            if (colony.InSiegeMode)
            {
                if (Pipliz.Time.SecondsSinceStartDoubleThisFrame - colony.LastSiegeModeSpawn < siegeModeCooldown)
                {
                    return;
                }
                else
                {
                    colony.LastSiegeModeSpawn = Pipliz.Time.SecondsSinceStartDoubleThisFrame;
                }
            }

            if (Pipliz.Time.SecondsSinceStartDoubleThisFrame - nextZombieSpawnTime > MONSTERS_DELAY_THRESHOLD_SECONDS)
            {
                // lagging behind, or no cooldown set: teleport to current time
                data.NextZombieSpawnTimes[colony] = Pipliz.Time.SecondsSinceStartDoubleThisFrame;
            }
            data.ColoniesRequiringZombies.AddIfUnique(colony);
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterWorldLoad, GameLoader.NAMESPACE + ".Monsters.PandaMonsterSpawner.AfterWorldLoad")]
        [ModLoader.ModCallbackDependsOn("pipliz.server.monsterspawner.register")]
        private static void AfterWorldLoad()
        {
            Instance                      = new PandaMonsterSpawner();
            MonsterTracker.MonsterSpawner = Instance;
            PandaLogger.Log("PandaMonsterSpawner Initialized!");
        }

        public bool SpawnForBanner(Banner banner, IColonyDifficultySetting difficulty, Colony colony, float cooldown, IPandaZombie boss)
        {
            int recycleFrequency = MonsterSpawner.GetPathRecycleFrequency(1f / cooldown);
            NPCType zombieTypeToSpawn = difficulty.GetZombieTypeToSpawn(colony);
            float maxPathDistance = difficulty.GetMaxPathDistance(colony, zombieTypeToSpawn, banner.SafeRadius);
            return CaclulateZombie(banner, colony, zombieTypeToSpawn, recycleFrequency, maxPathDistance);
        }

        public static bool CaclulateZombie(Banner banner, Colony colony, NPCType typeToSpawn, int RecycleFrequency = 1, float maxSpawnWalkDistance = 500f, IPandaZombie boss = null)
        {
            Path path;
            if (RecycleFrequency > 1 && PathCache.TryGetPath(RecycleFrequency, banner.Position, maxSpawnWalkDistance, out path))
            {
                SpawnPandaZombie(typeToSpawn, path, colony, boss);
                return true;
            }
            Vector3Int positionFinal;
            switch (MonsterSpawner.TryGetSpawnLocation(banner.Position, 100, 200, maxSpawnWalkDistance, out positionFinal))
            {
                case MonsterSpawner.ESpawnResult.Success:
                    if (AIManager.ZombiePathFinder.TryFindPath(positionFinal, banner.Position, out path, 2000000000) == EPathFindingResult.Success)
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

        public static void SpawnPandaZombie(NPCType typeToSpawn, Path path, Colony colony, IPandaZombie boss)
        {
            var monster = new Zombie(typeToSpawn, path, colony);

            if (boss != null)
                if (boss.ZombieHPBonus != 0)
                {
                    var fi = monster
                            .GetType().GetField("health",
                                                BindingFlags.GetField | BindingFlags.NonPublic |
                                                BindingFlags.Instance);

                    fi.SetValue(monster, (float) fi.GetValue(monster) + boss.ZombieHPBonus);
                }

            if (boss.GetType() == typeof(IPandaBoss) && colony.FollowerCount > Configuration.GetorDefault("MinColonistsCountForBosses", 50))
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