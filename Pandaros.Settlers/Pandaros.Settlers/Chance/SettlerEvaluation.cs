using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Pandaros.Settlers.Entities;
using NPC;

namespace Pandaros.Settlers.Chance
{
    public class SettlerEvaluation : ISpawnSettlerEvaluator
    {
        public string Name => "SettlerEvaluation";

        public float SpawnChance(Players.Player p, Colony c, PlayerState state)
        {
            float chance = .3f;
            var remainingBeds =  BedBlockTracker.GetCount(p) - state.ColonistCount;

            if (remainingBeds < 1)
                chance -= 0.1f;

            if (remainingBeds >= state.MaxPerSpawn)
                chance += 0.1f;

            var hoursofFood = Stockpile.GetStockPile(p).TotalFood / c.FoodUsePerHour;

            if (hoursofFood < 48)
                chance -= 0.4f;
            else
                chance += 0.2f;

            if (JobTracker.GetCount(p) > state.MaxPerSpawn)
                chance += .4f;
            else
                chance -= .2f;

            if (state.Difficulty != GameDifficulty.Easy)
                if (c.InSiegeMode || 
                c.LastSiegeModeSpawn != 0 &&
                Pipliz.Time.SecondsSinceStartDouble - c.LastSiegeModeSpawn > TimeSpan.FromMinutes(5).TotalSeconds)
                chance -= 0.4f;

            return chance;
        }
    }
}
