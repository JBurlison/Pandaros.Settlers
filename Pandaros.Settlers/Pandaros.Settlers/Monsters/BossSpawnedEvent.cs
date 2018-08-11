using NPC;
using Pandaros.Settlers.Entities;
using System;

namespace Pandaros.Settlers.Monsters
{
    public class BossSpawnedEvent : EventArgs
    {
        public BossSpawnedEvent(PlayerState ps, IMonster boss)
        {
            Player = ps;
            Boss   = boss;
        }

        public PlayerState Player { get; }

        public IMonster Boss { get; }
    }
}