using Jobs;
using Jobs.Implementations.Construction;
using NPC;
using Pandaros.API;
using Pandaros.API.Models;
using Pandaros.Settlers.NBT;
using System;
using System.Collections.Generic;

namespace Pandaros.Settlers.Jobs.Construction
{
    [ModLoader.ModManager]
    public class ArchitectBuilder : IConstructionType
    {
        private static List<ArchitectIterator> _needsChunkLoaded = new List<ArchitectIterator>();

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnShouldKeepChunkLoaded, GameLoader.NAMESPACE + ".Jobs.Construction.ArchitectBuilder.OnShouldKeepChunkLoaded")]
        public static void OnShouldKeepChunkLoaded(ChunkUpdating.KeepChunkLoadedData data)
        {
            foreach (var iterator in _needsChunkLoaded)
            {
                if (iterator != null &&
                    iterator.CurrentPosition != null &&
                    iterator.CurrentPosition.IsWithinBounds(data.CheckedChunk.Position, data.CheckedChunk.Bounds))
                    data.Result = true;
            }
        }

        public int OnStockpileNewItemCount => 0;

        public void DoJob(IIterationType iterationType, IAreaJob areaJob, ConstructionJobInstance job, ref NPCBase.NPCState state)
        {
            int i = 0;
            var bpi = iterationType as ArchitectIterator;

            if (bpi == null)
            {
                SettlersLogger.Log(ChatColor.yellow, "iterationType must be of type ArchitectIterator for the ArchitectBuilder.");
                state.SetIndicator(new Shared.IndicatorState(5f, ColonyBuiltIn.ItemTypes.ERRORIDLE.Name));
                AreaJobTracker.RemoveJob(areaJob);
                return;
            }

            while (true) // move past air
            {
                if (i > 4000)
                    break;

                i++;

                var adjX = iterationType.CurrentPosition.x - bpi.BuilderSchematic.StartPos.x;
                var adjY = iterationType.CurrentPosition.y - bpi.BuilderSchematic.StartPos.y;
                var adjZ = iterationType.CurrentPosition.z - bpi.BuilderSchematic.StartPos.z;
                var prvX = bpi.PreviousPosition.x - bpi.BuilderSchematic.StartPos.x;
                var prvY = bpi.PreviousPosition.y - bpi.BuilderSchematic.StartPos.y;
                var prvZ = bpi.PreviousPosition.z - bpi.BuilderSchematic.StartPos.z;

                if (World.TryGetTypeAt(iterationType.CurrentPosition, out ItemTypes.ItemType foundType))
                {
                    state.SetCooldown(2);
                    state.SetIndicator(new Shared.IndicatorState(2, foundType.Name));

                    if (foundType.ItemIndex == ColonyBuiltIn.ItemTypes.AIR ||
                        foundType.Name == ColonyBuiltIn.ItemTypes.BANNER)
                    {
                        if (!MoveNext(iterationType, areaJob, job, bpi, prvX, prvY, prvZ))
                            return;

                        continue;
                    }

                    try
                    {
                        bpi.BuilderSchematic.Blocks[adjX, adjY, adjZ].BlockID = foundType.Name;
                    }
                    catch (IndexOutOfRangeException)
                    {
                        SettlersLogger.Log(ChatColor.red, $"Index out of range on ArchitectBuilder {adjX}, {adjY}, {adjZ} to a max of {bpi.BuilderSchematic.Blocks.GetLength(0)}, {bpi.BuilderSchematic.Blocks.GetLength(1)}, {bpi.BuilderSchematic.Blocks.GetLength(2)}.");

                        CleanupJob(iterationType, areaJob, job, bpi, prvX, prvY, prvZ);
                        break;
                    }

                    if (!foundType.Name.Contains("bedend"))
                        ServerManager.TryChangeBlock(iterationType.CurrentPosition, SettlersBuiltIn.ItemTypes.SELECTOR, new BlockChangeRequestOrigin(job.Owner), ESetBlockFlags.DefaultAudio);

                    MoveNext(iterationType, areaJob, job, bpi, prvX, prvY, prvZ);
                }
                else
                {
                    if (!_needsChunkLoaded.Contains(bpi))
                        _needsChunkLoaded.Add(bpi);

                    ChunkQueue.QueuePlayerSurrounding(iterationType.CurrentPosition.ToChunk());
                    state.SetIndicator(new Shared.IndicatorState(5f, ColonyBuiltIn.ItemTypes.ERRORIDLE.Name));
                }

                break;
            }
        }

        private static bool MoveNext(IIterationType iterationType, IAreaJob areaJob, ConstructionJobInstance job, ArchitectIterator bpi, int prvX, int prvY, int prvZ)
        {
            if (bpi.PreviousPosition != Pipliz.Vector3Int.invalidPos &&
                prvX <= bpi.BuilderSchematic.XMax &&
                prvY <= bpi.BuilderSchematic.YMax &&
                prvZ <= bpi.BuilderSchematic.ZMax &&
               !bpi.BuilderSchematic.Blocks[prvX, prvY, prvZ].BlockID.Contains("bedend"))
                ServerManager.TryChangeBlock(bpi.PreviousPosition, ItemId.GetItemId(bpi.BuilderSchematic.Blocks[prvX, prvY, prvZ].BlockID), new BlockChangeRequestOrigin(job.Owner), ESetBlockFlags.DefaultAudio);

            if (!bpi.MoveNext())
            {
                CleanupJob(iterationType, areaJob, job, bpi, prvX, prvY, prvZ);
                return false;
            }

            return true;
        }

        private static void CleanupJob(IIterationType iterationType, IAreaJob areaJob, ConstructionJobInstance job, ArchitectIterator bpi, int prvX, int prvY, int prvZ)
        {
            if (_needsChunkLoaded.Contains(bpi))
                _needsChunkLoaded.Remove(bpi);

            if (bpi.PreviousPosition != Pipliz.Vector3Int.invalidPos &&
               prvX <= bpi.BuilderSchematic.XMax &&
               prvY <= bpi.BuilderSchematic.YMax &&
               prvZ <= bpi.BuilderSchematic.ZMax &&
               !bpi.BuilderSchematic.Blocks[prvX, prvY, prvZ].BlockID.Contains("bedend"))
                ServerManager.TryChangeBlock(bpi.PreviousPosition, ItemId.GetItemId(bpi.BuilderSchematic.Blocks[prvX, prvY, prvZ].BlockID), new BlockChangeRequestOrigin(job.Owner), ESetBlockFlags.DefaultAudio);

            SchematicReader.SaveSchematic(areaJob.Owner, bpi.BuilderSchematic);
            AreaJobTracker.RemoveJob(areaJob);
        }
    }
}
