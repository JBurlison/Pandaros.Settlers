namespace Pandaros.Settlers
{
    public interface IMagicEffect
    {
        IPandaArmor SpecialArmor { get; set; }

        IPandaDamage SpecialDamage { get; set; }

        float HPBoost { get; set; }

        float HPTickRegen { get; set; }

        float AuraRange { get; set; }

        float CraftingSpeed { get; set; }

        float MovementSpeed { get; set; }
    }
}