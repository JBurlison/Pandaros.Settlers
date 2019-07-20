using Pandaros.API.Models;

namespace Pandaros.Settlers.StatusIcons
{
    public class Waiting : CSType
    {
        public override string icon { get; set; } = GameLoader.ICON_PATH + "Waiting.png";
        public override string name { get; set; } = GameLoader.NAMESPACE + ".Waiting";
    }
}
