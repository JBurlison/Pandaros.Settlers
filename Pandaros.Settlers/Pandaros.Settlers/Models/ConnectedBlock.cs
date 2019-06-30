using Pandaros.Settlers.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandaros.Settlers.Models
{
    public class ConnectedBlock
    {
        public string BlockType { get; set; }
        public List<BlockSide> Connections { get; set; }
        public string CalculationType { get; set; }
        public bool AutoChange { get; set; } = true;
        public RotationAxis RotationAxis { get; set; }
        public BlockRotationDegrees BlockRotationDegrees { get; set; }
    }
}
