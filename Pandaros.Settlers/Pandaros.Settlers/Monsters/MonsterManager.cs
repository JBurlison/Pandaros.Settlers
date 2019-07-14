using AI;
using Difficulty;
using Monsters;
using NPC;
using Pandaros.Settlers.ColonyManagement;
using Pandaros.Settlers.Entities;
using Pandaros.Settlers.Items;
using Pandaros.Settlers.Models;
using Pandaros.Settlers.Monsters;
using Pandaros.Settlers.Monsters.Bosses;
using Pandaros.Settlers.Monsters.Normal;
using Pipliz;
using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using static AI.PathingManager;
using static AI.PathingManager.PathFinder;
using Random = Pipliz.Random;
using Time = Pipliz.Time;

namespace Pandaros.Settlers.Monsters
{
    [ModLoader.ModManager]
    public class MonsterManager : IPathingThreadAction
    {
        private static double _nextUpdateTime;
        private static double _justQueued;
        private static int _nextBossUpdateTime = int.MaxValue;
        private static MonsterManager _monsterManager = new MonsterManager();
        private static Queue<IPandaBoss> _pandaBossesSpawnQueue = new Queue<IPandaBoss>();
        private static localization.LocalizationHelper _localizationHelper = new localization.LocalizationHelper(GameLoader.NAMESPACE, "Monsters");
        public static Dictionary<ColonyState, IPandaBoss> SpawnedBosses { get; private set; } = new Dictionary<ColonyState, IPandaBoss>();

        private static readonly List<IPandaBoss> _bossList = new List<IPandaBoss>();

        private static int _boss = -1;
        public static bool BossActive { get; private set; }

        public static int MinBossSpawnTimeSeconds // 15 minutes
            => SettlersConfiguration.GetorDefault(nameof(MinBossSpawnTimeSeconds), 900);

        public static int MaxBossSpawnTimeSeconds // 1/2 hour
            => SettlersConfiguration.GetorDefault(nameof(MaxBossSpawnTimeSeconds), 1800);

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

        public IPandaBoss CurrentPandaBoss { get; set; }

        public void PathingThreadAction(PathingContext context)
        {
            if (BossActive)
            {
                foreach (var colony in ServerManager.ColonyTracker.ColoniesByID.Values)
                {
                    var bannerGoal = colony.Banners.FirstOrDefault();

                    if (bannerGoal == null)
                        continue;

                    var cs = ColonyState.GetColonyState(colony);

                    if (cs.BossesEnabled &&
                        cs.ColonyRef.OwnerIsOnline() &&
                        colony.FollowerCount > SettlersConfiguration.GetorDefault("MinColonistsCountForBosses", 100))
                    {
                        if (CurrentPandaBoss != null && !SpawnedBosses.ContainsKey(cs))
                        {
                            Vector3Int positionFinal;
                            switch (((MonsterSpawner)MonsterTracker.MonsterSpawner).TryGetSpawnLocation(context, bannerGoal.Position, bannerGoal.SafeRadius, 200, 500f, out positionFinal))
                            {
                                case MonsterSpawner.ESpawnResult.Success:
                                    if (context.Pathing.TryFindPath(positionFinal, bannerGoal.Position, out var path, 2000000000) == EPathFindingResult.Success)
                                    {
                                        var pandaboss = (IPandaBoss)CurrentPandaBoss.GetNewInstance(path, colony);
                                        _pandaBossesSpawnQueue.Enqueue(pandaboss);
                                        SpawnedBosses.Add(cs, pandaboss);
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
                        }
                    }
                }
            }
        }


        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnUpdate, GameLoader.NAMESPACE + ".Managers.MonsterManager.Update")]
        public static void OnUpdate()
        {
            if (!World.Initialized)
                return;

            var secondsSinceStartDouble = Time.SecondsSinceStartDouble;

            if (_nextUpdateTime < secondsSinceStartDouble)
            {
                IMonster m = null;

                foreach (var monster in GetAllMonsters())
                    if (m == null || UnityEngine.Vector3.Distance(monster.Value.Position, m.Position) > 15 && Random.NextBool())
                    {
                        m = monster.Value;
                        AudioManager.SendAudio(monster.Value.Position, GameLoader.NAMESPACE + ".ZombieAudio");
                    }

                _nextUpdateTime = secondsSinceStartDouble + 5;
            }

            if (World.Initialized)
            {
                if (!TimeCycle.IsDay && 
                    !BossActive &&
                    _nextBossUpdateTime <= secondsSinceStartDouble)
                {
                    BossActive = true;
                    var bossType   = GetMonsterType();
                    _monsterManager.CurrentPandaBoss = bossType;
                    ServerManager.PathingManager.QueueAction(_monsterManager);
                    _justQueued = secondsSinceStartDouble + 5;

                    if (Players.CountConnected != 0)
                        PandaLogger.Log(ChatColor.yellow, $"Boss Active! Boss is: {bossType.name}");
                }

                if (BossActive && _justQueued < secondsSinceStartDouble) 
                {
                    var   turnOffBoss   = true;

                    if (_pandaBossesSpawnQueue.Count > 0)
                    {
                        var pandaboss = _pandaBossesSpawnQueue.Dequeue();
                        var cs = ColonyState.GetColonyState(pandaboss.OriginalGoal);
                        BossSpawned?.Invoke(MonsterTracker.MonsterSpawner, new BossSpawnedEvent(cs, pandaboss));

                        ModLoader.Callbacks.OnMonsterSpawned.Invoke(pandaboss);
                        MonsterTracker.Add(pandaboss);
                        cs.ColonyRef.OnZombieSpawn(true);
                        cs.FaiedBossSpawns = 0;
                        PandaChat.Send(cs, _localizationHelper, $"[{pandaboss.name}] {pandaboss.AnnouncementText}", ChatColor.red);

                        if (!string.IsNullOrEmpty(pandaboss.AnnouncementAudio))
                            cs.ColonyRef.ForEachOwner(o => AudioManager.SendAudio(o.Position, pandaboss.AnnouncementAudio));
                    }

                    foreach (var colony in ServerManager.ColonyTracker.ColoniesByID.Values)
                    {
                        var bannerGoal = colony.Banners.FirstOrDefault();
                        var cs = ColonyState.GetColonyState(colony);

                        if (bannerGoal != null &&
                            cs.BossesEnabled &&
                            cs.ColonyRef.OwnerIsOnline() &&
                            colony.FollowerCount > SettlersConfiguration.GetorDefault("MinColonistsCountForBosses", 100))
                        {

                            if (SpawnedBosses.ContainsKey(cs) &&
                                    SpawnedBosses[cs].IsValid &&
                                    SpawnedBosses[cs].CurrentHealth > 0)
                            {
                                if (colony.TemporaryData.GetAsOrDefault("BossIndicator", 0) < Time.SecondsSinceStartInt)
                                {
                                    Indicator.SendIconIndicatorNear(new Vector3Int(SpawnedBosses[cs].Position),
                                                                    SpawnedBosses[cs].ID,
                                                                    new IndicatorState(1, ItemId.GetItemId(GameLoader.NAMESPACE + ".Poisoned").Id,
                                                                                        false, false));

                                    colony.TemporaryData.SetAs("BossIndicator", Time.SecondsSinceStartInt + 1);
                                }

                                turnOffBoss = false;
                            }
                        }


                        if (turnOffBoss)
                        {
                            if (Players.CountConnected != 0 && SpawnedBosses.Count != 0)
                            {
                                PandaLogger.Log(ChatColor.yellow, $"All bosses cleared!");
                                var boss = SpawnedBosses.FirstOrDefault().Value;
                                PandaChat.SendToAll($"[{boss.name}] {boss.DeathText}", _localizationHelper, ChatColor.red);
                            }

                            BossActive = false;
                            SpawnedBosses.Clear();
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
                PandaChat.SendThrottle(cs, _localizationHelper, "NoBanner", ChatColor.red, SettlerManager.PenalizeFood(cs.ColonyRef, 0.15f) * 100 + "%");

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
            if (d.ResultDamage > 0 && d.HitSourceType == ModLoader.OnHitData.EHitSourceType.Monster && player.ActiveColony != null && !(d.HitSourceObject is IPandaZombie))
            {
                var state = ColonyState.GetColonyState(player.ActiveColony);
                d.ResultDamage += state.Difficulty.MonsterDamage;
            }
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnNPCHit, GameLoader.NAMESPACE + ".Managers.MonsterManager.OnNPCHit")]
        public static void OnNPCHit(NPCBase npc, ModLoader.OnHitData d)
        {
            if (d.ResultDamage > 0 && d.HitSourceType == ModLoader.OnHitData.EHitSourceType.Monster && !(d.HitSourceObject is IPandaZombie))
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

            if (pandaArmor != null && Random.NextFloat() <= pandaArmor.MissChance)
            {
                d.ResultDamage = 0;
                return;
            }

            if (pamdaDamage == null && d.HitSourceType == ModLoader.OnHitData.EHitSourceType.NPC)
            {
                var npc = d.HitSourceObject as NPCBase;
                var inv = SettlerInventory.GetSettlerInventory(npc);
                SettlerManager.IncrimentSkill(npc);
                skilled = inv.GetSkillModifier();

                if (inv.Weapon != null && Items.Weapons.WeaponFactory.WeaponLookup.TryGetValue(inv.Weapon.Id, out var wep))
                    pamdaDamage = wep;
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

            if (skillRoll < skilled)
                d.ResultDamage += d.ResultDamage;

            d.ResultDamage = d.ResultDamage - d.ResultDamage * cs.Difficulty.MonsterDamageReduction;

            if (d.ResultDamage < 0)
                d.ResultDamage = 0;

            if (d.HitSourceType == ModLoader.OnHitData.EHitSourceType.NPC)
            {
                var npc = d.HitSourceObject as NPCBase;
                var inv = SettlerInventory.GetSettlerInventory(npc);
                inv.IncrimentStat("Damage Done", d.ResultDamage);

                if (skillRoll < skilled)
                    inv.IncrimentStat("Double Damage Hits");
            }

            if (d.ResultDamage >= monster.CurrentHealth)
            {
                monster.OnRagdoll();
                var rewardMonster = monster as IPandaZombie;
                string monsterType = "zombie";

                if (rewardMonster != null)
                    monsterType = rewardMonster.MosterType;

                if (monster.OriginalGoal.OwnerIsOnline())
                {
                    if (!string.IsNullOrEmpty(monsterType) &&
                        LootTables.Lookup.TryGetValue(monsterType, out var lootTable))
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
                AudioManager.SendAudio(monster.Position, GameLoader.NAMESPACE + ".ZombieAudio");
        }

        public static Dictionary<int, IMonster> GetAllMonsters()
        {
            return typeof(MonsterTracker).GetField("allMonsters", BindingFlags.Static | BindingFlags.NonPublic)
                                         .GetValue(null) as Dictionary<int, IMonster>;
        }
    }
}