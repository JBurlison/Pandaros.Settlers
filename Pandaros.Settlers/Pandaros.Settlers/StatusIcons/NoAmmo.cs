using Pandaros.API.Models;

namespace Pandaros.Settlers.StatusIcons
{
    public class NoAmmo : CSType
    {
        public override string icon { get; set; } = GameLoader.ICON_PATH + "NoAmmo.png";
        public override string name { get; set; } = GameLoader.NAMESPACE + ".NoAmmo";
    }
}
