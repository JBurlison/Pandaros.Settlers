using Pandaros.Settlers.Items;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandaros.Settlers.Models
{
    public enum BlockSide
    {
        [BlockSideVectorValues(0, 0, 0)]
        Invalid,
        [BlockSideVectorValues(1, 1, 0)]
        XpYp,
        [BlockSideVectorValues(0, 1, 1)]
        ZpYp,
        [BlockSideVectorValues(0, 1, 0)]
        Yp,
        [BlockSideVectorValues(0, 1, -1)]
        ZnYp,
        [BlockSideVectorValues(-1, 1, 0)]
        XnYp,
        [BlockSideVectorValues(1, 0, 0)]
        Xp,
        [BlockSideVectorValues(0, 0, 1)]
        Zp,
        [BlockSideVectorValues(0, 0, -1)]
        Zn,
        [BlockSideVectorValues(-1, 0, 0)]
        Xn,
        [BlockSideVectorValues(1, -1, 0)]
        XpYn,
        [BlockSideVectorValues(0, -1, 1)]
        ZpYn,
        [BlockSideVectorValues(0, -1, 0)]
        Yn,
        [BlockSideVectorValues(0, -1, -1)]
        ZnYn,
        [BlockSideVectorValues(-1, -1, 0)]
        XnYn
    }
}
