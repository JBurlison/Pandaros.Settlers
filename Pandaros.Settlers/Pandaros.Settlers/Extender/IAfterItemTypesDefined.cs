using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandaros.Settlers.Extender
{
    public interface IAfterItemTypesDefined : ISettlersExtension
    {
        void AfterItemTypesDefined();
    }
}
