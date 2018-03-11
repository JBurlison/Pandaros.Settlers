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
        float ZombieHPBonus { get; }
        bool KilledBefore { get; set; }
        Dictionary<ushort, int> KillRewards { get; }
        Dictionary<DamageType, float> Damage { get; }
        List<DamageType> ElementalArmor { get; }
        Dictionary<DamageType, float> AdditionalResistance { get; }
        IPandaBoss GetNewBoss(Path path, Players.Player p);
    }
}
