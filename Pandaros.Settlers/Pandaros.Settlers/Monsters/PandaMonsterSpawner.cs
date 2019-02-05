using AI;
using Difficulty;
using Monsters;
using NPC;
using Pandaros.Settlers.Managers;
using Pandaros.Settlers.Monsters.Bosses;
using Pandaros.Settlers.Monsters.Normal;
using Pipliz;
using System.Collections.Generic;
using System.Reflection;
using static BlockEntities.Implementations.BannerTracker;

namespace Pandaros.Settlers.Monsters
{
    [ModLoader.ModManager]
    public class PandaMonsterSpawner 
    {
        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnMonsterSpawned, GameLoader.NAMESPACE + ".Monsters.PandaMonsterSpawner.OnMonsterSpawned")]
        public static void OnMonsterSpawned(IMonster monster)
        {
            var cs = Entities.ColonyState.GetColonyState(monster.OriginalGoal);

            if (MonsterManager.BossActive && MonsterManager.SpawnedBosses.TryGetValue(cs, out var boss) && boss != null && boss.ZombieHPBonus != 0)
            {
                var fi = monster
                        .GetType().GetField("health",
                                            BindingFlags.GetField | BindingFlags.NonPublic |
                                            BindingFlags.Instance);

                fi.SetValue(monster, (float)fi.GetValue(monster) + boss.ZombieHPBonus);
            }

            if (monster.GetType() == typeof(IPandaBoss) && monster.OriginalGoal.FollowerCount > Configuration.GetorDefault("MinColonistsCountForBosses", 50))
            {
                var fi = monster
                        .GetType().GetField("health",
                                            BindingFlags.GetField | BindingFlags.NonPublic | BindingFlags.Instance);

                fi.SetValue(monster, (float)fi.GetValue(monster) + monster.OriginalGoal.FollowerCount * .10f);
            }
        }
    }
}