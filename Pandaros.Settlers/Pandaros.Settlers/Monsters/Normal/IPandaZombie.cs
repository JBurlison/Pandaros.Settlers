using Monsters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandaros.Settlers.Monsters.Normal
{
    public interface IPandaZombie : IMonster, IPandaDamage, IPandaArmor, IKillReward, INameable
    {
        float ZombieHPBonus { get; }
    }
}
