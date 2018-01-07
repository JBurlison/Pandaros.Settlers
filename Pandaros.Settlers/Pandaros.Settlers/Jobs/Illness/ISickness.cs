using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pandaros.Settlers.Jobs.Illness
{
    public interface ISickness
    {
        string Name { get; }
        
        float DamagePerSecond { get; }

        ushort IndicatorIcon { get; }

        ushort[] Cure { get; }
    }
}
