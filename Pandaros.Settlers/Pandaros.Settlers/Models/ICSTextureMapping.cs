using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pandaros.Settlers.Models
{
    public interface ICSTextureMapping : INameable
    {
        string emissive { get; }
        string albedo { get; }
        string normal { get; }
        string height { get; }
    }
}
