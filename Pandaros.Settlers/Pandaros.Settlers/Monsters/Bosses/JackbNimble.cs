using NPC;
using Pandaros.Settlers.Entities;
using Pipliz.JSON;
using Server.AI;
using Server.Monsters;
using Server.NPCs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pandaros.Settlers.Monsters.Bosses
{
    [ModLoader.ModManager]
    public class JackbNimble : Zombie, IPandaBoss
    {
        private Dictionary<DamageType, float> _damage = new Dictionary<DamageType, float>()
        {
            { DamageType.Void, 10f },
            { DamageType.Physical, 10f }
        };

        private Dictionary<DamageType, float> _additionalResistance = new Dictionary<DamageType, float>()
        {
            { DamageType.Fire, 0.20f },
        };

        private float _totalHealth = 40000;
        public static string Key = GameLoader.NAMESPACE + ".Monsters.Bosses.JackbNimble";
        static NPCTypeMonsterSettings _mts;
        static Dictionary<ushort, int> REWARDS = new Dictionary<ushort, int>()
        {
            { Items.Mana.Item.ItemIndex, 10 }
        };

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterItemTypesDefined, GameLoader.NAMESPACE + ".Monsters.Bosses.JackbNimble.Register"),
            ModLoader.ModCallbackDependsOn("pipliz.server.loadnpctypes"),
            ModLoader.ModCallbackProvidesFor("pipliz.server.registermonstertextures")]
        public static void Register()
        {
            JSONNode m = new JSONNode()
               .SetAs("keyName", Key)
               .SetAs("printName", "Jack-b-Nimble")
               .SetAs("npcType", "monster");

            var ms = new JSONNode()
                .SetAs("albedo", GameLoader.NPC_PATH + "JackbNimble.png")
                .SetAs("normal", GameLoader.NPC_PATH + "ZombieQueen_normal.png")
                .SetAs("emissive", GameLoader.NPC_PATH + "ZombieQueen_emissive.png")
                .SetAs("initialHealth", 2000)
                .SetAs("movementSpeed", 3.25f)
                .SetAs("punchCooldownMS", 500)
                .SetAs("punchDamage", 100);

            m.SetAs("data", ms);
            _mts = new NPCTypeMonsterSettings(m);
            NPCType.AddSettings(_mts);
        }

        public IPandaBoss GetNewBoss(Path path, Players.Player p)
        {
            return new JackbNimble(path, p);
        }

        public string AnnouncementText => "Catch me if you can!";
        public string DeathText => "I just was not fast enough...";
        public string Name => "Jack-b-Nimble";
        public override float TotalHealth => _totalHealth;
        public bool KilledBefore { get => false; set => killedBefore = value; }
        public string AnnouncementAudio => GameLoader.NAMESPACE + "ZombieAudio";
        public float ZombieMultiplier => 1f;
        public float ZombieHPBonus => 0;

        public Dictionary<ushort, int> KillRewards => REWARDS;
        public Dictionary<DamageType, float> Damage => _damage;
        public float MissChance => 0.10f;
        public DamageType ElementalArmor => DamageType.Air;

        public Dictionary<DamageType, float> AdditionalResistance => _additionalResistance;

        public JackbNimble() :
            base(NPCType.GetByKeyNameOrDefault(Key), new Path(), new Players.Player(NetworkID.Invalid))
        {
        }

        public JackbNimble(Path path, Players.Player originalGoal) :
            base (NPCType.GetByKeyNameOrDefault(Key), path, originalGoal)
        {
            Colony c = Colony.Get(originalGoal);
            var ps = PlayerState.GetPlayerState(originalGoal);
            var hp = c.FollowerCount * (ps.Difficulty.BossHPPerColonist - (ps.Difficulty.BossHPPerColonist * .5f));

            if (hp < _totalHealth)
                _totalHealth = hp;

            health = _totalHealth;
        }

        public override bool Update()
        {
            killedBefore = false;
            return base.Update();
        }
    }
}
