using System.Collections.Generic;
using NPC;
using Pandaros.Settlers.Entities;
using Pandaros.Settlers.Items;
using Pipliz;
using Pipliz.JSON;
using Server.AI;
using Server.Monsters;
using Server.NPCs;

namespace Pandaros.Settlers.Monsters.Bosses
{
    [ModLoader.ModManagerAttribute]
    public class Bulging : Zombie, IPandaBoss
    {
        public static string Key = GameLoader.NAMESPACE + ".Monsters.Bosses.Bulging";
        private static NPCTypeMonsterSettings _mts;

        private static readonly Dictionary<ushort, int> REWARDS = new Dictionary<ushort, int>
        {
            {Mana.Item.ItemIndex, 10}
        };

        private readonly float _totalHealth = 20000;

        public Bulging() :
            base(NPCType.GetByKeyNameOrDefault(Key), new Path(), new Players.Player(NetworkID.Invalid))
        {
        }

        public Bulging(Path path, Players.Player originalGoal) :
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
            return new Bulging(path, p);
        }

        public string AnnouncementText => "I DONT FEEL SO GOOD";
        public string DeathText => "Boom.";

        public string Name => "Bulging";

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
            {DamageType.Void, 10f},
            {DamageType.Physical, 10f}
        };

        public DamageType ElementalArmor => DamageType.Air;

        public Dictionary<DamageType, float> AdditionalResistance { get; } = new Dictionary<DamageType, float>
        {
            {DamageType.Physical, 0.15f}
        };

        public float MissChance => 0.05f;

        public override bool Update()
        {
            killedBefore = false;
            return base.Update();
        }

        [ModLoader.ModCallbackAttribute(ModLoader.EModCallbackType.AfterItemTypesDefined,
            GameLoader.NAMESPACE + ".Monsters.Bosses.Bulging.Register")]
        [ModLoader.ModCallbackDependsOnAttribute("pipliz.server.loadnpctypes")]
        [ModLoader.ModCallbackProvidesForAttribute("pipliz.server.registermonstertextures")]
        public static void Register()
        {
            var m = new JSONNode()
                   .SetAs("keyName", Key)
                   .SetAs("printName", "Bulging")
                   .SetAs("npcType", "monster");

            var ms = new JSONNode()
                    .SetAs("albedo", GameLoader.NPC_PATH + "Bulging.png")
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

        [ModLoader.ModCallbackAttribute(ModLoader.EModCallbackType.OnMonsterDied,
            GameLoader.NAMESPACE + ".Monsters.Bosses.Bulging.OnMonsterDied")]
        public static void OnMonsterDied(IMonster monster)
        {
            var boss = monster as Bulging;

            if (boss != null)
            {
                var ps            = PlayerState.GetPlayerState(boss.OriginalGoal);
                var banner        = BannerTracker.Get(boss.OriginalGoal);
                var numberToSpawn = ps.Difficulty.Rank * 10;
                var colony        = Colony.Get(boss.originalGoal);

                if (numberToSpawn == 0)
                    numberToSpawn = 10;

                var pos = new Vector3Int(boss.Position);

                for (var i = 0; i < numberToSpawn; i++)
                    PandaMonsterSpawner.CaclulateZombie(banner, colony, MonsterSpawner.GetTypeToSpawn(colony.FollowerCount));
            }
        }
    }
}