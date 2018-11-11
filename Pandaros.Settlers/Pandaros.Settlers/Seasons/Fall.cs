using BlockTypes;
using System.Collections.Generic;

namespace Pandaros.Settlers.Seasons
{
    public class Fall : ISeason
    {
        public Dictionary<string, Dictionary<ushort, List<ushort>>> SeasonalBlocks { get; } = new Dictionary<string, Dictionary<ushort, List<ushort>>>();

        public string SeasonAfter { get; } = nameof(Winter);
        public string Name { get; } = nameof(Fall);
        public float FoodMultiplier { get; }
        public float WoodMultiplier { get; }
        public double DayTemperatureDifferance { get; } = -10;
        public double NightTemperatureDifferance { get; } = -20;
    }
}
