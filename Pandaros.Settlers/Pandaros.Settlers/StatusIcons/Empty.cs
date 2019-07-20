using Pandaros.API.Models;

namespace Pandaros.Settlers.StatusIcons
{
    public class Empty : CSType
    {
        public override string icon { get; set; } = GameLoader.ICON_PATH + "Empty.png";
        public override string name { get; set; } = GameLoader.NAMESPACE + ".Empty";
    }
}
