using BlockTypes;
using Jobs;
using NPC;
using Pandaros.Settlers.Buildings.NBT;
using Pipliz;
using Pipliz.Mods.BaseGame.Construction;
using Shared;
using System.Linq;

namespace Pandaros.Settlers.Jobs.Construction
{

    public class SchematicBuilder : IConstructionType
    {
        public EAreaType AreaType => EAreaType.BuilderArea;

        public EAreaMeshType AreaTypeMesh => EAreaMeshType.ThreeD;
        System.Collections.Generic.List<ItemTypes.ItemTypeDrops> _gatherResults = new System.Collections.Generic.List<ItemTypes.ItemTypeDrops>();

        public void DoJob(IIterationType iterationType, IAreaJob areaJob, ConstructionJobInstance job, ref NPCBase.NPCState state)
        {
            SchematicBlock block = default(SchematicBlock);
            int i = 0;

            while (true) // This is to move past air.
            {
                if (i > 4000)
                    break;
        
                var bpi = iterationType as SchematicIterator;

                var adjX = iterationType.CurrentPosition.x - bpi.BuilderSchematic.StartPos.x;
                var adjY = iterationType.CurrentPosition.y - bpi.BuilderSchematic.StartPos.y;
                var adjZ = iterationType.CurrentPosition.z - bpi.BuilderSchematic.StartPos.z;

                if (bpi != null &&
                    bpi.BuilderSchematic.XMax > adjX &&
                    bpi.BuilderSchematic.YMax > adjY &&
                    bpi.BuilderSchematic.ZMax > adjZ)
                {
                    if (bpi.BuilderSchematic.Blocks != null && bpi.BuilderSchematic.Blocks.Length > 0)
                        block = bpi.BuilderSchematic.Blocks[adjX, adjY, adjZ];
                    else
                        block = bpi.BuilderSchematic.CSBlocks[adjX, adjY, adjZ];
                }

                if (block == default(SchematicBlock))
                    block = SchematicBlock.Air;

                var mapped = block.MappedBlock;
                var buildType = ItemTypes.GetType(mapped.CSIndex);

                if (buildType == null)
                {
                    state.SetIndicator(new Shared.IndicatorState(5f, BuiltinBlocks.ErrorIdle));
                    AreaJobTracker.RemoveJob(areaJob);
                    return;
                }

                if (World.TryGetTypeAt(iterationType.CurrentPosition, out ushort foundTypeIndex))
                {
                    i++;

                    if (foundTypeIndex == buildType.ItemIndex) // check if the blocks are the same, if they are, move past. Most of the time this will be air.
                        if (iterationType.MoveNext())
                            continue;
                        else
                        {
                            state.SetIndicator(new Shared.IndicatorState(5f, BuiltinBlocks.ErrorIdle));
                            AreaJobTracker.RemoveJob(areaJob);
                            return;
                        }

                    Stockpile ownerStockPile = areaJob.Owner.Stockpile;

                    bool ok = buildType.ItemIndex == BuiltinBlocks.Air;

                    if (!ok && ownerStockPile.Contains(buildType.ItemIndex))
                        ok = true;

                    if (!ok && !string.IsNullOrWhiteSpace(buildType.ParentType))
                    {
                       var parentType = ItemTypes.GetType(buildType.ParentType);

                        if (!ok && ownerStockPile.Contains(parentType.ItemIndex))
                            ok = true;
                    }

                    if (ok)
                    {
                        if (foundTypeIndex != BuiltinBlocks.Air && foundTypeIndex != BuiltinBlocks.Water)
                        {
                            var foundItem = ItemTypes.GetType(foundTypeIndex);

                            if (foundItem != null && foundItem.ItemIndex != BuiltinBlocks.Air && foundItem.OnRemoveItems != null && foundItem.OnRemoveItems.Count > 0)
                                ownerStockPile.Add(foundItem.OnRemoveItems.Select(itm => itm.item).ToList());
                        }
                        if (ServerManager.TryChangeBlock(iterationType.CurrentPosition, foundTypeIndex, buildType.ItemIndex, areaJob.Owner, ESetBlockFlags.DefaultAudio) == EServerChangeBlockResult.Success)
                        {
                            if (buildType.ItemIndex != BuiltinBlocks.Air)
                            {
                                if (--job.StoredItemCount <= 0)
                                {
                                    job.ShouldTakeItems = true;
                                    state.JobIsDone = true;
                                }

                                ownerStockPile.TryRemove(buildType.ItemIndex);
                            }
                        }
                        else
                            PandaLogger.Log(ChatColor.yellow, "Failed to TryChangeBlock. Iterator position: {0} Start Pos: {1} Adjusted Pos: [{2}, {3}, {4}]. Schematic: {5} Item To Place: {6}", iterationType.CurrentPosition, bpi.BuilderSchematic.StartPos, adjX, adjY, adjZ, bpi.BuilderSchematic, buildType.ItemIndex);

                    }
                }
                else
                    PandaLogger.Log(ChatColor.yellow, "Failed to TryGetTypeAt. Iterator position: {0} Start Pos: {1} Adjusted Pos: [{2}, {3}, {4}]. Schematic: {5} Item To Place: {6}", iterationType.CurrentPosition, bpi.BuilderSchematic.StartPos, adjX, adjY, adjZ, bpi.BuilderSchematic, buildType.ItemIndex);

                if (iterationType.MoveNext())
                {
                    if (buildType.ItemIndex != BuiltinBlocks.Air)
                        state.SetIndicator(new Shared.IndicatorState(GetCooldown(), buildType.ItemIndex));
                    else
                        state.SetIndicator(new Shared.IndicatorState(GetCooldown(), foundTypeIndex));

                    return;
                }
                else
                {
                    // failed to find next position to do job at, self-destruct
                    state.SetIndicator(new Shared.IndicatorState(5f, BuiltinBlocks.ErrorIdle));
                    AreaJobTracker.RemoveJob(areaJob);
                    return;
                }
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
            return Random.NextFloat(1.5f, 2.5f);
        }
    }
}
