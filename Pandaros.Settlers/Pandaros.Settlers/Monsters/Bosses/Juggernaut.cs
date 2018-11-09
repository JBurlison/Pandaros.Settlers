using AI;
using Monsters;
using NPC;
using Pandaros.Settlers.Entities;
using Pipliz.JSON;
using System.Collections.Generic;

namespace Pandaros.Settlers.Monsters.Bosses
{
    [ModLoader.ModManager]
    public class Juggernaut : Zombie, IPandaBoss
    {
        public static string Key = GameLoader.NAMESPACE + ".Monsters.Bosses.Juggernaut";
        private static NPCTypeMonsterSettings _mts;

        private float _totalHealth = 40000;

        public Juggernaut() :
            base(NPCType.GetByKeyNameOrDefault(Key), new Path(), GameLoader.StubColony)
        {
        }

        public Juggernaut(Path path, Colony originalGoal) :
            base(NPCType.GetByKeyNameOrDefault(Key), path, originalGoal)
        {
            var ps = ColonyState.GetColonyState(originalGoal);
            _totalHealth = originalGoal.FollowerCount * (ps.Difficulty.BossHPPerColonist * 2.25f);
            health = _totalHealth;
        }

        public IPandaBoss GetNewBoss(Path path, Colony p)
        {
            return new Juggernaut(path, p);
        }

        public string AnnouncementText => "IM THE JUGGERNAUT B$#CH!";
        public string DeathText => "Juggernaut want to smash.....";
        public string Name => "Juggernaut";
        public override float TotalHealth => _totalHealth;

        public bool KilledBefore
        {
            get => killedBefore;
            set => killedBefore = value;
        }

        public string AnnouncementAudio => GameLoader.NAMESPACE + "ZombieAudio";
        public float ZombieMultiplier => 1f;
        public float ZombieHPBonus => 50;

        public string LootTableName => BossLoot.LootTableName;

        public Dictionary<DamageType, float> Damage { get; } = new Dictionary<DamageType, float>
        {
            {DamageType.Void, 70f},
            {DamageType.Physical, 70f}
        };

        public DamageType ElementalArmor => DamageType.Physical;

        public Dictionary<DamageType, float> AdditionalResistance { get; } = new Dictionary<DamageType, float>
        {
            {DamageType.Physical, 0.4f},
            {DamageType.Air, 0.4f},
            {DamageType.Earth, 0.4f},
            {DamageType.Water, 0.4f}
        };

        public float MissChance => 0f;

        public override bool Update()
        {
            killedBefore = false;
            return base.Update();
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterItemTypesDefined,
            GameLoader.NAMESPACE + ".Monsters.Bosses.Juggernaut.Register")]
        [ModLoader.ModCallbackDependsOn("pipliz.server.loadnpctypes")]
        [ModLoader.ModCallbackProvidesFor("pipliz.server.registermonstertextures")]
        public static void Register()
        {
            var m = new JSONNode()
                   .SetAs("keyName", Key)
                   .SetAs("printName", "Juggernaut")
                   .SetAs("npcType", "monster");

            var ms = new JSONNode()
                    .SetAs("albedo", GameLoader.NPC_PATH + "Juggernaut.png")
                    .SetAs("normal", GameLoader.NPC_PATH + "Juggernaut_normal.png")
                    .SetAs("emissive", GameLoader.NPC_PATH + "Juggernaut_emissive.png")
                    .SetAs("initialHealth", 40000)
                    .SetAs("movementSpeed", .9f)
                    .SetAs("punchCooldownMS", 3000)
                    .SetAs("punchDamage", 100);

            m.SetAs("data", ms);
            _mts = new NPCTypeMonsterSettings(m);
            NPCType.AddSettings(_mts);
        }
    }
}