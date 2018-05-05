using Pandaros.Settlers.Entities;
using Pandaros.Settlers.Items.Machines;
using Pandaros.Settlers.Monsters.Bosses;
using Server.Monsters;
using Server.NPCs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Pandaros.Settlers.Managers
{
    [ModLoader.ModManager]
    public static class MonsterManager
    {
        private static Stopwatch _maxTimePerTick = new Stopwatch();
        static double _nextUpdateTime;
        static int _nextBossUpdateTime = int.MaxValue;
        public static bool BossActive { get; private set; } = false;
        public static int MinBossSpawnTimeSeconds { get { return Configuration.GetorDefault(nameof(MinBossSpawnTimeSeconds), 900); } } // 15 minutes
        public static int MaxBossSpawnTimeSeconds { get { return Configuration.GetorDefault(nameof(MaxBossSpawnTimeSeconds), 1800); } } // 1/2 hour
        private static Dictionary<PlayerState, IPandaBoss> _spawnedBosses = new Dictionary<PlayerState, IPandaBoss>();
        private static List<IPandaBoss> _bossList = new List<IPandaBoss>();

        private static int _boss = -1;
        public static event EventHandler<Monsters.BossSpawnedEvent> BossSpawned;

        public static void AddBoss(IPandaBoss m)
        {
            lock (_bossList)
                _bossList.Add(m);
        }

        private static IPandaBoss GetMonsterType()
        {
            IPandaBoss t = null;

            lock (_bossList)
            {
                var rand = _boss;

                while(rand == _boss)
                    rand = Pipliz.Random.Next(0, _bossList.Count);

                t = _bossList[rand];
                _boss = rand;
            }

            return t;
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnUpdate, GameLoader.NAMESPACE + ".Managers.MonsterManager.Update")]
        public static void OnUpdate()
        {
            if (!World.Initialized || !ServerManager.WorldSettings.ZombiesEnabled || Server.AI.AIManager.IsBusy())
                return;

            double secondsSinceStartDouble = Pipliz.Time.SecondsSinceStartDouble;

            if (_nextUpdateTime < secondsSinceStartDouble)
            {
                IMonster m = null;

                foreach (var monster in GetAllMonsters())
                {
                    if (m == null || (Vector3.Distance(monster.Value.Position, m.Position) > 10 && Pipliz.Random.NextBool()))
                    {
                        m = monster.Value;
                        ServerManager.SendAudio(monster.Value.Position, GameLoader.NAMESPACE + "ZombieAudio");
                    }
                }

                _nextUpdateTime = secondsSinceStartDouble + 5;
            }

            IPandaBoss bossType = null;

            if (World.Initialized &&
                ServerManager.WorldSettings.ZombiesEnabled &&
                !Server.AI.AIManager.IsBusy())
            {
                _maxTimePerTick.Reset();
                _maxTimePerTick.Start();

                if (!BossActive &&
                    _nextBossUpdateTime < secondsSinceStartDouble)
                {
                    BossActive = true;
                    bossType = GetMonsterType();

                    if (Players.CountConnected != 0)
                        PandaLogger.Log(ChatColor.yellow, $"Boss Active! Boss is: {bossType.Name}");
                }

                if (BossActive)
                {
                    bool turnOffBoss = true;
                    WorldSettings worldSettings = ServerManager.WorldSettings;
                    float num = (!worldSettings.MonstersDoubled) ? 1 : 2;
                    var banners = BannerTracker.GetCount();

                    for (int i = 0; i < banners; i++)
                    {
                        if (_maxTimePerTick.Elapsed.TotalMilliseconds > Monsters.PandaMonsterSpawner.MonsterVariables.MSPerTick)
                            break;

                        if (BannerTracker.TryGetAtIndex(i, out var bannerGoal))
                        {
                            var ps = PlayerState.GetPlayerState(bannerGoal.Owner);
                            var colony = Colony.Get(ps.Player);

                            if (ps.BossesEnabled &&
                                ps.Player.IsConnected &&
                                colony.FollowerCount > Configuration.GetorDefault("MinColonistsCountForBosses", 15))
                            {
                                if (bossType != null &&
                                    !_spawnedBosses.ContainsKey(ps) &&
                                    MonsterSpawner.TryGetSpawnLocation(bannerGoal, 300f, out var start) == MonsterSpawner.ESpawnResult.Success &&
                                    Server.AI.AIManager.ZombiePathFinder.TryFindPath(start, bannerGoal.KeyLocation, out var path, 2000000000) == Server.AI.EPathFindingResult.Success)
                                {
                                    var pandaboss = bossType.GetNewBoss(path, ps.Player);
                                    _spawnedBosses.Add(ps, pandaboss);

                                    BossSpawned?.Invoke(MonsterTracker.MonsterSpawner, new Monsters.BossSpawnedEvent(ps, pandaboss));
                                    ModLoader.TriggerCallbacks<IMonster>(ModLoader.EModCallbackType.OnMonsterSpawned, pandaboss);

                                    MonsterTracker.Add(pandaboss);
                                    colony.OnZombieSpawn(true);

                                    PandaChat.Send(ps.Player, $"[{pandaboss.Name}] {pandaboss.AnnouncementText}", ChatColor.red);

                                    if (!string.IsNullOrEmpty(pandaboss.AnnouncementAudio))
                                        ServerManager.SendAudio(ps.Player.Position, pandaboss.AnnouncementAudio);
                                }

                                if (_spawnedBosses.ContainsKey(ps) &&
                                    _spawnedBosses[ps].IsValid &&
                                    _spawnedBosses[ps].CurrentHealth > 0)
                                {
                                    if (ps.Player.GetTempValues(true).GetOrDefault("BossIndicator", 0) < Pipliz.Time.SecondsSinceStartInt)
                                    {
                                        Server.Indicator.SendIconIndicatorNear(new Pipliz.Vector3Int(_spawnedBosses[ps].Position), _spawnedBosses[ps].ID, new Shared.IndicatorState(1, GameLoader.Poisoned_Icon, false, false));
                                        ps.Player.GetTempValues(true).Set("BossIndicator", Pipliz.Time.SecondsSinceStartInt + 1);
                                    }

                                    if (_spawnedBosses[ps].ZombieMultiplier != 0)
                                        num *= _spawnedBosses[ps].ZombieMultiplier;

                                    Monsters.PandaMonsterSpawner.Instance.SpawnForBanner(bannerGoal, true, num, secondsSinceStartDouble, true, _spawnedBosses[ps]);
                                    turnOffBoss = false;
                                }
                            }
                        }
                    }
                    
                    if (turnOffBoss)
                    {
                        if (Players.CountConnected != 0 && _spawnedBosses.Count != 0)
                        {
                            PandaLogger.Log(ChatColor.yellow, $"All bosses cleared!");
                            var boss = _spawnedBosses.FirstOrDefault().Value;
                            PandaChat.SendToAll($"[{boss.Name}] {boss.DeathText}", ChatColor.red);
                        }

                        BossActive = false;
                        _spawnedBosses.Clear();
                        GetNextBossSpawnTime();
                    }
                }

                _maxTimePerTick.Stop();
            }
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterWorldLoad, GameLoader.NAMESPACE + ".Managers.MonsterManager.AfterWorldLoad")]
        public static void AfterWorldLoad()
        {
            GetNextBossSpawnTime();
            _maxTimePerTick.Start();
        }
        
        private static void GetNextBossSpawnTime()
        {
            _nextBossUpdateTime = Pipliz.Time.SecondsSinceStartInt + Pipliz.Random.Next(MinBossSpawnTimeSeconds, MaxBossSpawnTimeSeconds);
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnPlayerHit, GameLoader.NAMESPACE + ".Managers.MonsterManager.OnPlayerHit")]
        public static void OnPlayerHit(Players.Player player, ModLoader.OnHitData d)
        {
            if (d.ResultDamage > 0 && d.HitSourceType == ModLoader.OnHitData.EHitSourceType.Monster)
            {
                var state = PlayerState.GetPlayerState(player);
                d.ResultDamage += state.Difficulty.MonsterDamage;
            }
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnNPCHit, GameLoader.NAMESPACE + ".Managers.MonsterManager.OnNPCHit")]
        public static void OnNPCHit(NPC.NPCBase npc, ModLoader.OnHitData d)
        {
            if (d.ResultDamage > 0 && d.HitSourceType == ModLoader.OnHitData.EHitSourceType.Monster) 
            {
                var state = PlayerState.GetPlayerState(npc.Colony.Owner);
                d.ResultDamage += state.Difficulty.MonsterDamage;
            }
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnMonsterHit, GameLoader.NAMESPACE + ".Managers.MonsterManager.OnMonsterHit")]
        public static void OnMonsterHit(IMonster monster, ModLoader.OnHitData d)
        {
            var ps = PlayerState.GetPlayerState(monster.OriginalGoal);
            var pandaArmor = monster as IPandaArmor;
            var turret = d.HitSourceObject as IPandaDamage;

            if (pandaArmor != null && Pipliz.Random.NextFloat() <= pandaArmor.MissChance)
            {
                d.ResultDamage = 0;
                return;
            }

            if (pandaArmor != null && 
                turret != null)
            {
                var damage = 0f;

                foreach (var dt in turret.Damage)
                {
                    var tmpDmg = dt.Key.CalcDamage(pandaArmor.ElementalArmor, dt.Value);

                    if (pandaArmor.AdditionalResistance.TryGetValue(dt.Key, out var flatResist))
                        tmpDmg = tmpDmg - (tmpDmg * flatResist);

                    damage += tmpDmg;
                }

                d.ResultDamage = damage;
            }
            else if (pandaArmor != null)
            {
                d.ResultDamage = DamageType.Physical.CalcDamage(pandaArmor.ElementalArmor, d.ResultDamage);

                if (pandaArmor.AdditionalResistance.TryGetValue(DamageType.Physical, out var flatResist))
                    d.ResultDamage = d.ResultDamage - (d.ResultDamage * flatResist);
            }

            d.ResultDamage = d.ResultDamage - (d.ResultDamage * ps.Difficulty.MonsterDamageReduction);

            if (Pipliz.Random.NextFloat() > .5f)
                ServerManager.SendAudio(monster.Position, GameLoader.NAMESPACE + "ZombieAudio");
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnMonsterDied, GameLoader.NAMESPACE)]
        public static void MonsterDied(IMonster monster)
        {
            var rewardMonster = monster as IKillReward;
            if (rewardMonster != null && rewardMonster.OriginalGoal.IsConnected)
            {
                var stockpile = Stockpile.GetStockPile(rewardMonster.OriginalGoal);

                foreach (var reward in rewardMonster.KillRewards)
                {
                    stockpile.Add(reward.Key, reward.Value);

                    if (ItemTypes.TryGetType(reward.Key, out var item))
                        PandaChat.Send(rewardMonster.OriginalGoal, $"You have been awarded {reward.Value}x {item.Name}!", ChatColor.orange);
                }
            }
        }

        public static Dictionary<int, IMonster> GetAllMonsters()
        {
            return typeof(MonsterTracker).GetField("allMonsters", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic).GetValue(null) as Dictionary<int, IMonster>;
        }
    }
}
