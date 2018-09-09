using BlockTypes;
using System.Collections.Generic;

namespace Pandaros.Settlers.Seasons
{
    public class Winter : ISeason
    {
        public Dictionary<string, Dictionary<ushort, List<ushort>>> SeasonalBlocks { get; } = new Dictionary<string, Dictionary<ushort, List<ushort>>>();

        public string SeasonAfter { get; } = nameof(Spring);
        public string Name { get; } = nameof(Winter);
        public float FoodMultiplier { get; }
        public float WoodMultiplier { get; }
        public double MinDayTemperature { get; } = 10;
        public double MaxDayTemperature { get; } = 37;
        public double MinNightTemperature { get; } = 2;
        public double MaxNightTemperature { get; } = 25;
    }
}
