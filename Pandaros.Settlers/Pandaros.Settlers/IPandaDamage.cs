using System.Collections.Generic;

namespace Pandaros.Settlers
{
    public interface IPandaDamage
    {
        Dictionary<DamageType, float> Damage { get; }
    }
}