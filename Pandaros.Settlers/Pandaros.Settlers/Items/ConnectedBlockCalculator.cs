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
        public static Dictionary<List<BlockSide>, List<MeshRotationEuler>> BlockRotations { get; } = new Dictionary<List<BlockSide>, List<MeshRotationEuler>>(new ListComparer<BlockSide>());

        private static BlockSide[] _blockTypes = (BlockSide[])Enum.GetValues(typeof(BlockSide));

        public static List<ICSType> GetPermutations(ICSType baseBlock)
        {
            List<ICSType> cSTypes = new List<ICSType>();
            
            if (baseBlock.ConnectedBlock.CalculateRotations != null)
            {

            }
            else
                cSTypes.Add(baseBlock);

            return cSTypes;
        }

    }
}
