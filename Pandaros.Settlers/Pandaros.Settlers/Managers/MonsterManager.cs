using NPC;
using Pandaros.Settlers.Entities;
using Pandaros.Settlers.Monsters;
using Pandaros.Settlers.Monsters.Bosses;
using Pipliz;
using Server;
using Server.AI;
using Server.Monsters;
using Shared;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Random = Pipliz.Random;
using Time = Pipliz.Time;

namespace Pandaros.Settlers.Managers
{
    [ModLoader.ModManager]
    public static class MonsterManager
    {
        private static double _nextUpdateTime;
        private static int _nextBossUpdateTime = int.MaxValue;

        private static readonly Dictionary<PlayerState, IPandaBoss> _spawnedBosses =
            new Dictionary<PlayerState, IPandaBoss>();

        private static readonly List<IPandaBoss> _bossList = new List<IPandaBoss>();

        private static int _boss = -1;
        public static bool BossActive { get; private set; }

        public static int MinBossSpawnTimeSeconds // 15 minutes
            => Configuration.GetorDefault(nameof(MinBossSpawnTimeSeconds), 900);

        public static int MaxBossSpawnTimeSeconds // 1/2 hour
            => Configuration.GetorDefault(nameof(MaxBossSpawnTimeSeconds), 1800);

        public static event EventHandler<BossSpawnedEvent> BossSpawned;

        public static void AddBoss(IPandaBoss m)
        {
            lock (_bossList)
            {
                _bossList.Add(m);
            }
        }

        private static IPandaBoss GetMonsterType()
        {
            IPandaBoss t = null;

            lock (_bossList)
            {
                var rand = _boss;

                while (rand == _boss)
                    rand = Random.Next(0, _bossList.Count);

                t     = _bossList[rand];
                _boss = rand;
            }

            return t;
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnUpdate,
            GameLoader.NAMESPACE + ".Managers.MonsterManager.Update")]
        public static void OnUpdate()
        {
            if (!World.Initialized || AIManager.IsBusy())
                return;

            var secondsSinceStartDouble = Time.SecondsSinceStartDouble;

            if (_nextUpdateTime < secondsSinceStartDouble)
            {
                IMonster m = null;

                foreach (var monster in GetAllMonsters())
                    if (m == null || Vector3.Distance(monster.Value.Position, m.Position) > 15 && Random.NextBool())
                    {
                        m = monster.Value;
                        ServerManager.SendAudio(monster.Value.Position, GameLoader.NAMESPACE + ".ZombieAudio");
                    }

                _nextUpdateTime = secondsSinceStartDouble + 5;
            }

            IPandaBoss bossType = null;

            if (World.Initialized &&
                !AIManager.IsBusy())
            {
                if (!BossActive &&
                    _nextBossUpdateTime <= secondsSinceStartDouble)
                {
                    BossActive = true;
                    bossType   = GetMonsterType();

                    if (Players.CountConnected != 0)
                        PandaLogger.Log(ChatColor.yellow, $"Boss Active! Boss is: {bossType.Name}");
                }

                if (BossActive)
                {
                    var   turnOffBoss   = true;
                    var   worldSettings = ServerManager.WorldSettings;
  
                    Dictionary<PlayerState, List<Banner>> banners = new Dictionary<PlayerState, List<Banner>>();
                    var spawnBanners = new List<Banner>();

                    for (var i = 0; i < BannerTracker.GetCount(); i++)
                        if (BannerTracker.TryGetAtIndex(i, out var newBanner))
                        { 
                            var bps = PlayerState.GetPlayerState(newBanner.Owner);

                            if (!banners.ContainsKey(bps))
                                banners.Add(bps, new List<Banner>());

                            banners[bps].Add(newBanner);
                        }


                    foreach (var bkvp in banners)
                    {
                        if (bkvp.Value.Count > 1)
                        {
                            var next = Pipliz.Random.Next(bkvp.Value.Count);
                            spawnBanners.Add(bkvp.Value[next]);
                        }
                        else if (bkvp.Value.Count == 1)
                            spawnBanners.Add(bkvp.Value[0]);
                    }

                    foreach (var bannerGoal in spawnBanners)
                    {
                        var ps = PlayerState.GetPlayerState(bannerGoal.Owner);
                        var colony = Colony.Get(ps.Player);

                        if (ps.BossesEnabled &&
                            ps.Player.IsConnected &&
                            colony.FollowerCount > Configuration.GetorDefault("MinColonistsCountForBosses", 15))
                        {
                            if (bossType != null &&
                                !_spawnedBosses.ContainsKey(ps))
                            {
                                Vector3Int positionFinal;
                                switch (MonsterSpawner.TryGetSpawnLocation(bannerGoal, 500f, out positionFinal))
                                {
                                    case MonsterSpawner.ESpawnResult.Success:
                                        if (AIManager.ZombiePathFinder.TryFindPath(positionFinal, bannerGoal.KeyLocation, out var path, 2000000000) == EPathFindingResult.Success)
                                        {
                                            var pandaboss = bossType.GetNewBoss(path, ps.Player);
                                            _spawnedBosses.Add(ps, pandaboss);

                                            BossSpawned?.Invoke(MonsterTracker.MonsterSpawner,
                                                                new BossSpawnedEvent(ps, pandaboss));

                                            ModLoader.TriggerCallbacks<IMonster>(ModLoader.EModCallbackType.OnMonsterSpawned,
                                                                                    pandaboss);

                                            MonsterTracker.Add(pandaboss);
                                            colony.OnZombieSpawn(true);
                                            ps.FaiedBossSpawns = 0;

                                            PandaChat.Send(ps.Player, $"[{pandaboss.Name}] {pandaboss.AnnouncementText}",
                                                            ChatColor.red);

                                            if (!string.IsNullOrEmpty(pandaboss.AnnouncementAudio))
                                                ServerManager.SendAudio(ps.Player.Position, pandaboss.AnnouncementAudio);
                                        }

                                        break;
                                    case MonsterSpawner.ESpawnResult.NotLoaded:
                                    case MonsterSpawner.ESpawnResult.Impossible:
                                        colony.OnZombieSpawn(true);
                                        break;
                                    case MonsterSpawner.ESpawnResult.Fail:
                                        CantSpawnBoss(ps, colony);
                                        break;
                                }

                                if (_spawnedBosses.ContainsKey(ps) &&
                                    _spawnedBosses[ps].IsValid &&
                                    _spawnedBosses[ps].CurrentHealth > 0)
                                {
                                    if (ps.Player.GetTempValues(true).GetOrDefault("BossIndicator", 0) <
                                        Time.SecondsSinceStartInt)
                                    {
                                        Indicator.SendIconIndicatorNear(new Vector3Int(_spawnedBosses[ps].Position),
                                                                        _spawnedBosses[ps].ID,
                                                                        new IndicatorState(1, GameLoader.Poisoned_Icon,
                                                                                            false, false));

                                        ps.Player.GetTempValues(true)
                                            .Set("BossIndicator", Time.SecondsSinceStartInt + 1);
                                    }

                                    turnOffBoss = false;
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
                }
            }
        }

        private static void CantSpawnBoss(PlayerState ps, Colony colony)
        {
            ps.FaiedBossSpawns++;

            if (ps.FaiedBossSpawns > 10)
                PandaChat.SendThrottle(ps.Player, $"WARNING: Unable to spawn boss. Please ensure you have a path to your banner. You will be penalized {SettlerManager.PenalizeFood(colony, 0.15f)} food.", ChatColor.red);

            colony.OnZombieSpawn(false);
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterWorldLoad,
            GameLoader.NAMESPACE + ".Managers.MonsterManager.AfterWorldLoad")]
        public static void AfterWorldLoad()
        {
            GetNextBossSpawnTime();
        }

        private static void GetNextBossSpawnTime()
        {
            _nextBossUpdateTime = Time.SecondsSinceStartInt + Random.Next(MinBossSpawnTimeSeconds, MaxBossSpawnTimeSeconds);
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnPlayerHit,
            GameLoader.NAMESPACE + ".Managers.MonsterManager.OnPlayerHit")]
        public static void OnPlayerHit(Players.Player player, ModLoader.OnHitData d)
        {
            if (d.ResultDamage > 0 && d.HitSourceType == ModLoader.OnHitData.EHitSourceType.Monster)
            {
                var state = PlayerState.GetPlayerState(player);
                d.ResultDamage += state.Difficulty.MonsterDamage;
            }
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnNPCHit,
            GameLoader.NAMESPACE + ".Managers.MonsterManager.OnNPCHit")]
        public static void OnNPCHit(NPCBase npc, ModLoader.OnHitData d)
        {
            if (d.ResultDamage > 0 && d.HitSourceType == ModLoader.OnHitData.EHitSourceType.Monster)
            {
                var state = PlayerState.GetPlayerState(npc.Colony.Owner);
                d.ResultDamage += state.Difficulty.MonsterDamage;
            }
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnMonsterHit,
            GameLoader.NAMESPACE + ".Managers.MonsterManager.OnMonsterHit")]
        public static void OnMonsterHit(IMonster monster, ModLoader.OnHitData d)
        {
            var ps         = PlayerState.GetPlayerState(monster.OriginalGoal);
            var pandaArmor = monster as IPandaArmor;
            var turret     = d.HitSourceObject as IPandaDamage;

            if (pandaArmor != null && Random.NextFloat() <= pandaArmor.MissChance)
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
                        tmpDmg = tmpDmg - tmpDmg * flatResist;

                    damage += tmpDmg;
                }

                d.ResultDamage = damage;
            }
            else if (pandaArmor != null)
            {
                d.ResultDamage = DamageType.Physical.CalcDamage(pandaArmor.ElementalArmor, d.ResultDamage);

                if (pandaArmor.AdditionalResistance.TryGetValue(DamageType.Physical, out var flatResist))
                    d.ResultDamage = d.ResultDamage - d.ResultDamage * flatResist;
            }

            d.ResultDamage = d.ResultDamage - d.ResultDamage * ps.Difficulty.MonsterDamageReduction;

            if (Random.NextFloat() > .5f)
                ServerManager.SendAudio(monster.Position, GameLoader.NAMESPACE + ".ZombieAudio");
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
                        PandaChat.Send(rewardMonster.OriginalGoal,
                                       $"You have been awarded {reward.Value}x {item.Name}!", ChatColor.orange);
                }
            }
        }

        public static Dictionary<int, IMonster> GetAllMonsters()
        {
            return typeof(MonsterTracker).GetField("allMonsters", BindingFlags.Static | BindingFlags.NonPublic)
                                         .GetValue(null) as Dictionary<int, IMonster>;
        }
    }
}