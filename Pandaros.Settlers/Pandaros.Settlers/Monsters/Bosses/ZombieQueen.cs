using NPC;
using Pandaros.Settlers.Entities;
using Pipliz.JSON;
using Server.AI;
using Server.Monsters;
using Server.NPCs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Pandaros.Settlers.Monsters.Bosses
{
    [ModLoader.ModManager]
    public class ZombieQueen : Zombie, IPandaBoss
    {
        private float _totalHealth = Configuration.GetorDefault("MaxBossHP_ZombieQueen", 5000);
        public static string Key = GameLoader.NAMESPACE + ".Monsters.Bosses.ZombieQueen";
        static NPCTypeMonsterSettings _mts;
        private double _updateTime;

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterItemTypesDefined, GameLoader.NAMESPACE + ".Monsters.Bosses.ZombieQueen.Register"),
            ModLoader.ModCallbackDependsOn("pipliz.server.loadnpctypes"),
            ModLoader.ModCallbackProvidesFor("pipliz.server.registermonstertextures")]
        public static void Register()
        {
            JSONNode m = new JSONNode()
               .SetAs("keyName", Key)
               .SetAs("printName", "Zombie Queen")
               .SetAs("npcType", "monster");

            var ms = new JSONNode()
                .SetAs("albedo", GameLoader.TEXTURE_FOLDER_PANDA + "/albedo/ZombieQueen.png")
                .SetAs("normal", GameLoader.TEXTURE_FOLDER_PANDA + "/normal/ZombieQueen.png")
                .SetAs("emissive", GameLoader.TEXTURE_FOLDER_PANDA + "/emissive/ZombieQueen.png")
                .SetAs("initialHealth", Configuration.GetorDefault("MaxBossHP_ZombieQueen", 5000))
                .SetAs("movementSpeed", 1.3f)
                .SetAs("punchCooldownMS", 2000)
                .SetAs("punchDamage", 70);

            m.SetAs("data", ms);
            _mts = new NPCTypeMonsterSettings(m);
            NPCType.AddSettings(_mts);
        }

        public IPandaBoss GetNewBoss(Path path, Players.Player p)
        {
            return new ZombieQueen(path, p);
        }

        public string AnnouncementText => "Get them my pretties!";

        public string Name => "ZombieQueen";

        public override float TotalHealth => _totalHealth;

        public bool KilledBefore { get => killedBefore; set => killedBefore = value; }

        public string AnnouncementAudio => null;

        public float ZombieMultiplier => 0;

        public string DeathText => "I'll get you next time my pretties!";

        public ZombieQueen(Path path, Players.Player originalGoal) :
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

            if (_updateTime < Pipliz.Time.SecondsSinceStartDouble)
            {
                var ps = PlayerState.GetPlayerState(OriginalGoal);
                var monster = MonsterTracker.Find(position, 20, ps.Difficulty.BossHPPerColonist);
                var zombie = monster as Zombie;

                if (zombie != null)
                {
                    var c = Colony.Get(ps.Player);

                    if (NPCTracker.TryGetNear(Position, 30, out var npc))
                    {
                        ServerManager.SendAudio(zombie.Position, GameLoader.NAMESPACE + ".TeleportPadMachineAudio");
                        MethodInfo setPos = zombie.GetType().GetMethod("SetPosition",  BindingFlags.NonPublic | BindingFlags.Instance);
                        setPos.Invoke(zombie, new object[] { npc.Position });
                        zombie.SendUpdate();
                        MethodInfo updateChunkPosition = zombie.GetType().GetMethod("UpdateChunkPosition", BindingFlags.NonPublic | BindingFlags.Instance);
                        updateChunkPosition.Invoke(zombie, new Object[0]);
                        MethodInfo reconsiderDecision = zombie.GetType().GetMethod("ReconsiderDecision", BindingFlags.NonPublic | BindingFlags.Instance);
                        reconsiderDecision.Invoke(zombie, new Object[0]);
                        ServerManager.SendAudio(npc.Position.Vector, GameLoader.NAMESPACE + ".TeleportPadMachineAudio");
                    }
                }

                _updateTime = Pipliz.Time.SecondsSinceStartDouble + 10;
            }

            return base.Update();
        }
    }
}
