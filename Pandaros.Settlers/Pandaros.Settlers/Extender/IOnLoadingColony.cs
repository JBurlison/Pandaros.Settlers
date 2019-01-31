using Pipliz.JSON;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandaros.Settlers.Extender
{
    public interface IOnLoadingColony : ISettersExtension
    {
        void OnLoadingColony(Colony c, JSONNode n);
    }
}
