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
    [ModLoader.ModManager]
    public class ColonyFactory
    {
        public static ColonyStructureGenerator Generator { get; set; }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterItemTypesDefined, GameLoader.NAMESPACE + ".WorldGen.ColonyFactory.LoadColonyGenerator")]
        [ModLoader.ModCallbackDependsOn("create_servermanager_trackers")]
        public static void LoadColonyGenerator()
        {
            var terrainGen = ServerManager.TerrainGenerator as TerrainGenerator;
            var treeGenerator = terrainGen.StructureGenerator as TerrainGenerator.DefaultTreeStructureGenerator;

            Generator = new ColonyStructureGenerator(treeGenerator.MetaBiomeProvider, treeGenerator.MaximumSteepness, treeGenerator);
            Generator.InnerGenerator = treeGenerator;
            terrainGen.StructureGenerator = Generator;

            foreach (var file in Directory.EnumerateFiles(GameLoader.MOD_FOLDER + "/WorldGen/Buildings/"))
            {
                if (file.Contains(".GeneratorSettings.json"))
                {

                }
                else
                {
                    //Generator.AddGeneratedStructure(new GeneratedStructure(new fNbt.NbtFile(file)));
                    var schematic = SchematicReader.LoadSchematic(new fNbt.NbtFile(file), Pipliz.Vector3Int.minimum);
                    var structure = MapStructure(schematic, new fNbt.NbtFile(file));
                    Generator.AddGeneratedStructure(structure);
                }
            }
        }

        public static GeneratedStructure MapStructure(Schematic schematic, fNbt.NbtFile nbt)
        {
            var structure = new GeneratedStructure(nbt);
            structure.Name = schematic.Name;

            for (int Y = 0; Y < schematic.YMax; Y++)
            {
                for (int Z = 0; Z < schematic.ZMax; Z++)
                {
                    for (int X = 0; X < schematic.XMax; X++)
                    {
                        SchematicBlock block = schematic.GetBlock(X, Y, Z);
                        structure.Blocks[X, Y, Z] = new StructureBlock(X, Y, Z, block.MappedBlock.CSIndex);
                    }
                }
            }

            return structure;
        }
    }
}