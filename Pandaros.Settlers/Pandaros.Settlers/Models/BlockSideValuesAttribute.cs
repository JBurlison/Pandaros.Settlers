using Pandaros.Settlers.Models;
using System;
using System.Collections.Generic;

namespace Pandaros.Settlers.Models
{
    public class BlockSideVectorValuesAttribute : Attribute
    {
        public int X { get; private set; }
        public int Y { get; private set; }
        public int Z { get; private set; }
        public BlockSide[] EquatableTo { get; private set; }

        public BlockSideVectorValuesAttribute(int x, int y, int z, params BlockSide[] equatableTo)
        {
            X = x;
            Y = y;
            Z = z;
            EquatableTo = equatableTo;
        }
    }
}
