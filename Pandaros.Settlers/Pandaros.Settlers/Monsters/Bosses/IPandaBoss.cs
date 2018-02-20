using Server.AI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pandaros.Settlers.Monsters.Bosses
{
    public interface IPandaBoss : Server.Monsters.IMonster
    {
        string Name { get; }
        string AnnouncementText { get; }
        string DeathText { get; }
        string AnnouncementAudio { get; }
        float ZombieMultiplier { get; }
        bool KilledBefore { get; set; }
        IPandaBoss GetNewBoss(Path path, Players.Player p);
    }
}
