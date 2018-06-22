using Pandaros.Settlers.Extender;
using System.Collections.Generic;

namespace Pandaros.Settlers
{
    public interface IPandaArmor : INameable
    {
        float MissChance { get; }
        DamageType ElementalArmor { get; }
        Dictionary<DamageType, float> AdditionalResistance { get; }
    }
}