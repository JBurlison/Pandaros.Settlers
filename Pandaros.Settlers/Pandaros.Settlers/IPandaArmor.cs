using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pandaros.Settlers
{
    public interface IPandaArmor
    {
        float MissChance { get; }
        DamageType ElementalArmor { get; }
        Dictionary<DamageType, float> AdditionalResistance { get; }
    }
}
