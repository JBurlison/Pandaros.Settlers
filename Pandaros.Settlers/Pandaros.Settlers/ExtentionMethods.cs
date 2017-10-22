using NPC;
using Pipliz.APIProvider.Jobs;
using Server.NPCs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pandaros.Settlers
{
    public static class ExtentionMethods
    {
        public static void SetJobTime(this BlockJobBase job, double time)
        {
            typeof(BlockJobBase).GetField("followers", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).SetValue(job, Pipliz.Time.SecondsSinceStartDouble + time);
        }
    }
}
