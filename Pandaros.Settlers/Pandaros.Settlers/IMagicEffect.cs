using Pandaros.Settlers.Extender;
using Pandaros.Settlers.Items;

namespace Pandaros.Settlers
{
    public interface IMagicEffect : IPandaArmor, IPandaDamage, INameable, ILucky
    {
        float Skilled { get; set; }
        float HPBoost { get; }
        float HPTickRegen { get; }
        void Update();
    }
}