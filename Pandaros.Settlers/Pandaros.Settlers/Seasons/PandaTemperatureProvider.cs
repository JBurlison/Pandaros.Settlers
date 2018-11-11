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

        public PandaTemperatureProvider()
        {
            InnerGenerator = this;
        }

        public float GetTemperature(float height, float worldX, float worldZ, ref TerrainGenerator.MetaBiomePreciseStruct metaBiomeData)
        {
            var temps = new[] { SeasonsFactory.CurrentSeason.MaxDayTemperature, SeasonsFactory.CurrentSeason.MaxNightTemperature, SeasonsFactory.CurrentSeason.MinDayTemperature, SeasonsFactory.CurrentSeason.MinNightTemperature };
            double temp = temps.Average();

            if (TimeCycle.IsDay)
            {
                var tempDiff = SeasonsFactory.CurrentSeason.MaxDayTemperature - SeasonsFactory.CurrentSeason.MinDayTemperature;

                if (TimeCycle.TimeOfDayHours <= SeasonsFactory.MidDay)
                {
                    var pct = TimeCycle.TimeOfDayHours / SeasonsFactory.MidDay;
                    temp = tempDiff * pct;
                }
                else
                {
                    var pct = TimeCycle.TimeOfDayHours / TimeCycle.SunSet;
                    temp = tempDiff * pct;
                }

                temp += SeasonsFactory.CurrentSeason.MinDayTemperature;
            }
            else
            {
                var tempDiff = SeasonsFactory.CurrentSeason.MaxDayTemperature - SeasonsFactory.CurrentSeason.MinDayTemperature;

                if (TimeCycle.TimeOfDayHours <= SeasonsFactory.MidNight)
                {
                    var pct = TimeCycle.TimeOfDayHours / SeasonsFactory.MidNight;
                    temp = tempDiff * pct;
                }
                else
                {
                    var pct = TimeCycle.TimeOfDayHours / TimeCycle.SunRise;
                    temp = tempDiff * pct;
                }

                temp = SeasonsFactory.CurrentSeason.MaxNightTemperature - temp;
            }

            return (float)Math.Round(temp, 2);
        }
    }
}
