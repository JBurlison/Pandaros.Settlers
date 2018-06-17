using System.Collections.Generic;
using System.Reflection;
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
    public class ZombieQueen : Zombie, IPandaBoss
    {
        public static string Key = GameLoader.NAMESPACE + ".Monsters.Bosses.ZombieQueen";
        private static NPCTypeMonsterSettings _mts;

        private static readonly Dictionary<ushort, int> REWARDS = new Dictionary<ushort, int>
        {
            {Mana.Item.ItemIndex, 10}
        };

        private readonly float _totalHealth = 20000;
        private double _updateTime;

        public ZombieQueen() :
            base(NPCType.GetByKeyNameOrDefault(Key), new Path(), new Players.Player(NetworkID.Invalid))
        {
        }

        public ZombieQueen(Path path, Players.Player originalGoal) :
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
            return new ZombieQueen(path, p);
        }

        public string AnnouncementText => "Get them my pretties!";

        public string Name => "ZombieQueen";

        public override float TotalHealth => _totalHealth;

        public bool KilledBefore
        {
            get => killedBefore;
            set => killedBefore = value;
        }

        public string AnnouncementAudio => null;

        public float ZombieMultiplier => 1.1f;

        public string DeathText => "I'll get you next time my pretties!";

        public Dictionary<ushort, int> KillRewards => REWARDS;

        public float ZombieHPBonus => 0;
        public float MissChance => 0.05f;

        public Dictionary<DamageType, float> Damage { get; } = new Dictionary<DamageType, float>
        {
            {DamageType.Void, 25f},
            {DamageType.Physical, 55f}
        };

        public DamageType ElementalArmor => DamageType.Water;

        public Dictionary<DamageType, float> AdditionalResistance { get; } = new Dictionary<DamageType, float>();

        public override bool Update()
        {
            killedBefore = false;

            if (_updateTime < Time.SecondsSinceStartDouble)
            {
                var ps                = PlayerState.GetPlayerState(OriginalGoal);
                var c                 = Colony.Get(ps.Player);
                var alreadyTeleported = new List<IMonster>();

                for (var i = 0; i < ps.Difficulty.Rank - 1; i++)
                {
                    var monster = MonsterTracker.Find(position, 20, ps.Difficulty.ZombieQueenTargetTeleportHp);
                    var zombie  = monster as Zombie;

                    if (zombie != null &&
                        NPCTracker.TryGetNear(Position, 50, out var npc) &&
                        !alreadyTeleported.Contains(monster))
                    {
                        ServerManager.SendAudio(zombie.Position, GameLoader.NAMESPACE + ".TeleportPadMachineAudio");

                        var setPos = zombie
                                    .GetType().GetMethod("SetPosition", BindingFlags.NonPublic | BindingFlags.Instance);

                        setPos.Invoke(zombie,
                                      new object[] {AIManager.ClosestPositionNotAt(npc.Position, npc.Position)});

                        zombie.SendUpdate();

                        var updateChunkPosition =
                            zombie.GetType().GetMethod("UpdateChunkPosition",
                                                       BindingFlags.NonPublic | BindingFlags.Instance);

                        updateChunkPosition.Invoke(zombie, new object[0]);

                        var fi = zombie
                                .GetType().GetField("decision",
                                                    BindingFlags.GetField | BindingFlags.NonPublic |
                                                    BindingFlags.Instance);

                        fi.SetValue(zombie, default(ZombieDecision));
                        ServerManager.SendAudio(npc.Position.Vector, GameLoader.NAMESPACE + ".TeleportPadMachineAudio");
                        alreadyTeleported.Add(zombie);
                    }
                }

                _updateTime = Time.SecondsSinceStartDouble + ps.Difficulty.ZombieQueenTargetTeleportCooldownSeconds;
            }

            return base.Update();
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterItemTypesDefined,
            GameLoader.NAMESPACE + ".Monsters.Bosses.ZombieQueen.Register")]
        [ModLoader.ModCallbackDependsOnAttribute("pipliz.server.loadnpctypes")]
        [ModLoader.ModCallbackProvidesFor("pipliz.server.registermonstertextures")]
        public static void Register()
        {
            var m = new JSONNode()
                   .SetAs("keyName", Key)
                   .SetAs("printName", "Zombie Queen")
                   .SetAs("npcType", "monster");

            var ms = new JSONNode()
                    .SetAs("albedo", GameLoader.NPC_PATH + "ZombieQueen.png")
                    .SetAs("normal", GameLoader.NPC_PATH + "ZombieQueen_normal.png")
                    .SetAs("emissive", GameLoader.NPC_PATH + "ZombieQueen_emissive.png")
                    .SetAs("initialHealth", 20000)
                    .SetAs("movementSpeed", 1.3f)
                    .SetAs("punchCooldownMS", 2000)
                    .SetAs("punchDamage", 70);

            m.SetAs("data", ms);
            _mts = new NPCTypeMonsterSettings(m);
            NPCType.AddSettings(_mts);
        }
    }
}