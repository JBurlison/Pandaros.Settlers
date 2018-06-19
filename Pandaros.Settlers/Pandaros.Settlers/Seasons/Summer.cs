using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BlockTypes.Builtin;

namespace Pandaros.Settlers.Seasons
{
    public class Summer : ISeason
    {
        public Dictionary<string, ushort> SeasonalBlocks { get; } = new Dictionary<string, ushort>()
        {
            {
                BlockTypeRegistry.GRASS, BuiltinBlocks.GrassTemperate
            }
        };

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
