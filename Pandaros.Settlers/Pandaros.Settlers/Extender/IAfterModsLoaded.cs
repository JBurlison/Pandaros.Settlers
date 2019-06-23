using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandaros.Settlers.Extender
{
    public interface IAfterModsLoaded : ISettlersExtension
    {
        void AfterModsLoaded(List<ModLoader.ModDescription> list);
    }
}
