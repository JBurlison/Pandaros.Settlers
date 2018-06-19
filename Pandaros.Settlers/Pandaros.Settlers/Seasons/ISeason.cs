using System.Collections.Generic;

namespace Pandaros.Settlers.Seasons
{
    public interface ISeason
    {
        Dictionary<string, ushort> SeasonalBlocks { get; }

        string SeasonAfter { get; }
        string Name { get; }
        float FoodMultiplier { get; }
        float WoodMultiplier { get; }
        double MinDayTemperature { get; }
        double MaxDayTemperature { get; }
        double MinNightTemperature { get; }
        double MaxNightTemperature { get; }
    }
}