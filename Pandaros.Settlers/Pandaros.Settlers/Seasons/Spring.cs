using BlockTypes;
using System.Collections.Generic;

namespace Pandaros.Settlers.Seasons
{
    public class Spring : ISeason
    {
        public Dictionary<string, Dictionary<ushort, List<ushort>>> SeasonalBlocks { get; } = new Dictionary<string, Dictionary<ushort, List<ushort>>>();

        public string SeasonAfter { get; } = nameof(Summer);
        public string Name { get; } = nameof(Spring);
        public float FoodMultiplier { get; }
        public float WoodMultiplier { get; }
        public double DayTemperatureDifferance { get; } = 0;
        public double NightTemperatureDifferance { get; } = -10;
    }
}
