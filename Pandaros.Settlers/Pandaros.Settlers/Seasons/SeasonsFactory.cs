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
        public const double COMFORTABLE_TEMP_MIN = 16.66;
        public const double COMFORTABLE_TEMP_MAX = 27.77;
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
        public static double MidDay { get; set; }
        public static double MidNight { get; set; }
        private static readonly List<ISeason> _seasons = new List<ISeason>();
        private static HashSet<Chunk> _updatedChunks = new HashSet<Chunk>();

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
            var timeToMidDay = TimeCycle.TotalDayLength.Value.TotalHours / 2;
            var timeToMidNight = TimeCycle.TotalDayLength.Value.TotalHours / 2;
            MidDay = TimeCycle.SunRise + timeToMidDay;
            MidNight = TimeCycle.SunSet + timeToMidNight;

            _nextUpdate = TimeCycle.TotalTime.Value.Days + _daysBetweenSeasonChanges;
            ((TerrainGenerator)ServerManager.TerrainGenerator).TemperatureProvider = new PandaTemperatureProvider(((TerrainGenerator)ServerManager.TerrainGenerator).TemperatureProvider);
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnShouldKeepChunkLoaded, GameLoader.NAMESPACE + ".Seasons.SeasonsFactory.Process")]
        [ModLoader.ModCallbackDependsOn("bannercheck")] // after banner check so that it doesn't increase our delay-to-next-cooldown by 5x
        static void Process(ChunkUpdating.KeepChunkLoadedData data)
        {
            if (!data.Result)
            {
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
                data.CheckedChunk.ForeachDataReplaceLocked(ChangeSeason, ref didChange);
                if (didChange)
                {
                    Pipliz.Vector3Int position = data.CheckedChunk.Position.Add(8, 8, 8);
                    if (data.ChunkLoadedSource == ChunkUpdating.KeepChunkLoadedData.EChunkLoadedSource.Updater)
                    {
                        // updater won't send the change by itself, so manually send it
                        data.CheckedChunk.LockRead();
                        try
                        {
                            Players.SendToNearbyDrawDistance(position, data.CheckedChunk.GetNetworkPacket(), 200000);
                        }
                        finally
                        {
                            data.CheckedChunk.UnlockRead();
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

                    if (TimeCycle.TotalTime.Value.Days > _nextUpdate)
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
                        _nextUpdate = TimeCycle.TotalTime.Value.Days + _daysBetweenSeasonChanges;
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
            var query = ((TerrainGenerator)ServerManager.TerrainGenerator).QueryData(iteration.Chunk.Position.x, iteration.Chunk.Position.z);

            try
            {
                if (ServerManager.TerrainGenerator is ITerrainGenerator generator)
                {
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
    }
}