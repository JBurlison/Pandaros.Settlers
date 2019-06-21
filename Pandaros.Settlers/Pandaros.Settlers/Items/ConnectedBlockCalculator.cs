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
        private static List<int> _rotations = new List<int>()
        {
            90,
            180,
            270
        };

        public static List<ICSType> GetPermutations(ICSType baseBlock)
        {
            List<ICSType> cSTypes = new List<ICSType>();
            //Enum.GetValues(typeof(BlockSides)

            if (baseBlock.ConnectedBlock.CalculateRotations != null)
            {

            }

            return cSTypes;
        }
    }
}
