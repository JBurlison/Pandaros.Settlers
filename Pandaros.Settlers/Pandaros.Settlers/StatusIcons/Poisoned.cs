using Pandaros.API.Models;

namespace Pandaros.Settlers.StatusIcons
{
    public class Poisoned : CSType
    {
        public override string icon { get; set; } = GameLoader.ICON_PATH + "Poisoned.png";
        public override string name { get; set; } = GameLoader.NAMESPACE + ".Poisoned";
    }
}
