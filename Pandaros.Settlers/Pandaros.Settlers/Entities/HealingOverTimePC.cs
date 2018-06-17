using System;
using System.Collections.Generic;
using Pipliz;

namespace Pandaros.Settlers.Entities
{
    [ModLoader.ModManagerAttribute]
    public class HealingOverTimePC
    {
        private static long _nextUpdate;
        private static readonly List<HealingOverTimePC> _instances = new List<HealingOverTimePC>();
        private static readonly List<HealingOverTimePC> _toRemove = new List<HealingOverTimePC>();

        public HealingOverTimePC(Players.Player pc, float initialHeal, float totalHoT, int durationSeconds)
        {
            HealPerTic      = totalHoT / durationSeconds;
            DurationSeconds = durationSeconds;
            InitialHeal     = initialHeal;
            Target          = pc;
            TotalHoTTime    = totalHoT;
            TicksLeft       = durationSeconds;

            if (NewInstance != null)
                NewInstance(this, null);

            _instances.Add(this);
            Target.Heal(InitialHeal);
            Tick += HealingOverTimeNPC_Tick;
        }

        public Players.Player Target { get; }

        public float TotalHoTTime { get; }

        public float InitialHeal { get; }

        public int DurationSeconds { get; }

        public int TicksLeft { get; private set; }

        public float HealPerTic { get; }
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
            GameLoader.NAMESPACE + ".Entities.HealingOverTimePC.Update")]
        public static void Update()
        {
            if (Time.MillisecondsSinceStart > _nextUpdate && _instances.Count > 0)
            {
                _toRemove.Clear();

                foreach (var healing in _instances)
                {
                    if (healing.Tick != null)
                        healing.Tick(healing, null);

                    if (healing.TicksLeft <= 0)
                        _toRemove.Add(healing);
                }

                foreach (var remove in _toRemove)
                    _instances.Remove(remove);

                _toRemove.Clear();

                _nextUpdate = Time.MillisecondsSinceStart + 1000;
            }
        }
    }
}