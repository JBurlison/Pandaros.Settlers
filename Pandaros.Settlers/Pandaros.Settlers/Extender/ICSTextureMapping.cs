using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pandaros.Settlers.Extender
{
    public interface ICSTextureMapping : IJsonConvertable
    {
        string Name { get; }
        string emissive { get; }
        string albedo { get; }
        string normal { get; }
        string height { get; }
    }
}
