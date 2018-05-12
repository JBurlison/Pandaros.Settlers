using System;
using Pandaros.Settlers.Entities;
using Server.Monsters;

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