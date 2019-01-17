using static Pandaros.Settlers.Items.Armor.ArmorFactory;

namespace Pandaros.Settlers.Items.Armor
{
    public interface IArmor : IMagicEffect
    {
        ArmorSlot Slot { get; }
        float ArmorRating { get; }
        int Durability { get; set; }
    }
}
