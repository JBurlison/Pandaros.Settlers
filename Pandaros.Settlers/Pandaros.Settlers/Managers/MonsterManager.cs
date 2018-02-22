using Pandaros.Settlers.Entities;
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

        private static int _boss = 0;
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
                t = _bossList[_boss];

                _boss++;

                if (_boss >= _bossList.Count)
                    _boss = 0;
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

            if (_maxTimePerTick.Elapsed.TotalMilliseconds > Monsters.PandaMonsterSpawner.MonsterVariables.MSPerTick)
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
                    var banners = BannerTracker.GetBanners();

                    for (int i = 0; i < banners.Count; i++)
                    {
                        Banner bannerGoal = banners.GetValueAtIndex(i);
                        var ps = PlayerState.GetPlayerState(bannerGoal.Owner);
                        var colony = Colony.Get(ps.Player);

                        if (ps.BossesEnabled && 
                            ps.Player.IsConnected && 
                            colony.FollowerCount > Configuration.GetorDefault("MinColonistsCountForBosses", 15))
                        {
                            if (bossType != null && 
                                !_spawnedBosses.ContainsKey(ps) &&
                                MonsterSpawner.TryGetSpawnLocation(bannerGoal, 200f, out var start) == MonsterSpawner.ESpawnResult.Success &&
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
                                Server.Indicator.SendIconIndicatorNear(new Pipliz.Vector3Int(_spawnedBosses[ps].Position), _spawnedBosses[ps].ID, new Shared.IndicatorState(1, GameLoader.Poisoned_Icon, false, false));

                                if (_spawnedBosses[ps].ZombieMultiplier != 0)
                                    num *= _spawnedBosses[ps].ZombieMultiplier;

                                Monsters.PandaMonsterSpawner.Instance.SpawnForBanner(bannerGoal, true, num, secondsSinceStartDouble, true);
                                turnOffBoss = false;
                            }
                        }
                    }
                    
                    if (turnOffBoss)
                    {
                        if (Players.CountConnected != 0)
                            PandaLogger.Log(ChatColor.yellow, $"All bosses cleared!");

                        foreach (var p in _spawnedBosses)
                        {
                            var stockpile = Stockpile.GetStockPile(p.Key.Player);
                            PandaChat.Send(p.Key.Player, $"[{p.Value.Name}] {p.Value.DeathText}", ChatColor.red);

                            foreach (var reward in p.Value.KillRewards)
                            {
                                stockpile.Add(reward.Key, reward.Value);
                                if (ItemTypes.TryGetType(reward.Key, out var item))
                                    PandaChat.Send(p.Key.Player, $"You have been awarded {reward.Value}x {item.Name}!", ChatColor.red);
                            }

                            
                            PandaChat.Send(p.Key.Player, $"[{p.Value.Name}] {p.Value.DeathText}", ChatColor.orange);
                        }

                        BossActive = false;
                        _spawnedBosses.Clear();
                        GetNextBossSpawnTime();
                    }
                }
            }
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterWorldLoad, GameLoader.NAMESPACE + ".Managers.MonsterManager.AfterWorldLoad")]
        public static void AfterWorldLoad()
        {
            GetNextBossSpawnTime();
            AddBoss(new ZombieQueen(new Server.AI.Path(), Players.GetPlayer(NetworkID.Server)));
            AddBoss(new Hoarder(new Server.AI.Path(), Players.GetPlayer(NetworkID.Server)));
            AddBoss(new Juggernaut(new Server.AI.Path(), Players.GetPlayer(NetworkID.Server)));
            _maxTimePerTick.Start();
        }
        
        private static void GetNextBossSpawnTime()
        {
            _nextBossUpdateTime = Pipliz.Time.SecondsSinceStartInt + Pipliz.Random.Next(MinBossSpawnTimeSeconds, MaxBossSpawnTimeSeconds);
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterSelectedWorld, GameLoader.NAMESPACE + ".Managers.MonsterManager.RegisterAudio"),
            ModLoader.ModCallbackProvidesFor("pipliz.server.loadaudiofiles"), ModLoader.ModCallbackDependsOn("pipliz.server.registeraudiofiles")]
        public static void RegisterAudio()
        {
            GameLoader.AddSoundFile(GameLoader.NAMESPACE + "ZombieAudio", new List<string>()
            {
                GameLoader.AUDIO_FOLDER_PANDA + "/Zombie1.ogg",
                GameLoader.AUDIO_FOLDER_PANDA + "/Zombie2.ogg",
                GameLoader.AUDIO_FOLDER_PANDA + "/Zombie3.ogg",
                GameLoader.AUDIO_FOLDER_PANDA + "/Zombie4.ogg",
            });
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnPlayerHit, GameLoader.NAMESPACE + ".Managers.MonsterManager.OnPlayerHit")]
        public static void OnPlayerHit(Players.Player player, Pipliz.Box<float> box)
        {
            if (box.item1 > 0)
            {
                var state = PlayerState.GetPlayerState(player);
                box.Set(box.item1 + state.Difficulty.MonsterDamage);
            }
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnNPCHit, GameLoader.NAMESPACE + ".Managers.MonsterManager.OnNPCHit")]
        public static void OnNPCHit(NPC.NPCBase npc, Pipliz.Box<float> box)
        {
            if (box.item1 > 0)
            {
                var state = PlayerState.GetPlayerState(npc.Colony.Owner);
                box.Set(box.item1 + state.Difficulty.MonsterDamage);
            }
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnMonsterHit, GameLoader.NAMESPACE + ".Managers.MonsterManager.OnMonsterHit")]
        public static void OnMonsterHit(IMonster monster, Pipliz.Box<float> box)
        {
            var ps = PlayerState.GetPlayerState(monster.OriginalGoal);
            box.Set(box.item1 - (box.item1 * ps.Difficulty.MonsterDamageReduction));

            if (Pipliz.Random.NextFloat() > .5f)
                ServerManager.SendAudio(monster.Position, GameLoader.NAMESPACE + "ZombieAudio");
        }

        public static Dictionary<int, IMonster> GetAllMonsters()
        {
            return typeof(MonsterTracker).GetField("allMonsters", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic).GetValue(null) as Dictionary<int, IMonster>;
        }
    }
}
