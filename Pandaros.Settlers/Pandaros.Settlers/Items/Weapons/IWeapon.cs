namespace Pandaros.Settlers.Items.Weapons
{
    public interface IWeapon
    {
        IMagicEffect MagicEffect { get; }
        float Damage { get; }
        int Durability { get; set; }
        ItemTypesServer.ItemTypeRaw ItemType { get; }
        string Name { get; }
    }
}