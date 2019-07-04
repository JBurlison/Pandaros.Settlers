using AI;
using Monsters;
using NPC;
using Pandaros.Settlers.Entities;
using Pandaros.Settlers.Models;
using Pandaros.Settlers.Monsters.Normal;
using Pipliz;
using Pipliz.JSON;
using Shared;
using System.Collections.Generic;
using System.Linq;

namespace Pandaros.Settlers.Monsters.Bosses
{
    [ModLoader.ModManager]
    public class FallenRanger : Zombie, IPandaBoss
    {
        public static string Key = GameLoader.NAMESPACE + ".Monsters.Bosses.FallenRanger";
        private static NPCTypeMonsterSettings _mts;
        private double _cooldown = 2;
        private float _totalHealth = 40000;

        public FallenRanger() :
            base(NPCType.GetByKeyNameOrDefault(Key), new Path(), GameLoader.StubColony)
        {
        }

        public FallenRanger(Path path, Colony originalGoal) :
            base(NPCType.GetByKeyNameOrDefault(Key), path, originalGoal)
        {
            var ps = ColonyState.GetColonyState(originalGoal);
            _totalHealth = originalGoal.FollowerCount * ps.Difficulty.BossHPPerColonist;
            TotalHealth = _totalHealth;
            CurrentHealth = _totalHealth;
        }

        public IPandaZombie GetNewInstance(Path path, Colony p)
        {
            return new FallenRanger(path, p);
        }

        public string AnnouncementText => "I've got you in my sights!";
        public string DeathText => "Looks like I have to work on my aim.";
        public string name => "Fallen Ranger";
        public override float TotalHealth => _totalHealth;
        public int MinColonists => 150;
        public bool KilledBefore
        {
            get => false;
            set => killedBefore = value;
        }

        public string AnnouncementAudio => GameLoader.NAMESPACE + ".ZombieAudio";
        public float ZombieMultiplier => 1f;
        public float ZombieHPBonus => 0;
        public float MissChance => 0.05f;
        public string MosterType => "Boss";

        public Dictionary<DamageType, float> Damage { get; } = new Dictionary<DamageType, float>
        {
            {DamageType.Void, 20f},
            {DamageType.Physical, 30f}
        };

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

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnUpdate, GameLoader.NAMESPACE + ".Monsters.Bosses.FallenRanger.OnUpdate")]
        public void OnUpdate()
        {
            if (Time.SecondsSinceStartDouble > _cooldown)
            {
                if (Players.FindClosestAlive(Position, out var p, out var dis) &&
                    dis <= 30 &&
                    VoxelPhysics.CanSee(Position, p.Position))
                {
                    Indicator.SendIconIndicatorNear(new Vector3Int(Position), ID,
                                                    new IndicatorState(10, ItemId.GetItemId(GameLoader.NAMESPACE + ".BowIcon").Id));

                    Server.AnimationManager.AnimatedObjects[Server.AnimationManager.ARROW].SendMoveToInterpolated(Position, p.Position);
                    ServerManager.SendParticleTrail(Position, p.Position, 2);
                    AudioManager.SendAudio(Position, "bowShoot");
                    p.Health -= Damage.Sum(kvp => kvp.Key.CalcDamage(DamageType.Physical, kvp.Value));
                    AudioManager.SendAudio(p.Position, "fleshHit");
                    _cooldown = Time.SecondsSinceStartDouble + 10;
                }
                else if (NPCTracker.TryGetNear(Position, 30, out var npc) &&
                         VoxelPhysics.CanSee(Position, npc.Position.Vector))
                {
                    Indicator.SendIconIndicatorNear(new Vector3Int(Position), ID,
                                                    new IndicatorState(10, ItemId.GetItemId(GameLoader.NAMESPACE + ".BowIcon").Id));

                    AudioManager.SendAudio(Position, "bowShoot");
                    Server.AnimationManager.AnimatedObjects[Server.AnimationManager.ARROW].SendMoveToInterpolated(Position, npc.Position.Vector);
                    ServerManager.SendParticleTrail(Position, npc.Position.Vector, 2);
                    npc.OnHit(Damage.Sum(kvp => kvp.Key.CalcDamage(DamageType.Physical, kvp.Value)), this,
                              ModLoader.OnHitData.EHitSourceType.Monster);

                    AudioManager.SendAudio(npc.Position.Vector, "fleshHit");
                    _cooldown = Time.SecondsSinceStartDouble + 10;
                }
            }

            killedBefore = false;
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterItemTypesDefined,  GameLoader.NAMESPACE + ".Monsters.Bosses.FallenRanger.Register")]
        [ModLoader.ModCallbackDependsOn("pipliz.server.loadnpctypes")]
        [ModLoader.ModCallbackProvidesFor("pipliz.server.registermonstertextures")]
        public static void Register()
        {
            var m = new JSONNode()
                   .SetAs("keyName", Key)
                   .SetAs("printName", "Fallen Ranger")
                   .SetAs("npcType", "monster");

            var ms = new JSONNode()
                    .SetAs("albedo", GameLoader.BLOCKS_NPC_PATH + "FallenRanger.png")
                    .SetAs("normal", GameLoader.BLOCKS_NPC_PATH + "ZombieQueen_normal.png")
                    .SetAs("emissive", GameLoader.BLOCKS_NPC_PATH + "ZombieQueen_emissive.png")
                    .SetAs("initialHealth", 4000)
                    .SetAs("movementSpeed", 1.25f)
                    .SetAs("punchCooldownMS", 2000)
                    .SetAs("punchDamage", 100);

            m.SetAs("data", ms);
            _mts = new NPCTypeMonsterSettings(m);
            NPCType.AddSettings(_mts);
        }
    }
}