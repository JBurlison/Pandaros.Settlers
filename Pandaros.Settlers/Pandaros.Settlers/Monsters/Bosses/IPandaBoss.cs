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
        string AnnouncementAudio { get; }
        bool DoubleZombies { get; }
        bool KilledBefore { get; set; }
        IPandaBoss GetNewBoss(Path path, Players.Player p);
    }
}
