using NPC;
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
    public class Hoarder : Zombie, IPandaBoss
    {
        private float _totalHealth = Configuration.GetorDefault("MaxBossHP_Hoarder", 6000);
        public static string Key = GameLoader.NAMESPACE + ".Monsters.Bosses.Hoarder";
        static NPCTypeMonsterSettings _mts;

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterItemTypesDefined, GameLoader.NAMESPACE + ".Monsters.Bosses.Hoarder.Register"),
            ModLoader.ModCallbackDependsOn("pipliz.server.loadnpctypes"),
            ModLoader.ModCallbackProvidesFor("pipliz.server.registermonstertextures")]
        public static void Register()
        {
            JSONNode m = new JSONNode()
               .SetAs("keyName", Key)
               .SetAs("printName", "Hoarder")
               .SetAs("npcType", "monster");

            var ms = new JSONNode()
                .SetAs("albedo", GameLoader.TEXTURE_FOLDER_PANDA + "/albedo/Hoarder.png")
                .SetAs("normal", GameLoader.TEXTURE_FOLDER_PANDA + "/normal/Hoarder.png")
                .SetAs("emissive", GameLoader.TEXTURE_FOLDER_PANDA + "/emissive/Hoarder.png")
                .SetAs("initialHealth", Configuration.GetorDefault("MaxBossHP_Hoarder", 6000))
                .SetAs("movementSpeed", .75f)
                .SetAs("punchCooldownMS", 500)
                .SetAs("punchDamage", 25);

            m.SetAs("data", ms);
            _mts = new NPCTypeMonsterSettings(m);
            NPCType.AddSettings(_mts);
        }

        public IPandaBoss GetNewBoss(Path path, Players.Player p)
        {
            return new Hoarder(path, p);
        }

        public string AnnouncementText => "FEAR THE ZOMBIE HORDE!";

        public bool DoubleZombies => true;

        public string Name => "Hoarder";

        public override float TotalHealth => _totalHealth;

        public bool KilledBefore { get => killedBefore; set => killedBefore = value; }

        public Hoarder(Path path, Players.Player originalGoal) :
            base (NPCType.GetByKeyNameOrDefault(Key), path, originalGoal)
        {
            Colony c = Colony.Get(originalGoal);

            var hp = c.FollowerCount * 50;

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
