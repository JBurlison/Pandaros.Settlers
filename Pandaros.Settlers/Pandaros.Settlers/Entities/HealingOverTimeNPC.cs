using System;
using System.Collections.Generic;
using System.Linq;
using NPC;
using Pipliz;
using Shared;

namespace Pandaros.Settlers.Entities
{
    [ModLoader.ModManager]
    public class HealingOverTimeNPC
    {
        private static long _nextUpdate;
        private static readonly List<HealingOverTimeNPC> _instances = new List<HealingOverTimeNPC>();
        private static readonly List<HealingOverTimeNPC> _toRemove = new List<HealingOverTimeNPC>();

        public HealingOverTimeNPC(NPCBase nPC, float initialHeal, float totalHoT, int durationSeconds, ushort indicator)
        {
            HealPerTic      = totalHoT / durationSeconds;
            DurationSeconds = durationSeconds;
            InitialHeal     = initialHeal;
            Target          = nPC;
            TotalHoTTime    = totalHoT;
            TicksLeft       = durationSeconds;
            Indicator       = indicator;

            NewInstance?.Invoke(this, null);

            _instances.Add(this);
            Target.Heal(InitialHeal);
            Target.SetIndicatorState(new IndicatorState(1, Indicator));
            Tick += HealingOverTimeNPC_Tick;
        }

        public NPCBase Target { get; }

        public float TotalHoTTime { get; }

        public float InitialHeal { get; }

        public int DurationSeconds { get; }

        public int TicksLeft { get; private set; }

        public float HealPerTic { get; }
        public ushort Indicator { get; }
        public static event EventHandler NewInstance;

        public event EventHandler Complete;

        public event EventHandler Tick;

        private void HealingOverTimeNPC_Tick(object sender, EventArgs e)
        {
            TicksLeft--;
            Target.Heal(HealPerTic);

            if (TicksLeft <= 0 && Complete != null)
                Complete(this, null);
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnUpdate,
            GameLoader.NAMESPACE + ".Entities.HealingOverTimeNPC.Update")]
        public static void Update()
        {
            if (Time.MillisecondsSinceStart > _nextUpdate && _instances.Count > 0)
            {
                _toRemove.Clear();

                foreach (var healing in _instances)
                {
                    healing.Target.SetIndicatorState(new IndicatorState(1, healing.Indicator));

                    healing.Tick?.Invoke(healing, null);

                    if (healing.TicksLeft <= 0)
                        _toRemove.Add(healing);
                }

                foreach (var remove in _toRemove)
                    _instances.Remove(remove);

                _toRemove.Clear();

                _nextUpdate = Time.MillisecondsSinceStart + 1000;
            }
        }

        public static bool NPCIsBeingHealed(NPCBase npc)
        {
            return _instances.Any(a => a.Target == npc);
        }
    }
}