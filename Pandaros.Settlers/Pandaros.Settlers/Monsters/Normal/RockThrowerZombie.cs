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

namespace Pandaros.Settlers.Monsters.Normal
{
    [ModLoader.ModManager]
    public class RockThrowerZombie : Zombie, IPandaZombie
    {
        public static string Key = GameLoader.NAMESPACE + ".Monsters.Bosses.RockThrowerZombie";
        private static NPCTypeMonsterSettings _mts;
        private double _cooldown = 2;
        private float _totalHealth = 500;

        public RockThrowerZombie() :
            base(NPCType.GetByKeyNameOrDefault(Key), new Path(), GameLoader.StubColony)
        {
        }

        public RockThrowerZombie(Path path, Colony originalGoal) :
            base(NPCType.GetByKeyNameOrDefault(Key), path, originalGoal)
        {
            TotalHealth = _totalHealth;
            CurrentHealth = _totalHealth;
        }

        public IPandaZombie GetNewInstance(Path path, Colony p)
        {
            return new RockThrowerZombie(path, p);
        }


        public string name => "RockThrowerZombie";
        public override float TotalHealth => _totalHealth;
        public int MinColonists => 100;
        public bool KilledBefore
        {
            get => false;
            set => killedBefore = value;
        }

        public float ZombieHPBonus => 0;
        public float MissChance => 0.05f;
        public string MosterType => "PandaZombie";

        public Dictionary<DamageType, float> Damage { get; } = new Dictionary<DamageType, float>
        {
            {DamageType.Physical, 50f}
        };

        public DamageType ElementalArmor => DamageType.Physical;

        public Dictionary<DamageType, float> AdditionalResistance { get; } = new Dictionary<DamageType, float>
        {
            {DamageType.Physical, 0.15f}
        };

        public override bool Update()
        {
            if (Time.SecondsSinceStartDouble > _cooldown)
            {
                if (Players.FindClosestAlive(Position, out var p, out var dis) &&
                    dis <= 30 &&
                    VoxelPhysics.CanSee(Position, p.Position))
                {
                    Indicator.SendIconIndicatorNear(new Vector3Int(Position), ID,
                                                    new IndicatorState(2, ColonyBuiltIn.ItemTypes.SLINGBULLET.Name));

                    AudioManager.SendAudio(Position, "sling");
                    p.Health -= Damage.Sum(kvp => kvp.Key.CalcDamage(DamageType.Physical, kvp.Value));
                    AudioManager.SendAudio(p.Position, "fleshHit");
                    _cooldown = Time.SecondsSinceStartDouble + 4;
                }
                else if (NPCTracker.TryGetNear(Position, 30, out var npc) &&
                         VoxelPhysics.CanSee(Position, npc.Position.Vector))
                {
                    Indicator.SendIconIndicatorNear(new Vector3Int(Position), ID,
                                                    new IndicatorState(2, ColonyBuiltIn.ItemTypes.SLINGBULLET.Name));

                    AudioManager.SendAudio(Position, "sling");

                    npc.OnHit(Damage.Sum(kvp => kvp.Key.CalcDamage(DamageType.Physical, kvp.Value)), this,
                              ModLoader.OnHitData.EHitSourceType.Monster);

                    AudioManager.SendAudio(npc.Position.Vector, "fleshHit");
                    _cooldown = Time.SecondsSinceStartDouble + 4;
                }
            }

            return base.Update();
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterItemTypesDefined,  GameLoader.NAMESPACE + ".Monsters.Normal.RockThrowerZombie.Register")]
        [ModLoader.ModCallbackDependsOn("pipliz.server.loadnpctypes")]
        [ModLoader.ModCallbackProvidesFor("pipliz.server.registermonstertextures")]
        public static void Register()
        {
            var m = new JSONNode()
                   .SetAs("keyName", Key)
                   .SetAs("printName", "Rock Thrower Zombie")
                   .SetAs("npcType", "monster");

            var ms = new JSONNode()
                    .SetAs("albedo", GameLoader.BLOCKS_NPC_PATH + "FallenRanger.png")
                    .SetAs("normal", GameLoader.BLOCKS_NPC_PATH + "ZombieQueen_normal.png")
                    .SetAs("emissive", GameLoader.BLOCKS_NPC_PATH + "ZombieQueen_emissive.png")
                    .SetAs("initialHealth", 500)
                    .SetAs("movementSpeed", 1.25f)
                    .SetAs("punchCooldownMS", 2000)
                    .SetAs("punchDamage", 50);

            m.SetAs("data", ms);
            _mts = new NPCTypeMonsterSettings(m);
            NPCType.AddSettings(_mts);
        }
    }
}