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
    public class FallenRanger : Zombie, IPandaBoss
    {
        private Dictionary<DamageType, float> _damage = new Dictionary<DamageType, float>()
        {
            { DamageType.Void, 10f },
            { DamageType.Physical, 20f }
        };

        private Dictionary<DamageType, float> _additionalResistance = new Dictionary<DamageType, float>()
        {
            { DamageType.Physical, 0.15f },
        };

        private float _totalHealth = 40000;
        public static string Key = GameLoader.NAMESPACE + ".Monsters.Bosses.FallenRanger";
        static NPCTypeMonsterSettings _mts;
        double _cooldown = 2;

        static Dictionary<ushort, int> REWARDS = new Dictionary<ushort, int>()
        {
            { Items.Mana.Item.ItemIndex, 10 }
        };

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterWorldLoad, GameLoader.NAMESPACE + ".Monsters.Bosses.FallenRanger.AfterWorldLoad"),
            ModLoader.ModCallbackProvidesFor(GameLoader.NAMESPACE + ".Managers.MonsterManager.AfterWorldLoad")]
        public static void AfterWorldLoad()
        {
            Managers.MonsterManager.AddBoss(new FallenRanger(new Server.AI.Path(), Players.GetPlayer(NetworkID.Server)));
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterItemTypesDefined, GameLoader.NAMESPACE + ".Monsters.Bosses.FallenRanger.Register"),
            ModLoader.ModCallbackDependsOn("pipliz.server.loadnpctypes"),
            ModLoader.ModCallbackProvidesFor("pipliz.server.registermonstertextures")]
        public static void Register()
        {
            JSONNode m = new JSONNode()
               .SetAs("keyName", Key)
               .SetAs("printName", "Fallen Ranger")
               .SetAs("npcType", "monster");

            var ms = new JSONNode()
                .SetAs("albedo", GameLoader.TEXTURE_FOLDER_PANDA + "/albedo/FallenRanger.png")
                .SetAs("normal", GameLoader.TEXTURE_FOLDER_PANDA + "/normal/ZombieQueen.png")
                .SetAs("emissive", GameLoader.TEXTURE_FOLDER_PANDA + "/emissive/ZombieQueen.png")
                .SetAs("initialHealth", 4000)
                .SetAs("movementSpeed", 1.25f)
                .SetAs("punchCooldownMS", 2000)
                .SetAs("punchDamage", 100);

            m.SetAs("data", ms);
            _mts = new NPCTypeMonsterSettings(m);
            NPCType.AddSettings(_mts);
        }

        public IPandaBoss GetNewBoss(Path path, Players.Player p)
        {
            return new FallenRanger(path, p);
        }

        public string AnnouncementText => "I've got you in my sights!";
        public string DeathText => "Looks like I have to work on my aim.";
        public string Name => "Fallen Ranger";
        public override float TotalHealth => _totalHealth;
        public bool KilledBefore { get => false; set => killedBefore = value; }
        public string AnnouncementAudio => GameLoader.NAMESPACE + "ZombieAudio";
        public float ZombieMultiplier => 1f;
        public float ZombieHPBonus => 0;
        public float MissChance => 0.05f;

        public Dictionary<ushort, int> KillRewards => REWARDS;
        public Dictionary<DamageType, float> Damage => _damage;

        public DamageType ElementalArmor => DamageType.Earth;

        public Dictionary<DamageType, float> AdditionalResistance => _additionalResistance;

        public FallenRanger(Path path, Players.Player originalGoal) :
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
            if (Pipliz.Time.SecondsSinceStartDouble > _cooldown &&
                NPCTracker.TryGetNear(Position, 50, out var npc))
            {
                Server.Indicator.SendIconIndicatorNear(new Pipliz.Vector3Int(Position), ID, new Shared.IndicatorState(2, GameLoader.Bow_Icon));
                ServerManager.SendAudio(Position, "bowShoot");
                npc.OnHit(100, this, ModLoader.OnHitData.EHitSourceType.Monster);
                ServerManager.SendAudio(npc.Position.Vector, "fleshHit");
                _cooldown = Pipliz.Time.SecondsSinceStartDouble + 4;
            }

            killedBefore = false;
            return base.Update();
        }
    }
}
