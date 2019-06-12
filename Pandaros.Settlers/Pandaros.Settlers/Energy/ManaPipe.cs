using Pandaros.Settlers.Items;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandaros.Settlers.Energy
{
    //1058c7
    public class ManaPipe : CSType
    {
        public override string name { get; set; } = GameLoader.NAMESPACE + ".ManaPipe";

        public override string icon { get; set; } = Path.Combine(GameLoader.ICON_PATH, "Pipe.png");

        public override int? destructionTime { get; set; } = 10000;

        public override string mesh { get; set; } = Path.Combine(GameLoader.MESH_PATH, "Pipe.obj");

        public override string sideall { get; set; } = GameLoader.NAMESPACE + ".Pipe";

        public override string onPlaceAudio { get; set; } = "Pandaros.Settlers.Metal";

        public override string onRemoveAudio { get; set; } = "Pandaros.Settlers.MetalRemove";

        public override List<string> categories { get; set; } = new List<string>()
        {
            "Mana",
            "Machine"
        };

        public override int? maxStackSize { get; set; } = 300;
    }

    public class ManaPipeXp : ManaPipe
    {
        // public override List<string> categories { get; set; }
        public override string name { get; set; } = GameLoader.NAMESPACE + ".ManaPipeXp";

        public override MeshRotationEuler meshRotationEuler { get; set; } = new MeshRotationEuler()
        {
            x = 90
        };
    }

    public class ManaPipeTextureMapping : CSTextureMapping
    {
        public override string name => GameLoader.NAMESPACE + ".Pipe";
        public override string albedo => Path.Combine(GameLoader.BLOCKS_ALBEDO_PATH, "Pipe_Albedo.png");
        public override string emissive => Path.Combine(GameLoader.BLOCKS_EMISSIVE_PATH, "Pipe_Emissive.png");
        public override string normal => Path.Combine(GameLoader.BLOCKS_NORMAL_PATH, "Pipe_Normal.png");
        public override string height => Path.Combine(GameLoader.BLOCKS_HEIGHT_PATH, "Pipe_height.png");
    }
}
