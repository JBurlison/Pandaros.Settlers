using BlockTypes;
using Pipliz.JSON;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerrainGeneration;

namespace Pandaros.Settlers.WorldGen
{
    [ModLoader.ModManager]
    public static class Ore
    {
        public static JSONNode LoadedOres { get; set; }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterItemTypesDefined, GameLoader.NAMESPACE + ".WorldGen")]
        [ModLoader.ModCallbackDependsOn("create_servermanager_trackers")]
        public static void AddOres()
        {
            foreach (var info in GameLoader.AllModInfos)
                if (info.Value.TryGetAs(GameLoader.NAMESPACE + ".jsonFiles", out JSONNode jsonFilles))
                {
                    foreach (var jsonNode in jsonFilles.LoopArray())
                    {
                        if (jsonNode.TryGetAs("fileType", out string jsonFileType) && jsonFileType == GameLoader.NAMESPACE + ".OreLayers" && jsonNode.TryGetAs("relativePath", out string menuFilePath))
                        {
                            var newMenu = JSON.Deserialize(info.Key + "\\" + menuFilePath);

                            if (LoadedOres == null)
                                LoadedOres = newMenu;
                            else
                            {
                                LoadedOres.Merge(newMenu);
                            }

                            PandaLogger.Log("Loaded Menu: {0}", menuFilePath);
                        }
                    }
                }

            try
            {
                var terrainGen = ServerManager.TerrainGenerator as TerrainGenerator;
                var stoneGen = terrainGen.FinalChunkModifier as TerrainGenerator.InfiniteStoneLayerGenerator;
                var oreGen = stoneGen.InnerGenerator as TerrainGenerator.OreLayersGenerator;

                foreach (var ore in LoadedOres.LoopArray())
                {
                    if (ore.TryGetAs("Chance", out byte chance) && ore.TryGetAs("Type", out string type) && ore.TryGetAs("Depth", out byte depth))
                    {
                        try
                        {
                            var item = ItemTypes.GetType(type);

                            if (item != null && item.ItemIndex != BuiltinBlocks.Air)
                                oreGen.AddLayer(depth, item.ItemIndex, chance);
                            else
                                PandaLogger.Log(ChatColor.yellow, "Unable to find item {0}", type);
                        }
                        catch (Exception ex)
                        {
                            PandaLogger.LogError(ex);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                PandaLogger.LogError(ex);
            }
        }
    }
}
