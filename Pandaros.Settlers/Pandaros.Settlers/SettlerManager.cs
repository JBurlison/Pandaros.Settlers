using NPC;
using Pandaros.Settlers.Chance;
using Pandaros.Settlers.Entities;
using Pandaros.Settlers.Research;
using Pipliz;
using Pipliz.Chatting;
using Server.NPCs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Pandaros.Settlers
{
    [ModLoader.ModManager]
    public class SettlerManager
    {
        public const int MAX_BUYABLE = 10;
        public const int MIN_PERSPAWN = 5;
        public const int ABSOLUTE_MAX_PERSPAWN = 20;
        public const double BED_LEAVE_HOURS = 5;
        public static readonly Version MOD_VER = new Version(0, 5, 0, 0);
        public static readonly double LOABOROR_LEAVE_HOURS = TimeSpan.FromDays(7).TotalHours;
        private static Thread _foodThread = new Thread(() => UpdateFoodUse());

        public static SerializableDictionary<string, ColonyState> CurrentStates { get; private set; }
        public static GameDifficulty Difficulty { get; set; }

        private static Dictionary<string, ISpawnSettlerEvaluator> _deciders = new Dictionary<string, ISpawnSettlerEvaluator>();
        private static bool _worldLoaded = false;
        private static System.Random _r = new System.Random();
        private static DateTime _nextfoodSendTime = DateTime.MinValue;
        private static double _nextLaborerTime = TimeCycle.TotalTime + _r.Next(2, 6);
        private static double _nextbedTime = TimeCycle.TotalTime + _r.Next(1, 2);
        private static float _baseFoodPerHour;
        public static bool RUNNING = false;

        public static ColonyState CurrentColonyState
        {
            get
            {
                if (!string.IsNullOrEmpty(ServerManager.WorldName) &&
                    CurrentStates.ContainsKey(ServerManager.WorldName))
                    return CurrentStates[ServerManager.WorldName];

                return null;    
            }
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterStartup, GameLoader.NAMESPACE + ".AfterStartup")]
        public static void AfterStartup()
        {
            PandaLogger.Log(ChatColor.lime, "Active. Version {0}", MOD_VER);
            RUNNING = true;
            ChatCommands.CommandManager.RegisterCommand(new GameDifficultyChatCommand());
            
            CurrentStates = SaveManager.LoadState();

            if (CurrentStates == null)
                CurrentStates = new SerializableDictionary<string, ColonyState>();

            _foodThread.IsBackground = true;
            _foodThread.Start();
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterItemTypesDefined, GameLoader.NAMESPACE + ".AfterItemTypesDefined")]
        public static void AfterItemTypesDefined()
        {
            PandaResearch.Register();
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnQuitLate, GameLoader.NAMESPACE + "OnQuitLate")]
        public static void OnQuitLate()
        {
            RUNNING = false;
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterWorldLoad, GameLoader.NAMESPACE + ".AfterWorldLoad")]
        public static void AfterWorldLoad()
        {
            _worldLoaded = true;
            PandaLogger.Log(ChatColor.lime, "World load detected. Starting monitor...");
            CheckWorld();
            _baseFoodPerHour = ServerManager.ServerVariables.NPCFoodUsePerHour;

            RegisterEvaluator(new Chance.SettlerEvaluation());
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnPlayerConnectedLate, GameLoader.NAMESPACE + ".OnPlayerConnectedLate")]
        public static void OnPlayerConnectedLate(Players.Player p)
        {
            if (p.IsConnected)
            {
                Colony colony = Colony.Get(p);
                PlayerState state = GetPlayerState(p, colony);
                PandaChat.Send(p, string.Format("You are not allowed to recruit over {0} colonists. If you build it... they will come.", MAX_BUYABLE), ChatColor.grey);
                PandaChat.Send(p, string.Format("Game difficulty set to {0}", state.Difficulty), ChatColor.maroon);
                GameDifficultyChatCommand.PossibleCommands(p, ChatColor.grey);
                Update(colony);
            }
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnUpdate, GameLoader.NAMESPACE + ".OnUpdate")]
        public static void OnUpdate()
        {
            bool update = false;

            if (CheckIfColonistsWhereBought())
                update = true;

            if (EvaluateSettlers())
                update = true;

            if (EvaluateLaborers())
                update = true; 

            if (EvaluateBeds())
                update = true;

            if (update)
                Players.PlayerDatabase.ForeachValue(p => Update(Colony.Get(p)));
        }

        private static bool CheckIfColonistsWhereBought()
        {
            bool update = false;

            if (_worldLoaded)
            {
                CheckWorld();
                Players.PlayerDatabase.ForeachValue(p =>
                {
                    if (p.IsConnected)
                    {
                        Colony colony = Colony.Get(p);
                        PlayerState state = GetPlayerState(p, colony);

                        while (colony.FollowerCount > MAX_BUYABLE &&
                               state.ColonistCount < colony.FollowerCount) // if we have the expected number of colonists, we skip.
                        {
                            PandaChat.Send(p, string.Format("You are not allowed to recruit over {0} colonists. If you build it... they will come.", MAX_BUYABLE), ChatColor.red);
                            KillLaborerOrRandomColonist(colony);
                        }

                        if (colony.FollowerCount <= MAX_BUYABLE ||
                            state.ColonistCount > colony.FollowerCount)
                        {
                            state.ColonistCount = colony.FollowerCount;
                            update = true;
                        }
                    }
                });
            }

            return update;
        }


        public static void KillLaborerOrRandomColonist(Colony colony)
        {
            if (colony.LaborerCount > 0)
            {
                KillColonist(colony.FindLaborer());
            }
            else
            {
                var colonists = GetPlayerState(colony.Owner, colony).ColonyInterface.Followers;

                if (colonists.Count > 0)
                    KillColonist(colonists[_r.Next(0, colonists.Count)]);
            }
        }

        public static void KillColonist(NPCBase npc)
        {
            npc.OnDeath();
        }

        public static void RegisterEvaluator(ISpawnSettlerEvaluator evaluator)
        {
            if (evaluator != null && !string.IsNullOrEmpty(evaluator.Name))
                lock (_deciders)
                _deciders[evaluator.Name] = evaluator;
        }

        private static void CheckWorld()
        {
            if (!string.IsNullOrEmpty(ServerManager.WorldName) &&
                !CurrentStates.ContainsKey(ServerManager.WorldName))
                CurrentStates.Add(ServerManager.WorldName, new ColonyState());
        }

        public static PlayerState GetPlayerState(Players.Player p, Colony c)
        {
            var colony = CurrentColonyState;
            
            if (colony != null)
            {
                string playerId = p.ID.ToString();

                if (!colony.PlayerStates.ContainsKey(playerId))
                    colony.PlayerStates.Add(playerId, new PlayerState());

                if (colony.PlayerStates[playerId].ColonyInterface == null)
                    colony.PlayerStates[playerId].SetupColonyRefrences(c);

                if (colony.PlayerStates[playerId].ColonistCount == 0 && 
                    c.FollowerCount != 0)
                    colony.PlayerStates[playerId].ColonistCount = c.FollowerCount;

                return colony.PlayerStates[playerId];
            }

            return null;
        }

        public static bool EvaluateSettlers()
        {
            bool update = false;


            Players.PlayerDatabase.ForeachValue(p =>
            {
                if (p.IsConnected)
                {
                    Colony colony = Colony.Get(p);
                    PlayerState state = GetPlayerState(p, colony);

                    if (state.NextGenTime == 0)
                        state.NextGenTime = TimeCycle.TotalTime + _r.Next(4, 14 - p.GetTemporaryValueOrDefault(PandaResearch.GetTempValueKey(PandaResearch.TimeBetween), 0));

                    if (TimeCycle.TotalTime > state.NextGenTime)
                    {
                        if (colony.FollowerCount >= MAX_BUYABLE)
                        {
                            float chance = p.GetTemporaryValueOrDefault<float>(PandaResearch.GetTempValueKey(PandaResearch.SettlerChance), 0f) + state.Difficulty.AdditionalChance;

                            lock (_deciders)
                                foreach (var d in _deciders)
                                    chance += d.Value.SpawnChance(p, colony, state);

                            chance = chance / _deciders.Count;

                            var rand = state.Rand.Next(1, 100);

                            if (chance > 0 && chance * 100 > rand)
                            {
                                var addCount = System.Math.Floor(state.MaxPerSpawn * chance);
                                PandaChat.Send(p, string.Format(SettlerReasoning.GetSettleReason(), addCount), ChatColor.magenta);

                                for (int i = 0; i < addCount; i++)
                                {
                                    NPCBase newGuy = new NPCBase(NPCType.GetByKeyNameOrDefault("pipliz.laborer"), BannerTracker.Get(p).KeyLocation.Vector, colony);

                                    colony.RegisterNPC(newGuy);
                                }

                                state.ColonistCount += (int)addCount;
                                update = true;
                                state.FoodDivider = 0;
                            }

                        }

                        state.NextGenTime = TimeCycle.TotalTime + _r.Next(4, 14 - p.GetTemporaryValueOrDefault(PandaResearch.GetTempValueKey(PandaResearch.TimeBetween), 0));
                    }
                }
            });

            if (update)
                SaveManager.SaveState(CurrentStates);

            return update;
        }

        public static void Update(Colony colony)
        {
            if (colony.Owner.IsConnected)
            {
                Colony.SendColonistCount(colony.Owner);
                Colony.SendLaborerCount(colony.Owner);
            }
        }

        public static void UpdateFoodUse()
        {
            while (RUNNING)
            {
                Players.PlayerDatabase.ForeachValue(player =>
                {
                    if (player.ID.type != NetworkID.IDType.Server)
                    {
                        Colony colony = Colony.Get(player);
                        var ps = GetPlayerState(colony.Owner, colony);

                        var food = _baseFoodPerHour;

                        if (ps.Difficulty != GameDifficulty.Normal && colony.FollowerCount > 0)
                        {
                            if (ps.FoodDivider == 0)
                                ps.FoodDivider = _r.Next(Pipliz.Math.CeilToInt(colony.FollowerCount * 3.50f), colony.FollowerCount * 4);

                            var multiplier = (ps.FoodDivider / colony.FollowerCount) - colony.Owner.GetTemporaryValueOrDefault<float>(PandaResearch.GetTempValueKey(PandaResearch.ReducedWaste), 0f);

                            food += (_baseFoodPerHour * multiplier);

                            if (colony.FollowerCount >= MAX_BUYABLE)
                                food = food * ps.Difficulty.FoodMultiplier;
                        }

                        if (colony.InSiegeMode)
                            food = food * ServerManager.ServerVariables.NPCfoodUseMultiplierSiegeMode;

                        colony.FoodUsePerHour = food;
                    }
                });

                System.Threading.Thread.Sleep(1000);
            }
        }

        private static bool EvaluateLaborers()
        {
            bool update = false;

            if (TimeCycle.IsDay && TimeCycle.TotalTime > _nextLaborerTime)
            {
                Players.PlayerDatabase.ForeachValue(p =>
                {
                    if (p.IsConnected)
                    {
                        List<NPC.NPCBase> unTrack = new List<NPCBase>();
                        Colony colony = Colony.Get(p);
                        PlayerState state = GetPlayerState(p, colony);

                        for (int i = 0; i < colony.LaborerCount; i++)
                        {
                            var npc = colony.FindLaborer(i);

                            if (!state.KnownLaborers.ContainsKey(npc))
                                state.KnownLaborers.Add(npc, TimeCycle.TotalTime + LOABOROR_LEAVE_HOURS);
                        }

                        int left = 0;
                        
                        foreach (var idKvp in state.KnownLaborers)
                        {
                            if (!idKvp.Key.NPCType.IsLaborer || colony.GetLaborerIndex(idKvp.Key) == 0)
                                unTrack.Add(idKvp.Key);
                            else if (idKvp.Value < TimeCycle.TotalTime)
                            {
                                left++;
                                unTrack.Add(idKvp.Key);
                                KillLaborerOrRandomColonist(colony);
                            }
                        }

                        if (left > 0)
                            PandaChat.Send(p, string.Concat(SettlerReasoning.GetNoJobReason(), string.Format(" {0} colonists have left your colony.", left)), ChatColor.red);

                        state.ColonistCount -= left;

                        foreach (var npc in unTrack)
                            if (state.KnownLaborers.ContainsKey(npc))
                                state.KnownLaborers.Remove(npc);

                        update = unTrack.Count != 0;
                    }
                });

                _nextLaborerTime = TimeCycle.TotalTime + _r.Next(2, 6);
                SaveManager.SaveState(CurrentStates);
            }

            return update;
        }

        private static bool EvaluateBeds()
        {
            bool update = false;
            try
            {
                if (!TimeCycle.IsDay && TimeCycle.TotalTime > _nextbedTime)
                {
                    Players.PlayerDatabase.ForeachValue(p =>
                    {
                        if (p.IsConnected)
                        {
                            List<NPC.NPCBase> unTrack = new List<NPCBase>();
                            Colony colony = Colony.Get(p);
                            PlayerState state = GetPlayerState(p, colony);
                            var remainingBeds = BedBlockTracker.GetCount(p) - state.ColonistCount;
                            int left = 0;
 
                            if (remainingBeds >= 0)
                                state.NeedsABed = 0;
                            else
                            {
                                if (state.NeedsABed == 0)
                                {
                                    state.NeedsABed = TimeCycle.TotalTime + BED_LEAVE_HOURS;
                                    PandaChat.Send(p, SettlerReasoning.GetNeedBed(), ChatColor.grey);
                                }
                                
                                if (state.NeedsABed != 0 && state.NeedsABed < TimeCycle.TotalTime)
                                {
                                    var toRemove = remainingBeds * -1;
                                    PandaLogger.Log(ChatColor.red, "Could not get colonists refrence.");

                                    for (int i = 0; i < toRemove; i++)
                                    {
                                        left++;
                                        KillLaborerOrRandomColonist(colony);
                                    }
                                    
                                    state.NeedsABed = 0;
                                }

                                if (left > 0)
                                {
                                    PandaChat.Send(p, string.Concat(SettlerReasoning.GetNoBed(), string.Format(" {0} colonists have left your colony.", left)), ChatColor.red);
                                    state.ColonistCount -= left;
                                    update = true;
                                }
                            }
                        }
                    });

                    _nextbedTime = TimeCycle.TotalTime + _r.Next(1, 2);
                    SaveManager.SaveState(CurrentStates);
                }
            }
            catch (Exception ex)
            {
                PandaLogger.LogError(ex, "EvaluateBeds");
            }

            return update;
        }
    }
}
