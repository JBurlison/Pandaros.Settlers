using Pandaros.Settlers.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandaros.Settlers.Items
{
    [ModLoader.ModManager]
    public static class ConnectedBlockCalculator
    {
        public static Dictionary<string, IConnectedBlockCalculationType> CalculationTypes { get; } = new Dictionary<string, IConnectedBlockCalculationType>(StringComparer.InvariantCultureIgnoreCase);
        public static Dictionary<string, Dictionary<List<BlockSide>, MeshRotationEuler>> BlockRotations { get; } = new Dictionary<string, Dictionary<List<BlockSide>, MeshRotationEuler>>();

        private static Dictionary<BlockSide, Dictionary<RotationAxis, Dictionary<BlockRotationDegrees, BlockSide>>> _blocksideRotations = new Dictionary<BlockSide, Dictionary<RotationAxis, Dictionary<BlockRotationDegrees, BlockSide>>>();

        private static BlockSide[] _blockSides = (BlockSide[])Enum.GetValues(typeof(BlockSide));
        private static RotationAxis[] _blockRotations = (RotationAxis[])Enum.GetValues(typeof(RotationAxis));
        private static BlockRotationDegrees[] _blockRotationDegrees = (BlockRotationDegrees[])Enum.GetValues(typeof(BlockRotationDegrees));
        private static ListComparer<BlockSide> _blockSideCompare = new ListComparer<BlockSide>();

        static ConnectedBlockCalculator()
        {
            foreach (var kvp in CalculationTypes)
            {
                List<List<BlockSide>> blockSides = new List<List<BlockSide>>();
                
                for (int i = 1; i < kvp.Value.MaxConnections; i++)
                {
                    var permutations = kvp.Value.AvailableBlockSides.GetPermutations(i);

                    foreach (var permutation in permutations)
                    {
                        var per = permutation.ToList();
                        per.Sort();

                        if (!blockSides.Contains(per, _blockSideCompare))
                            blockSides.Add(per);
                    }
                }

                foreach (var blockSidePermutationList in blockSides)
                {
                    BlockRotations[kvp.Key] = new Dictionary<List<BlockSide>, MeshRotationEuler>(_blockSideCompare);
                    var rotationEuler = new MeshRotationEuler();

                    foreach (var side in blockSidePermutationList)
                    {
                        foreach (RotationAxis axis in _blockRotations)
                            foreach (BlockRotationDegrees rotationDegrees in _blockRotationDegrees)
                            {

                            }
                    }
                }
            }
        }

        public static List<ICSType> GetPermutations(ICSType baseBlock)
        {
            List<ICSType> cSTypes = new List<ICSType>();

            if (string.IsNullOrWhiteSpace(baseBlock.ConnectedBlock.CalculationType))
            {

            }
            else
                cSTypes.Add(baseBlock);

            return cSTypes;
        }

    }
}
