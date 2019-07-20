using Pandaros.API.Models;

namespace Pandaros.Settlers.StatusIcons
{
    public class Broken : CSType
    {
        public override string icon { get; set; } = GameLoader.ICON_PATH + "Broken.png";
        public override string name { get; set; } = GameLoader.NAMESPACE + ".Broken";
    }
}
