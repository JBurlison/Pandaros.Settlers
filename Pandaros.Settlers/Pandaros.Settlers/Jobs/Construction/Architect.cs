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
        public EAreaType AreaType => EAreaType.BuilderArea;
        public EAreaMeshType AreaTypeMesh => EAreaMeshType.ThreeD;

        public void DoJob(IIterationType iterationType, IAreaJob areaJob, ConstructionJobInstance job, ref NPCBase.NPCState state)
        {
            int i = 0;
            var bpi = iterationType as SchematicIterator;

            if (bpi == null)
            {
                PandaLogger.Log(ChatColor.yellow, "iterationType must be of type SchematicIterator for the Archetect.");
                state.SetIndicator(new Shared.IndicatorState(5f, BuiltinBlocks.ErrorIdle));
                AreaJobTracker.RemoveJob(areaJob);
                return;
            }

            if (World.TryGetTypeAt(iterationType.CurrentPosition, out ushort foundTypeIndex))
            {

            }
          

            if (iterationType.MoveNext())
            {
                state.SetIndicator(new Shared.IndicatorState(5f, BuiltinBlocks.ErrorIdle));
                return;
            }
            else
            {
                // failed to find next position to do job at, self-destruct
                state.SetIndicator(new Shared.IndicatorState(5f, BuiltinBlocks.ErrorIdle));
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
