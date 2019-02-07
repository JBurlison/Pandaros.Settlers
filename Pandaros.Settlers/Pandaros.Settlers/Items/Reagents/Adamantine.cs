using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandaros.Settlers.Items.Reagents
{
    public class Adamantine : CSType
    {
        public static string NAME = "Pandaros.Settlers.AutoLoad.Adamantine";
        public override string name { get; set; } = NAME;
        public override int? maxStackSize => 600;
        public override bool? isPlaceable => false;
        public override string icon { get; set; } = GameLoader.ICON_PATH + "Adamantine.png";
        public override List<string> categories { get; set; } = new List<string>()
        {
            "ingredient",
            "Adamantine"
        };
    }
}
