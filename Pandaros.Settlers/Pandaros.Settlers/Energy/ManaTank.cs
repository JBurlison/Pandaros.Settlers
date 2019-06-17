using Pandaros.Settlers.Items;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandaros.Settlers.Energy
{
    public class ManaTankTextureMapping : CSTextureMapping
    {
        public override string name => GameLoader.NAMESPACE + ".Tank";
        public override string albedo => Path.Combine(GameLoader.BLOCKS_ALBEDO_PATH, "Tank_Albedo.png");
        public override string emissive => Path.Combine(GameLoader.BLOCKS_EMISSIVE_PATH, "Tank_Emissive.png");
        public override string normal => Path.Combine(GameLoader.BLOCKS_NORMAL_PATH, "Tank_Normal.png");
    }


    public class ManaTankBase : CSType
    {
        public override List<string> categories { get; set; } = new List<string>()
            {
                "Mana",
                "Energy",
                "Machine"
            };
        public override string icon { get; set; } = Path.Combine(GameLoader.ICON_PATH, "Tank.png");
        public override string onPlaceAudio { get; set; } = "Pandaros.Settlers.Metal";
        public override string onRemoveAudio { get; set; } = "Pandaros.Settlers.MetalRemove";
        public override int? maxStackSize { get; set; } = 300;
        public override int? destructionTime { get; set; } = 500;
        public override string sideall { get; set; } = GameLoader.NAMESPACE + ".Tank";
        public override List<OnRemove> onRemove { get; set; } = new List<OnRemove>()
        {
            new OnRemove(1, 1, GameLoader.NAMESPACE + ".TankEmpty")
        };
        public override ConnectedBlock ConnectedBlock { get; set; } = new ConnectedBlock()
        {
            BlockType = "ManaPipe",
            Connections = new List<Models.BlockSides>()
            {
                Models.BlockSides.Xn,
                Models.BlockSides.Xp,
                Models.BlockSides.Yn,
                Models.BlockSides.Yp,
                Models.BlockSides.Zn,
                Models.BlockSides.Zp
            }
        };
    }


    public class ManaTankGenerate : ManaTankBase
    {
        public override string name { get; set; } = GameLoader.NAMESPACE + ".TankFull";
        public override string mesh { get; set; } = Path.Combine(GameLoader.MESH_PATH, "Tank_Full.obj");
    }

    public class ManaTankEmptyGenerate : ManaTankBase
    {
        public override string name { get; set; } = GameLoader.NAMESPACE + ".TankEmpty";
        public override string mesh { get; set; } = Path.Combine(GameLoader.MESH_PATH, "Tank_empty.obj");
    }

    public class ManaTankQuarterGenerate : ManaTankBase
    {
        public override string name { get; set; } = GameLoader.NAMESPACE + ".TankQuarter";
        public override string mesh { get; set; } = Path.Combine(GameLoader.MESH_PATH, "Tank_quarter.obj");
    }

    public class ManaTankThreeQuarterGenerate : ManaTankBase
    {
        public override string name { get; set; } = GameLoader.NAMESPACE + ".TankThreeQuarter";
        public override string mesh { get; set; } = Path.Combine(GameLoader.MESH_PATH, "Tank_3quarter.obj");
    }

    public class ManaTankHalfGenerate : ManaTankBase
    {
        public override string name { get; set; } = GameLoader.NAMESPACE + ".TankHalf";
        public override string mesh { get; set; } = Path.Combine(GameLoader.MESH_PATH, "Tank_half.obj");
    }
}
