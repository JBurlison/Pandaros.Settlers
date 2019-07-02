using AI;
using Monsters;
using NPC;
using Pandaros.Settlers.Entities;
using Pandaros.Settlers.Monsters.Normal;
using Pipliz.JSON;
using System.Collections.Generic;

namespace Pandaros.Settlers.Monsters.Bosses
{
    [ModLoader.ModManager]
    public class JackbNimble : Zombie, IPandaBoss
    {
        public static string Key = GameLoader.NAMESPACE + ".Monsters.Bosses.JackbNimble";
        private static NPCTypeMonsterSettings _mts;

        private float _totalHealth = 40000;

        public JackbNimble() :
            base(NPCType.GetByKeyNameOrDefault(Key), new Path(), GameLoader.StubColony)
        {
        }

        public JackbNimble(Path path, Colony originalGoal) :
            base(NPCType.GetByKeyNameOrDefault(Key), path, originalGoal)
        {
            var ps = ColonyState.GetColonyState(originalGoal);
            _totalHealth = originalGoal.FollowerCount * (ps.Difficulty.BossHPPerColonist - ps.Difficulty.BossHPPerColonist * .5f);
            TotalHealth = _totalHealth;
            CurrentHealth = _totalHealth;
        }

        public IPandaZombie GetNewInstance(Path path, Colony p)
        {
            return new JackbNimble(path, p);
        }

        public string AnnouncementText => "Catch me if you can!";
        public string DeathText => "I just was not fast enough...";
        public string name => "Jack-b-Nimble";
        public override float TotalHealth => _totalHealth;

        public bool KilledBefore
        {
            get => false;
            set => killedBefore = value;
        }

        public string AnnouncementAudio => GameLoader.NAMESPACE + ".ZombieAudio";
        public float ZombieMultiplier => 1f;
        public float ZombieHPBonus => 0;
        public string MosterType => "Boss";

        public Dictionary<DamageType, float> Damage { get; } = new Dictionary<DamageType, float>
        {
            {DamageType.Void, 10f},
            {DamageType.Physical, 10f}
        };

        public float MissChance => 0.10f;
        public DamageType ElementalArmor => DamageType.Air;

        public Dictionary<DamageType, float> AdditionalResistance { get; } = new Dictionary<DamageType, float>
        {
            {DamageType.Fire, 0.20f}
        };

        public override bool Update()
        {
            killedBefore = false;
            return base.Update();
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterItemTypesDefined,
            GameLoader.NAMESPACE + ".Monsters.Bosses.JackbNimble.Register")]
        [ModLoader.ModCallbackDependsOn("pipliz.server.loadnpctypes")]
        [ModLoader.ModCallbackProvidesFor("pipliz.server.registermonstertextures")]
        public static void Register()
        {
            var m = new JSONNode()
                   .SetAs("keyName", Key)
                   .SetAs("printName", "Jack-b-Nimble")
                   .SetAs("npcType", "monster");

            var ms = new JSONNode()
                    .SetAs("albedo", GameLoader.BLOCKS_NPC_PATH + "JackbNimble.png")
                    .SetAs("normal", GameLoader.BLOCKS_NPC_PATH + "ZombieQueen_normal.png")
                    .SetAs("emissive", GameLoader.BLOCKS_NPC_PATH + "ZombieQueen_emissive.png")
                    .SetAs("initialHealth", 2000)
                    .SetAs("movementSpeed", 3.25f)
                    .SetAs("punchCooldownMS", 500)
                    .SetAs("punchDamage", 100);

            m.SetAs("data", ms);
            _mts = new NPCTypeMonsterSettings(m);
            NPCType.AddSettings(_mts);
        }
    }
}