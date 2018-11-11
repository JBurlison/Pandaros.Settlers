using Pandaros.Settlers.Extender;
using System.Collections.Generic;

namespace Pandaros.Settlers.Seasons
{
    // TODO: Support for biomes
    public interface ISeason : INameable
    {
        Dictionary<string, Dictionary<ushort, List<ushort>>> SeasonalBlocks { get; }

        string SeasonAfter { get; }
        float FoodMultiplier { get; }
        float WoodMultiplier { get; }
        double DayTemperatureDifferance { get; }
        double NightTemperatureDifferance { get; }
    }
}