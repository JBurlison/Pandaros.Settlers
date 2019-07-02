using AI;
using Monsters;
using NPC;
using Pandaros.Settlers.Entities;
using Pandaros.Settlers.Monsters.Normal;
using Pipliz.JSON;
using System.Collections.Generic;
using UnityEngine;
using Time = Pipliz.Time;

namespace Pandaros.Settlers.Monsters.Bosses
{
    [ModLoader.ModManager]
    public class PutridCorpse : Zombie, IPandaBoss
    {
        public static string Key = GameLoader.NAMESPACE + ".Monsters.Bosses.PutridCorpse";
        private static NPCTypeMonsterSettings _mts;
        private int _nextBossUpdateTime = int.MinValue;
        private float _totalHealth = 20000;

        public PutridCorpse() :
            base(NPCType.GetByKeyNameOrDefault(Key), new Path(), GameLoader.StubColony)
        {
        }

        public PutridCorpse(Path path, Colony originalGoal) :
            base(NPCType.GetByKeyNameOrDefault(Key), path, originalGoal)
        {
            var ps = ColonyState.GetColonyState(originalGoal);
            _totalHealth = originalGoal.FollowerCount * ps.Difficulty.BossHPPerColonist;
            TotalHealth = _totalHealth;
            CurrentHealth = _totalHealth;
        }

        public IPandaZombie GetNewInstance(Path path, Colony p)
        {
            return new PutridCorpse(path, p);
        }

        public string AnnouncementText => "Hehehe Smell that?!?!?! Come a little closer...";
        public string DeathText => "ffffffaaarrt....";

        public string name => "Putrid Corpse";

        public override float TotalHealth => _totalHealth;

        public bool KilledBefore
        {
            get => killedBefore;
            set => killedBefore = value;
        }

        public string AnnouncementAudio => GameLoader.NAMESPACE + ".ZombieAudio";

        public float ZombieMultiplier => 1f;
        public float ZombieHPBonus => 20;
        public string MosterType => "Boss";

        public Dictionary<DamageType, float> Damage { get; } = new Dictionary<DamageType, float>
        {
            {DamageType.Void, 25f},
            {DamageType.Physical, 50f}
        };

        public float MissChance => 0.05f;
        public DamageType ElementalArmor => DamageType.Earth;

        public Dictionary<DamageType, float> AdditionalResistance { get; } = new Dictionary<DamageType, float>
        {
            {DamageType.Physical, 0.15f}
        };

        public override bool Update()
        {
            killedBefore = false;
            return base.Update();
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnUpdate, GameLoader.NAMESPACE + ".Monsters.Bosses.PutridCorpse.OnUpdate")]
        public void OnUpdate()
        {
            if (_nextBossUpdateTime < Time.SecondsSinceStartInt)
            {
                foreach (var follower in originalGoal.Followers)
                {
                    var dis = Vector3.Distance(Position, follower.Position.Vector);

                    if (dis <= 20)
                        follower.OnHit(10);
                }

                originalGoal.ForEachOwner(o =>
                {
                    if (Vector3.Distance(Position, o.Position) <= 20)
                        Players.TakeHit(o, 10, true);
                });

                _nextBossUpdateTime = Time.SecondsSinceStartInt + 5;
            }


            killedBefore = false;
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterItemTypesDefined, GameLoader.NAMESPACE + ".Monsters.Bosses.PutridCorpse.Register")]
        [ModLoader.ModCallbackDependsOn("pipliz.server.loadnpctypes")]
        [ModLoader.ModCallbackProvidesFor("pipliz.server.registermonstertextures")]
        public static void Register()
        {
            var m = new JSONNode()
                   .SetAs("keyName", Key)
                   .SetAs("printName", "PutridCorpse")
                   .SetAs("npcType", "monster");

            var ms = new JSONNode()
                    .SetAs("albedo", GameLoader.BLOCKS_NPC_PATH + "PutridCorpse.png")
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
    }
}