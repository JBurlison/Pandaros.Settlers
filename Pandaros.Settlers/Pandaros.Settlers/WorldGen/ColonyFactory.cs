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
    [ModLoader.ModManager]
    public class ColonyFactory
    {
        public static List<GeneratedStructure> _structures = new List<GeneratedStructure>();
        public static Dictionary<Vector3Int, GeneratedStructure> _placedStructures = new Dictionary<Vector3Int, GeneratedStructure>();
        static string SaveLocation = GameLoader.SAVE_LOC + "WorldGen/Buildings/";

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
                var schematic = SchematicReader.LoadSchematic(new fNbt.NbtFile(file), Pipliz.Vector3Int.minimum);
                var structure = MapStructure(schematic, new fNbt.NbtFile(file));
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
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterNetworkSetup, GameLoader.NAMESPACE + ".WorldGen.ColonyFactory.AfterNetworkSetup")]
        public static void AfterNetworkSetup()
        {
            bool placed = Configuration.GetorDefault("NpcColoniesPlaced", false);

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

                                var itt = new ColonyIterator(next.SchematicSize);
                                bool canMoveNext = true;

                                while (canMoveNext)
                                {
                                    var block = next.GetBlock(itt.CurrentPosition.x, itt.CurrentPosition.y, itt.CurrentPosition.z);
                                    var currentPos = spawnLocation.Add(itt.CurrentPosition.x, itt.CurrentPosition.y, itt.CurrentPosition.z);
                                    World.TryChangeBlock(currentPos, block.Type);
                                    canMoveNext = itt.MoveNext();
                                }

                                PandaLogger.Log("Spawned {0} at {1}", next.Name, spawnLocation.ToString());
                            }
                        }
                        catch (Exception ex)
                        {
                            PandaLogger.LogError(ex);
                        }
                    }

                    Configuration.SetValue("NpcColoniesPlaced", true);
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
                    PandaLogger.Log("Proposed Spot: {0}", randSpot);

                    // look for ground
                    while (World.TryGetTypeAt(randSpot, out ushort item) && item != BuiltinBlocks.Air &&
                            World.TryGetTypeAt(randSpot.Add(0, -1, 0), out ushort underItem) && underItem == BuiltinBlocks.Air)
                    {
                        if (item != BuiltinBlocks.Air)
                            randSpot = randSpot.Add(0, 1, 0);
                        else
                            randSpot = randSpot.Add(0, -1, 0);
                    }

                    bool isClear = true;

                    for (int x = 1; x < 5; x++)
                        for (int z = 1; z < 5; z++)
                            if (World.TryGetTypeAt(randSpot.Add(x, 0, z), out ushort item) && item != BuiltinBlocks.Air)
                            {
                                isClear = false;
                                PandaLogger.Log("Random Spot not clear");
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