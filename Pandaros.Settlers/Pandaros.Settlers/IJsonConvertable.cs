using Pipliz.JSON;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pandaros.Settlers
{
    public interface IJsonConvertable
    {
        JSONNode ToJsonNode();
    }
}
