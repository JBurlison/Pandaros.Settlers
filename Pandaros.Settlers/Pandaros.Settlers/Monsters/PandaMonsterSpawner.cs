using Monsters;
using Pandaros.Settlers.Entities;
using Pandaros.Settlers.Monsters.Bosses;
using System.Reflection;

namespace Pandaros.Settlers.Monsters
{
    [ModLoader.ModManager]
    public class PandaMonsterSpawner 
    {
        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnMonsterSpawned, GameLoader.NAMESPACE + ".Monsters.PandaMonsterSpawner.OnMonsterSpawned")]
        public static void OnMonsterSpawned(IMonster monster)
        {
            var cs = Entities.ColonyState.GetColonyState(monster.OriginalGoal);

            var fi = monster
                       .GetType().GetField("health",
                                           BindingFlags.GetField | BindingFlags.NonPublic |
                                           BindingFlags.Instance);

            float hpBonus = monster.OriginalGoal.FollowerCount * cs.Difficulty.MonsterHPPerColonist;

            if (MonsterManager.BossActive && MonsterManager.SpawnedBosses.TryGetValue(cs, out var boss) && boss != null && boss.ZombieHPBonus != 0)
                hpBonus += boss.ZombieHPBonus;

            if (monster.GetType() == typeof(IPandaBoss) && monster.OriginalGoal.FollowerCount > Configuration.GetorDefault("MinColonistsCountForBosses", 100))
                hpBonus += monster.OriginalGoal.FollowerCount * .10f;

            fi.SetValue(monster, (float)fi.GetValue(monster) + hpBonus);
        }
    }
}