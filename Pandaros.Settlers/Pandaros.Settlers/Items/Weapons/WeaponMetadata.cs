namespace Pandaros.Settlers.Items.Weapons
{
    public class WeaponMetadata : IWeapon
    {
        public WeaponMetadata(float damage, int durability, string name, ItemTypesServer.ItemTypeRaw item)
        {
            Damage     = damage;
            Name       = name;
            ItemType   = item;
            Durability = durability;
        }

        public string Name { get; }

        public float Damage { get; }

        public ItemTypesServer.ItemTypeRaw ItemType { get; }

        public int Durability { get; set; }

        public IMagicEffect MagicEffect => null;
    }
}