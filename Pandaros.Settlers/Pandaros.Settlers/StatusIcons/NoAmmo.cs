using Pandaros.Settlers.Items;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pandaros.Settlers.Models;

namespace Pandaros.Settlers.StatusIcons
{
    public class NoAmmo : CSType
    {
        public override string icon { get; set; } = GameLoader.ICON_PATH + "NoAmmo.png";
        public override string name { get; set; } = GameLoader.NAMESPACE + ".NoAmmo";
    }
}
