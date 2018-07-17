using Pandaros.Settlers.Extender;
using Pandaros.Settlers.Items.Temperature;

namespace Pandaros.Settlers
{
    public interface IMagicEffect : IPandaArmor, IPandaDamage, ITemperatureRegulator, INameable
    {
        float HPBoost { get; }
        float HPTickRegen { get; }
        void Update();
    }
}