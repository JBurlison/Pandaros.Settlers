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

        public BlockSideVectorValuesAttribute(int x, int y, int z)
        {
            X = x;
            Y = y;
            Z = z;
        }
    }
}
