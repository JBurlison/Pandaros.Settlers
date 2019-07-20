using Pandaros.API.Models;

namespace Pandaros.Settlers.StatusIcons
{
    public class NoMonster : CSType
    {
        public override string icon { get; set; } = GameLoader.ICON_PATH + "NoMonster.png";
        public override string name { get; set; } = GameLoader.NAMESPACE + ".Monster";
    }
}
