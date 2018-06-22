using Pandaros.Settlers.Extender;
using Pipliz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pandaros.Settlers.Items.Temperature
{
    public enum TemperatureType
    {
        None = 0,
        Heat,
        Cold,
        Both
    }

    public interface ITemperatureRegulator 
    {
        int RadiusOfTemperatureAdjustment { get; }
        double TemperatureAdjusted { get; }
        Vector3Int Position { get; set; }
        TemperatureType TemperatureRegulated { get; }
    }
}
