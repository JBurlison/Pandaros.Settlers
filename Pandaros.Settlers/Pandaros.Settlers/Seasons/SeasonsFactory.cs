using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Pandaros.Settlers.Items;
using Pandaros.Settlers.Items.Temperature;
using Pandaros.Settlers.Items.Weapons;
using Pipliz;
using Pipliz.Collections.Threadsafe;
using Math = System.Math;

namespace Pandaros.Settlers.Seasons
{
    public enum TemperatureScale
    {
        Celcius,
        Ferinheight
    }

    public enum ComfortLevel
    {
        TooHot,
        TooCold,
        JustRight
    }

    [ModLoader.ModManager]
    public static class SeasonsFactory
    {
        public const double COMFORTABLE_TEMP_MIN = 62;
        public const double COMFORTABLE_TEMP_MAX = 82;
        public const string DEGREE_SYMBOL = "°";
        private const int CYCLE_SECONDS = 2;
        private const int CHUNKS_PER_CYCLE = 100;
        private static int _currentMax = CHUNKS_PER_CYCLE;
        private static int _currentMin;
        private static readonly int _daysBetweenSeasonChanges = 10;
        private static int _previousSesion;
        private static int _currentSeason;
        private static int _nextSeason = 1;
        private static double _nextUpdate = TimeCycle.TotalTime + 10;
        private static double _nextCycleTime;
        private static double _midDay;
        private static double _midNight;
        private static readonly List<ISeason> _seasons = new List<ISeason>();
        
        public static ISeason CurrentSeason => _seasons[_currentSeason];

        public static ISeason NextSeason => _seasons[_nextSeason];

        public static ISeason PreviousSeason => _seasons[_previousSesion];

        /// <summary>
        ///     in Fahrenheit
        /// </summary>
        public static double Temperature { get; private set; }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnShouldKeepChunkLoaded, GameLoader.NAMESPACE + ".Seasons.SeasonsFactory.OnShouldKeepChunkLoaded")]
        public static void OnShouldKeepChunkLoaded(ChunkUpdating.KeepChunkLoadedData chunkCallback)
        {
            chunkCallback.Result = true;
        }

        public static double ConvertCelsiusToFahrenheit(double c)
        {
            return ((9.0 / 5.0) * c) + 32;
        }

        public static double ConvertFahrenheitToCelsius(double f)
        {
            return (5.0 / 9.0) * (f - 32);
        }

        public static ComfortLevel GetComfortLevel(NPC.NPCBase npc)
        {
            var inv = Entities.SettlerInventory.GetSettlerInventory(npc);
            double effectiveTemp = Temperature;

            foreach (var item in inv.Armor)
            {
                if (GetComfortLevel(effectiveTemp) == ComfortLevel.JustRight)
                    break;

                if (!item.Value.IsEmpty() && ArmorFactory.ArmorLookup.ContainsKey(item.Value.Id))
                {
                    var armor = ArmorFactory.ArmorLookup[item.Value.Id];

                    if (armor != null && armor.TemperatureRegulated != TemperatureType.None)
                        effectiveTemp = FactorTempChange(effectiveTemp, armor);
                }
            }

            if (GetComfortLevel(effectiveTemp) != ComfortLevel.JustRight && 
                !inv.Weapon.IsEmpty() && 
                WeaponFactory.WeaponLookup.ContainsKey(inv.Weapon.Id))
            {
                var weapon = WeaponFactory.WeaponLookup[inv.Weapon.Id];

                if (weapon != null && weapon.TemperatureRegulated != TemperatureType.None)
                    effectiveTemp = FactorTempChange(effectiveTemp, weapon);
            }

            return GetComfortLevel(effectiveTemp);
        }

        public static ComfortLevel GetComfortLevel(double temp)
        {
            if (COMFORTABLE_TEMP_MIN <= temp)
                return ComfortLevel.TooCold;

            if (COMFORTABLE_TEMP_MAX >= temp)
                return ComfortLevel.TooHot;

            return ComfortLevel.JustRight;
        }

        public static double FactorTempChange(double currentTemp, ITemperatureRegulator regulator)
        {
            if (GetComfortLevel(currentTemp) == ComfortLevel.JustRight)
                return currentTemp;

            if (regulator.TemperatureRegulated == TemperatureType.Cold || 
                regulator.TemperatureRegulated == TemperatureType.Both)
            {
                if (currentTemp < COMFORTABLE_TEMP_MIN)
                    currentTemp += regulator.TemperatureAdjusted;


                if (currentTemp > COMFORTABLE_TEMP_MAX)
                    currentTemp = COMFORTABLE_TEMP_MAX;
            }

            if (regulator.TemperatureRegulated == TemperatureType.Heat ||
                regulator.TemperatureRegulated == TemperatureType.Both)
            {
                if (currentTemp > COMFORTABLE_TEMP_MAX)
                    currentTemp -= regulator.TemperatureAdjusted;

                if (currentTemp < COMFORTABLE_TEMP_MIN)
                    currentTemp = COMFORTABLE_TEMP_MIN;
            }


            return currentTemp;
        }


        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterWorldLoad, GameLoader.NAMESPACE + ".Seasons.SeasonsFactory.AfterWorldLoad"), 
        ModLoader.ModCallbackDependsOn(GameLoader.NAMESPACE + ".Extender.SettlersExtender.AfterWorldLoad")]
        public static void AfterWorldLoad()
        {
            var worldChunks = GetWorldChunks();
            _previousSesion = _seasons.Count - 1;
            var timeToMidDay = TimeCycle.DayLength / 2;
            var timeToMidNight = TimeCycle.NightLength / 2;
            _midDay = TimeCycle.SunRise + timeToMidDay;
            _midNight = TimeCycle.SunSet + timeToMidNight;
            ChunkUpdating.PlayerLoadedMaxRange = ChunkUpdating.PlayerLoadedMaxRange * 3;
            ChunkUpdating.PlayerLoadedMinRange = ChunkUpdating.PlayerLoadedMaxRange;
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnUpdate, GameLoader.NAMESPACE + ".Seasons.SeasonsFactory.ChangeSeasons")]
        public static void ChangeSeasons()
        {
            if (World.Initialized && Time.SecondsSinceStartDouble > _nextCycleTime)
            {
                var worldChunks = GetWorldChunks();
                var i = 0;

                Temperature = GetTemprature();

                worldChunks.ForeachValue(c =>
                {
                    if (i >= _currentMin && i < _currentMax)
                        ChangeSeason(c);

                    if (i > _currentMax)
                        return;

                    i++;
                });

                _currentMax += CHUNKS_PER_CYCLE;
                _currentMin += CHUNKS_PER_CYCLE;
                _nextCycleTime = Time.SecondsSinceStartDouble + CYCLE_SECONDS;

                if (_currentMax > worldChunks.Count)
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
                    PandaChat.SendToAll($"The season in now {CurrentSeason.Name}", ChatColor.olive, ChatStyle.bold);
                    PandaLogger.Log(ChatColor.olive, $"The season in now {CurrentSeason.Name}");
                }
            }
        }

        private static void ChangeSeason(Chunk c)
        {
            try
            {
                if (c.DataState != Chunk.ChunkDataState.DataFull)
                    return;

                var data = c.Data;
                var didThing = false;
                c.LockWriteData();


                for (var d = 0; d < 4096; d++)
                {
                    var existingType = data[d];

                    foreach (var type in BlockTypeRegistry.Mappings)
                        if (type.Value.Contains(existingType) &&
                            PreviousSeason.SeasonalBlocks.ContainsKey(type.Key) &&
                            CurrentSeason.SeasonalBlocks[type.Key] != existingType)
                        {
                            var localPos = new Vector3Byte(d);

                            data = data.Set(localPos, CurrentSeason.SeasonalBlocks[type.Key]);

                            if (!didThing)
                            {
                                didThing = true;
                                c.IsDirty = true;
                            }

                            ServerManager.SendBlockChange(localPos.ToWorld(c.Position),
                                                          CurrentSeason.SeasonalBlocks[type.Key]);
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

            PandaLogger.Log(ChatColor.lime, sb.ToString());
        }

        private static ThreadedDictionary<Vector3Int, Chunk> GetWorldChunks()
        {
            return (ThreadedDictionary<Vector3Int, Chunk>) typeof(World)
                                                          .GetField("chunks",
                                                                    BindingFlags.NonPublic | BindingFlags.Static)
                                                         ?.GetValue(null);
        }

        private static double GetTemprature()
        {
            double retVal = 75;

            if (TimeCycle.IsDay)
            {
                var tempDiff = CurrentSeason.MaxDayTemperature - CurrentSeason.MinDayTemperature;

                if (TimeCycle.TimeOfDay <= _midDay)
                {
                    var pct = TimeCycle.TimeOfDay / _midDay;
                    retVal = tempDiff * pct;
                }
                else
                {
                    var pct = TimeCycle.TimeOfDay / TimeCycle.SunSet;
                    retVal = tempDiff * pct;
                }

                retVal += CurrentSeason.MinDayTemperature;
            }
            else
            {
                var tempDiff = CurrentSeason.MaxDayTemperature - CurrentSeason.MinDayTemperature;

                if (TimeCycle.TimeOfDay <= _midNight)
                {
                    var pct = TimeCycle.TimeOfDay / _midNight;
                    retVal = tempDiff * pct;
                }
                else
                {
                    var pct = TimeCycle.TimeOfDay / TimeCycle.SunRise;
                    retVal = tempDiff * pct;
                }

                retVal = CurrentSeason.MaxNightTemperature - retVal;
            }

            return Math.Round(retVal, 2);
        }
    }
}