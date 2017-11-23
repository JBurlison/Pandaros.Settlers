using NPC;
using Server.AI;
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

        }
    }
}
