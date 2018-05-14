using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BlockTypes.Builtin;

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
        public float MinDayTemperature { get; }
        public float MaxDayTemperature { get; }
        public float MinNightTemperature { get; }
        public float MaxNightTemperature { get; }
    }
}
