using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Pipliz;

namespace Pandaros.Settlers.ColonyManager
{
    public class TrackedPosition : IEquatable<TrackedPosition>
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Z { get; set; }
        public ushort Id { get; set; }

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
    }

    [ModLoader.ModManager]
    public class BlockTracker
    {
        static QueueFactory<Tuple<Players.Player, TrackedPosition>> _recordPositionFactory = new QueueFactory<Tuple<Players.Player, TrackedPosition>>("RecordPositions");
        static Dictionary<Vector3Int, ushort> _queuedPositions = new Dictionary<Vector3Int, ushort>();


        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnShouldKeepChunkLoaded, GameLoader.NAMESPACE + ".ColonyManager.BlockTracker.OnShouldKeepChunkLoaded")]
        public static void OnShouldKeepChunkLoaded(ChunkUpdating.KeepChunkLoadedData data)
        {
            foreach (var iterator in _queuedPositions.Keys)
            {
                if (iterator.IsWithinBounds(data.CheckedChunk.Position, data.CheckedChunk.Bounds))
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
                var saveLoc = GameLoader.SAVE_LOC + "players/" + player.ID + "/";

                if (Directory.Exists(saveLoc))
                {
                    saveLoc += "originalBlocks.bin";

                    using (var fileStream = new FileStream(saveLoc, FileMode.OpenOrCreate, FileAccess.Read))
                    {
                        var buffLength = 35;
                        var buffIndex = 0;

                        while (fileStream.Length > buffIndex + buffLength)
                        {
                            try
                            {
                                var buffAll = new byte[buffLength];
                                var count = fileStream.Read(buffAll, buffIndex, buffLength);

                                if (count == 0)
                                    break;

                                buffIndex += count;
                                var x = new byte[10];
                                var y = new byte[10];
                                var z = new byte[10];
                                var id = new byte[5];

                                Array.Copy(buffAll, 0, x, 0, 10);
                                Array.Copy(buffAll, 10, y, 0, 10);
                                Array.Copy(buffAll, 20, z, 0, 10);
                                Array.Copy(buffAll, 30, id, 0, 5);

                                if (int.TryParse(ASCIIEncoding.ASCII.GetString(x), out int intX) &&
                                    int.TryParse(ASCIIEncoding.ASCII.GetString(y), out int intY) &&
                                    int.TryParse(ASCIIEncoding.ASCII.GetString(z), out int intZ) &&
                                    ushort.TryParse(ASCIIEncoding.ASCII.GetString(id), out ushort intId))
                                {
                                    var pos = new Vector3Int(intX, intY, intZ);
                                    _queuedPositions[pos] = intId;
                                    ChunkQueue.QueuePlayerRequest(pos, player);
                                }
                            }
                            catch (Exception ex)
                            {
                                PandaLogger.Log("File Length: {0} Index: {1}", fileStream.Length, buffIndex);
                                PandaLogger.LogError(ex);

                                System.Threading.Thread.Sleep(1000);
                            }
                        }

                        System.Threading.Thread.Sleep(5000);

                        List<Vector3Int> replaced = new List<Vector3Int>();

                        foreach (var posKvp in _queuedPositions)
                            if (World.TryChangeBlock(posKvp.Key, posKvp.Value))
                                replaced.Add(posKvp.Key);

                        foreach (var replace in replaced)
                            _queuedPositions.Remove(replace);
                    }

                    File.Delete(saveLoc);
                }
            });
        }

        private static void _recordPositionFactory_DoWork(object sender, Tuple<Players.Player, TrackedPosition> e)
        {
            var saveLoc = GameLoader.SAVE_LOC + "players/" + e.Item1.ID + "/";

            if (!Directory.Exists(saveLoc))
                Directory.CreateDirectory(saveLoc);

            saveLoc += "originalBlocks.bin";
 
            using (var fileStream = new FileStream(saveLoc, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite))
            {
                var buffLength = 35;
                var buffIndex = 0;
               
                while (fileStream.Length > buffIndex)
                {
                    var buffAll = new byte[buffLength];
                    var count = fileStream.Read(buffAll, buffIndex, buffLength);

                    if (count == 0)
                        break;

                    buffIndex += count;
                    var x = new byte[10];
                    var y = new byte[10];
                    var z = new byte[10];
                    var id = new byte[5];

                    Array.Copy(buffAll, 0, x, 0, 10);
                    Array.Copy(buffAll, 10, y, 0, 10);
                    Array.Copy(buffAll, 20, z, 0, 10);
                    Array.Copy(buffAll, 30, id, 0, 5);

                    if (int.TryParse(ASCIIEncoding.ASCII.GetString(x), out int outInt) && e.Item2.X == outInt &&
                        int.TryParse(ASCIIEncoding.ASCII.GetString(y), out outInt) && e.Item2.Y == outInt &&
                        int.TryParse(ASCIIEncoding.ASCII.GetString(z), out outInt) && e.Item2.Z == outInt)
                        return;
                }

                fileStream.Seek(0, SeekOrigin.End);

                fileStream.Write(Encoding.ASCII.GetBytes(e.Item2.X.ToString().PadLeft(10, '0')), 0, 10);
                fileStream.Write(Encoding.ASCII.GetBytes(e.Item2.X.ToString().PadLeft(10, '0')), 0, 10);
                fileStream.Write(Encoding.ASCII.GetBytes(e.Item2.X.ToString().PadLeft(10, '0')), 0, 10);
                fileStream.Write(Encoding.ASCII.GetBytes(e.Item2.Id.ToString().PadLeft(5, '0')), 0, 5);
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
            if (d.RequestOrigin.AsPlayer == null ||
                d.RequestOrigin.AsPlayer.ID.type == NetworkID.IDType.Server ||
                d.RequestOrigin.AsPlayer.ID.type == NetworkID.IDType.Invalid)
                return;

            _recordPositionFactory.Enqueue(new Tuple<Players.Player, TrackedPosition>(d.RequestOrigin.AsPlayer, new TrackedPosition()
            {
                Id = d.TypeOld.ItemIndex,
                X = d.Position.x,
                Y = d.Position.y,
                Z = d.Position.z
            }));
        }
    }
}
