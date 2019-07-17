using Jobs;
using NPC;
using Pandaros.Settlers.Models;
using Pandaros.Settlers.NBT;
using Pipliz.Mods.BaseGame.Construction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                PandaLogger.Log(ChatColor.yellow, "iterationType must be of type ArchitectIterator for the ArchitectBuilder.");
                state.SetIndicator(new Shared.IndicatorState(5f, ColonyBuiltIn.ItemTypes.ERRORIDLE.Name));
                AreaJobTracker.RemoveJob(areaJob);
                return;
            }

            var adjX = iterationType.CurrentPosition.x - bpi.BuilderSchematic.StartPos.x;
            var adjY = iterationType.CurrentPosition.y - bpi.BuilderSchematic.StartPos.y;
            var adjZ = iterationType.CurrentPosition.z - bpi.BuilderSchematic.StartPos.z;

            while (true) // move past air
            {
                if (i > 4000)
                    break;

                i++;

                if (World.TryGetTypeAt(iterationType.CurrentPosition, out ItemTypes.ItemType foundType))
                {
                    if (foundType.ItemIndex == ColonyBuiltIn.ItemTypes.AIR)
                        continue;

                    if (foundType.Name == ColonyBuiltIn.ItemTypes.BANNER)
                    {
                        if (!bpi.MoveNext())
                            CleanupJob(iterationType, areaJob, job, bpi, foundType);

                        break;
                    }

                    var prvX = bpi.PreviousPosition.x - bpi.BuilderSchematic.StartPos.x;
                    var prvY = bpi.PreviousPosition.y - bpi.BuilderSchematic.StartPos.y;
                    var prvZ = bpi.PreviousPosition.z - bpi.BuilderSchematic.StartPos.z;

                    if (bpi.BuilderSchematic.Blocks.GetLength(0) - 1 < adjX ||
                        bpi.BuilderSchematic.Blocks.GetLength(1) - 1 < adjY ||
                        bpi.BuilderSchematic.Blocks.GetLength(2) - 1 < adjZ)
                    {
                        CleanupJob(iterationType, areaJob, job, bpi, foundType);
                        break;
                    }

                    try
                    {
                        bpi.BuilderSchematic.Blocks[adjX, adjY, adjZ].BlockID = foundType.Name;
                    }
                    catch (IndexOutOfRangeException)
                    {
                        PandaLogger.Log(ChatColor.red, $"Indes out of range on ArchitectBuilder {adjX}, {adjY}, {adjZ} to a max of {bpi.BuilderSchematic.Blocks.GetLength(0)}, {bpi.BuilderSchematic.Blocks.GetLength(1)}, {bpi.BuilderSchematic.Blocks.GetLength(2)}.");

                        CleanupJob(iterationType, areaJob, job, bpi, foundType);
                        break;
                    }

                    if (bpi.PreviousPosition != Pipliz.Vector3Int.invalidPos)
                        ServerManager.TryChangeBlock(bpi.PreviousPosition, ItemId.GetItemId(bpi.BuilderSchematic.Blocks[prvX, prvY, prvZ].BlockID), new BlockChangeRequestOrigin(job.Owner), ESetBlockFlags.DefaultAudio);

                    var changeResult = ServerManager.TryChangeBlock(iterationType.CurrentPosition, SettlersBuiltIn.ItemTypes.SELECTOR, new BlockChangeRequestOrigin(job.Owner), ESetBlockFlags.DefaultAudio);

                    state.SetCooldown(2);
                    state.SetIndicator(new Shared.IndicatorState(2, foundType.Name));

                    if (changeResult != EServerChangeBlockResult.Success)
                    {
                        if (!_needsChunkLoaded.Contains(bpi))
                            _needsChunkLoaded.Add(bpi);

                        state.SetIndicator(new Shared.IndicatorState(5f, foundType.Name));
                        ChunkQueue.QueuePlayerSurrounding(iterationType.CurrentPosition.ToChunk());
                        break;
                    }

                    if (!bpi.MoveNext())
                    {
                        CleanupJob(iterationType, areaJob, job, bpi, foundType);
                        break;
                    }
                }
                else
                {
                    if (!_needsChunkLoaded.Contains(bpi))
                        _needsChunkLoaded.Add(bpi);

                    ChunkQueue.QueuePlayerSurrounding(iterationType.CurrentPosition.ToChunk());
                    state.SetIndicator(new Shared.IndicatorState(5f, ColonyBuiltIn.ItemTypes.ERRORIDLE.Name));
                    break;
                }
            }
        }

        private static void CleanupJob(IIterationType iterationType, IAreaJob areaJob, ConstructionJobInstance job, ArchitectIterator bpi, ItemTypes.ItemType foundType)
        {
            if (_needsChunkLoaded.Contains(bpi))
                _needsChunkLoaded.Remove(bpi);

            ServerManager.TryChangeBlock(iterationType.CurrentPosition, foundType.ItemIndex, new BlockChangeRequestOrigin(job.Owner), ESetBlockFlags.DefaultAudio);

            // failed to find next position to do job at, self-destruct
            SchematicReader.SaveSchematic(areaJob.Owner, bpi.BuilderSchematic);
            AreaJobTracker.RemoveJob(areaJob);
        }
    }
}
