using Pandaros.Settlers.Items.Armor;
using Pandaros.Settlers.Items.Temperature;
using Pandaros.Settlers.Items.Weapons;
using Pipliz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TerrainGeneration;
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
        private const int CYCLE_SECONDS = 2000;
        private const int RefreshCycleTimeMinMS = 10000;
        private const int RefreshCycleTimeMaxMS = 20000;
        private static readonly int _daysBetweenSeasonChanges = 10;
        private static int _previousSesion;
        private static int _currentSeason;
        private static int _nextSeason = 1;
        private static double _nextUpdate = 0;
        private static double _tempUpdate = 0;
        private static double _midDay;
        private static double _midNight;
        private static readonly List<ISeason> _seasons = new List<ISeason>();
        private static HashSet<Chunk> _updatedChunks = new HashSet<Chunk>();
        private static Dictionary<Vector3Int, double> _temps = new Dictionary<Vector3Int, double>();

        public static ISeason CurrentSeason => _seasons[_currentSeason];

        public static ISeason NextSeason => _seasons[_nextSeason];

        public static ISeason PreviousSeason => _seasons[_previousSesion];

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
            var chuck = World.GetChunk(npc.Position.ToChunk());
            double effectiveTemp = ((TerrainGenerator)ServerManager.TerrainGenerator).QueryData(chuck.Position.x, chuck.Position.z).Temperature;

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
            _previousSesion = _seasons.Count - 1;
            var timeToMidDay = TimeCycle.DayLength / 2;
            var timeToMidNight = TimeCycle.NightLength / 2;
            _midDay = TimeCycle.SunRise + timeToMidDay;
            _midNight = TimeCycle.SunSet + timeToMidNight;

            _nextUpdate = TimeCycle.TotalTime + (24 * _daysBetweenSeasonChanges);
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnShouldKeepChunkLoaded, GameLoader.NAMESPACE + ".Seasons.SeasonsFactory.Process")]
        [ModLoader.ModCallbackDependsOn("bannercheck")] // after banner check so that it doesn't increase our delay-to-next-cooldown by 5x
        static void Process(ChunkUpdating.KeepChunkLoadedData data)
        {
            if (!data.Result)
            {
                if (data.CheckedChunk.Data.IsAir)  // don't keep extra air chunks loaded (won't be replacing air)
                    return;

                // up the keep-loaded-range to include all send chunks
                for (int i = 0; i < Players.CountConnected; i++)
                {
                    Players.Player player = Players.GetConnectedByIndex(i);

                    if (player != null && (player.VoxelPosition - data.CheckedChunk.Position).MaxPartAbs <= player.DrawDistance + 24)
                    {
                        data.Result = true;
                        break;
                    }
                }
            }

            if (data.Result) // set the time-till-next-callback to our refresh cycle time
                data.MillisecondsTillNextCheck = Pipliz.Random.Next(RefreshCycleTimeMinMS, RefreshCycleTimeMaxMS);

            if (_updatedChunks.Add(data.CheckedChunk))
            {
                
                // unseen chunk since season switch
                bool didChange = false;
                bool lockedAlready = data.ChunkLoadedSource != ChunkUpdating.KeepChunkLoadedData.EChunkLoadedSource.Updater;
                data.CheckedChunk.ForeachDataReplace(ChangeSeason, ref didChange, lockedAlready);
                if (didChange)
                {
                    Pipliz.Vector3Int position = data.CheckedChunk.Position.Add(8, 8, 8);
                    if (data.ChunkLoadedSource == ChunkUpdating.KeepChunkLoadedData.EChunkLoadedSource.Updater)
                    {
                        // updater won't send the change by itself, so manually send it
                        data.CheckedChunk.LockReadData();
                        try
                        {
                            Players.SendToNearbyDrawDistance(position, data.CheckedChunk.GetNetworkPacket(), 200000);
                        }
                        finally
                        {
                            data.CheckedChunk.UnlockReadData();
                        }
                    }
                    else
                    {
                        // generator or savegame loader source; they may already have a player queued up and will send it to that. Add all connected players nearby to that list.
                        int connectedPlayers = Players.CountConnected;
                        for (int i = 0; i < connectedPlayers; i++)
                        {
                            Players.Player player = Players.GetConnectedByIndex(i);
                            if (player != null && (player.VoxelPosition - position).MaxPartAbs < player.DrawDistance + 24)
                            {
                                data.CheckedChunk.AddReceivingPlayer(player);
                            }
                        }
                    }
                    data.CheckedChunk.IsDirty = true;
                }
            }
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnUpdate, GameLoader.NAMESPACE + ".Seasons.SeasonsFactory.Update")]
        public static void Update()
        {
            if (!World.Initialized)
                return;

            if (Time.SecondsSinceStartDouble > _tempUpdate)
                try
                {
                    _tempUpdate = Time.SecondsSinceStartDouble + 60;
                    _temps.Clear();

                    if (TimeCycle.TotalTime > _nextUpdate)
                    {
                        _currentSeason = _nextSeason;
                        _nextSeason++;

                        if (_nextSeason >= _seasons.Count)
                        {
                            _nextSeason = 0;
                            _previousSesion = _seasons.Count - 1;
                        }
                        else
                            _previousSesion = _nextSeason - 1;

                        _updatedChunks.Clear();
                        _nextUpdate = TimeCycle.TotalTime + (24 * _daysBetweenSeasonChanges);
                        PandaChat.SendToAll($"The season in now {CurrentSeason.Name}", ChatColor.olive, ChatStyle.bold);
                        PandaLogger.Log(ChatColor.olive, $"The season in now {CurrentSeason.Name}");
                    }
                }
                catch (Exception ex)
                {
                    PandaLogger.LogError(ex);
                }
        }

        private static ushort ChangeSeason(ref Chunk.DataIteration iteration, ref bool didChange)
        {
            ushort retVal = iteration.DataType;

            try
            {
                if (ServerManager.TerrainGenerator is ITerrainGenerator generator)
                {
                    var query = ((TerrainGenerator)ServerManager.TerrainGenerator).QueryData(iteration.Chunk.Position.x, iteration.Chunk.Position.z);

                    foreach (var type in BlockTypeRegistry.Mappings)
                        if (type.Value.Contains(iteration.DataType) &&
                            PreviousSeason.SeasonalBlocks.ContainsKey(type.Key) &&
                            CurrentSeason.SeasonalBlocks[type.Key].ContainsKey(query.Biome.TopBlockType) &&
                            !CurrentSeason.SeasonalBlocks[type.Key][query.Biome.TopBlockType].Contains(iteration.DataType))
                        {
                            retVal = CurrentSeason.SeasonalBlocks[type.Key][query.Biome.TopBlockType].GetRandomItem();
                            didChange = true;
                            break;
                        }
                }
            }
            catch (Exception ex)
            {
                PandaLogger.LogError(ex);
            }

            return retVal;
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

        // TODO: Fix midnight and noon temps.
        /// <summary>
        ///     Get temp based on time of day. The closer to noon the hotter, The closer to midnight the cooler.
        /// </summary>
        /// <returns></returns>
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