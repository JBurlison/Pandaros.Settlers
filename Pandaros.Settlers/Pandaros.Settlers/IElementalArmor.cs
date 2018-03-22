using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pandaros.Settlers
{
    public interface IElementalArmor
    {
        DamageType ElementalArmor { get; }
        Dictionary<DamageType, float> AdditionalResistance { get; }
    }
}
