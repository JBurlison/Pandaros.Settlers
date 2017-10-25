using Pipliz.APIProvider.Jobs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Pandaros.Settlers.Entities
{
    public class BlockJobBaseExtender
    {
        private MethodInfo _overrideCooldown;
        private FieldInfo _timeJob;
        private Type _type = typeof(BlockJobBase);

        public BlockJobBase blockJob { get; private set; }
        public NPC.NPCBase NPC { get; private set; }

        public double TimeJob
        {
            get
            {
                return (double)_timeJob.GetValue(blockJob);
            }
            set
            {
                _timeJob.SetValue(blockJob, Pipliz.Time.SecondsSinceStartDouble + value);
            }
        }

        public BlockJobBaseExtender(NPC.NPCBase npc, BlockJobBase job)
        {
            NPC = npc;
            blockJob = job;
            _timeJob = _type.GetField("timeJob", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            _overrideCooldown = _type.GetMethod("OverrideCooldown", BindingFlags.Instance | BindingFlags.NonPublic);
            
        }

        protected void OverrideCooldown(double cooldownLeft)
        {
            TimeJob = cooldownLeft;
            StackTrace stackTrace = new StackTrace();
        }
    }
}
