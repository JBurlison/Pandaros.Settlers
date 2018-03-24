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
        private Dictionary<DamageType, float> _damage = new Dictionary<DamageType, float>()
        {
            { DamageType.Void, 25f },
            { DamageType.Physical, 55f }
        };

        private Dictionary<DamageType, float> _additionalResistance = new Dictionary<DamageType, float>()
        {

        };

        private float _totalHealth = 20000;
        public static string Key = GameLoader.NAMESPACE + ".Monsters.Bosses.ZombieQueen";
        static NPCTypeMonsterSettings _mts;
        private double _updateTime;
        static Dictionary<ushort, int> REWARDS = new Dictionary<ushort, int>()
        {
            { Items.Mana.Item.ItemIndex, 10 }
        };

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
                .SetAs("initialHealth", 20000)
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

        public Dictionary<ushort, int> KillRewards => REWARDS;

        public float ZombieHPBonus => 0;

        public Dictionary<DamageType, float> Damage => _damage;

        public DamageType ElementalArmor => DamageType.Water;

        public Dictionary<DamageType, float> AdditionalResistance => _additionalResistance;

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
                var c = Colony.Get(ps.Player);
                List<IMonster> alreadyTeleported = new List<IMonster>();

                for (int i = 0; i < ps.Difficulty.Rank - 1; i++)
                {
                    var monster = MonsterTracker.Find(position, 20, ps.Difficulty.ZombieQueenTargetTeleportHp);
                    var zombie = monster as Zombie;

                    if (zombie != null && 
                        NPCTracker.TryGetNear(Position, 50, out var npc) && 
                        !alreadyTeleported.Contains(monster))
                    {
                        ServerManager.SendAudio(zombie.Position, GameLoader.NAMESPACE + ".TeleportPadMachineAudio");
                        MethodInfo setPos = zombie.GetType().GetMethod("SetPosition", BindingFlags.NonPublic | BindingFlags.Instance);
                        setPos.Invoke(zombie, new object[] { Server.AI.AIManager.ClosestPositionNotAt(npc.Position, npc.Position) });
                        zombie.SendUpdate();
                        MethodInfo updateChunkPosition = zombie.GetType().GetMethod("UpdateChunkPosition", BindingFlags.NonPublic | BindingFlags.Instance);
                        updateChunkPosition.Invoke(zombie, new Object[0]);
                        var fi = zombie.GetType().GetField("decision", BindingFlags.GetField | BindingFlags.NonPublic | BindingFlags.Instance);
                        fi.SetValue(zombie, default(ZombieDecision));
                        ServerManager.SendAudio(npc.Position.Vector, GameLoader.NAMESPACE + ".TeleportPadMachineAudio");
                        alreadyTeleported.Add(zombie);
                    }
                    
                }

                _updateTime = Pipliz.Time.SecondsSinceStartDouble + ps.Difficulty.ZombieQueenTargetTeleportCooldownSeconds;
            }

            return base.Update();
        }
    }
}
