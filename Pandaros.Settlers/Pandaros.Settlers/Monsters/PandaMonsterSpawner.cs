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

        public void PathingThreadAction(PathingContext context)
        {
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
