using AI;
using Monsters;
using NPC;
using Pandaros.Settlers.Entities;
using Pipliz;
using Pipliz.JSON;
using System.Collections.Generic;

namespace Pandaros.Settlers.Monsters.Bosses
{
    [ModLoader.ModManager]
    public class Bulging : Zombie, IPandaBoss
    {
        public static string Key = GameLoader.NAMESPACE + ".Monsters.Bosses.Bulging";
        private static NPCTypeMonsterSettings _mts;
        private float _totalHealth = 20000;

        public Bulging() :
            base(NPCType.GetByKeyNameOrDefault(Key), new Path(), GameLoader.StubColony)
        {
        }

        public Bulging(Path path, Colony originalGoal) :
            base(NPCType.GetByKeyNameOrDefault(Key), path, originalGoal)
        {
            var ps = ColonyState.GetColonyState(originalGoal);
            _totalHealth = originalGoal.FollowerCount * ps.Difficulty.BossHPPerColonist;
            TotalHealth = _totalHealth;
            CurrentHealth = _totalHealth;
        }

        public IPandaBoss GetNewBoss(Path path, Colony p)
        {
            return new Bulging(path, p);
        }

        public string AnnouncementText => "I DONT FEEL SO GOOD";
        public string DeathText => "Boom.";

        public string name => "Bulging";

        public override float TotalHealth => _totalHealth;

        public bool KilledBefore
        {
            get => killedBefore;
            set => killedBefore = value;
        }

        public string AnnouncementAudio => GameLoader.NAMESPACE + "ZombieAudio";

        public float ZombieMultiplier => 1f;
        public float ZombieHPBonus => 20;
        public string LootTableName => BossLoot.LootTableName;

        public Dictionary<DamageType, float> Damage { get; } = new Dictionary<DamageType, float>
        {
            {DamageType.Void, 10f},
            {DamageType.Physical, 10f}
        };

        public DamageType ElementalArmor => DamageType.Air;

        public Dictionary<DamageType, float> AdditionalResistance { get; } = new Dictionary<DamageType, float>
        {
            {DamageType.Physical, 0.15f}
        };

        public float MissChance => 0.05f;

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnUpdate, GameLoader.NAMESPACE + ".Monsters.Bosses.Bulging.OnUpdate")]
        public void OnUpdate()
        {
            killedBefore = false;
        }

        public override bool Update()
        {
            killedBefore = false;
            return base.Update();
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterItemTypesDefined,
            GameLoader.NAMESPACE + ".Monsters.Bosses.Bulging.Register")]
        [ModLoader.ModCallbackDependsOn("pipliz.server.loadnpctypes")]
        [ModLoader.ModCallbackProvidesFor("pipliz.server.registermonstertextures")]
        public static void Register()
        {
            var m = new JSONNode()
                   .SetAs("keyName", Key)
                   .SetAs("printName", "Bulging")
                   .SetAs("npcType", "monster");

            var ms = new JSONNode()
                    .SetAs("albedo", GameLoader.BLOCKS_NPC_PATH + "Bulging.png")
                    .SetAs("normal", GameLoader.BLOCKS_NPC_PATH + "Hoarder_normal.png")
                    .SetAs("emissive", GameLoader.BLOCKS_NPC_PATH + "Hoarder_emissive.png")
                    .SetAs("initialHealth", 20000)
                    .SetAs("movementSpeed", .75f)
                    .SetAs("punchCooldownMS", 3000)
                    .SetAs("punchDamage", 100);

            m.SetAs("data", ms);
            _mts = new NPCTypeMonsterSettings(m);
            NPCType.AddSettings(_mts);
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnMonsterDied,
            GameLoader.NAMESPACE + ".Monsters.Bosses.Bulging.OnMonsterDied")]
        public static void OnMonsterDied(IMonster monster)
        {
            var boss = monster as Bulging;

            if (boss != null)
            {
                var numberToSpawn = ColonyState.GetColonyState(boss.OriginalGoal).Difficulty.Rank * 10;

                if (numberToSpawn == 0)
                    numberToSpawn = 10;

                var pos = new Vector3Int(boss.Position);

                for (var i = 0; i < numberToSpawn; i++)
                   ((MonsterSpawner)MonsterTracker.MonsterSpawner).QueueSpawnZombie(boss.OriginalGoal.GetClosestBanner(boss.position), MonsterSpawner.GetTypeToSpawn(boss.OriginalGoal.FollowerCount));
            }
        }
    }
}