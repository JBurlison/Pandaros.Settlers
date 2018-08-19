using NPC;
using Pandaros.Settlers.Entities;
using System;

namespace Pandaros.Settlers.Monsters
{
    public class BossSpawnedEvent : EventArgs
    {
        public BossSpawnedEvent(ColonyState cs, IMonster boss)
        {
            Colony = cs;
            Boss   = boss;
        }

        public ColonyState Colony { get; }

        public IMonster Boss { get; }
    }
}