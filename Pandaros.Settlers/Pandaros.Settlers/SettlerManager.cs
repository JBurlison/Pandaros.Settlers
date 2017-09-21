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

        public static SerializableDictionary<string, ColonyState> CurrentStates { get; private set; }
        private static Dictionary<string, ISpawnSettlerEvaluator> _deciders = new Dictionary<string, ISpawnSettlerEvaluator>();
        private static Thread _spawnThread;
        private static bool _worldLoaded = false;

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

            _spawnThread = new Thread(new ThreadStart(SpawnThread));
            _spawnThread.IsBackground = true;
            _spawnThread.Start();
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnPlayerConnectedLate, "Pandaros.Settlers.OnPlayerConnectedLate")]
        public static void OnPlayerConnectedLate(Players.Player p)
        {
            Chat.Send(p, string.Format("<color=red>You are not allowed to recruit over {0} colonists. If you build it... they will come.</color>", MAX_BUYABLE));
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnUpdate, "Pandaros.Settlers.OnUpdate")]
        public static void OnUpdate()
        {
            if (_worldLoaded)
            {
                CheckWorld();
                Players.PlayerDatabase.ForeachValue(p =>
                {
                    Colony colony = Colony.Get(p);
                    PlayerState state = GetPlayerState(p);

                    while (colony.FollowerCount > MAX_BUYABLE && 
                           state.ColonistCount != colony.FollowerCount) // if we have the expected number of colonists, we skip.
                    {
                        Chat.Send(p, string.Format("<color=red>You are not allowed to recruit over {0} colonists. If you build it... they will come.</color>", MAX_BUYABLE));

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

                    if (colony.FollowerCount <= MAX_BUYABLE)
                    {
                        state.ColonistCount = colony.FollowerCount; 
                    }
                });
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

        public static PlayerState GetPlayerState(Players.Player p)
        {
            var colony = CurrentColonyState;

            if (colony != null)
            {
                if (!colony.PlayerStates.ContainsKey(p.ID.ToString()))
                    colony.PlayerStates.Add(p.ID.ToString(), new PlayerState());

                return colony.PlayerStates[p.ID.ToString()];
            }

            return null;
        }

        public static void SpawnThread()
        {
            bool hasGeneratedToday = false;

            while (true)
            {
                if (!hasGeneratedToday && TimeCycle.IsDay)
                {
                    Players.PlayerDatabase.ForeachValue(p =>
                    {
                        if (p.IsConnected)
                        { 
                            Colony colony = Colony.Get(p);
                            PlayerState state = GetPlayerState(p);
                            PandaLogger.Log("Evaluating new settlers today for " + p.Name);

                            if (colony.FollowerCount >= MAX_BUYABLE)
                            {
                                double chance = 0;

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
                                    Chat.Send(p, string.Concat("<color=green>", string.Format(SettleReason.GetReason(), addCount), "</color>"));

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

                    hasGeneratedToday = true;
                }

                if (hasGeneratedToday && !TimeCycle.IsDay)
                    hasGeneratedToday = false;

                SaveManager.SaveState(CurrentStates); 
                System.Threading.Thread.Sleep(1000);
            }
        }

        
    }
}
