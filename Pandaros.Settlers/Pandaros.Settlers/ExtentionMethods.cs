using NPC;
using Pandaros.Settlers.Entities;
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
        public static double NextDouble(this Random rng, double min, double max)
        {
            return rng.NextDouble() * (max - min) + min;
        }

        public static void SetJobTime(this BlockJobBase job, double time)
        {
            typeof(BlockJobBase).GetField("timeJob", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).SetValue(job, Pipliz.Time.SecondsSinceStartDouble + time);
        }

        public static double GetJobTime(this BlockJobBase job)
        {
            return (double)typeof(BlockJobBase).GetField("timeJob", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(job);
        }

        public static List<InventoryItem> GetInventory(this Inventory inv)
        {
            return typeof(Inventory).GetField("items", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(inv) as List<InventoryItem>;
        }

        public static BedBlock GetBed(this NPCBase job)
        {
            return typeof(NPCBase).GetField("bed", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(job) as BedBlock;
        }
    }
}
