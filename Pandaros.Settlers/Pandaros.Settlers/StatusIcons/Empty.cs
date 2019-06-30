using Pandaros.Settlers.Items;
using Pandaros.Settlers.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandaros.Settlers.StatusIcons
{
    public class Empty : CSType
    {
        public override string icon { get; set; } = GameLoader.ICON_PATH + "Empty.png";
        public override string name { get; set; } = GameLoader.NAMESPACE + ".Empty";
    }
}
