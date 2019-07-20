using Monsters;
using Pandaros.API.Monsters;

namespace Pandaros.Settlers.AI
{
    [ModLoader.ModManager]
    public class Monsters
    {
        private static double _nextUpdateTime;

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnUpdate, GameLoader.NAMESPACE + ".Managers.MonsterManager.Update")]
        public static void OnUpdate()
        {
            if (!World.Initialized)
                return;

            var secondsSinceStartDouble = Pipliz.Time.SecondsSinceStartDouble;

            if (_nextUpdateTime < secondsSinceStartDouble)
            {
                IMonster m = null;

                foreach (var monster in MonsterManager.GetAllMonsters())
                    if (m == null || UnityEngine.Vector3.Distance(monster.Value.Position, m.Position) > 15 && Pipliz.Random.NextBool())
                    {
                        m = monster.Value;
                        AudioManager.SendAudio(monster.Value.Position, GameLoader.NAMESPACE + ".ZombieAudio");
                    }

                _nextUpdateTime = secondsSinceStartDouble + 5;
            }
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnMonsterHit, GameLoader.NAMESPACE + ".Managers.MonsterManager.OnMonsterHit")]
        public static void OnMonsterHit(IMonster monster, ModLoader.OnHitData d)
        {
            if (Pipliz.Random.NextFloat() > .5f)
                AudioManager.SendAudio(monster.Position, GameLoader.NAMESPACE + ".ZombieAudio");
        }
    }
}
