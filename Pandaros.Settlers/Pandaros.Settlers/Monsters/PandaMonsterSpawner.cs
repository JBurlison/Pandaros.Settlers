using AI;
using Monsters;
using Pandaros.Settlers.Entities;
using Pandaros.Settlers.Monsters.Bosses;
using Pandaros.Settlers.Monsters.Normal;
using Pipliz;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using static AI.PathingManager;
using static AI.PathingManager.PathFinder;

namespace Pandaros.Settlers.Monsters
{
    [ModLoader.ModManager]
    public class PandaMonsterSpawner : IPathingThreadAction
    {
        public static List<IPandaZombie> PandaZombies { get; set; } = new List<IPandaZombie>();
        private static Queue<IPandaZombie> _spawnQueue = new Queue<IPandaZombie>();
        private static Queue<IPandaZombie> _pathingQueue = new Queue<IPandaZombie>();
        private static PandaMonsterSpawner _pandaPathing = new PandaMonsterSpawner();
        private static double _updateTime = 0;

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnMonsterSpawned, GameLoader.NAMESPACE + ".Monsters.PandaMonsterSpawner.OnMonsterSpawned")]
        public static void OnMonsterSpawned(IMonster monster)
        {
            var cs = Entities.ColonyState.GetColonyState(monster.OriginalGoal);

            var fi = monster.CurrentHealth;

            float hpBonus = monster.OriginalGoal.FollowerCount * cs.Difficulty.MonsterHPPerColonist;

            if (MonsterManager.BossActive && MonsterManager.SpawnedBosses.TryGetValue(cs, out var boss) && boss != null && boss.ZombieHPBonus != 0)
                hpBonus += boss.ZombieHPBonus;

            monster.CurrentHealth = monster.CurrentHealth + hpBonus;
            monster.TotalHealth = monster.CurrentHealth + hpBonus;
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnUpdate, GameLoader.NAMESPACE + ".Managers.MonsterManager.Update")]
        public static void OnUpdate()
        {
            if (!World.Initialized)
                return;

            while (_spawnQueue.Count > 0)
            {

            }

            foreach(var colony in ServerManager.ColonyTracker.ColoniesByID.Values)
            {
                if (colony.DifficultySetting.ShouldSpawnZombies(colony))
                {
                    
                }
            }

        }

        public void PathingThreadAction(PathingContext context)
        {
            List<IPandaZombie> zombiesToSpawn = new List<IPandaZombie>();

            lock(_pathingQueue)
                while (_pathingQueue.Count > 0)
                    zombiesToSpawn.Add(_pathingQueue.Dequeue());

            foreach (var colony in ServerManager.ColonyTracker.ColoniesByID.Values)
            {
                var bannerGoal = colony.Banners.ToList().GetRandomItem();
                var cs = ColonyState.GetColonyState(colony);

                if (cs.ColonyRef.OwnerIsOnline())
                {
                    Vector3Int positionFinal;
                    switch (((MonsterSpawner)MonsterTracker.MonsterSpawner).TryGetSpawnLocation(context, bannerGoal.Position, bannerGoal.SafeRadius, 200, 500f, out positionFinal))
                    {
                        case MonsterSpawner.ESpawnResult.Success:
                            if (context.Pathing.TryFindPath(positionFinal, bannerGoal.Position, out var path, 2000000000) == EPathFindingResult.Success)
                            {

                            }

                            break;
                        case MonsterSpawner.ESpawnResult.NotLoaded:
                        case MonsterSpawner.ESpawnResult.Impossible:
                            colony.OnZombieSpawn(true);
                            break;
                        case MonsterSpawner.ESpawnResult.Fail:

                            break;
                    }
                }
            }
        }
    }
}
