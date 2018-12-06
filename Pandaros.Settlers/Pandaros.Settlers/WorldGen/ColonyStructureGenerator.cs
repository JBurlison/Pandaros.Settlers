using BlockTypes;
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

            foreach (var structure in _placedStructures)
            {
                // using vector 2 because I only need 2 points. Y acts as Z
                var xp = structure.Value.LastPlaced.x + 1;
                var zp = structure.Value.LastPlaced.y + 1;

                if ((x == xp && z == zp) ||
                    (x == xp && z == structure.Value.LastPlaced.y) ||
                    (x == structure.Value.LastPlaced.x && z == zp))
                {
                    var calcX = x - structure.Key.x;
                    var caclZ = z - structure.Key.z;
                    
                    for (int i = 0; i < structure.Value.SchematicSize.YMax; i++)
                    {
                        var struc = structure.Value.GetBlock(calcX, i, caclZ);
                        PandaLogger.Log("placing block {0} at [{1}, {2}, {3}]", struc.Type, x, structure.Value.Ymin + i, z);
                        struc.Position.x = (sbyte)x;
                        struc.Position.y = (sbyte)(structure.Value.Ymin + i);
                        struc.Position.z = (sbyte)z;
                        data.Blocks.Add(struc);
                    }

                    structure.Value.LastPlaced = new Vector2Int(x, z);
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

                var yblock = data.Blocks.LastOrDefault(b => b.Type != default(ushort));
                var currentPos = new Vector3Int(x, yblock.Position.y, z);
                bool canBuild = true;
                var distance = UnityEngine.Vector3.Distance(new UnityEngine.Vector3(0, 0, 0), currentPos.Vector);

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
                    //if (yblock.Type != default(ushort))
                    //{
                    _next.Ymin = currentPos.y;
                    //_next.Ymin = currentPos.y;
                    //currentPos.y = _next.Ymin;
                    _placedStructures.Add(currentPos, _next);

                    for (int i = 0; i < _next.SchematicSize.YMax; i++)
                    {
                        var struc = _next.GetBlock(0, i, 0);
                        PandaLogger.Log("{0} placing initial block {1}", struc.Type, _next.Name);
                        struc.Position.x = (sbyte)x;
                        struc.Position.y = (sbyte)(_next.Ymin + i);
                        struc.Position.z = (sbyte)z;
                        data.Blocks.Add(struc);
                    }

                    _next.LastPlaced = new Vector2Int(x, z);
                    _next = null;
                    PandaLogger.Log(ChatColor.lime, "Colony Placed at [{0}, {1}, {2}]", currentPos.x, currentPos.y, currentPos.z);
                    //}
                    //else
                    //    InnerGenerator.TryAddStructure(ref data);
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
