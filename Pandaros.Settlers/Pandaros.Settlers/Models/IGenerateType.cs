﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandaros.Settlers.Models
{
    public interface ICSGenerateType
    {
        ICSType baseType { get; }
        string generateType { get; }
        string typeName { get; }
    }
}
