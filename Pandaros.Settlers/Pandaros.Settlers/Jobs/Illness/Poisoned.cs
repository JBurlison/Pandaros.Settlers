using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pandaros.Settlers.Jobs.Illness
{
    public class Poisoned : ISickness
    {
        static ushort[] _cure = new[] { Items.Healing.Antidote.Item.ItemIndex };

        public string Name => "Poisoned";

        public float DamagePerSecond => 4;

        public ushort IndicatorIcon => GameLoader.Poisoned_Icon;

        public ushort[] Cure => _cure;
    }
}
