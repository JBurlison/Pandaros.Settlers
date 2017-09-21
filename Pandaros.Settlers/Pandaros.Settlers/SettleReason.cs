using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pandaros.Settlers
{
    public static class SettleReason
    {
        static Random rnd = new Random();

        public static string GetReason()
        {
            return _reasons[rnd.Next(0, _reasons.Count - 1)];
        }

        static List<string> _reasons = new List<string>()
        {
            { "Like field of dreams....If you build it, they will come. {0} settlers have decided to join your colony" }
        };
    }
}
