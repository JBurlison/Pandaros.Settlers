using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pandaros.Settlers.Models;
using Pipliz;
using Pipliz.JSON;

namespace Pandaros.Settlers.ColonyManager
{
    [ModLoader.ModManager]
    public class BlockTracker
    {
        static QueueFactory<TrackedPosition> _recordPositionFactory = new QueueFactory<TrackedPosition>("RecordPositions", 1);
        static List<TrackedPosition> _queuedPositions = new List<TrackedPosition>();
        private static readonly byte[] _SOH = new[] { (byte)0x02 };

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnShouldKeepChunkLoaded, GameLoader.NAMESPACE + ".ColonyManager.BlockTracker.OnShouldKeepChunkLoaded")]
        public static void OnShouldKeepChunkLoaded(ChunkUpdating.KeepChunkLoadedData data)
        {
            lock(_queuedPositions)
            foreach (var iterator in _queuedPositions)
            {
                if (iterator.GetVector().IsWithinBounds(data.CheckedChunk.Position, data.CheckedChunk.Bounds))
                    data.Result = true;
            }
        }

        static BlockTracker()
        {
            _recordPositionFactory.DoWork += _recordPositionFactory_DoWork;
            _recordPositionFactory.Start();
        }

        public static void RewindPlayersBlocks(Players.Player player)
        {
            Task.Run(() =>
            {
                using (TrackedPositionContext db = new TrackedPositionContext())
                {
                    if (db.Positions.Any())
                    {
                        foreach (var trackedPos in db.Positions.Where(p => p.PlayerId == player.ID.ToString()))
                        {
                            var oldest = db.Positions.Where(o => o.X == trackedPos.X && o.Y == trackedPos.Y && o.Z == trackedPos.Z).OrderBy(tp => tp.TimeTracked).FirstOrDefault();

                            if (!_queuedPositions.Any(pos => pos.Equals(oldest)))
                            {
                                lock (_queuedPositions)
                                    _queuedPositions.Add(oldest);

                                ChunkQueue.QueuePlayerRequest(oldest.GetVector().ToChunk(), player);
                            }
                        }

                        System.Threading.Thread.Sleep(5000);

                        List<TrackedPosition> replaced = new List<TrackedPosition>();

                        foreach (var trackedPos in _queuedPositions)
                            if (ServerManager.TryChangeBlock(trackedPos.GetVector(), (ushort)trackedPos.BlockId) == EServerChangeBlockResult.Success)
                                replaced.Add(trackedPos);

                        lock (_queuedPositions)
                            foreach (var replace in replaced)
                                _queuedPositions.Remove(replace);

                        db.Positions.RemoveRange(db.Positions.Where(p => p.PlayerId == player.ID.ToString()));
                        db.SaveChanges();
                    }
                }
            });
        }


        public static void RewindColonyBlocks(Colony colony)
        {
            Task.Run(() =>
            {
                using (TrackedPositionContext db = new TrackedPositionContext())
                {
                    if (db.Positions.Any())
                    {
                        foreach (var trackedPos in db.Positions.Where(p => p.ColonyId == colony.Name))
                        {
                            var oldest = db.Positions.Where(o => o.X == trackedPos.X && o.Y == trackedPos.Y && o.Z == trackedPos.Z).OrderBy(tp => tp.TimeTracked).FirstOrDefault();

                            if (!_queuedPositions.Any(pos => pos.Equals(oldest)))
                            {
                                lock (_queuedPositions)
                                    _queuedPositions.Add(oldest);

                                ChunkQueue.QueuePlayerRequest(oldest.GetVector().ToChunk(), colony.Owners.FirstOrDefault());
                            }
                        }

                        System.Threading.Thread.Sleep(5000);

                        List<TrackedPosition> replaced = new List<TrackedPosition>();

                        foreach (var trackedPos in _queuedPositions)
                            if (ServerManager.TryChangeBlock(trackedPos.GetVector(), (ushort)trackedPos.BlockId) == EServerChangeBlockResult.Success)
                                replaced.Add(trackedPos);

                        lock (_queuedPositions)
                            foreach (var replace in replaced)
                                _queuedPositions.Remove(replace);

                        db.Positions.RemoveRange(db.Positions.Where(p => p.ColonyId == colony.Name));
                        db.SaveChanges();
                    }
                }
            });
        }


        private static void _recordPositionFactory_DoWork(object sender, TrackedPosition pos)
        {
            try
            {
                using (TrackedPositionContext db = new TrackedPositionContext())
                {
                    db.Positions.Add(pos);
                    db.SaveChanges();
                }
            }
            catch (DbEntityValidationException e)
            {
                foreach (var eve in e.EntityValidationErrors)
                {
                    PandaLogger.Log(ChatColor.red, "Entity of type \"{0}\" in state \"{1}\" has the following validation errors:", eve.Entry.Entity.GetType().Name, eve.Entry.State);

                    foreach (var ve in eve.ValidationErrors)
                        PandaLogger.Log(ChatColor.red, "- Property: \"{0}\", Error: \"{1}\"", ve.PropertyName, ve.ErrorMessage);
                }
                throw;
            }
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnQuitLate, GameLoader.NAMESPACE + ".ColonyManager.BlockTracker.OnQuitLate")]
        public static void OnQuitLate()
        {
            _recordPositionFactory.Dispose();
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnTryChangeBlock, GameLoader.NAMESPACE + ".ColonyManager.BlockTracker.OnTryChangeBlockUser")]
        public static void OnTryChangeBlockUser(ModLoader.OnTryChangeBlockData d)
        {
            if (d.RequestOrigin.AsPlayer != null &&
                d.RequestOrigin.AsPlayer.ID.type != NetworkID.IDType.Server &&
                d.RequestOrigin.AsPlayer.ID.type != NetworkID.IDType.Invalid)
            {
                _recordPositionFactory.Enqueue(new TrackedPosition()
                {
                    BlockId = d.TypeOld.ItemIndex,
                    X = d.Position.x,
                    Y = d.Position.y,
                    Z = d.Position.z,
                    TimeTracked = DateTime.UtcNow,
                    PlayerId = d.RequestOrigin.AsPlayer.ID.ToString()
                });
            }
            else if (d.RequestOrigin.AsColony != null)
            {
                _recordPositionFactory.Enqueue(new TrackedPosition()
                {
                    BlockId = d.TypeOld.ItemIndex,
                    X = d.Position.x,
                    Y = d.Position.y,
                    Z = d.Position.z,
                    TimeTracked = DateTime.UtcNow,
                    ColonyId = d.RequestOrigin.AsColony.Name
                });
            }

        }
    }
}
