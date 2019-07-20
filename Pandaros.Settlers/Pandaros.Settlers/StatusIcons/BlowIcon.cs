using Pandaros.API.Models;

namespace Pandaros.Settlers.StatusIcons
{
    public class BlowIcon : CSType
    {
        public override string icon { get; set; } = GameLoader.ICON_PATH + "bow.png";
        public override string name { get; set; } = GameLoader.NAMESPACE + ".BowIcon";
    }
}
