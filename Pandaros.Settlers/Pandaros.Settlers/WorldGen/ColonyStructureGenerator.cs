using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerrainGeneration;
using static TerrainGeneration.TerrainGenerator;

namespace Pandaros.Settlers.WorldGen
{
    public class ColonyStructureGenerator : DefaultTreeStructureGenerator
    {
        public ColonyStructureGenerator(IMetaBiomeProvider metaBiomeProvider, float maxSteepForStructureGlobal) : 
            base(metaBiomeProvider, maxSteepForStructureGlobal)
        {
            
        }

        new public void TryAddStructure(ref StructureGeneratorData data)
        {
           
        }

        public static void AddGeneratedStructure(GeneratedStructure structure)
        {

        }
    }
}
