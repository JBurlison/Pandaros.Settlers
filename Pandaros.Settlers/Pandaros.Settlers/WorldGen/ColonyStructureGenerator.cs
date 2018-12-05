using Pipliz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerrainGeneration;
using static TerrainGeneration.TerrainGenerator;

namespace Pandaros.Settlers.WorldGen
{
    public class ColonyStructureGenerator : IStructureGenerator
    {
        Dictionary<Vector3Int, GeneratedStructure> _placedStructures = new Dictionary<Vector3Int, GeneratedStructure>();
        List<GeneratedStructure> _structures = new List<GeneratedStructure>();
        GeneratedStructure _next;

        public IStructureGenerator InnerGenerator { get; set; }

        public ColonyStructureGenerator(IMetaBiomeProvider metaBiomeProvider, float maxSteepForStructureGlobal, DefaultTreeStructureGenerator tree) 
        {
            InnerGenerator = tree;
        }

        public void TryAddStructure(ref StructureGeneratorData data)
        {
            int x = data.WorldX & 15;
            int z = data.WorldZ & 15;

            foreach(var structure in _placedStructures)
            {
                // using vector 2 because I only need 2 points. Y acts as Z
                var xp = structure.Value.LastPlaced.x + 1;
                var zp = structure.Value.LastPlaced.y + 1;

                if ((x == xp && z == zp) ||
                    (x == xp && z == structure.Value.LastPlaced.y) ||
                    (x == structure.Value.LastPlaced.x && z == zp))
                {
                    for (int i = 0; i < structure.Value.SchematicSize.YMax; i++)
                    {

                    }

                    return;
                }
            }

            if (data.Steepness <= .1)
            {
                if (_next == null && _structures.Count > 0)
                {
                    _next = _structures.GetRandomItem();
                    _next.NumberOfPlacements--;

                    if (_next.NumberOfPlacements <= 0)
                        _structures.Remove(_next);
                }

                if (_next == null || _next.SpawnChance < data.Random.NextFloat())
                {
                    InnerGenerator.TryAddStructure(ref data);
                    return;
                }

                var currentPos = new Vector3Int(x, data.WorldY, z);
                bool canBuild = true;
                var distance = 150f;

                // check of we are too close to another structure
                foreach (var kvp in _placedStructures)
                {
                    var newDist = UnityEngine.Vector3.Distance(kvp.Key.Vector, currentPos.Vector);

                    if (newDist < distance)
                        distance = newDist;
                }

                if (_next.DistanceBetweenOtherStructuresMin > 0)
                    canBuild = _next.DistanceBetweenOtherStructuresMin < distance;

                if (canBuild && _next.DistanceBetweenOtherStructuresMax > 0)
                    canBuild = _next.DistanceBetweenOtherStructuresMax > distance;

                if (canBuild)
                {
                    for (int i = 0; i < _next.Blocks.Length; ++i)
                    {
                        StructureBlock structureBlock = _next.Blocks[i];
                        structureBlock.Position.x += (sbyte)x;
                        structureBlock.Position.y += (short)data.WorldY;
                        structureBlock.Position.z += (sbyte)z;
                        data.Blocks.Add(structureBlock);
                    }

                    _next = null;
                    PandaLogger.Log(ChatColor.lime, "Colony Placed at [{0}, {1}, {2}]", x, data.WorldY, z);
                }
                else
                    InnerGenerator.TryAddStructure(ref data);
            }
            else
                InnerGenerator.TryAddStructure(ref data);
        }

        public void AddGeneratedStructure(GeneratedStructure structure)
        {
            _structures.Add(structure);
        }
    }
}
