using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerrainGeneration;

namespace Pandaros.Settlers.Seasons
{
    public class PandaTemperatureProvider : TerrainGenerator.ITemperatureProvider
    {
        public TerrainGenerator.ITemperatureProvider InnerGenerator { get; set; }

        public PandaTemperatureProvider(TerrainGenerator.ITemperatureProvider defaultProvider)
        {
            InnerGenerator = defaultProvider;
        }

        public float GetTemperature(float height, float worldX, float worldZ, ref TerrainGenerator.MetaBiomePreciseStruct metaBiomeData)
        {
            double temp = InnerGenerator.GetTemperature(height, worldX, worldZ, ref metaBiomeData);

            if (TimeCycle.IsDay)
            {
                temp += SeasonsFactory.CurrentSeason.DayTemperatureDifferance;
            }
            else
            {
                temp += SeasonsFactory.CurrentSeason.NightTemperatureDifferance;
            }

            return (float)Pipliz.Math.Clamp(Math.Round(temp, 2), -10, 33);
        }
    }
}
