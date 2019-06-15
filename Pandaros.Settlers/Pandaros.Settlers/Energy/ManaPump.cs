using Pandaros.Settlers.Items;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandaros.Settlers.Energy
{
    public class ManaPumpTextureMapping : CSTextureMapping
    {
        public override string name => GameLoader.NAMESPACE + ".Pump";
        public override string albedo => Path.Combine(GameLoader.BLOCKS_ALBEDO_PATH, "Manapump_Albedo.png");
        public override string emissive => Path.Combine(GameLoader.BLOCKS_EMISSIVE_PATH, "Manapump_Emissive.png");
        public override string normal => Path.Combine(GameLoader.BLOCKS_NORMAL_PATH, "Manapump_Normal.png");
    }

    public class ManaPumpGenerate : CSGenerateType
    {
        public override string generateType { get; set; } = "rotateBlock";
        public override string typeName { get; set; } = GameLoader.NAMESPACE + ".Pump";
        public override ICSType baseType { get; set; } = new CSType()
        {
            categories = new List<string>()
            {
                "Mana",
                "Energy",
                "Machine"
            },
            mesh = Path.Combine(GameLoader.MESH_PATH, "Manapump.obj"),
            icon = Path.Combine(GameLoader.ICON_PATH, "Pump.png"),
            onPlaceAudio = "Pandaros.Settlers.Metal",
            onRemoveAudio = "Pandaros.Settlers.MetalRemove",
            maxStackSize  = 300,
            destructionTime = 5000,
            sideall = GameLoader.NAMESPACE + ".Pump",
            meshRotationEuler = new MeshRotationEuler()
            {
                y = 90
            }
        };
    }
}
