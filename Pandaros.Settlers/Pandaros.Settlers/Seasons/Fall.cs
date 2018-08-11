using BlockTypes;
using System.Collections.Generic;

namespace Pandaros.Settlers.Seasons
{
    public class Fall : ISeason
    {
        public Dictionary<string, ushort> SeasonalBlocks { get; } = new Dictionary<string, ushort>()
        {
            {
                BlockTypeRegistry.GRASS, BuiltinBlocks.GrassSavanna
            }
        };

        public string SeasonAfter { get; } = nameof(Winter);
        public string Name { get; } = nameof(Fall);
        public float FoodMultiplier { get; }
        public float WoodMultiplier { get; }
        public double MinDayTemperature { get; } = 40;
        public double MaxDayTemperature { get; } = 70;
        public double MinNightTemperature { get; } = 30;
        public double MaxNightTemperature { get; } = 60;
    }
}
