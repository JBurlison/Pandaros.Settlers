using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pandaros.Settlers.Jobs.Illness
{
    public class Infection : ISickness
    {
        static ushort[] _cure = new[] { Items.Healing.Anitbiotic.Item.ItemIndex, Items.Healing.TreatedBandage.Item.ItemIndex };

        public string Name => "Infection";

        public float DamagePerSecond => 2;

        public ushort IndicatorIcon => GameLoader.Infection_Icon;

        public ushort[] Cure => _cure;
    }
}
