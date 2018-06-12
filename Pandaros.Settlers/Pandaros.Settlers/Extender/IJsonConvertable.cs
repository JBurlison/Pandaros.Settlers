using Pipliz.JSON;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pandaros.Settlers.Extender
{
    public interface IJsonConvertable
    {
        JSONNode ToJsonNode();
    }
}
