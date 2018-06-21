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
        Heat,
        Cold,
        Both
    }

    public interface ITemperatureRegulator : ICSType
    {
        int Radius { get; }
        double MinTemperatureAdjusted { get; }
        double MaxTemperatureAdjusted { get; }
        Vector3Int Position { get; set; }
        TemperatureType TemperatureRegulated { get; }
    }
}
