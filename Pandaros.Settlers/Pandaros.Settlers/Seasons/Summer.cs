using BlockTypes;
using System.Collections.Generic;

namespace Pandaros.Settlers.Seasons
{
    public class Summer : ISeason
    {
        public Dictionary<string, Dictionary<ushort, List<ushort>>> SeasonalBlocks { get; } = new Dictionary<string, Dictionary<ushort, List<ushort>>>();

        public string SeasonAfter { get; } = nameof(Fall);
        public string Name { get; } = nameof(Summer);
        public float FoodMultiplier { get; }
        public float WoodMultiplier { get; }
        public double MinDayTemperature { get; } = 72;
        public double MaxDayTemperature { get; } = 96;
        public double MinNightTemperature { get; } = 60;
        public double MaxNightTemperature { get; } = 78;
    }
}
