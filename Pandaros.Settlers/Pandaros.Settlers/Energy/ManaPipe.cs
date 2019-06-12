using Pandaros.Settlers.Items;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandaros.Settlers.Energy
{
    public class ManaPipe : CSType
    {
        public override string name { get; set; }

        public override string icon { get; set; }

        public override int? destructionTime { get; } = 10;

        
    }
}
