using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerrainGeneration;

namespace Pandaros.Settlers.WorldGen
{
    public class GeneratedStructure : TerrainGenerator.DefaultTreeStructureGenerator.IStructure
    {
        public string Name { get; set; }

        public StructureBlock[] Blocks { get; set; }
    }
}
