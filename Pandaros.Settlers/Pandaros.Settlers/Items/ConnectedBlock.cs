using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandaros.Settlers.Items
{
    public class ConnectedBlock
    {
        public string blockType { get; set; }
        public bool? xp { get; set; }
        public bool? xn { get; set; }
        public bool? yp { get; set; }
        public bool? yn { get; set; }
        public bool? zp { get; set; }
        public bool? zn { get; set; }
    }
}
