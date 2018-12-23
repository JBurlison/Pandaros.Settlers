using AI;
using Monsters;
using NPC;
using Pandaros.Settlers.Entities;
using Pandaros.Settlers.Items;
using Pandaros.Settlers.Monsters;
using Pandaros.Settlers.Monsters.Bosses;
using Pipliz;
using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Random = Pipliz.Random;
using Time = Pipliz.Time;

namespace Pandaros.Settlers.Managers
{
    [ModLoader.ModManager]
    public static class MonsterManager
    {
        private static double _nextUpdateTime;
        private static int _nextBossUpdateTime = int.MaxValue;

        private static readonly Dictionary<ColonyState, IPandaBoss> _spawnedBosses =  new Dictionary<ColonyState, IPandaBoss>();

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

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnUpdate, GameLoader.NAMESPACE + ".Managers.MonsterManager.Update")]
        public static void OnUpdate()
        {
            if (!World.Initialized || AIManager.IsBusy())
                return;

            var secondsSinceStartDouble = Time.SecondsSinceStartDouble;

            if (_nextUpdateTime < secondsSinceStartDouble)
            {
                IMonster m = null;

                foreach (var monster in GetAllMonsters())
                    if (m == null || UnityEngine.Vector3.Distance(monster.Value.Position, m.Position) > 15 && Random.NextBool())
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
                    var   worldSettings = ServerManager.WorldSettingsVariable;

                    foreach (var colony in ServerManager.ColonyTracker.ColoniesByID.Values)
                    {
                        var bannerGoal = colony.Banners[0];

                        if (colony.Banners.Length > 1)
                        {
                            var next = Pipliz.Random.Next(colony.Banners.Length);
                            bannerGoal = colony.Banners[next];
                        }

                        var cs = ColonyState.GetColonyState(colony);

                        if (false &&
                            cs.BossesEnabled &&
                            cs.ColonyRef.OwnerIsOnline() &&
                            colony.FollowerCount > Configuration.GetorDefault("MinColonistsCountForBosses", 100))
                        {
                            if (bossType != null &&
                                !_spawnedBosses.ContainsKey(cs))
                            {
                                Vector3Int positionFinal;
                                switch (MonsterSpawner.TryGetSpawnLocation(bannerGoal.Position, bannerGoal.SafeRadius, 200, 500f, out positionFinal))
                                {
                                    case MonsterSpawner.ESpawnResult.Success:
                                        if (AIManager.ZombiePathFinder.TryFindPath(positionFinal, bannerGoal.Position, out var path, 2000000000) == EPathFindingResult.Success)
                                        {
                                            var pandaboss = bossType.GetNewBoss(path, colony);
                                            _spawnedBosses.Add(cs, pandaboss);

                                            BossSpawned?.Invoke(MonsterTracker.MonsterSpawner,
                                                                new BossSpawnedEvent(cs, pandaboss));

                                            ModLoader.TriggerCallbacks<IMonster>(ModLoader.EModCallbackType.OnMonsterSpawned,
                                                                                    pandaboss);

                                            MonsterTracker.Add(pandaboss);
                                            colony.OnZombieSpawn(true);
                                            cs.FaiedBossSpawns = 0;
                                            PandaChat.Send(cs, $"[{pandaboss.Name}] {pandaboss.AnnouncementText}", ChatColor.red);

                                            if (!string.IsNullOrEmpty(pandaboss.AnnouncementAudio))
                                               colony.ForEachOwner(o => ServerManager.SendAudio(o.Position, pandaboss.AnnouncementAudio));
                                        }

                                        break;
                                    case MonsterSpawner.ESpawnResult.NotLoaded:
                                    case MonsterSpawner.ESpawnResult.Impossible:
                                        colony.OnZombieSpawn(true);
                                        break;
                                    case MonsterSpawner.ESpawnResult.Fail:
                                        CantSpawnBoss(cs);
                                        break;
                                }

                                if (_spawnedBosses.ContainsKey(cs) &&
                                    _spawnedBosses[cs].IsValid &&
                                    _spawnedBosses[cs].CurrentHealth > 0)
                                {
                                    if (colony.TemporaryData.GetAsOrDefault("BossIndicator", 0) < Time.SecondsSinceStartInt)
                                    {
                                        Indicator.SendIconIndicatorNear(new Vector3Int(_spawnedBosses[cs].Position),
                                                                        _spawnedBosses[cs].ID,
                                                                        new IndicatorState(1, GameLoader.Poisoned_Icon,
                                                                                            false, false));

                                        colony.TemporaryData.SetAs("BossIndicator", Time.SecondsSinceStartInt + 1);
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

        private static void CantSpawnBoss(ColonyState cs)
        {
            cs.FaiedBossSpawns++;

            if (cs.FaiedBossSpawns > 10)
                PandaChat.SendThrottle(cs, $"WARNING: Unable to spawn boss. Please ensure you have a path to your banner. You have been penalized {SettlerManager.PenalizeFood(cs.ColonyRef, 0.15f)} food.", ChatColor.red);

            cs.ColonyRef.OnZombieSpawn(false);
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterWorldLoad, GameLoader.NAMESPACE + ".Managers.MonsterManager.AfterWorldLoad")]
        public static void AfterWorldLoad()
        {
            GetNextBossSpawnTime();
        }

        private static void GetNextBossSpawnTime()
        {
            _nextBossUpdateTime = Time.SecondsSinceStartInt + Random.Next(MinBossSpawnTimeSeconds, MaxBossSpawnTimeSeconds);
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnPlayerHit, GameLoader.NAMESPACE + ".Managers.MonsterManager.OnPlayerHit")]
        public static void OnPlayerHit(Players.Player player, ModLoader.OnHitData d)
        {
            if (d.ResultDamage > 0 && d.HitSourceType == ModLoader.OnHitData.EHitSourceType.Monster && player.ActiveColony != null)
            {
                var state = ColonyState.GetColonyState(player.ActiveColony);
                d.ResultDamage += state.Difficulty.MonsterDamage;
            }
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnNPCHit, GameLoader.NAMESPACE + ".Managers.MonsterManager.OnNPCHit")]
        public static void OnNPCHit(NPCBase npc, ModLoader.OnHitData d)
        {
            if (d.ResultDamage > 0 && d.HitSourceType == ModLoader.OnHitData.EHitSourceType.Monster)
            {
                var state = ColonyState.GetColonyState(npc.Colony);
                d.ResultDamage += state.Difficulty.MonsterDamage;
            }
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnMonsterHit, GameLoader.NAMESPACE + ".Managers.MonsterManager.OnMonsterHit")]
        public static void OnMonsterHit(IMonster monster, ModLoader.OnHitData d)
        {
            var cs         = ColonyState.GetColonyState(monster.OriginalGoal);
            var pandaArmor = monster as IPandaArmor;
            var pamdaDamage     = d.HitSourceObject as IPandaDamage;
            var skilled = 0f;

            if (pamdaDamage == null && d.HitSourceType == ModLoader.OnHitData.EHitSourceType.NPC)
            {
                var npc = d.HitSourceObject as NPCBase;
                var inv = SettlerInventory.GetSettlerInventory(npc);
                skilled = inv.GetSkillModifier();

                if (inv.Weapon != null && Items.Weapons.WeaponFactory.WeaponLookup.TryGetValue(inv.Weapon.Id, out var wep))
                    pamdaDamage = wep;
            }

            if (pandaArmor != null && Random.NextFloat() <= pandaArmor.MissChance)
            {
                d.ResultDamage = 0;
                return;
            }

            if (pandaArmor != null && pamdaDamage != null)
            {
                d.ResultDamage = Items.Weapons.WeaponFactory.CalcDamage(pandaArmor, pamdaDamage);
            }
            else if (pandaArmor != null)
            {
                d.ResultDamage = DamageType.Physical.CalcDamage(pandaArmor.ElementalArmor, d.ResultDamage);

                if (pandaArmor.AdditionalResistance.TryGetValue(DamageType.Physical, out var flatResist))
                    d.ResultDamage = d.ResultDamage - d.ResultDamage * flatResist;
            }

            double skillRoll = Pipliz.Random.Next() + skilled;

            if (skillRoll > skilled)
                d.ResultDamage += d.ResultDamage;

            d.ResultDamage = d.ResultDamage - d.ResultDamage * cs.Difficulty.MonsterDamageReduction;


            if (d.ResultDamage >= monster.CurrentHealth)
            {
                var rewardMonster = monster as IKillReward;

                if (rewardMonster != null && rewardMonster.OriginalGoal.OwnerIsOnline())
                {
                    var stockpile = rewardMonster.OriginalGoal.Stockpile;
                    if (!string.IsNullOrEmpty(rewardMonster.LootTableName) &&
                        Items.LootTables.Lookup.TryGetValue(rewardMonster.LootTableName, out var lootTable))
                    {
                        float luck = 0;

                        if (d.HitSourceObject is ILucky luckSrc)
                            luck = luckSrc.Luck;
                        else if ((d.HitSourceType == ModLoader.OnHitData.EHitSourceType.PlayerClick ||
                                d.HitSourceType == ModLoader.OnHitData.EHitSourceType.PlayerProjectile) &&
                                d.HitSourceObject is Players.Player player)
                        {
                            var ps = PlayerState.GetPlayerState(player);

                            foreach (var armor in ps.Armor)
                                if (Items.Armor.ArmorFactory.ArmorLookup.TryGetValue(armor.Value.Id, out var a))
                                    luck += a.Luck;

                            if (Items.Weapons.WeaponFactory.WeaponLookup.TryGetValue(ps.Weapon.Id, out var w))
                                luck += w.Luck;
                        }
                        else if (d.HitSourceType == ModLoader.OnHitData.EHitSourceType.NPC &&
                                d.HitSourceObject is NPCBase nPC)
                        {
                            var inv = SettlerInventory.GetSettlerInventory(nPC);

                            foreach (var armor in inv.Armor)
                                if (Items.Armor.ArmorFactory.ArmorLookup.TryGetValue(armor.Value.Id, out var a))
                                    luck += a.Luck;

                            if (Items.Weapons.WeaponFactory.WeaponLookup.TryGetValue(inv.Weapon.Id, out var w))
                                luck += w.Luck;
                        }

                        var roll = lootTable.GetDrops(luck);

                        foreach (var item in roll)
                            monster.OriginalGoal.Stockpile.Add(item.Key, item.Value);
                    }
                }
            }
            else if (Random.NextFloat() > .5f)
                ServerManager.SendAudio(monster.Position, GameLoader.NAMESPACE + ".ZombieAudio");
        }

        public static Dictionary<int, IMonster> GetAllMonsters()
        {
            return typeof(MonsterTracker).GetField("allMonsters", BindingFlags.Static | BindingFlags.NonPublic)
                                         .GetValue(null) as Dictionary<int, IMonster>;
        }
    }
}