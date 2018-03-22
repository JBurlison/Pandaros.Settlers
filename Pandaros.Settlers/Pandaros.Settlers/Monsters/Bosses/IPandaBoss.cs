using Server.AI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pandaros.Settlers.Monsters.Bosses
{
    public interface IPandaBoss : Server.Monsters.IMonster, IElementalDamager, IElementalArmor, IKillReward
    {
        string Name { get; }
        string AnnouncementText { get; }
        string DeathText { get; }
        string AnnouncementAudio { get; }
        float ZombieMultiplier { get; }
        float ZombieHPBonus { get; }
        bool KilledBefore { get; set; }
        IPandaBoss GetNewBoss(Path path, Players.Player p);
    }
}
