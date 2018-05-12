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
        float MinDayTemperature { get; }
        float MaxDayTemperature { get; }
        float MinNightTemperature { get; }
        float MaxNightTemperature { get; }
    }
}