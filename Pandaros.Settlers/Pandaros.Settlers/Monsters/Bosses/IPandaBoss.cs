using AI;
using Monsters;
using Pandaros.Settlers.Monsters.Normal;

namespace Pandaros.Settlers.Monsters.Bosses
{
    public interface IPandaBoss : IPandaZombie
    {
        string AnnouncementText { get; }
        string DeathText { get; }
        string AnnouncementAudio { get; }
        float ZombieMultiplier { get; }
        bool KilledBefore { get; set; }
    }
}