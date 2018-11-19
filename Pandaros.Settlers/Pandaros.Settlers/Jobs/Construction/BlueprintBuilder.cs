using BlockTypes;
using Jobs;
using NPC;
using Pandaros.Settlers.Buildings.NBT;
using Pipliz;
using Pipliz.Mods.BaseGame.Construction;
using Shared;

namespace Pandaros.Settlers.Jobs.Construction
{
    public class BlueprintBuilder : IConstructionType
    {
        public EAreaType AreaType => EAreaType.BuilderArea;

        public EAreaMeshType AreaTypeMesh => EAreaMeshType.ThreeD;
        System.Collections.Generic.List<ItemTypes.ItemTypeDrops> _gatherResults = new System.Collections.Generic.List<ItemTypes.ItemTypeDrops>();

        public void DoJob(IIterationType iterationType, IAreaJob areaJob, ConstructionJobInstance job, ref NPCBase.NPCState state)
        {
            Block block = default(Block);
            Vector3Int jobPosition = iterationType.CurrentPosition;

            try
            {
                var bpi = iterationType as BlueprintIterator;
                var adjX = jobPosition.x - bpi.BuilderSchematic.StartPos.x;
                var adjY = jobPosition.y - bpi.BuilderSchematic.StartPos.y;
                var adjZ = jobPosition.z - bpi.BuilderSchematic.StartPos.z;

                if (bpi != null &&
                    bpi.BuilderSchematic.XMax > adjX &&
                    bpi.BuilderSchematic.YMax > adjY &&
                    bpi.BuilderSchematic.ZMax > adjZ)
                    block = bpi.BuilderSchematic.Blocks[adjX, adjY, adjZ];
                else
                    PandaLogger.Log(ChatColor.yellow, "Unable to find scematic position {0}", jobPosition);
            }
            catch (System.Exception) { }

            if (block == default(Block))
                block = Block.Air;

            var mapped = block.MappedBlock;
            var buildType = ItemTypes.GetType(mapped.CSIndex);

            if (iterationType == null || buildType == null || buildType.ItemIndex == 0)
            {
                AreaJobTracker.RemoveJob(areaJob);
                return;
            }

            int iMax = 4096;
            while (iMax-- > 0)
            {
                if (World.TryGetTypeAt(jobPosition, out ushort foundTypeIndex))
                {
                    if (foundTypeIndex == 0 || foundTypeIndex == BuiltinBlocks.Water)
                    {
                        Stockpile ownerStockPile = areaJob.Owner.Stockpile;

                        if (ownerStockPile.Contains(buildType.ItemIndex))
                        {
                            if (ServerManager.TryChangeBlock(jobPosition, foundTypeIndex, buildType.ItemIndex, areaJob.Owner, ESetBlockFlags.DefaultAudio) == EServerChangeBlockResult.Success)
                            {
                                if (--job.StoredItemCount <= 0)
                                {
                                    job.ShouldTakeItems = true;
                                    state.JobIsDone = true;
                                }
                                ownerStockPile.TryRemove(buildType.ItemIndex);
                                if (iterationType.MoveNext())
                                {
                                    state.SetIndicator(new Shared.IndicatorState(GetCooldown(), buildType.ItemIndex));
                                }
                                else
                                {
                                    // failed to find next position to do job at, self-destruct
                                    state.SetIndicator(new Shared.IndicatorState(5f, BuiltinBlocks.ErrorIdle));
                                    AreaJobTracker.RemoveJob(areaJob);
                                }
                                return;
                            }
                            else
                            {
                                // shouldn't really happen, world not loaded (just checked)
                                state.SetIndicator(new Shared.IndicatorState(5f, BuiltinBlocks.ErrorMissing, true, false));
                            }
                        }
                        else
                        {
                            // missing building item
                            state.SetIndicator(new Shared.IndicatorState(Random.NextFloat(5f, 8f), buildType.ItemIndex, true, false));
                        }
                        return; // either changed a block or set indicator, job done
                    }
                    else
                    {
                        // move iterator, not placing at non-air blocks
                        if (iterationType.MoveNext())
                        {
                            continue; // found non-air, try next loop
                        }
                        else
                        {
                            // failed to find next position to do job at, self-destruct
                            state.SetIndicator(new Shared.IndicatorState(5f, BuiltinBlocks.ErrorIdle));
                            AreaJobTracker.RemoveJob(areaJob);
                            return;
                        }
                    }
                    // unreachable
                }
                else
                {
                    state.SetIndicator(new Shared.IndicatorState(5f, BuiltinBlocks.ErrorMissing, true, false));
                    return; // end loop, wait for world to load
                }
                // unreachable
            }
            // reached loop count limit
            state.SetCooldown(1.0);
        }

        public static float GetCooldown()
        {
            return Random.NextFloat(1.5f, 2.5f);
        }
    }
}
