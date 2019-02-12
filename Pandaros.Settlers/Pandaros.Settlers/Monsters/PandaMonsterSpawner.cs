using Monsters;
using Pandaros.Settlers.Entities;
using Pandaros.Settlers.Monsters.Bosses;
using Pipliz;
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

            var fi = monster.CurrentHealth;

            float hpBonus = monster.OriginalGoal.FollowerCount * cs.Difficulty.MonsterHPPerColonist;

            if (MonsterManager.BossActive && MonsterManager.SpawnedBosses.TryGetValue(cs, out var boss) && boss != null && boss.ZombieHPBonus != 0)
                hpBonus += boss.ZombieHPBonus;

            monster.CurrentHealth = monster.CurrentHealth + hpBonus;
            monster.TotalHealth = monster.CurrentHealth + hpBonus;
        }
    }
}