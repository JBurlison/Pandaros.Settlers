using NPC;
using Pandaros.Settlers.Chance;
using Pandaros.Settlers.Entities;
using Pipliz.Chatting;
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
        public const int ABSOLUTE_MAX_PERSPAWN = 50;
        public const double LOABOROR_LEAVE_HOURS = 14;
        public const double BED_LEAVE_HOURS = 5;

        public static SerializableDictionary<string, ColonyState> CurrentStates { get; private set; }
        private static Dictionary<string, ISpawnSettlerEvaluator> _deciders = new Dictionary<string, ISpawnSettlerEvaluator>();
        private static bool _worldLoaded = false;
        private static Random _r = new Random();
        private static double _nextGenTime = TimeCycle.TotalTime + _r.Next(6, 18);
        private static double _nextLaborerTime = TimeCycle.TotalTime + _r.Next(2, 6);
        private static double _nextbedTime = TimeCycle.TotalTime + _r.Next(1, 2);

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

        [ModLoader.ModCallbackAttribute(ModLoader.EModCallbackType.AfterStartup, "Pandaros.Settlers.AfterStartup")]
        public static void AfterStartup()
        {
            PandaLogger.Log("Active.");
            CurrentStates = SaveManager.LoadState();

            if (CurrentStates == null)
                CurrentStates = new SerializableDictionary<string, ColonyState>();
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterWorldLoad, "Pandaros.Settlers.AfterWorldLoad")]
        public static void AfterWorldLoad()
        {
            _worldLoaded = true;
            PandaLogger.Log("World load detected. Starting monitor...");
            CheckWorld();

            RegisterEvaluator(new Chance.SettelerEvaluation());
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnPlayerConnectedLate, "Pandaros.Settlers.OnPlayerConnectedLate")]
        public static void OnPlayerConnectedLate(Players.Player p)
        {
            Chat.Send(p, string.Format("<color=red>You are not allowed to recruit over {0} colonists. If you build it... they will come.</color>", MAX_BUYABLE));
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnUpdate, "Pandaros.Settlers.OnUpdate")]
        public static void OnUpdate()
        {
            EvaluateSettelers();
            EvaluateLaborers();
            EvaluateBeds();
        }

        private static void CheckIfColonistsWhereBought()
        {
            if (_worldLoaded)
            {
                CheckWorld();
                Players.PlayerDatabase.ForeachValue(p =>
                {
                    Colony colony = Colony.Get(p);
                    PlayerState state = GetPlayerState(p, colony);
                   
                    while (colony.FollowerCount > MAX_BUYABLE && 
                           state.ColonistCount != colony.FollowerCount) // if we have the expected number of colonists, we skip.
                    {
                        Chat.Send(p, string.Format("<color=red>You are not allowed to recruit over {0} colonists. If you build it... they will come.</color>", MAX_BUYABLE));
                        KillColonist(colony);
                    }

                    if (colony.FollowerCount <= MAX_BUYABLE)
                    {
                        state.ColonistCount = colony.FollowerCount; 
                    }
                });
            }
        }

        public static void KillColonist(Colony colony)
        {
            if (colony.LaborerCount > 0)
            {
                colony.RemoveNPC(colony.FindLaborer());
            }
            else
            {
                for (int c = 0; c < 8; c++)
                {
                    // colony takes 8 hits to kill a colonist
                    colony.TakeMonsterHit(0, int.MaxValue);
                }
            }
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
                if (!colony.PlayerStates.ContainsKey(p.ID.ToString()))
                    colony.PlayerStates.Add(p.ID.ToString(), new PlayerState());

                if (colony.PlayerStates[p.ID.ToString()].ColonistCount == 0 && 
                    c.FollowerCount != 0)
                    colony.PlayerStates[p.ID.ToString()].ColonistCount = c.FollowerCount;

                return colony.PlayerStates[p.ID.ToString()];
            }

            return null;
        }

        public static void EvaluateSettelers()
        {
            CheckIfColonistsWhereBought();

            if (TimeCycle.TotalTime > _nextGenTime)
            {
                Players.PlayerDatabase.ForeachValue(p =>
                {
                    if (p.IsConnected)
                    { 
                        Colony colony = Colony.Get(p);
                        PlayerState state = GetPlayerState(p, colony);
                        PandaLogger.Log("Evaluating new settlers today for " + p.Name);

                        if (colony.FollowerCount >= MAX_BUYABLE)
                        {
                            double chance = 0.2;

                            lock (_deciders)
                                foreach (var d in _deciders)
                                    chance += d.Value.SpawnChance(p, colony, state);

                            chance = chance / _deciders.Count;

                            PandaLogger.Log(p.Name + " has a " + chance * 100 + "% chance of getting settlers");
                            var rand = state.Rand.Next(1, 100);
                            PandaLogger.Log(p.Name + " rolled a " + rand + "%");

                            if (chance > 0 && chance * 100 > rand)
                            {
                                var addCount = Math.Floor(state.MaxPerSpawn * chance);

                                PandaLogger.Log("Adding " + addCount + " settlers to " + p.Name);
                                Chat.Send(p, string.Concat("<color=magenta>", string.Format(SettlerReasoning.GetSettleReason(), addCount), "</color>"));

                                for (int i = 0; i < addCount; i++)
                                {
                                    NPCBase newGuy = new NPCBase(NPCType.GetByKeyNameOrDefault("pipliz.laborer"), BannerTracker.Get(p).KeyLocation.Vector, colony);
                                    colony.RegisterNPC(newGuy);
                                }

                                state.ColonistCount += (int)addCount;

                                colony.SendUpdate();
                            }

                        }
                }
                });

                _nextGenTime = TimeCycle.TotalTime + _r.Next(4, 18);
                SaveManager.SaveState(CurrentStates);
            }
        }

        private static void EvaluateLaborers()
        {
            if (TimeCycle.IsDay && TimeCycle.TotalTime > _nextLaborerTime)
            {
                Players.PlayerDatabase.ForeachValue(p =>
                {
                    if (p.IsConnected)
                    {
                        List<NPC.NPCBase> unTrack = new List<NPCBase>();
                        Colony colony = Colony.Get(p);
                        PlayerState state = GetPlayerState(p, colony);
                        PandaLogger.Log("Evaluating laborers for " + p.Name);

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
                                KillColonist(colony);
                            }
                        }

                        if (left > 0)
                            Chat.Send(p, string.Concat("<color=red>", SettlerReasoning.GetNoJobReason(), string.Format(" {0} colonists have left your colony.</color>", left)));

                        state.ColonistCount -= left;

                        foreach (var npc in unTrack)
                            if (state.KnownLaborers.ContainsKey(npc))
                                state.KnownLaborers.Remove(npc);

                        if (unTrack.Count != 0)
                            colony.SendUpdate();
                    }
                });

                _nextLaborerTime = TimeCycle.TotalTime + _r.Next(2, 6);
                SaveManager.SaveState(CurrentStates);
            }
        }

        private static void EvaluateBeds()
        {
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
                            PandaLogger.Log("Evaluating Beds for " + p.Name);

                            PandaLogger.Log("Number of Beds: " + BedBlockTracker.GetCount(p));
                            PandaLogger.Log("Number of Colonists: " + state.ColonistCount);
                            var remainingBeds = BedBlockTracker.GetCount(p) - state.ColonistCount;
                            int left = 0;

                            if (remainingBeds > 0)
                                state.NeedsABed = 0;
                            else
                            {
                                if (state.NeedsABed == 0)
                                {
                                    state.NeedsABed = TimeCycle.TotalTime + BED_LEAVE_HOURS;
                                    Chat.Send(p, string.Concat("<color=grey>", SettlerReasoning.GetNeedBed(), "</color>"));
                                }
                                
                                if (state.NeedsABed != 0 && state.NeedsABed < TimeCycle.TotalTime)
                                {
                                    for (int i = 0; i < remainingBeds * -1; i++)
                                    {
                                        left++;
                                        KillColonist(colony);
                                    }

                                    state.NeedsABed = 0;
                                }

                                if (left > 0)
                                {
                                    Chat.Send(p, string.Concat("<color=red>", SettlerReasoning.GetNoBed(), string.Format(" {0} colonists have left your colony.</color>", left)));
                                    state.ColonistCount -= left;
                                    colony.SendUpdate();
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
                PandaLogger.LogError("EvaluateBeds", ex);

                if (ex.InnerException != null)
                    PandaLogger.LogError("EvaluateBeds inner ex", ex.InnerException);
            }
        }
    }
}
