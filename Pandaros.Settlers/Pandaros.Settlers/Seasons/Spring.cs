using BlockTypes;
using System.Collections.Generic;

namespace Pandaros.Settlers.Seasons
{
    public class Spring : ISeason
    {
        public Dictionary<string, ushort> SeasonalBlocks { get; } = new Dictionary<string, ushort>()
        {
            {
                BlockTypeRegistry.GRASS, BuiltinBlocks.GrassTundra
            }
        };

        public string SeasonAfter { get; } = nameof(Summer);
        public string Name { get; } = nameof(Spring);
        public float FoodMultiplier { get; }
        public float WoodMultiplier { get; }
        public double MinDayTemperature { get; } = 55;
        public double MaxDayTemperature { get; } = 76;
        public double MinNightTemperature { get; } = 40;
        public double MaxNightTemperature { get; } = 70;
    }
}
