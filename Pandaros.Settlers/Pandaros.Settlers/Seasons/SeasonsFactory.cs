using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Pipliz;
using Pipliz.Collections.Threadsafe;
using Math = System.Math;

namespace Pandaros.Settlers.Seasons
{
   // [ModLoader.ModManagerAttribute]
    public static class SeasonsFactory
    {
        private const int CYCLE_SECONDS = 2;
        private const int CHUNKS_PER_CYCLE = 1000;
        private static readonly int _totalChunks = 20000;
        private static int _numberOfCycles = 20;
        private static int _currentMax = CHUNKS_PER_CYCLE;
        private static int _currentMin;
        private static int _currentDayInSeason = 0;
        private static readonly int _daysBetweenSeasonChanges = 5;
        private static int _previousSesion;
        private static int _currentSeason;
        private static int _nextSeason = 1;
        private static double _nextUpdate = TimeCycle.TotalTime + 10;
        private static double _nextCycleTime;
        private static readonly List<ISeason> _seasons = new List<ISeason>();

        public static ISeason CurrentSeason => _seasons[_currentSeason];

        public static ISeason NextSeason => _seasons[_nextSeason];

        public static ISeason PreviousSeason => _seasons[_previousSesion];

        [ModLoader.ModCallbackAttribute(ModLoader.EModCallbackType.AfterWorldLoad,
            GameLoader.NAMESPACE + ".Seasons.SeasonsFactory.AfterWorldLoad"), 
        ModLoader.ModCallbackDependsOn(GameLoader.NAMESPACE + ".Gameloader.SettlersExtender.AfterWorldLoad")]
        public static void AfterWorldLoad()
        {
            var worldChunks = GetWorldChunks();
            _numberOfCycles = (int) Math.Ceiling((double) worldChunks.Count / CHUNKS_PER_CYCLE);
            _previousSesion = _seasons.Count - 1;
            ChunkUpdating.PlayerLoadedMaxRange = ChunkUpdating.PlayerLoadedMaxRange * 3;
            ChunkUpdating.PlayerLoadedMinRange = ChunkUpdating.PlayerLoadedMaxRange;
        }

        [ModLoader.ModCallbackAttribute(ModLoader.EModCallbackType.OnUpdate,
            GameLoader.NAMESPACE + ".Seasons.SeasonsFactory.ChangeSeasons")]
        public static void ChangeSeasons()
        {
            if (Time.SecondsSinceStartDouble > _nextCycleTime)
            {
                var worldChunks = GetWorldChunks();
                var i           = 0;

                worldChunks.ForeachValue(c =>
                {
                    if (i >= _currentMin && i < _currentMax)
                    {
                        c.LockWriteData();

                        try
                        {
                            if (c.DataState != Chunk.ChunkDataState.DataFull)
                                return;
                            
                            var data     = c.Data;
                            var didThing = false;

                            for (var d = 0; d < 4096; d++)
                            {
                                var existingType = data[d];

                                foreach (var type in BlockTypeRegistry.Mappings)
                                    if ((type.Value.Contains(existingType) &&
                                        NextSeason.SeasonalBlocks.ContainsKey(type.Key)) ||
                                        (PreviousSeason.SeasonalBlocks.ContainsKey(type.Key) &&
                                         PreviousSeason.SeasonalBlocks[type.Key] == existingType))
                                    {
                                        var localPos = new Vector3Byte(d);

                                        data = data.Set(localPos, NextSeason.SeasonalBlocks[type.Key]);

                                        if (!didThing)
                                        {
                                            didThing  = true;
                                            c.IsDirty = true;
                                        }

                                        ServerManager.SendBlockChange(localPos.ToWorld(c.Position),
                                                                      NextSeason.SeasonalBlocks[type.Key]);
                                        break;
                                    }
                            }

                            if (didThing)
                                c.Data = data;
                        }
                        finally
                        {
                            c.UnlockWriteData();
                        }
                    }

                    i++;
                });

                _currentMax    += CHUNKS_PER_CYCLE;
                _currentMin    += CHUNKS_PER_CYCLE;
                _nextCycleTime =  Time.SecondsSinceStartDouble + CYCLE_SECONDS;

                if (_currentMax > _totalChunks)
                {
                    _currentMax = CHUNKS_PER_CYCLE;
                    _currentMin = 0;
                }

                if (TimeCycle.TotalTime > _nextUpdate)
                {
                    _currentSeason = _nextSeason;

                    _nextSeason++;

                    if (_nextSeason >= _seasons.Count)
                    {
                        _nextSeason     = 0;
                        _previousSesion = _seasons.Count - 1;
                    }
                    else
                        _previousSesion = _nextSeason - 1;

                    _currentMax    = CHUNKS_PER_CYCLE;
                    _currentMin    = 0;
                    _nextCycleTime = 0;
                    _nextUpdate    = TimeCycle.TotalTime + (24 * _daysBetweenSeasonChanges);
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

                    if (afterSeason != null)
                    {
                        var indexOfSeason = _seasons.IndexOf(afterSeason);
                        var newIndex      = indexOfSeason - 1;

                        if (newIndex < 0)
                            newIndex = 0;

                        if (newIndex <= _seasons.Count)
                            _seasons.Insert(newIndex, season);
                        else
                            _seasons.Add(season);
                    }
                    else
                    {
                        _seasons.Add(season);
                    }
                }
                else
                {
                    _seasons.Add(season);
                }
            }
        }

        public static void ResortSeasons()
        {
            for (var j = 0; j < _seasons.Count; j++)
            for (var i = 0; i < _seasons.Count - 1; i++)
            {
                var s = _seasons[i];
                _seasons.Remove(s);
                AddSeason(s);
            }

            var sb = new StringBuilder();
            sb.Append("Season order: ");

            for (var j = 0; j < _seasons.Count; j++)
            {
                sb.Append(_seasons[j].Name);

                if (j != _seasons.Count - 1)
                    sb.Append(", ");
            }

            PandaLogger.Log(sb.ToString());
        }

        private static ThreadedDictionary<Vector3Int, Chunk> GetWorldChunks()
        {
            return (ThreadedDictionary<Vector3Int, Chunk>) typeof(World)
                                                          .GetField("chunks",
                                                                    BindingFlags.NonPublic | BindingFlags.Static)
                                                         ?.GetValue(null);
        }
    }
}