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

        public int DistanceBetweenOtherStructuresMax { get; set; } = -1;
        public int DistanceBetweenOtherStructuresMin { get; set; } = 200;
        public int NumberOfPlacements { get; set; } = 1;
        public float SpawnChance { get; set; } = .05f;

        public StructureBlock[] Blocks { get; set; }
    }
}
