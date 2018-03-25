using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pandaros.Settlers
{
    public interface IPandaDamage
    {
        Dictionary<DamageType, float> Damage { get; }
    }
}
