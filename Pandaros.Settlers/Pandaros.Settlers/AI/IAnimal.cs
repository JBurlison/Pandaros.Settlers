using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandaros.Settlers.AI
{
    // TODO: Figure out wheat this needs
    public interface IAnimal
    {
        double RoamUpdate { get; }
        int RoamRange { get; }
    }
}
