using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandaros.Settlers.Managers
{
    [ModLoader.ModManager]
    public static class TimeManager
    {
        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnUpdate, GameLoader.NAMESPACE + ".Managers.TimeManager.OnUpdate")]
        public static void OnUpdate()
        {
            if (Configuration.GetorDefault("NightsDisabled", false) && !TimeCycle.IsDay)
                TimeCycle.AddTime(TimeCycle.TotalNightLength);
        }
    }
}
