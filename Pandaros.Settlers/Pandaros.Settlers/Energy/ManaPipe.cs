using Pandaros.Settlers.Items;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// pipe fill color: 1058c7
namespace Pandaros.Settlers.Energy
{
    public class ManaPipeTextureMapping : CSTextureMapping
    {
        public override string name => GameLoader.NAMESPACE + ".Pipe";
        public override string albedo => Path.Combine(GameLoader.BLOCKS_ALBEDO_PATH, "Pipe_Albedo.png");
        public override string emissive => Path.Combine(GameLoader.BLOCKS_EMISSIVE_PATH, "Pipe_Emissive.png");
        public override string normal => Path.Combine(GameLoader.BLOCKS_NORMAL_PATH, "Pipe_Normal.png");
        public override string height => Path.Combine(GameLoader.BLOCKS_HEIGHT_PATH, "Pipe_height.png");
    }

    public class ManaPipeBase : CSType
    {
        public const string MANA_PIPE = GameLoader.NAMESPACE + ".ManaPipe";
        public override string sideall { get; set; } = GameLoader.NAMESPACE + ".Pipe";
        public override string onPlaceAudio { get; set; } = "Pandaros.Settlers.Metal";
        public override string onRemoveAudio { get; set; } = "Pandaros.Settlers.MetalRemove";
        public override int? maxStackSize { get; set; } = 300;
        public override int? destructionTime { get; set; } = 500;
        public override List<OnRemove> onRemove { get; set; } = new List<OnRemove>()
        {
            new OnRemove(1, 1, MANA_PIPE)
        };
        public override List<string> categories { get; set; } = new List<string>()
        {
            "Mana",
            "Energy",
            "Machine"
        };
    }

    public class ManaPipe : ManaPipeBase
    {
        public override string name { get; set; } = MANA_PIPE;
        public override string icon { get; set; } = Path.Combine(GameLoader.ICON_PATH, "Pipe.png");
        public override string mesh { get; set; } = Path.Combine(GameLoader.MESH_PATH, "Pipe.obj");
        public override List<string> categories { get; set; } = new List<string>()
        {
            "Mana",
            "Energy",
            "Machine"
        };
        public override ConnectedBlock ConnectedBlock { get; set; } = new ConnectedBlock()
        {
            BlockType = "ManaPipe",
            Connections = new List<Models.BlockSide>()
            {
                Models.BlockSide.Zn,
                Models.BlockSide.Zp
            },
            CalculationType = "Pipe"
        };
    }

    public class ManaPipeThreeWay : ManaPipeBase
    {
        public override string name { get; set; } = GameLoader.NAMESPACE + ".ManaPipeThreeWay";
        public override string mesh { get; set; } = Path.Combine(GameLoader.MESH_PATH, "Pipe_3way.obj");
        public override ConnectedBlock ConnectedBlock { get; set; } = new ConnectedBlock()
        {
            BlockType = "ManaPipe",
            Connections = new List<Models.BlockSide>()
            {
                Models.BlockSide.Yp,
                Models.BlockSide.Zn,
                Models.BlockSide.Zp
            },
            CalculationType = "Pipe"
        };
    }
}
