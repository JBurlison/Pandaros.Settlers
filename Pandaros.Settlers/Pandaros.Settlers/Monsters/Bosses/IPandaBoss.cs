using AI;
using Monsters;

namespace Pandaros.Settlers.Monsters.Bosses
{
    public interface IPandaBoss : IMonster, IPandaDamage, IPandaArmor, IKillReward, INameable
    {
        string AnnouncementText { get; }
        string DeathText { get; }
        string AnnouncementAudio { get; }
        float ZombieMultiplier { get; }
        float ZombieHPBonus { get; }
        bool KilledBefore { get; set; }
        IPandaBoss GetNewBoss(Path path, Colony c);
    }
}