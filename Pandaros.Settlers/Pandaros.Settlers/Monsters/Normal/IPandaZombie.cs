using AI;
using Monsters;
using Pandaros.Settlers.Monsters.Bosses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandaros.Settlers.Monsters.Normal
{
    public interface IPandaZombie : IMonster, IPandaDamage, IPandaArmor, INameable
    {
        float ZombieHPBonus { get; }
        string MosterType { get; }

        IPandaZombie GetNewInstance(Path path, Colony c);
    }
}
