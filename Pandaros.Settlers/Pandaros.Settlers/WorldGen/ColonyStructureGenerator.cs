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
        private Vector3Int _lastSpawned = new Vector3Int(150, 30, 150);

        List<GeneratedStructure> _structures = new List<GeneratedStructure>();
        GeneratedStructure _next;

        public IStructureGenerator InnerGenerator { get; set; }

        public ColonyStructureGenerator(IMetaBiomeProvider metaBiomeProvider, float maxSteepForStructureGlobal, DefaultTreeStructureGenerator tree) 
        {
            InnerGenerator = tree;
        }

        public void TryAddStructure(ref StructureGeneratorData data)
        {
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

                int x = data.WorldX & 15;
                int z = data.WorldZ & 15;
                var currentPos = new Vector3Int(x, data.WorldY, z);
                bool canBuild = true;
                var distance = UnityEngine.Vector3.Distance(_lastSpawned.Vector, currentPos.Vector);

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

                    _lastSpawned = currentPos;
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
