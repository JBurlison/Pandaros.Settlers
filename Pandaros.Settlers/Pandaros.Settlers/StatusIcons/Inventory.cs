using Pandaros.API.Models;

namespace Pandaros.Settlers.StatusIcons
{
    public class Inventory : CSType
    {
        public override string icon { get; set; } = GameLoader.ICON_PATH + "inventory.png";
        public override string name { get; set; } = GameLoader.NAMESPACE + ".Inventory";
    }
}
