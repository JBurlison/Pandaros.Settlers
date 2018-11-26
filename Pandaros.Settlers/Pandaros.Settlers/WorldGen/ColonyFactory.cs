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
        public static void LoadColonies()
        {
            var terrainGen = ServerManager.TerrainGenerator as TerrainGenerator;
            var treeGenerator = terrainGen.StructureGenerator as TerrainGenerator.DefaultTreeStructureGenerator;
            foreach (var file in Directory.EnumerateFiles(GameLoader.MOD_FOLDER + "/WorldGen/Buildings/", "*.schematic"))
            {
                var structure = MapStructure(SchematicReader.LoadSchematic(new fNbt.NbtFile(file), Pipliz.Vector3Int.minimum));
                treeGenerator.RegisterStructure(structure);
            }

            TerrainGenerator.BiomeConfigLocation area = new TerrainGenerator.BiomeConfigLocation()
            {
                MaxPrecipitationFraction = float.MaxValue,
                MinPrecipitationFraction = float.MinValue,
                MaxTemperature = float.MaxValue,
                MinTemperature = float.MinValue
            };

            TerrainGenerator.DefaultTreeStructureGenerator.StructuresData data = new TerrainGenerator.DefaultTreeStructureGenerator.StructuresData()
            {

            };

            TerrainGenerator.MetaBiomeLocationLimits requiresMetaBiome = new TerrainGenerator.MetaBiomeLocationLimits()
            {

            };

            treeGenerator.RegisterStructureSpawnChances(area, data, requiresMetaBiome);
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
