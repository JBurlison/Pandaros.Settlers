using BlockTypes;
using Jobs;
using NetworkUI;
using NetworkUI.Items;
using NPC;
using Pandaros.Settlers.Items;
using Pandaros.Settlers.Research;
using Pipliz;
using Pipliz.Mods.BaseGame.Construction;
using Shared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandaros.Settlers.Jobs.Construction
{
    public class Architect : IConstructionType
    {
        public int OnStockpileNewItemCount => 1;

        public void DoJob(IIterationType iterationType, IAreaJob areaJob, ConstructionJobInstance job, ref NPCBase.NPCState state)
        {
            var bpi = iterationType as SchematicIterator;

            if (bpi == null)
            {
                PandaLogger.Log(ChatColor.yellow, "iterationType must be of type SchematicIterator for the Archetect.");
                state.SetIndicator(new Shared.IndicatorState(5f, ColonyBuiltIn.ItemTypes.ERRORIDLE.Name));
                AreaJobTracker.RemoveJob(areaJob);
                return;
            }

            if (iterationType.MoveNext())
            {
                state.SetIndicator(new Shared.IndicatorState(5f, ColonyBuiltIn.ItemTypes.ERRORIDLE.Name));
                return;
            }
            else
            {
                // failed to find next position to do job at, self-destruct
                state.SetIndicator(new Shared.IndicatorState(5f, ColonyBuiltIn.ItemTypes.ERRORIDLE.Name));
                AreaJobTracker.RemoveJob(areaJob);
                PandaLogger.Log(ChatColor.yellow, "Failed to MoveNext after while. Iterator position: {0}.", iterationType.CurrentPosition);
                return;
            }
        }

        public static float GetCooldown()
        {
            return Pipliz.Random.NextFloat(1.5f, 2.5f);
        }
    }
}
