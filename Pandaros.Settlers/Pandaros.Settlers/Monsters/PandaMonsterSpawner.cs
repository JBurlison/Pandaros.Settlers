using General.Settings;
using NPC;
using Pandaros.Settlers.Entities;
using Pipliz;
using Server.AI;
using Server.Monsters;
using Server.NPCs;
using Server.TerrainGeneration;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Pandaros.Settlers.Monsters
{
    [ModLoader.ModManager]
    public class PandaMonsterSpawner : MonsterSpawner
    {
        NPCType[] _monsterTypes = new NPCType[100];

        private Stopwatch maxTimePerTick = new Stopwatch();
        private Variables variables = new Variables();
        private static double siegeModeCooldown = 3.0;

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterWorldLoad, GameLoader.NAMESPACE + ".Monsters.PandaMonsterSpawner.Fetch"), 
            ModLoader.ModCallbackDependsOn("pipliz.server.monsterspawner.fetchnpctypes")]
        private static void Fetch()
        {

        }
        
        public new void Update()
        {
            Update(false);
        }

        public void Update(bool force)
        {
            if (!ServerManager.WorldSettings.ZombiesEnabled || !World.Initialized || AIManager.IsBusy())
                return;

            WorldSettings worldSettings = ServerManager.WorldSettings;

            bool flag = worldSettings.ZombiesEnabled && (TimeCycle.ShouldSpawnMonsters || worldSettings.MonstersDayTime);
            int num = (!worldSettings.MonstersDoubled) ? 1 : 2;
            double secondsSinceStartDouble = Pipliz.Time.SecondsSinceStartDouble;
            var banners = BannerTracker.GetBanners();

            maxTimePerTick.Reset();
            maxTimePerTick.Start();

            for (int i = 0; i < banners.Count; i++)
            {
                if (maxTimePerTick.Elapsed.TotalMilliseconds > variables.MSPerTick)
                    break;

                Banner valueAtIndex = banners.GetValueAtIndex(i);
                var ps = PlayerState.GetPlayerState(valueAtIndex.Owner);
                
                if (valueAtIndex != null && valueAtIndex.KeyLocation.IsValid)
                {
                    Colony colony = Colony.Get(valueAtIndex.Owner);

                    if (flag || (ps.Difficulty != GameDifficulty.Normal && force))
                    {
                        float num2 = GetMaxZombieCount(colony.FollowerCount) * num;

                        if (num2 > 0f)
                        {
                            if (MonsterTracker.MonstersPerPlayer(valueAtIndex.Owner) < num2)
                            {
                                if (colony.InSiegeMode)
                                {
                                    if (secondsSinceStartDouble - colony.LastSiegeModeSpawn < siegeModeCooldown)
                                        continue;
                         
                                    colony.LastSiegeModeSpawn = secondsSinceStartDouble;
                                }

                                SpawnZombie(colony, valueAtIndex);
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

            maxTimePerTick.Stop();
        }
        
    }
}
