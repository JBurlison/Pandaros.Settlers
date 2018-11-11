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
        public double DayTemperatureDifferance { get; } = 15;
        public double NightTemperatureDifferance { get; }  = 0;
    }
}
