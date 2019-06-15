using BlockTypes;
using Pandaros.Settlers.NBT;
using Pipliz;
using Pipliz.JSON;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TerrainGeneration;

namespace Pandaros.Settlers.WorldGen
{
    //[ModLoader.ModManager]
    public class ColonyFactory
    {
        public static List<GeneratedStructure> _structures = new List<GeneratedStructure>();
        public static Dictionary<Vector3Int, GeneratedStructure> _placedStructures = new Dictionary<Vector3Int, GeneratedStructure>();
        static string SaveLocation = GameLoader.SAVE_LOC + "WorldGen/Buildings/";
        static BoundsInt _working = new BoundsInt(Vector3Int.minimum, Vector3Int.minimum);

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterItemTypesDefined, GameLoader.NAMESPACE + ".WorldGen.ColonyFactory.LoadColonyGenerator")]
        [ModLoader.ModCallbackDependsOn("create_servermanager_trackers")]
        public static void LoadColonyGenerator()
        {
            List<string> settings = new List<string>();

            foreach (var file in Directory.EnumerateFiles(GameLoader.MOD_FOLDER + "/WorldGen/Buildings/"))
                ProcessSchemaFiles(settings, file);

            if (!Directory.Exists(SaveLocation))
                Directory.CreateDirectory(SaveLocation);

            foreach (var file in Directory.EnumerateFiles(SaveLocation))
                ProcessSchemaFiles(settings, file);

            foreach (var sch in _structures)
            {
                foreach (var sf in settings)
                    if (sf.Contains(sch.Name))
                    {
                        var json = JSON.Deserialize(sf);

                        if (json.TryGetAs("DistanceBetweenOtherStructuresMax", out int distMax))
                            sch.DistanceBetweenOtherStructuresMax = distMax;

                        if (json.TryGetAs("DistanceBetweenOtherStructuresMin", out int distMin))
                            sch.DistanceBetweenOtherStructuresMin = distMin;

                        if (json.TryGetAs("NumberOfPlacements", out int numPlace))
                            sch.NumberOfPlacements = numPlace;

                        break;
                    }

                SaveGeneratedStructureSettings(sch);
            }
        }

        private static void ProcessSchemaFiles(List<string> settings, string file)
        {
            if (file.Contains(".GeneratorSettings.json"))
            {
                settings.Add(file);
            }
            else
            {
                var structure = MapStructure(new fNbt.NbtFile(file));
                _structures.Add(structure);
            }
        }

        public static void SaveGeneratedStructureSettings(GeneratedStructure sch)
        {
            JSONNode schematicSettings = new JSONNode();
            schematicSettings.SetAs("DistanceBetweenOtherStructuresMax", sch.DistanceBetweenOtherStructuresMax);
            schematicSettings.SetAs("DistanceBetweenOtherStructuresMin", sch.DistanceBetweenOtherStructuresMin);
            schematicSettings.SetAs("NumberOfPlacements", sch.NumberOfPlacements);

            JSON.Serialize(SaveLocation + sch.Name + ".GeneratorSettings.json", schematicSettings);
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnShouldKeepChunkLoaded, GameLoader.NAMESPACE + ".WorldGen.ColonyFactory.OnShouldKeepChunkLoaded")]
        public static void OnShouldKeepChunkLoaded(ChunkUpdating.KeepChunkLoadedData data)
        {
            foreach (var structure in _placedStructures)
            {
                if (BoundsInt.Intersects(structure.Value.Bounds, data.CheckedChunk.Bounds))
                    data.Result = true;
            }

            if (BoundsInt.Intersects(_working, data.CheckedChunk.Bounds))
                data.Result = true;
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterNetworkSetup, GameLoader.NAMESPACE + ".WorldGen.ColonyFactory.AfterNetworkSetup")]
        public static void AfterNetworkSetup()
        {
            bool placed = SettlersConfiguration.GetorDefault("NpcColoniesPlaced", false);

            if (!placed)
            {
                Thread t = new Thread(() =>
                {
                    var terrainGen = ServerManager.TerrainGenerator as TerrainGenerator;

                    while (_structures.Count != 0)
                    {
                        try
                        {
                            GeneratedStructure next = null;

                            if (_structures.Count > 0)
                            {
                                next = _structures.GetRandomItem();
                                PandaLogger.Log("Next Colony: {0}", next.Name);
                                next.NumberOfPlacements--;

                                if (next.NumberOfPlacements <= 0)
                                    _structures.Remove(next);

                                SaveGeneratedStructureSettings(next);
                            }

                            if (next != null)
                            {
                                var startLocation = GetRandomStartLocation(terrainGen);
                                var spawnLocation = GetRandomPoint(startLocation, next.DistanceBetweenOtherStructuresMin, next.DistanceBetweenOtherStructuresMax);
                                next.Bounds = new BoundsInt(spawnLocation, spawnLocation.Add(next.SchematicSize.XMax, next.SchematicSize.YMax, next.SchematicSize.ZMax));
                                _placedStructures.Add(spawnLocation, next);
                                ChunkQueue.QueueBannerBox(next.Bounds.min, next.Bounds.max);

                                Thread.Sleep(5000);

                                for (int x = 0; x < next.SchematicSize.XMax; x++)
                                    for (int y = 0; y < next.SchematicSize.YMax; y++)
                                        for (int z = 0; z < next.SchematicSize.ZMax; z++)
                                        {
                                            var block = next.GetBlock(x, y, z);
                                            var currentPos = spawnLocation.Add(x, y, z);
                                            ServerManager.TryChangeBlock(currentPos, block.Type);
                                        }

                                PandaLogger.Log("Spawned {0} at {1}", next.Name, spawnLocation.ToString());
                            }
                        }
                        catch (Exception ex)
                        {
                            PandaLogger.LogError(ex);
                        }
                    }

                    SettlersConfiguration.SetValue("NpcColoniesPlaced", true);
                });

                t.IsBackground = true;
                t.Start();
            }
        }

        private static Vector3Int GetRandomStartLocation(TerrainGenerator terrainGen)
        {
            PandaLogger.Log("Getting Random Start Location");
            Vector3Int startLocation = terrainGen.GetDefaultSpawnLocation();

            if (_placedStructures.Count != 0)
                startLocation = _placedStructures.ElementAt(Pipliz.Random.Next(_placedStructures.Count)).Key;

            PandaLogger.Log("Random Start Location: {0}", startLocation);
            return startLocation;
        }

        public static Vector3Int GetRandomPoint(Vector3Int start, int min, int max)
        {
            var randSpot = start;
            bool found = false;
            PandaLogger.Log("Getting Random Spot");

            while (!found)
            {
                try
                {
                    var randX = Pipliz.Random.Next(min, max);
                    var randZ = Pipliz.Random.Next(min, max);

                    if (Pipliz.Random.NextBool())
                        randX = randX * -1;

                    if (Pipliz.Random.NextBool())
                        randZ = randZ * -1;

                    randSpot = start.Add(randX, 0, randZ);
                    _working = new BoundsInt(randSpot.Add(0, randSpot.x * -1, 0), randSpot.Add(0, 100, 0));
                    ChunkQueue.QueueBannerBox(_working.min, _working.max);
                    Thread.Sleep(5000);


                    // look for ground
                    while (!World.TryGetTypeAt(randSpot, out ItemTypes.ItemType item) || item.ItemIndex != ColonyBuiltIn.ItemTypes.AIR.Id ||
                            !World.TryGetTypeAt(randSpot.Add(0, -1, 0), out ushort underItem) || (underItem == ColonyBuiltIn.ItemTypes.AIR.Id || underItem == ColonyBuiltIn.ItemTypes.WATER.Id))
                    {
                        if (item != null)
                        {
                            if (item.ItemIndex != ColonyBuiltIn.ItemTypes.AIR.Id)
                                randSpot = randSpot.Add(0, 1, 0);
                            else
                                randSpot = randSpot.Add(0, -1, 0);
                        }
                        else
                            PandaLogger.Log("cant get item");
                    }

                    PandaLogger.Log("Proposed Spot: {0}", randSpot);

                    bool isClear = true;
                    int size = 2;
                    _working = new BoundsInt(randSpot.Add(0, -1, 0), randSpot.Add(size, 0, size));
                    ChunkQueue.QueueBannerBox(_working.min, _working.max);
                    Thread.Sleep(300);


                    for (int x = 0; x < size; x++)
                        for (int z = 0; z < size; z++)
                            if (!World.TryGetTypeAt(randSpot.Add(x, 0, z), out ushort item) || item != ColonyBuiltIn.ItemTypes.AIR.Id)
                            {
                                isClear = false;
                            }

                    if (isClear)
                    {
                        foreach (var placed in _placedStructures)
                        {
                            var dist = (placed.Key - randSpot).Magnitude;

                            if (dist < min|| dist < placed.Value.DistanceBetweenOtherStructuresMin)
                            {
                                isClear = false;
                                PandaLogger.Log("spot too close to {0} or {1} at distance {2}", placed.Key, randSpot, dist);
                                break;
                            }
                        }
                    }

                    if (isClear)
                        found = true;
                }
                catch (Exception ex)
                {
                    PandaLogger.LogError(ex);
                }
            }

            PandaLogger.Log("Random Spot: {0}", randSpot);
            return randSpot;
        }

        public static GeneratedStructure MapStructure(fNbt.NbtFile nbt)
        {
            var structure = new GeneratedStructure(nbt);
            structure.Name = Path.GetFileNameWithoutExtension(nbt.FileName);
            return structure;
        }
    }
}