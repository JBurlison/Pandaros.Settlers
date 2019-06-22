using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandaros.Settlers.Models
{
    public enum BlockSides
    {
        XpZpYp = BitWiseBlockSides.Xp & BitWiseBlockSides.Zp & BitWiseBlockSides.Yp,
        XpYp = BitWiseBlockSides.Xp & BitWiseBlockSides.Yp,
        XpZnYp = BitWiseBlockSides.Xp & BitWiseBlockSides.Zn & BitWiseBlockSides.Yp,
        ZpYp = BitWiseBlockSides.Zp & BitWiseBlockSides.Yp,
        Yp = BitWiseBlockSides.Yp,
        ZnYp = BitWiseBlockSides.Zn & BitWiseBlockSides.Yp,
        XnZpYp = BitWiseBlockSides.Xn & BitWiseBlockSides.Zp & BitWiseBlockSides.Yp,
        XnYp = BitWiseBlockSides.Xn & BitWiseBlockSides.Yp,
        XnZnYp = BitWiseBlockSides.Xn & BitWiseBlockSides.Zn & BitWiseBlockSides.Yp,
        XpZp = BitWiseBlockSides.Xp & BitWiseBlockSides.Zp,
        Xp = BitWiseBlockSides.Xp,
        XpZn = BitWiseBlockSides.Xp & BitWiseBlockSides.Zn,
        Zp = BitWiseBlockSides.Zp,
        Zn = BitWiseBlockSides.Zn,
        XnZp = BitWiseBlockSides.Xn & BitWiseBlockSides.Zp,
        Xn = BitWiseBlockSides.Xn,
        XnZn = BitWiseBlockSides.Xn & BitWiseBlockSides.Zn,
        XpZpYn = BitWiseBlockSides.Xp & BitWiseBlockSides.Zp & BitWiseBlockSides.Yn,
        XpYn = BitWiseBlockSides.Xp & BitWiseBlockSides.Yn,
        XpZnYn = BitWiseBlockSides.Xp & BitWiseBlockSides.Zn & BitWiseBlockSides.Yn,
        ZpYn = BitWiseBlockSides.Zp & BitWiseBlockSides.Yn,
        Yn = BitWiseBlockSides.Yn,
        ZnYn = BitWiseBlockSides.Zn & BitWiseBlockSides.Yn,
        XnZpYn = BitWiseBlockSides.Yn & BitWiseBlockSides.Zp & BitWiseBlockSides.Yn,
        XnYn = BitWiseBlockSides.Xn & BitWiseBlockSides.Yn,
        XnZnYn = BitWiseBlockSides.Yn & BitWiseBlockSides.Zn & BitWiseBlockSides.Yn
    }

    public enum BitWiseBlockSides
    {
        Xp = 0,
        Xn = 1,
        Zp = 2 << 0,
        Zn = 2 << 1,
        Yp = 2 << 2,
        Yn = 2 << 3
    }
}
