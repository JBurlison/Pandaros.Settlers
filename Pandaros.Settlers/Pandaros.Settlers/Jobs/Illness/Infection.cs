using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pandaros.Settlers.Jobs.Illness
{
    public class Infection : ISickness
    {
        public string Name => "Infection";

        public float DamagePerSecond => 2;

        public string IndicatorIcon => "Infection.png";
    }
}
