using AI;
using Monsters;
using NPC;
using Pandaros.Settlers.Entities;
using Pipliz.JSON;
using System.Collections.Generic;

namespace Pandaros.Settlers.Monsters.Bosses
{
    [ModLoader.ModManager]
    public class Phase : Zombie, IPandaBoss
    {
        public static string Key = GameLoader.NAMESPACE + ".Monsters.Bosses.Phase";
        private static NPCTypeMonsterSettings _mts;
        private float _totalHealth = 40000;

        public Phase() :
            base(NPCType.GetByKeyNameOrDefault(Key), new Path(), GameLoader.StubColony)
        {
        }

        public Phase(Path path, Colony originalGoal) :
            base(NPCType.GetByKeyNameOrDefault(Key), path, originalGoal)
        {
            var ps = ColonyState.GetColonyState(originalGoal);
            _totalHealth = originalGoal.FollowerCount * (ps.Difficulty.BossHPPerColonist - ps.Difficulty.BossHPPerColonist * .20f);
            TotalHealth = _totalHealth;
            CurrentHealth = _totalHealth;
        }

        public IPandaBoss GetNewBoss(Path path, Colony p)
        {
            return new Phase(path, p);
        }

        public string AnnouncementText => "You cant see me....";
        public string DeathText => "I just wanted to stay out of sight.";
        public string name => "Phase";
        public override float TotalHealth => _totalHealth;

        public bool KilledBefore
        {
            get => false;
            set => killedBefore = value;
        }

        public string AnnouncementAudio => GameLoader.NAMESPACE + ".ZombieAudio";
        public float ZombieMultiplier => 1f;
        public float ZombieHPBonus => 0;

        public string LootTableName => BossLoot.LootTableName;

        public Dictionary<DamageType, float> Damage { get; } = new Dictionary<DamageType, float>
        {
            {DamageType.Void, 10f},
            {DamageType.Physical, 10f}
        };

        public DamageType ElementalArmor => DamageType.Water;

        public Dictionary<DamageType, float> AdditionalResistance { get; } = new Dictionary<DamageType, float>
        {
            {DamageType.Air, 0.20f}
        };

        public float MissChance => 0.35f;

        public override bool Update()
        {
            killedBefore = false;
            return base.Update();
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterItemTypesDefined,
            GameLoader.NAMESPACE + ".Monsters.Bosses.Phase.Register")]
        [ModLoader.ModCallbackDependsOn("pipliz.server.loadnpctypes")]
        [ModLoader.ModCallbackProvidesFor("pipliz.server.registermonstertextures")]
        public static void Register()
        {
            var m = new JSONNode()
                   .SetAs("keyName", Key)
                   .SetAs("printName", "Phase")
                   .SetAs("npcType", "monster");

            var ms = new JSONNode()
                    .SetAs("albedo", GameLoader.BLOCKS_NPC_PATH + "Phase.png")
                    .SetAs("normal", GameLoader.BLOCKS_NPC_PATH + "ZombieQueen_normal.png")
                    .SetAs("emissive", GameLoader.BLOCKS_NPC_PATH + "ZombieQueen_emissive.png")
                    .SetAs("initialHealth", 2000)
                    .SetAs("movementSpeed", 2.25f)
                    .SetAs("punchCooldownMS", 500)
                    .SetAs("punchDamage", 100);

            m.SetAs("data", ms);
            _mts = new NPCTypeMonsterSettings(m);
            NPCType.AddSettings(_mts);
        }
    }
}