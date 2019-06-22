using Pandaros.Settlers.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandaros.Settlers.Items
{
    public static class ConnectedBlockCalculator
    {
        public static Dictionary<string, IConnectedBlockCalculationType> CalculationTypes { get; } = new Dictionary<string, IConnectedBlockCalculationType>(StringComparer.InvariantCultureIgnoreCase);

        private static List<int> _rotations = new List<int>()
        {
            90,
            180,
            270
        };

        private static BlockSides[] _blockTypes = (BlockSides[])Enum.GetValues(typeof(BlockSides));

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
