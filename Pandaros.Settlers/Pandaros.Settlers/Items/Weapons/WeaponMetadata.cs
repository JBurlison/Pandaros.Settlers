namespace Pandaros.Settlers.Items
{
    public class WeaponMetadata
    {
        public WeaponMetadata(float                       damage, int durability, MetalType metalType,
                              WeaponType                  weaponType,
                              ItemTypesServer.ItemTypeRaw item)
        {
            Damage     = damage;
            Metal      = metalType;
            WeaponType = weaponType;
            ItemType   = item;
            Durability = durability;
        }

        public MetalType Metal { get; }

        public WeaponType WeaponType { get; }

        public float Damage { get; }

        public ItemTypesServer.ItemTypeRaw ItemType { get; }

        public int Durability { get; set; }
    }
}