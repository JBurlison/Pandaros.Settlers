using Pipliz;
using Server.AI;
using Server.Monsters;
using Server.TerrainGeneration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pandaros.Settlers.Monsters
{
    [ModLoader.ModManagerAttribute]
    public class MonsterSpawner //: IMonsterSpawner
    {
        public static ushort MissingMonster_Icon;
        //    private static readonly sbyte[] offsets = new sbyte[]
        //    {
        //        0,
        //        1,
        //        -1,
        //        2,
        //        -2
        //    };

        //    private static double siegeModeCooldown = 3.0;

        //    private Server.Monsters.MonsterSpawner.Variables variables = new Server.Monsters.MonsterSpawner.Variables();

        //    [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterWorldLoad, GameLoader.NAMESPACE + ".monsterspawner.register"), ModLoader.ModCallbackDependsOn("pipliz.monsterspawner.register")]
        //    private static void Register()
        //    {
        //        MonsterTracker.MonsterSpawner = new MonsterSpawner();
        //    }

        //    public void Update()
        //    {
        //        if (World.Initialized && !AIManager.IsBusy())
        //        {
        //            WorldSettings worldSettings = ServerManager.WorldSettings;
        //            bool flag = worldSettings.ZombiesEnabled && (!TimeCycle.IsDay || worldSettings.MonstersDayTime);
        //            int num = (!worldSettings.MonstersDoubled) ? 1 : 2;
        //            double secondsSinceStartDouble = Time.SecondsSinceStartDouble;
        //            var banners = BannerTracker.GetBanners();

        //            for (int i = 0; i < banners.Count; i++)
        //            {
        //                Banner banner = banners.GetValueAtIndex(i);

        //                if (banner == null || !banner.KeyLocation.IsValid)
        //                {
        //                    continue;
        //                }
        //                Colony colony = Colony.Get(banner.Owner);

        //                if (flag)
        //                {
        //                    int zombiesMax = variables.ZombiesPerNPC * colony.FollowerCount * num;
        //                    if (zombiesMax > 0)
        //                    {
        //                        if (MonsterTracker.MonstersPerPlayer(banner.Owner) < zombiesMax)
        //                        {
        //                            if (colony.InSiegeMode)
        //                            {
        //                                if (secondsSinceStartDouble - colony.LastSiegeModeSpawn < siegeModeCooldown)
        //                                {
        //                                    continue;
        //                                }
        //                                else
        //                                {
        //                                    colony.LastSiegeModeSpawn = secondsSinceStartDouble;
        //                                }
        //                            }
        //                            SpawnZombie(colony, banner);
        //                        }
        //                    }
        //                    else
        //                    {
        //                        colony.OnZombieSpawn(true);
        //                    }
        //                }
        //                else
        //                {
        //                    colony.OnZombieSpawn(true);
        //                }
        //            }
        //        }
        //    }

        //    private static void SpawnZombie(Colony colony, Banner bannerGoal)
        //    {
        //        Vector3Int start;
        //        if (MonsterSpawner.TryGetSpawnLocation(bannerGoal, out start))
        //        {
        //            Path path;
        //            if (AIManager.ZombiePathFinder.TryFindPath(start, bannerGoal.KeyLocation, out path) == EPathFindingResult.Success)
        //            {
        //                IMonster monster = new PandaZombie(path, bannerGoal.Owner);
        //                ModLoader.TriggerCallbacks<IMonster>(ModLoader.EModCallbackType.OnMonsterSpawned, monster);
        //                MonsterTracker.Add(monster);
        //                colony.OnZombieSpawn(true);
        //                return;
        //            }
        //        }
        //        colony.OnZombieSpawn(false);
        //    }

        //    private static bool TryGetSpawnLocation(Banner primaryBanner, out Vector3Int positionFinal)
        //    {
        //        Vector3Int keyLocation = primaryBanner.KeyLocation;
        //        var banners = BannerTracker.GetBanners();
        //        positionFinal = Vector3Int.invalidPos;

        //        for (int spawnTry = 0; spawnTry < 40; spawnTry++)
        //        {
        //            Vector3Int possiblePosition;
        //            do
        //            {
        //                possiblePosition.x = keyLocation.x + Pipliz.Random.Next(-primaryBanner.MaxSpawnRadius, primaryBanner.MaxSpawnRadius);
        //                possiblePosition.z = keyLocation.z + Pipliz.Random.Next(-primaryBanner.MaxSpawnRadius, primaryBanner.MaxSpawnRadius);
        //                possiblePosition.y = Pipliz.Math.RoundToInt(TerrainGenerator.UsedGenerator.GetHeight(possiblePosition.x, possiblePosition.z));
        //            } while ((keyLocation - possiblePosition).MaxPartAbs < primaryBanner.SafeRadius);

        //            for (int idxBanner = 0; idxBanner < banners.Count; idxBanner++)
        //            {
        //                var otherBanner = banners.GetValueAtIndex(idxBanner);
        //                Colony otherColony = Colony.Get(otherBanner.Owner);
        //                if ((otherBanner.KeyLocation - possiblePosition).MaxPartAbs < otherBanner.SafeRadius && otherColony.FollowerCount > 0)
        //                {
        //                    goto NEXT_TRY;
        //                }
        //            }

        //            for (int idxOffset = 0; idxOffset < offsets.Length; idxOffset++)
        //            {
        //                Vector3Int positionAITest = possiblePosition.Add(0, offsets[idxOffset], 0);
        //                if (AIManager.Loaded(positionAITest))
        //                {
        //                    if (AIManager.CanStandAt(positionAITest))
        //                    {
        //                        positionFinal = positionAITest;
        //                        return true;
        //                    }
        //                }
        //                else
        //                {
        //                    goto NEXT_TRY;
        //                }
        //            }

        //            NEXT_TRY:
        //            continue;
        //        }

        //        return false;
        //    }
    }
}