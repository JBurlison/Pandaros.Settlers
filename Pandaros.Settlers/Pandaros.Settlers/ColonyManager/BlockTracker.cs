using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Pipliz;
using Pipliz.JSON;

namespace Pandaros.Settlers.ColonyManager
{
    public class TrackedPosition : IEquatable<TrackedPosition>, IJsonDeserializable, IJsonSerializable
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Z { get; set; }
        public ushort Id { get; set; }

        public TrackedPosition() { }

        public TrackedPosition(JSONNode node)
        {
            JsonDeerialize(node);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var result = 0;
                result = (result * Id) ^ X;
                result = (result * Id) ^ Y;
                result = (result * Id) ^ Z;
                return result;
            }
        }

        public bool Equals(TrackedPosition other)
        {
            if (other == null)
                return false;

            return other.X == X && other.Y == Y && other.Z == Z;
        }

        public bool Equals(Vector3Int other)
        {
            if (other == null)
                return false;

            return other.x == X && other.y == Y && other.z == Z;
        }

        public Vector3Int GetVector()
        {
            return new Vector3Int(X, Y, Z);
        }

        public override string ToString()
        {
            return X.ToString().PadLeft(10) + Y.ToString().PadLeft(10) + Z.ToString().PadLeft(10) + Id.ToString().PadLeft(5);
        }

        public void JsonDeerialize(JSONNode node)
        {
            if (node.TryGetAs(nameof(X), out int x))
                X = x;

            if (node.TryGetAs(nameof(Y), out int y))
                Y = y;

            if (node.TryGetAs(nameof(Z), out int z))
                Z = z;

            if (node.TryGetAs(nameof(Id), out ushort id))
                Id = id;
        }

        public JSONNode JsonSerialize()
        {
            JSONNode retVal = new JSONNode();
            retVal[nameof(X)] = new JSONNode(X);
            retVal[nameof(Y)] = new JSONNode(Y);
            retVal[nameof(Z)] = new JSONNode(Z);
            retVal[nameof(Id)] = new JSONNode(Id);
            return retVal;
        }
    }

    [ModLoader.ModManager]
    public class BlockTracker
    {
        static QueueFactory<Tuple<Players.Player, TrackedPosition>> _recordPositionFactoryPlayer = new QueueFactory<Tuple<Players.Player, TrackedPosition>>("RecordPositionsPlayer", 1);
        static QueueFactory<Tuple<Colony, TrackedPosition>> _recordPositionFactoryColony = new QueueFactory<Tuple<Colony, TrackedPosition>>("RecordPositionsColony", 1);
        static List<TrackedPosition> _queuedPositions = new List<TrackedPosition>();
        private static readonly byte[] _SOH = new[] { (byte)0x02 };

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnShouldKeepChunkLoaded, GameLoader.NAMESPACE + ".ColonyManager.BlockTracker.OnShouldKeepChunkLoaded")]
        public static void OnShouldKeepChunkLoaded(ChunkUpdating.KeepChunkLoadedData data)
        {
            foreach (var iterator in _queuedPositions)
            {
                if (iterator.GetVector().IsWithinBounds(data.CheckedChunk.Position, data.CheckedChunk.Bounds))
                    data.Result = true;
            }
        }

        static BlockTracker()
        {
            _recordPositionFactoryPlayer.DoWork += _recordPositionFactory_DoWork;
            _recordPositionFactoryColony.DoWork += _recordPositionFactoryColony_DoWork;
            _recordPositionFactoryPlayer.Start();
            _recordPositionFactoryColony.Start();
        }

        public static void RewindPlayersBlocks(Players.Player player)
        {
            Task.Run(() =>
            {
                var saveLoc = string.Concat(GameLoader.SAVE_LOC, "players/", player.ID, "/", "originalBlocks.json");
                var trackedPositions = JSONExtentionMethods.GetSimpleListFromFile<TrackedPosition>(saveLoc);

                if (trackedPositions.Count > 0)
                {
                    foreach (var trackedPos in trackedPositions)
                        if (!_queuedPositions.Contains(trackedPos))
                        {
                            _queuedPositions.Add(trackedPos);
                            ChunkQueue.QueuePlayerRequest(trackedPos.GetVector(), player);
                        }

                    System.Threading.Thread.Sleep(5000);

                    List<TrackedPosition> replaced = new List<TrackedPosition>();

                    foreach (var trackedPos in _queuedPositions)
                        if (World.TryChangeBlock(trackedPos.GetVector(), trackedPos.Id))
                            replaced.Add(trackedPos);

                    foreach (var replace in replaced)
                        _queuedPositions.Remove(replace);


                    File.Delete(saveLoc);
                }
            });
        }


        public static void RewindColonyBlocks(Colony colony)
        {
            Task.Run(() =>
            {
                var saveLoc = string.Concat(GameLoader.SAVE_LOC, "colonies/", colony.Name, "/", "originalBlocks.json");
                var trackedPositions = JSONExtentionMethods.GetSimpleListFromFile<TrackedPosition>(saveLoc);

                if (trackedPositions.Count > 0)
                {
                    foreach (var trackedPos in trackedPositions)
                        if (!_queuedPositions.Contains(trackedPos))
                        {
                            _queuedPositions.Add(trackedPos);
                            ChunkQueue.QueuePlayerRequest(trackedPos.GetVector(), colony.Owners.FirstOrDefault());
                        }

                    System.Threading.Thread.Sleep(5000);

                    List<TrackedPosition> replaced = new List<TrackedPosition>();

                    foreach (var trackedPos in _queuedPositions)
                        if (World.TryChangeBlock(trackedPos.GetVector(), trackedPos.Id))
                            replaced.Add(trackedPos);

                    foreach (var replace in replaced)
                        _queuedPositions.Remove(replace);


                    File.Delete(saveLoc);
                }
            });
        }

        private static void _recordPositionFactoryColony_DoWork(object sender, Tuple<Colony, TrackedPosition> e)
        {
            var saveLoc = string.Concat(GameLoader.SAVE_LOC, "colonies/", e.Item1.Name, "/", "originalBlocks.json");
            var trackedPositions = JSONExtentionMethods.GetSimpleListFromFile<TrackedPosition>(saveLoc);

            if (trackedPositions.Count > 0 && trackedPositions.Contains(e.Item2))
                return;

            trackedPositions.Add(e.Item2);
            JSONExtentionMethods.SaveSimpleListToJson(saveLoc, trackedPositions);
        }

        private static void _recordPositionFactory_DoWork(object sender, Tuple<Players.Player, TrackedPosition> e)
        {
            var saveLoc = string.Concat(GameLoader.SAVE_LOC, "players/", e.Item1.ID, "/", "originalBlocks.json");
            var trackedPositions = JSONExtentionMethods.GetSimpleListFromFile<TrackedPosition>(saveLoc);

            if (trackedPositions.Count > 0 && trackedPositions.Contains(e.Item2))
                return;

            trackedPositions.Add(e.Item2);
            JSONExtentionMethods.SaveSimpleListToJson(saveLoc, trackedPositions);
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnQuitLate, GameLoader.NAMESPACE + ".ColonyManager.BlockTracker.OnQuitLate")]
        public static void OnQuitLate()
        {
            _recordPositionFactoryPlayer.Dispose();
            _recordPositionFactoryColony.Dispose();
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnTryChangeBlock, GameLoader.NAMESPACE + ".ColonyManager.BlockTracker.OnTryChangeBlockUser")]
        public static void OnTryChangeBlockUser(ModLoader.OnTryChangeBlockData d)
        {
            if (d.RequestOrigin.AsPlayer != null ||
                d.RequestOrigin.AsPlayer.ID.type != NetworkID.IDType.Server ||
                d.RequestOrigin.AsPlayer.ID.type != NetworkID.IDType.Invalid)
            {

                _recordPositionFactoryPlayer.Enqueue(new Tuple<Players.Player, TrackedPosition>(d.RequestOrigin.AsPlayer, new TrackedPosition()
                {
                    Id = d.TypeOld.ItemIndex,
                    X = d.Position.x,
                    Y = d.Position.y,
                    Z = d.Position.z
                }));
            }
        }
    }
}
