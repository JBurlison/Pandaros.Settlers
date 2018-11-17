using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandaros.Settlers.Buildings.NBT
{
    public class Block
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Z { get; set; }
        public int BlockID { get; set; }
        public int Data { get; set; }
        /// <summary>Returns ItemID:SubID</summary>
        public string ItemID { get { return BlockID + ":" + Data; } }
        public override string ToString()
        {
            return string.Format("ID: {3}:{4}, X: {0}, Y: {1}, Z: {2}", X, Y, Z, BlockID, Data);
        }
    }
}
