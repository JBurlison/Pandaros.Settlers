using Pandaros.Settlers.Items;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Pandaros.Settlers.Items.StaticItems;

namespace Pandaros.Settlers.ColonyManager
{
    public class ColonyManagementTool : CSType
    {
        public static string NAME = GameLoader.NAMESPACE + ".ColonyManagementTool";
        public override string Name => NAME;
        public override string icon => GameLoader.ICON_PATH + "ColonyManager.png";
        public override bool? isPlaceable => false;
        public override StaticItem StaticItemSettings => new StaticItem() { Name = GameLoader.NAMESPACE + ".ColonyManagementTool" };
    }

    public class ColonyTool
    {
    }
}
