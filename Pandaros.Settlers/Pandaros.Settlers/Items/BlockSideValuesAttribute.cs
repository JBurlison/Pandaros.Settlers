using Pandaros.Settlers.Models;
using System;
using System.Collections.Generic;

namespace Pandaros.Settlers.Items
{
    public class BlockSideVectorValuesAttribute : Attribute
    {
        public int X { get; private set; }
        public int Y { get; private set; }
        public int Z { get; private set; }
        Dictionary<RotationAxis, Dictionary<BlockRotationDegrees, BlockSide>> BlocksideRotations { get; set; }
        
        public BlockSideVectorValuesAttribute(int x, int z, int y, Dictionary<RotationAxis, Dictionary<BlockRotationDegrees, BlockSide>> rotations)
        {
            X = x;
            Y = y;
            Z = z;
            BlocksideRotations = rotations;
        }
    }
}
