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
        private int _lastSpawnedX = 150;
        private int _lastSpawnedZ = 150;
        List<GeneratedStructure> _structures = new List<GeneratedStructure>();
        GeneratedStructure _next;

        public ColonyStructureGenerator(IMetaBiomeProvider metaBiomeProvider, float maxSteepForStructureGlobal) : 
            base(metaBiomeProvider, maxSteepForStructureGlobal)
        {
            
        }

        public override void TryAddStructure(ref StructureGeneratorData data)
        {
            PandaLogger.Log("Gen");
            if (data.Steepness <= .3)
            {
                PandaLogger.Log("steep");
                if (_next == null)
                {
                    _next = _structures.GetRandomItem();
                    _next.NumberOfPlacements--;

                    if (_next.NumberOfPlacements <= 0)
                        _structures.Remove(_next);
                }

                if (_next.SpawnChance < data.Random.NextFloat())
                {
                    base.TryAddStructure(ref data);
                    return;
                }

                int x = data.WorldX & 15;
                int z = data.WorldZ & 15;
                bool canBuild = true;

                if (_next.DistanceBetweenOtherStructuresMin > 0)
                {
                    var minX = x - _next.DistanceBetweenOtherStructuresMin;
                    var minZ = z - _next.DistanceBetweenOtherStructuresMin;

                    canBuild = (minX + minZ) > (_lastSpawnedX + _lastSpawnedZ);
                    PandaLogger.Log("min {0}", canBuild);
                }

                if (canBuild && _next.DistanceBetweenOtherStructuresMax > 0)
                {
                    var minX = x - _next.DistanceBetweenOtherStructuresMax;
                    var minZ = z - _next.DistanceBetweenOtherStructuresMax;

                    canBuild = (minX + minZ) > (_lastSpawnedX + _lastSpawnedZ);
                }

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

                    _lastSpawnedX = x;
                    _lastSpawnedZ = z;
                    _next = null;
                    PandaLogger.Log(ChatColor.lime, "Colony Placed at [{0}, {1}, {2}]", x, data.WorldY, z);
                }
            }
            else
                base.TryAddStructure(ref data);
        }

        public void AddGeneratedStructure(GeneratedStructure structure)
        {
            _structures.Add(structure);
        }
    }
}
