using Pandaros.Settlers.Items.Temperature;

namespace Pandaros.Settlers
{
    public interface IMagicEffect
    {
        IPandaArmor SpecialArmor { get; }

        IPandaDamage SpecialDamage { get; }

        ITemperatureRegulator TemperatureRegulation { get; }

        float HPBoost { get; }

        float HPTickRegen { get; }

        float AuraRange { get; }

        float CraftingSpeed { get; }

        float MovementSpeed { get; }

        void Update();
    }
}