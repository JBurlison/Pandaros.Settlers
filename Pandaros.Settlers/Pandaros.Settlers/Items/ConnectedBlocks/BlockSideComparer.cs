using Pandaros.Settlers.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandaros.Settlers.Items
{
    public class BlockSideComparer : IEqualityComparer<List<BlockSide>>
    {
        public bool Equals(List<BlockSide> x, List<BlockSide> y)
        {
            foreach (BlockSide t in x)
            {
                var att = t.GetAttribute<BlockSideVectorValuesAttribute>();
                bool equals = false;

                foreach (var f in att.EquatableTo)
                    if (y.Contains(f))
                    {
                        equals = true;
                        break;
                    }

                if (!y.Contains(t) && !equals)
                    return false;
            }

            foreach (BlockSide t in y)
            {
                var att = t.GetAttribute<BlockSideVectorValuesAttribute>();
                bool equals = false;

                foreach (var f in att.EquatableTo)
                    if (x.Contains(f))
                    {
                        equals = true;
                        break;
                    }

                if (!x.Contains(t) && !equals)
                    return false;
            }

            return true;
        }

        public int GetHashCode(List<BlockSide> obj)
        {
            int hashcode = 0;
            foreach (BlockSide t in obj)
            {
                hashcode ^= t.GetHashCode();
            }
            return hashcode;
        }
    }
}
