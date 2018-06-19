using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BlockTypes.Builtin;

namespace Pandaros.Settlers.Seasons
{
    public class Winter : ISeason
    {
        public Dictionary<string, ushort> SeasonalBlocks { get; } = new Dictionary<string, ushort>()
        {
            {
                BlockTypeRegistry.GRASS, BuiltinBlocks.Snow
            }
        };

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
