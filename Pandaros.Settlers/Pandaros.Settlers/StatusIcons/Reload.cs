using Pandaros.Settlers.Items;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandaros.Settlers.StatusIcons
{
    public class Reload : CSType
    {
        public override string icon { get; set; } = GameLoader.ICON_PATH + "Reload.png";
        public override string name { get; set; } = GameLoader.NAMESPACE + ".Reload";
    }
}
