using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pandaros.Settlers
{
    public interface IElementalDamager
    {
        Dictionary<DamageType, float> Damage { get; }
    }
}
