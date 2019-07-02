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
    public class Hoarder : Zombie, IPandaBoss
    {
        public static string Key = GameLoader.NAMESPACE + ".Monsters.Bosses.Hoarder";
        private static NPCTypeMonsterSettings _mts;

        private float _totalHealth = 20000;

        public Hoarder() :
            base(NPCType.GetByKeyNameOrDefault(Key), new Path(), GameLoader.StubColony)
        {
        }

        public Hoarder(Path path, Colony originalGoal) :
            base(NPCType.GetByKeyNameOrDefault(Key), path, originalGoal)
        {
            var ps = ColonyState.GetColonyState(originalGoal);
            _totalHealth = originalGoal.FollowerCount * ps.Difficulty.BossHPPerColonist;
            TotalHealth = _totalHealth;
            CurrentHealth = _totalHealth;
        }

        public IPandaZombie GetNewInstance(Path path, Colony p)
        {
            return new Hoarder(path, p);
        }

        public string AnnouncementText => "FEAR THE ZOMBIE HORDE!";
        public string DeathText => "Gughghgugggghrrrghghgfggg......";

        public string name => "Hoarder";

        public override float TotalHealth => _totalHealth;

        public bool KilledBefore
        {
            get => killedBefore;
            set => killedBefore = value;
        }

        public string AnnouncementAudio => GameLoader.NAMESPACE + ".ZombieAudio";

        public float ZombieMultiplier => 1.2f;
        public float ZombieHPBonus => 0;
        public string MosterType => "Boss";

        public Dictionary<DamageType, float> Damage { get; } = new Dictionary<DamageType, float>
        {
            {DamageType.Void, 10f},
            {DamageType.Physical, 10f}
        };

        public float MissChance => 0.05f;

        public DamageType ElementalArmor => DamageType.Water;

        public Dictionary<DamageType, float> AdditionalResistance { get; } = new Dictionary<DamageType, float>
        {
            {DamageType.Physical, 0.15f}
        };

        public override bool Update()
        {
            killedBefore = false;
            return base.Update();
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterItemTypesDefined,
            GameLoader.NAMESPACE + ".Monsters.Bosses.Hoarder.Register")]
        [ModLoader.ModCallbackDependsOn("pipliz.server.loadnpctypes")]
        [ModLoader.ModCallbackProvidesFor("pipliz.server.registermonstertextures")]
        public static void Register()
        {
            var m = new JSONNode()
                   .SetAs("keyName", Key)
                   .SetAs("printName", "Hoarder")
                   .SetAs("npcType", "monster");

            var ms = new JSONNode()
                    .SetAs("albedo", GameLoader.BLOCKS_NPC_PATH + "Hoarder.png")
                    .SetAs("normal", GameLoader.BLOCKS_NPC_PATH + "Hoarder_normal.png")
                    .SetAs("emissive", GameLoader.BLOCKS_NPC_PATH + "Hoarder_emissive.png")
                    .SetAs("initialHealth", 20000)
                    .SetAs("movementSpeed", .75f)
                    .SetAs("punchCooldownMS", 500)
                    .SetAs("punchDamage", 25);

            m.SetAs("data", ms);
            _mts = new NPCTypeMonsterSettings(m);
            NPCType.AddSettings(_mts);
        }
    }
}