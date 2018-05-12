using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Pipliz;
using Pipliz.Collections.Threadsafe;
using Math = Pipliz.Math;

namespace Pandaros.Settlers.Seasons
{
    [ModLoader.ModManager]
    public static class SeasonsFactory
    {
        private const int CYCLE_SECONDS = 10;
        private const int CHUNKS_PER_CYCLE = 1000;
        private static readonly int _totalChunks = 20000;
        private static int _numberOfCycles = 20;
        private static int _currentMax = CHUNKS_PER_CYCLE;
        private static int _currentMin;
        private static int _currentDayInSeason = 0;
        private static int _daysBetweenSeasonChanges = 5;
        private static int _currentSeason = 0;
        private static int _nextSeason = 1;
        private static double _nextUpdate = TimeCycle.TotalTime + 10;
        private static double _nextCycleTime;
        private static readonly List<ISeason> _seasons = new List<ISeason>();

        public static ISeason CurrentSeason => _seasons[_currentSeason];

        public static ISeason NextSeason => _seasons[_nextSeason];

        [ModLoader.ModCallbackAttribute(ModLoader.EModCallbackType.AfterWorldLoad, GameLoader.NAMESPACE + ".Seasons.SeasonsFactory.AfterWorldLoad")]
        public static void AfterWorldLoad()
        {
            var worldChunks = GetWorldChunks();
            _numberOfCycles = (int)System.Math.Ceiling((double)worldChunks.Count / CHUNKS_PER_CYCLE);
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnUpdate, GameLoader.NAMESPACE + ".Seasons.SeasonsFactory.ChangeSeasons")]
        public static void ChangeSeasons()
        {
            if (TimeCycle.TotalTime > _nextUpdate && Time.SecondsSinceStartDouble > _nextCycleTime)
            {
                PandaLogger.Log(ChatColor.silver, "Time to change seasons!");

                var worldChunks = GetWorldChunks();
                var i           = 0;

                worldChunks.ForeachValue(c =>
                {
                    if (i >= _currentMin && i < _currentMax && c.Data is Chunk.ChunkDataFull)
                    {
                        var bounds = c.Bounds;
                        PandaLogger.Log("Chunk Found");

                        for (var x = 0; x <= 15; x++)
                        for (var y = 0; y <= 15; y++)
                        for (var z = 0; z <= 15; z++)
                            try
                            {
                                foreach (var type in BlockTypeRegistry.Mappings)
                                    if (type.Value.Contains(c.Data[x, y, z]) &&
                                        NextSeason.SeasonalBlocks.ContainsKey(type.Key))
                                    {
                                        c.Data = c.Data.Set(new Vector3Byte(x, y, z), NextSeason.SeasonalBlocks[type.Key]);
                                        //chunkData[x, y, z] = NextSeason.SeasonalBlocks[type.Key];
                                        PandaLogger.Log("Block Updated!");
                                        break;
                                    }

                            }
                            catch (Exception ex)
                            {
                                        PandaLogger.LogError(ex);
                            }

                        for (int p = 0; p < Players.CountConnected; p++)
                            c.AddReceivingPlayer(Players.GetConnectedByIndex(p));

                        c.SendToReceivingPlayers();
                    }

                    i++;
                });

                _currentMax    += CHUNKS_PER_CYCLE;
                _currentMin    += CHUNKS_PER_CYCLE;
                _nextCycleTime =  Time.SecondsSinceStartDouble + CYCLE_SECONDS;

                if (_currentMax > _totalChunks)
                {
                    _currentSeason = _nextSeason;

                    _nextSeason++;

                    if (_nextSeason >= _seasons.Count)
                        _nextSeason = 0;

                    _currentMax    = CHUNKS_PER_CYCLE;
                    _currentMin    = 0;
                    _nextCycleTime = 0;
                    _nextUpdate = TimeCycle.TotalTime + (24 * _daysBetweenSeasonChanges);
                }
            }
        }
        
        public static void AddSeason(ISeason season)
        {
            if (season != null)
            {
                if (!string.IsNullOrEmpty(season.SeasonAfter))
                {
                    var afterSeason = _seasons.FirstOrDefault(s => s.Name == season.SeasonAfter);
                    var indexOfSeason = _seasons.IndexOf(afterSeason);
                    var newIndex = indexOfSeason - 1;

                    if (newIndex < 0)
                        newIndex = 0;

                    if (newIndex <= _seasons.Count)
                        _seasons.Insert(newIndex, season);
                    else
                        _seasons.Add(season);
                }
                else
                    _seasons.Add(season);
            }
        }

        public static void ResortSeasons()
        {
            for (int j = 0; j < _seasons.Count; j++)
            for (int i = 0; i < _seasons.Count - 1; i++)
            {
                var s = _seasons[i];
                _seasons.Remove(s);
                AddSeason(s);
            }

            var sb = new StringBuilder();
            sb.Append("Season order: ");

            for (int j = 0; j < _seasons.Count; j++)
            {
                sb.Append(_seasons[j].Name);

                if (j != _seasons.Count - 1)
                    sb.Append(", ");
            }

            PandaLogger.Log(sb.ToString());
        }

        private static ThreadedDictionary<Vector3Int, Chunk> GetWorldChunks()
        {
            return (ThreadedDictionary<Vector3Int, Chunk>) typeof(World).GetField("chunks", BindingFlags.NonPublic | BindingFlags.Static)?.GetValue(null);
        }
    }
}