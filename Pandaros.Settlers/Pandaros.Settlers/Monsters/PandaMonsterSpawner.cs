using General.Settings;
using NPC;
using Pipliz;
using Server.AI;
using Server.Monsters;
using Server.NPCs;
using Server.TerrainGeneration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Pandaros.Settlers.Monsters
{
    [ModLoader.ModManager]
    public class PandaMonsterSpawner : IMonsterSpawner
    {
        public class MonsterWeight
        {
            // The higher the multiplier the more red zombies.
            // Maybe used for difficulty.
            public const int MULTIPLIER = 6;

            public double Power { get; set; }
            public double TotalPower { get; set; }

            private Dictionary<int, double> _powerPerCount = new Dictionary<int, double>();
            private Dictionary<int, double> _weightPerCount = new Dictionary<int, double>();
            private Dictionary<int, double> _totalWeightPerCount = new Dictionary<int, double>();

            public MonsterWeight(double power)
            {
                Power = power;
            }

            public double SpawnChance(int colonistCount, float maxMonsters)
            {
                if (!_powerPerCount.ContainsKey(colonistCount))
                {
                    double totalweight = 0;

                    foreach (var monster in Monsters)
                        totalweight += monster.Value.CalcWeight(colonistCount);

                    foreach (var monster in Monsters)
                        monster.Value.SetTotalWeight(colonistCount, totalweight);
                    
                    var pct = maxMonsters / (_totalWeightPerCount[colonistCount] / _weightPerCount[colonistCount]);
                    
                    // Give the edge to harder monsters the more colonists you have.
                    if (colonistCount / 25 > 0)
                    {
                        var bump = colonistCount % 25;
                        var add = (Power * bump) / _weightPerCount[colonistCount];
                        pct += pct * add * MULTIPLIER;
                    }

                    _powerPerCount[colonistCount] = pct;
                }

                return _powerPerCount[colonistCount];
            }

            public void SetTotalWeight(int colonistCount, double weight)
            {
                _totalWeightPerCount[colonistCount] = weight;
            }

            public double CalcWeight(int colonistCount)
            {
                if (!_weightPerCount.ContainsKey(colonistCount))
                    _weightPerCount[colonistCount] = (TotalPower / Power) * colonistCount;

                return _weightPerCount[colonistCount];
            }
        }

        private static readonly sbyte[] offsets = new sbyte[]
        {
            0,
            1,
            -1,
            2,
            -2
        };

        private static double siegeModeCooldown = 3.0;
        const int MAX_TRIES = 40;
        private MonsterSpawner.Variables variables = new MonsterSpawner.Variables();

        public static Dictionary<NPCType, MonsterWeight> Monsters { get; private set; } = new Dictionary<NPCType, MonsterWeight>();

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterWorldLoad, GameLoader.NAMESPACE + ".Monsters.PandaMonsterSpawner.Register"), ModLoader.ModCallbackDependsOn("pipliz.server.monsterspawner.register")]
        private static void Register()
        {
            MonsterTracker.MonsterSpawner = new PandaMonsterSpawner();
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterWorldLoad, GameLoader.NAMESPACE + ".Monsters.PandaMonsterSpawner.Fetch"), ModLoader.ModCallbackDependsOn("pipliz.server.monsterspawner.fetchnpctypes")]
        private static void Fetch()
        {
            var monsters = new[] {
               MonsterSpawner.MonsterCC,
               MonsterSpawner.MonsterCB,
               MonsterSpawner.MonsterCA,
               MonsterSpawner.MonsterBC,
               MonsterSpawner.MonsterBB,
               MonsterSpawner.MonsterBA,
               MonsterSpawner.MonsterAC,
               MonsterSpawner.MonsterAB,
               MonsterSpawner.MonsterAA,
            };

            double powerTotal = 0;

            foreach (var key in monsters)
                if (key.TryGetSettings<NPCTypeMonsterSettings>(out var settings))
                    Monsters[key] = GetMonsterWeight(settings);

            foreach (var mp in Monsters)
                powerTotal += mp.Value.Power;

            foreach (var monster in Monsters.OrderBy(mkvp => mkvp.Value.Power))
                monster.Value.TotalPower = powerTotal;

            Monsters = Monsters.OrderBy(mkvp => mkvp.Value.Power).ToDictionary(x => x.Key, x => x.Value);
        }

        public static MonsterWeight GetMonsterWeight(NPCTypeMonsterSettings settings)
        {
            return new MonsterWeight((settings.initialHealth / 10) + (settings.punchDamage * MonsterWeight.MULTIPLIER) + (settings.punchCooldownMS / 100) + (settings.movementSpeed * 10));
        }

        public float GetMaxZombieCount(float colonistCount)
        {
            float t = Mathf.InverseLerp(0f, this.variables.ZombiesPerNPCSecondaryAtCount, colonistCount);
            return Pipliz.Math.Lerp(this.variables.ZombiesPerNPC, this.variables.ZombiesPerNPCSecondary, t) * colonistCount;
        }

        public void Update()
        {
            if (!World.Initialized || AIManager.IsBusy())
            {
                return;
            }

            WorldSettings wsettings = ServerManager.WorldSettings;
            bool toSpawnZombies = wsettings.ZombiesEnabled && (TimeCycle.ShouldSpawnMonsters || wsettings.MonstersDayTime);
            int monsterMultiplier = wsettings.MonstersDoubled ? 2 : 1;
            double timeSinceStart = Pipliz.Time.SecondsSinceStartDouble;

            var banners = BannerTracker.GetBanners();
            for (int i = 0; i < banners.Count; i++)
            {
                Banner banner = banners.GetValueAtIndex(i);

                if (banner == null || !banner.KeyLocation.IsValid)
                    continue;

                Colony colony = Colony.Get(banner.Owner);

                if (toSpawnZombies)
                {
                    float zombiesMax = GetMaxZombieCount(colony.FollowerCount) * monsterMultiplier;

                    if (zombiesMax > 0f)
                    {
                        if (MonsterTracker.MonstersPerPlayer(banner.Owner) < zombiesMax)
                        {
                            if (colony.InSiegeMode)
                            {
                                if (timeSinceStart - colony.LastSiegeModeSpawn < siegeModeCooldown)
                                {
                                    continue;
                                }
                                else
                                {
                                    colony.LastSiegeModeSpawn = timeSinceStart;
                                }
                            }

                            SpawnZombie(colony, banner, GetMosterType(colony.FollowerCount, zombiesMax));
                        }
                    }
                    else
                    {
                        colony.OnZombieSpawn(true);
                    }
                }
                else
                {
                    colony.OnZombieSpawn(true);
                }
            }
        }

        private NPCType GetMosterType(int followerCount, float maxMonsters)
        {
            Dictionary<NPCType, double> spawnchances = new Dictionary<NPCType, double>();
            double maxChance = 0;
            double minChance = 0;

            foreach (var mtKvp in Monsters)
            {
                var chance = mtKvp.Value.SpawnChance(followerCount, maxMonsters);

                if (chance > 2)
                {
                    spawnchances[mtKvp.Key] = chance;

                    if (chance > maxChance)
                        maxChance = chance + (chance * .10);

                    if (chance < minChance)
                        minChance = chance - (chance * .01);
                }
            }
            
            if (spawnchances.Count > 0)
            {
                var roll = Pipliz.Random.NextDouble(minChance, maxChance);
                var winner = spawnchances.FirstOrDefault(sc => sc.Value <= roll).Key;

                if (!winner.IsValid)
                    winner = spawnchances.First().Key;

                return winner;
            }
            else
                return Monsters.First().Key; // spawn the weakest monster.
        }

        public static void SpawnZombie(Colony colony, Banner bannerGoal, NPCType typeToSpawn)
        {
            float maxSpawnWalkDistance = 500f;

            if (TimeCycle.ShouldSpawnMonsters && typeToSpawn.TryGetSettings(out NPCTypeMonsterSettings nPCTypeMonsterSettings))
                maxSpawnWalkDistance = TimeCycle.NightLengthLeftInRealSeconds * (nPCTypeMonsterSettings.movementSpeed * 0.85f) + (float)bannerGoal.SafeRadius;

            switch (TryGetSpawnLocation(bannerGoal, maxSpawnWalkDistance, out Vector3Int start))
            {
                case MonsterSpawner.ESpawnResult.Success:
                    {
                        Path path;
                        if (AIManager.ZombiePathFinder.TryFindPath(start, bannerGoal.KeyLocation, out path, 2000000000) == EPathFindingResult.Success)
                        {
                            IMonster monster = new Zombie(typeToSpawn, path, bannerGoal.Owner);
                            ModLoader.TriggerCallbacks<IMonster>(ModLoader.EModCallbackType.OnMonsterSpawned, monster);
                            MonsterTracker.Add(monster);
                            colony.OnZombieSpawn(true);
                        }
                        else
                            colony.OnZombieSpawn(false);

                        break;
                    }
                case MonsterSpawner.ESpawnResult.NotLoaded:
                    colony.OnZombieSpawn(true);
                    break;
                case MonsterSpawner.ESpawnResult.Fail:
                    colony.OnZombieSpawn(false);
                    break;
            }
        }

        public static MonsterSpawner.ESpawnResult TryGetSpawnLocation(Banner primaryBanner, float maxSpawnWalkDistance, out Vector3Int positionFinal)
        {
            Vector3Int bannerPos = primaryBanner.KeyLocation;
            var bannerList = BannerTracker.GetBanners();
            int safeRadius = primaryBanner.SafeRadius;
            int spawnRadius = Pipliz.Math.RoundToInt(Pipliz.Math.Min(maxSpawnWalkDistance, primaryBanner.MaxSpawnRadius));

            positionFinal = Vector3Int.invalidPos;

            for (int spawnTry = 0; spawnTry < MAX_TRIES; spawnTry++)
            {
                Vector3Int possiblePosition;
                Vector3Int dif;

                while (true)
                {
                    possiblePosition.x = bannerPos.x + Pipliz.Random.Next(-spawnRadius, spawnRadius);
                    possiblePosition.z = bannerPos.z + Pipliz.Random.Next(-spawnRadius, spawnRadius);
                    possiblePosition.y = Pipliz.Math.RoundToInt(TerrainGenerator.UsedGenerator.GetHeight(possiblePosition.x, possiblePosition.z));

                    dif = bannerPos - possiblePosition;

                    if (dif.MaxPartAbs > safeRadius && Pipliz.Math.Abs(dif.x) + Pipliz.Math.Abs(dif.z) < spawnRadius)
                        break;
                }

                for (int idxBanner = 0; idxBanner < bannerList.Count; idxBanner++)
                {
                    var otherBanner = bannerList.GetValueAtIndex(idxBanner);
                    Colony otherColony = Colony.Get(otherBanner.Owner);

                    if (otherColony.FollowerCount > 0 && (otherBanner.KeyLocation - possiblePosition).MaxPartAbs <= otherBanner.SafeRadius)
                        goto NEXT_TRY;
                }

                if (!AIManager.Loaded(possiblePosition))
                    return MonsterSpawner.ESpawnResult.NotLoaded;

                for (int idxOffset = 0; idxOffset < offsets.Length; idxOffset++)
                {
                    Vector3Int positionAITest = possiblePosition.Add(0, offsets[idxOffset], 0);

                    if (AIManager.CanStandAt(positionAITest))
                    {
                        positionFinal = positionAITest;
                        return MonsterSpawner.ESpawnResult.Success;
                    }
                }

                NEXT_TRY:
                continue;
            }

            return MonsterSpawner.ESpawnResult.Fail;
        }
    }
}
