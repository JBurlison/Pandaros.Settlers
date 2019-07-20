using Pandaros.API.Models;

namespace Pandaros.Settlers.StatusIcons
{
    public class Refuel : CSType
    {
        public override string icon { get; set; } = GameLoader.ICON_PATH + "Refuel.png";
        public override string name { get; set; } = GameLoader.NAMESPACE + ".Refuel";
    }
}
