using Pandaros.API.Models;

namespace Pandaros.Settlers.StatusIcons
{
    public class Repairing : CSType
    {
        public override string icon { get; set; } = GameLoader.ICON_PATH + "Repairing.png";
        public override string name { get; set; } = GameLoader.NAMESPACE + ".Repairing";
    }
}
