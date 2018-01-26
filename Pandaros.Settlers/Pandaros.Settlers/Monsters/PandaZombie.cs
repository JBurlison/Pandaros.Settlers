using NPC;
using Pipliz.JSON;
using Server.AI;
using Server.Monsters;
using Server.NPCs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pandaros.Settlers.Monsters
{
    public class PandaZombie : Zombie
    {
        

        public PandaZombie(NPCType type, Path path, Players.Player originalGoal) :
            base (type, path, originalGoal)
        {
            JSONNode m = new JSONNode()
                .SetAs("keyName", GameLoader.NAMESPACE + ".Monsters.BossA")
                .SetAs("printName", "Get Fucked.")
                .SetAs("npcType", "monster");

            var ms = new JSONNode()
                .SetAs("albedo", "MY_SKIN.jpg")
                .SetAs("normal", "MY_SKIN.jpg")
                .SetAs("special", "MY_SKIN.jpg")
                .SetAs("initialHealth", 10000)
                .SetAs("movementSpeed", .75f)
                .SetAs("punchCooldownMS", 500)
                .SetAs("punchDamage", 10);

            m.SetAs("data", ms);

            NPCTypeMonsterSettings mts = new NPCTypeMonsterSettings(m);
    
            MonsterTracker.Add(this);
        }
    }
}
