﻿using AI;
using Monsters;
using NPC;
using Pandaros.API;
using Pandaros.API.Entities;
using Pandaros.API.Monsters;
using Pipliz.JSON;
using System.Collections.Generic;
using System.Reflection;

namespace Pandaros.Settlers.Monsters.Bosses
{
    [ModLoader.ModManager]
    public class ZombieQueen : Zombie, IPandaBoss
    {
        public static string Key = GameLoader.NAMESPACE + ".Monsters.Bosses.ZombieQueen";
        private static NPCTypeMonsterSettings _mts;

        private float _totalHealth = 20000;
        private double _updateTime;

        public ZombieQueen() :
            base(NPCType.GetByKeyNameOrDefault(Key), new Path(), GameLoader.StubColony)
        {
        }

        public ZombieQueen(Path path, Colony originalGoal) :
            base(NPCType.GetByKeyNameOrDefault(Key), path, originalGoal)
        {
            var ps = ColonyState.GetColonyState(originalGoal);
            _totalHealth = originalGoal.FollowerCount * ps.Difficulty.BossHPPerColonist;
            TotalHealth = _totalHealth;
            CurrentHealth = _totalHealth;
        }

        public IPandaZombie GetNewInstance(Path path, Colony p)
        {
            return new ZombieQueen(path, p);
        }

        public string AnnouncementText => "Get them my pretties!";

        public string name => "ZombieQueen";

        public override float TotalHealth => _totalHealth;

        public bool KilledBefore
        {
            get => killedBefore;
            set => killedBefore = value;
        }

        public string AnnouncementAudio => null;

        public float ZombieMultiplier => 1.1f;

        public string DeathText => "I'll get you next time my pretties!";

        public float ZombieHPBonus => 0;
        public float MissChance => 0.05f;

        public Dictionary<DamageType, float> Damage { get; } = new Dictionary<DamageType, float>
        {
            {DamageType.Void, 25f},
            {DamageType.Physical, 55f}
        };

        public DamageType ElementalArmor => DamageType.Water;

        public Dictionary<DamageType, float> AdditionalResistance { get; } = new Dictionary<DamageType, float>();
        public string MosterType => "Boss";

        public int MinColonists => 150;

        public override bool Update()
        {
            killedBefore = false;
            return base.Update();
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnUpdate, GameLoader.NAMESPACE + ".Monsters.Bosses.ZombieQueen.OnUpdate")]
        public void OnUpdate()
        {
            killedBefore = false;

            if (_updateTime < Pipliz.Time.SecondsSinceStartDouble)
            {
                var alreadyTeleported = new List<IMonster>();
                var ps = ColonyState.GetColonyState(OriginalGoal);
                var rank = ps.Difficulty.Rank;
                var teleportHP = ps.Difficulty.GetorDefault("ZombieQueenTargetTeleportHp", 100);
                var cooldown = ps.Difficulty.GetorDefault("ZombieQueenTargetTeleportCooldownSeconds", 5);


                for (var i = 0; i < rank - 1; i++)
                {
                    var monster = MonsterTracker.Find(position, 20, teleportHP);
                    var zombie = monster as Zombie;

                    if (zombie != null &&
                        NPCTracker.TryGetNear(Position, 50, out var npc) &&
                        !alreadyTeleported.Contains(monster))
                    {
                        AudioManager.SendAudio(zombie.Position, GameLoader.NAMESPACE + ".TeleportPadMachineAudio");

                        var setPos = zombie.GetType().GetMethod("SetPosition", BindingFlags.NonPublic | BindingFlags.Instance);

                        if (PathingManager.TryCanStandNearNotAt(npc.Position, out var posFound, out var position) && posFound)
                            setPos.Invoke(zombie, new object[] { position });

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
                        AudioManager.SendAudio(npc.Position.Vector, GameLoader.NAMESPACE + ".TeleportPadMachineAudio");
                        alreadyTeleported.Add(zombie);
                    }
                }

                _updateTime = Pipliz.Time.SecondsSinceStartDouble + cooldown;
            }
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterItemTypesDefined,  GameLoader.NAMESPACE + ".Monsters.Bosses.ZombieQueen.Register")]
        [ModLoader.ModCallbackDependsOn("pipliz.server.loadnpctypes")]
        [ModLoader.ModCallbackProvidesFor("pipliz.server.registermonstertextures")]
        public static void Register()
        {
            var m = new JSONNode()
                   .SetAs("keyName", Key)
                   .SetAs("printName", "Zombie Queen")
                   .SetAs("npcType", "monster");

            var ms = new JSONNode()
                    .SetAs("albedo", GameLoader.BLOCKS_NPC_PATH + "ZombieQueen.png")
                    .SetAs("normal", GameLoader.BLOCKS_NPC_PATH + "ZombieQueen_normal.png")
                    .SetAs("emissive", GameLoader.BLOCKS_NPC_PATH + "ZombieQueen_emissive.png")
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