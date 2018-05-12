using System.Collections.Generic;

namespace Pandaros.Settlers
{
    public interface IPandaArmor
    {
        float MissChance { get; }
        DamageType ElementalArmor { get; }
        Dictionary<DamageType, float> AdditionalResistance { get; }
    }
}