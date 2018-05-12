using System.Collections.Generic;
using NPC;
using Pandaros.Settlers.Entities;
using Pandaros.Settlers.Items;
using Pipliz.JSON;
using Server.AI;
using Server.NPCs;
using UnityEngine;
using Time = Pipliz.Time;

namespace Pandaros.Settlers.Monsters.Bosses
{
    [ModLoader.ModManagerAttribute]
    public class PutridCorpse : Zombie, IPandaBoss
    {
        public static string Key = GameLoader.NAMESPACE + ".Monsters.Bosses.PutridCorpse";
        private static NPCTypeMonsterSettings _mts;

        private static readonly Dictionary<ushort, int> REWARDS = new Dictionary<ushort, int>
        {
            {Mana.Item.ItemIndex, 10}
        };

        private int _nextBossUpdateTime = int.MinValue;
        private readonly float _totalHealth = 20000;

        public PutridCorpse() :
            base(NPCType.GetByKeyNameOrDefault(Key), new Path(), new Players.Player(NetworkID.Invalid))
        {
        }

        public PutridCorpse(Path path, Players.Player originalGoal) :
            base(NPCType.GetByKeyNameOrDefault(Key), path, originalGoal)
        {
            var c  = Colony.Get(originalGoal);
            var ps = PlayerState.GetPlayerState(originalGoal);
            var hp = c.FollowerCount * ps.Difficulty.BossHPPerColonist;

            if (hp < _totalHealth)
                _totalHealth = hp;

            health = _totalHealth;
        }

        public IPandaBoss GetNewBoss(Path path, Players.Player p)
        {
            return new PutridCorpse(path, p);
        }

        public string AnnouncementText => "Hehehe Smell that?!?!?! Come a little closer...";
        public string DeathText => "ffffffaaarrt....";

        public string Name => "Putrid Corpse";

        public override float TotalHealth => _totalHealth;

        public bool KilledBefore
        {
            get => killedBefore;
            set => killedBefore = value;
        }

        public string AnnouncementAudio => GameLoader.NAMESPACE + "ZombieAudio";

        public float ZombieMultiplier => 1f;
        public float ZombieHPBonus => 20;
        public Dictionary<ushort, int> KillRewards => REWARDS;

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
            if (_nextBossUpdateTime < Time.SecondsSinceStartInt)
            {
                var c = Colony.Get(originalGoal);

                foreach (var follower in c.Followers)
                {
                    var dis = Vector3.Distance(Position, follower.Position.Vector);

                    if (dis <= 20)
                        follower.OnHit(10);
                }

                if (Vector3.Distance(Position, originalGoal.Position) <= 20)
                    Players.TakeHit(originalGoal, 10, true);

                _nextBossUpdateTime = Time.SecondsSinceStartInt + 5;
            }


            killedBefore = false;
            return base.Update();
        }

        [ModLoader.ModCallbackAttribute(ModLoader.EModCallbackType.AfterItemTypesDefined,
            GameLoader.NAMESPACE + ".Monsters.Bosses.PutridCorpse.Register")]
        [ModLoader.ModCallbackDependsOnAttribute("pipliz.server.loadnpctypes")]
        [ModLoader.ModCallbackProvidesForAttribute("pipliz.server.registermonstertextures")]
        public static void Register()
        {
            var m = new JSONNode()
                   .SetAs("keyName", Key)
                   .SetAs("printName", "PutridCorpse")
                   .SetAs("npcType", "monster");

            var ms = new JSONNode()
                    .SetAs("albedo", GameLoader.NPC_PATH + "PutridCorpse.png")
                    .SetAs("normal", GameLoader.NPC_PATH + "Hoarder_normal.png")
                    .SetAs("emissive", GameLoader.NPC_PATH + "Hoarder_emissive.png")
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