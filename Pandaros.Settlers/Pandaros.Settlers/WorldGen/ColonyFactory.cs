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
                    Generator.AddGeneratedStructure(new GeneratedStructure(new fNbt.NbtFile(file)));
                }
            }
        }
    }
}
