using Pandaros.Settlers.Entities;
using Server.Monsters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pandaros.Settlers.Monsters
{
    public class BossSpawnedEvent : EventArgs
    {
        public BossSpawnedEvent(PlayerState ps, IMonster boss)
        {
            Player = ps;
            Boss = boss;
        }

        public PlayerState Player { get; private set; }

        public IMonster Boss { get; private set; }
    }
}
