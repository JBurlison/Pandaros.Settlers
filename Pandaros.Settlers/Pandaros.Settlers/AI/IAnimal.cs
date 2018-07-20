using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandaros.Settlers.AI
{
    public interface IAnimal
    {
        double RoamUpdate { get; }
        int RoamRange { get; }
    }
}
