using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pandaros.Settlers.Items
{
    public interface ICSTextureMapping : IJsonConvertable, INameable
    {
        string emissive { get; }
        string albedo { get; }
        string normal { get; }
        string height { get; }
    }
}
