using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandaros.Settlers.Extender
{
    public interface IOnTimedUpdate
    {
        double NextUpdateTimeMin { get; }
        double NextUpdateTimeMax { get; }
        double NextUpdateTime { get; set; }
        void OnTimedUpdate();
    }
}
