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
    public class Juggernaut : Zombie, IPandaBoss
    {
        private float _totalHealth = Configuration.GetorDefault("MaxBossHP_Juggernaut", 10000);
        public static string Key = GameLoader.NAMESPACE + ".Monsters.Bosses.Juggernaut";
        static NPCTypeMonsterSettings _mts;

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterItemTypesDefined, GameLoader.NAMESPACE + ".Monsters.Bosses.Juggernaut.Register"),
            ModLoader.ModCallbackDependsOn("pipliz.server.loadnpctypes"),
            ModLoader.ModCallbackProvidesFor("pipliz.server.registermonstertextures")]
        public static void Register()
        {
            JSONNode m = new JSONNode()
               .SetAs("keyName", Key)
               .SetAs("printName", "Juggernaut")
               .SetAs("npcType", "monster");

            var ms = new JSONNode()
                .SetAs("albedo", GameLoader.TEXTURE_FOLDER_PANDA + "/albedo/Juggernaut.png")
                .SetAs("normal", GameLoader.TEXTURE_FOLDER_PANDA + "/normal/Juggernaut.png")
                .SetAs("emissive", GameLoader.TEXTURE_FOLDER_PANDA + "/emissive/Juggernaut.png")
                .SetAs("initialHealth", Configuration.GetorDefault("MaxBossHP_Juggernaut", 10000))
                .SetAs("movementSpeed", .75f)
                .SetAs("punchCooldownMS", 3000)
                .SetAs("punchDamage", 100);

            m.SetAs("data", ms);
            _mts = new NPCTypeMonsterSettings(m);
            NPCType.AddSettings(_mts);
        }

        public IPandaBoss GetNewBoss(Path path, Players.Player p)
        {
            return new Juggernaut(path, p);
        }

        public string AnnouncementText => "IM THE JUGGERNAUT B$#CH!";
        public string DeathText => "Juggernaut want to smash.....";
        public string Name => "Juggernaut";
        public override float TotalHealth => _totalHealth;
        public bool KilledBefore { get => killedBefore; set => killedBefore = value; }
        public string AnnouncementAudio => GameLoader.NAMESPACE + "ZombieAudio";
        public float ZombieMultiplier => 0f;

        public Juggernaut(Path path, Players.Player originalGoal) :
            base (NPCType.GetByKeyNameOrDefault(Key), path, originalGoal)
        {
            Colony c = Colony.Get(originalGoal);
            var ps = PlayerState.GetPlayerState(originalGoal);
            var hp = c.FollowerCount * (ps.Difficulty.BossHPPerColonist * 1.5f);

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
