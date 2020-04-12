using Pandaros.API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandaros.Settlers.Decorative
{

    public class DecorGenerateTypeBase : CSGenerateType
    {
        public override string generateType { get; set; } = "rotateBlock";
        public override ICSType baseType { get; set; } = new DecorTypeBase();
        public override string typeName { get; set; }
    }
}
