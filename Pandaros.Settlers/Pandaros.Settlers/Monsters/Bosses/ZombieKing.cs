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
    public class ZombieKing : Zombie, IPandaBoss
    {
        public static string Key = GameLoader.NAMESPACE + ".Monsters.Bosses.ZombieKing";
        private static NPCTypeMonsterSettings _mts;
        private float _totalHealth = 20000;

        public ZombieKing() :
            base(NPCType.GetByKeyNameOrDefault(Key), new Path(), GameLoader.StubColony)
        {
        }

        public ZombieKing(Path path, Colony originalGoal) :
            base(NPCType.GetByKeyNameOrDefault(Key), path, originalGoal)
        {
            var ps = ColonyState.GetColonyState(originalGoal);
            _totalHealth = originalGoal.FollowerCount * ps.Difficulty.BossHPPerColonist;
            TotalHealth = _totalHealth;
            CurrentHealth = _totalHealth;
        }

        public IPandaZombie GetNewInstance(Path path, Colony p)
        {
            return new ZombieKing(path, p);
        }

        public string AnnouncementText => "YOU WILL DO MY BIDDING!";
        public string DeathText => "UGH Help me you useless bags of meat......";

        public string name => "ZombieKing";
        public int MinColonists => 100;
        public override float TotalHealth => _totalHealth;

        public bool KilledBefore
        {
            get => killedBefore;
            set => killedBefore = value;
        }

        public string AnnouncementAudio => GameLoader.NAMESPACE + ".ZombieAudio";

        public float ZombieMultiplier => 1.0f;
        public float MissChance => 0.05f;
        public string MosterType => "Boss";
        public float ZombieHPBonus => 50;

        public Dictionary<DamageType, float> Damage { get; } = new Dictionary<DamageType, float>
        {
            {DamageType.Void, 30f},
            {DamageType.Physical, 50f}
        };

        public DamageType ElementalArmor => DamageType.Fire;

        public Dictionary<DamageType, float> AdditionalResistance { get; } = new Dictionary<DamageType, float>
        {
            {DamageType.Physical, 0.2f}
        };

        public override bool Update()
        {
            killedBefore = false;
            return base.Update();
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterItemTypesDefined,
            GameLoader.NAMESPACE + ".Monsters.Bosses.ZombieKing.Register")]
        [ModLoader.ModCallbackDependsOn("pipliz.server.loadnpctypes")]
        [ModLoader.ModCallbackProvidesFor("pipliz.server.registermonstertextures")]
        public static void Register()
        {
            var m = new JSONNode()
                   .SetAs("keyName", Key)
                   .SetAs("printName", "ZombieKing")
                   .SetAs("npcType", "monster");

            var ms = new JSONNode()
                    .SetAs("albedo", GameLoader.BLOCKS_NPC_PATH + "ZombieKing.png")
                    .SetAs("normal", GameLoader.BLOCKS_NPC_PATH + "ZombieQueen_normal.png")
                    .SetAs("emissive", GameLoader.BLOCKS_NPC_PATH + "ZombieQueen_emissive.png")
                    .SetAs("initialHealth", 20000)
                    .SetAs("movementSpeed", 1.5f)
                    .SetAs("punchCooldownMS", 1000)
                    .SetAs("punchDamage", 80);

            m.SetAs("data", ms);
            _mts = new NPCTypeMonsterSettings(m);
            NPCType.AddSettings(_mts);
        }
    }
}