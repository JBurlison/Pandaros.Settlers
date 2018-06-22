namespace Pandaros.Settlers.Items.Weapons
{
    public interface IWeapon : IMagicEffect
    {
        int Durability { get; set; }
        ItemTypesServer.ItemTypeRaw ItemType { get; }
    }
}