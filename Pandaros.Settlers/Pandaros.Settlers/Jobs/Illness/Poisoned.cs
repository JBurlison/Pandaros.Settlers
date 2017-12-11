using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pandaros.Settlers.Jobs.Illness
{
    public class Poisoned : ISickness
    {
        public string Name => "Poisoned";

        public float DamagePerSecond => 4;

        public string IndicatorIcon => "Poisoned.png";
    }
}
