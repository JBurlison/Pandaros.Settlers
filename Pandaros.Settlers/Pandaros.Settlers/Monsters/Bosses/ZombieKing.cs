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
    public class ZombieKing : PandaZombie, IPandaBoss
    {
        private Dictionary<DamageType, float> _damage = new Dictionary<DamageType, float>()
        {
            { DamageType.Void, 30f },
            { DamageType.Physical, 50f }
        };

        private Dictionary<DamageType, float> _additionalResistance = new Dictionary<DamageType, float>()
        {
            { DamageType.Physical, 0.2f }
        };

        private float _totalHealth = 20000;
        public static string Key = GameLoader.NAMESPACE + ".Monsters.Bosses.ZombieKing";
        static NPCTypeMonsterSettings _mts;
        static Dictionary<ushort, int> REWARDS = new Dictionary<ushort, int>()
        {
            { Items.Mana.Item.ItemIndex, 10 }
        };

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterItemTypesDefined, GameLoader.NAMESPACE + ".Monsters.Bosses.ZombieKing.Register"),
            ModLoader.ModCallbackDependsOn("pipliz.server.loadnpctypes"),
            ModLoader.ModCallbackProvidesFor("pipliz.server.registermonstertextures")]
        public static void Register()
        {
            JSONNode m = new JSONNode()
               .SetAs("keyName", Key)
               .SetAs("printName", "ZombieKing")
               .SetAs("npcType", "monster");

            var ms = new JSONNode()
                .SetAs("albedo", GameLoader.TEXTURE_FOLDER_PANDA + "/albedo/ZombieKing.png")
                .SetAs("normal", GameLoader.TEXTURE_FOLDER_PANDA + "/normal/ZombieQueen.png")
                .SetAs("emissive", GameLoader.TEXTURE_FOLDER_PANDA + "/emissive/ZombieQueen.png")
                .SetAs("initialHealth", 20000)
                .SetAs("movementSpeed", 1.5f)
                .SetAs("punchCooldownMS", 1000)
                .SetAs("punchDamage", 80);

            m.SetAs("data", ms);
            _mts = new NPCTypeMonsterSettings(m);
            NPCType.AddSettings(_mts);
        }

        public IPandaBoss GetNewBoss(Path path, Players.Player p)
        {
            return new ZombieKing(path, p);
        }

        public string AnnouncementText => "YOU WILL DO MY BIDDING!";
        public string DeathText => "UGH Help me you useless bags of meat......";

        public string Name => "ZombieKing";

        public override float TotalHealth => _totalHealth;

        public bool KilledBefore { get => killedBefore; set => killedBefore = value; }

        public string AnnouncementAudio => GameLoader.NAMESPACE + "ZombieAudio";

        public float ZombieMultiplier => 1f;
        
        public Dictionary<ushort, int> KillRewards => REWARDS;
        public float ZombieHPBonus => 150;
        public Dictionary<DamageType, float> Damage => _damage;

        public DamageType ElementalArmor => DamageType.Fire;

        public Dictionary<DamageType, float> AdditionalResistance => _additionalResistance;

        public ZombieKing(Path path, Players.Player originalGoal) :
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
