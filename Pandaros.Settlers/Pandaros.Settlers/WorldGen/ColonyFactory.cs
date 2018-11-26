using Pandaros.Settlers.NBT;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerrainGeneration;

namespace Pandaros.Settlers.WorldGen

{
    /// defaultstructuregenerator.registerstructurespawnchances
    public class ColonyFactory
    {
        public static ColonyStructureGenerator Generator { get; set; }

        public static void LoadColonies()
        {
            var terrainGen = ServerManager.TerrainGenerator as TerrainGenerator;
            var treeGenerator = terrainGen.StructureGenerator as TerrainGenerator.DefaultTreeStructureGenerator;

            Generator = new ColonyStructureGenerator(treeGenerator.MetaBiomeProvider, treeGenerator.MaximumSteepness);
            Generator.InnerGenerator = treeGenerator;
        }

        public static GeneratedStructure MapStructure(Schematic schematic)
        {
            var structure = new GeneratedStructure();
            List<StructureBlock> blocks = new List<StructureBlock>();
            structure.Name = schematic.Name;

            for (int Y = 0; Y < schematic.YMax; Y++)
            {
                for (int Z = 0; Z < schematic.ZMax; Z++)
                {
                    for (int X = 0; X < schematic.XMax; X++)
                    {
                        SchematicBlock block = schematic.GetBlock(Y, Z, X);

                        blocks.Add(new StructureBlock(X, Y, Z, block.MappedBlock.CSIndex));
                    }
                }
            }

            structure.Blocks = blocks.ToArray();

            return structure;
        }
    }
}
