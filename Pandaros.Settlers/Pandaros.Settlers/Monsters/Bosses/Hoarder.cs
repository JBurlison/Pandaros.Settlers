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
    public class Hoarder : Zombie, IPandaBoss
    {
        private Dictionary<DamageType, float> _damage = new Dictionary<DamageType, float>()
        {
            { DamageType.Void, 10f },
            { DamageType.Physical, 10f }
        };

        private Dictionary<DamageType, float> _additionalResistance = new Dictionary<DamageType, float>()
        {
            { DamageType.Physical, 0.15f }
        };

        private float _totalHealth = 20000;
        public static string Key = GameLoader.NAMESPACE + ".Monsters.Bosses.Hoarder";
        static NPCTypeMonsterSettings _mts;
        static Dictionary<ushort, int> REWARDS = new Dictionary<ushort, int>()
        {
            { Items.Mana.Item.ItemIndex, 10 }
        };

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterItemTypesDefined, GameLoader.NAMESPACE + ".Monsters.Bosses.Hoarder.Register"),
            ModLoader.ModCallbackDependsOn("pipliz.server.loadnpctypes"),
            ModLoader.ModCallbackProvidesFor("pipliz.server.registermonstertextures")]
        public static void Register()
        {
            JSONNode m = new JSONNode()
               .SetAs("keyName", Key)
               .SetAs("printName", "Hoarder")
               .SetAs("npcType", "monster");

            var ms = new JSONNode()
                .SetAs("albedo", "Hoarder.png")
                .SetAs("normal", "Hoarder_normal.png")
                .SetAs("emissive", "Hoarder_emissive.png")
                .SetAs("initialHealth", 20000)
                .SetAs("movementSpeed", .75f)
                .SetAs("punchCooldownMS", 500)
                .SetAs("punchDamage", 25);

            m.SetAs("data", ms);
            _mts = new NPCTypeMonsterSettings(m);
            NPCType.AddSettings(_mts);
        }

        public IPandaBoss GetNewBoss(Path path, Players.Player p)
        {
            return new Hoarder(path, p);
        }

        public string AnnouncementText => "FEAR THE ZOMBIE HORDE!";
        public string DeathText => "Gughghgugggghrrrghghgfggg......";

        public string Name => "Hoarder";

        public override float TotalHealth => _totalHealth;

        public bool KilledBefore { get => killedBefore; set => killedBefore = value; }

        public string AnnouncementAudio => GameLoader.NAMESPACE + "ZombieAudio";

        public float ZombieMultiplier => 1.2f;
        public float ZombieHPBonus => 0;
        public Dictionary<ushort, int> KillRewards => REWARDS;
        public Dictionary<DamageType, float> Damage => _damage;
        public float MissChance => 0.05f;

        public DamageType ElementalArmor => DamageType.Water;

        public Dictionary<DamageType, float> AdditionalResistance => _additionalResistance;

        public Hoarder() :
            base(NPCType.GetByKeyNameOrDefault(Key), new Path(), new Players.Player(NetworkID.Invalid))
        {
        }

        public Hoarder(Path path, Players.Player originalGoal) :
            base (NPCType.GetByKeyNameOrDefault(Key), path, originalGoal)
        {
            Colony c = Colony.Get(originalGoal);
            var ps = PlayerState.GetPlayerState(originalGoal);
            var hp = c.FollowerCount * ps.Difficulty.BossHPPerColonist;

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
