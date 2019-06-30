using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandaros.Settlers.Models
{
    public class CSGenerateType : ICSGenerateType
    {
        public virtual ICSType baseType { get; set; }

        public virtual string generateType { get; set; }

        public virtual string typeName { get; set; }
    }
}
