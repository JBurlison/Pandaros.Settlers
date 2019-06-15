﻿using Pandaros.Settlers.ColonyManagement;
using Pandaros.Settlers.Research;
using Pipliz.JSON;
using System;
using System.Collections.Generic;

namespace Pandaros.Settlers.Entities
{
    [ModLoader.ModManager]
    public class ColonyState
    {
        private static readonly Dictionary<Colony, ColonyState> _colonyStates = new Dictionary<Colony, ColonyState>();

        public Colony ColonyRef { get; set; }
        public int FaiedBossSpawns { get; set; }
        public bool CallToArmsEnabled { get; set; }
        public GameDifficulty Difficulty { get; set; } = SettlersConfiguration.DefaultDifficulty;
        public bool BossesEnabled { get; set; } = true;
        public bool SettlersEnabled { get; set; } = true;
        public bool NotifySettlers { get; set; } = true;
        public int ColonistsBought { get; set; }
        public double NextColonistBuyTime { get; set; }
        public double NeedsABed { get; set; }
        public int HighestColonistCount { get; set; }
        public double NextGenTime { get; set; }
        public int SettlersToggledTimes { get; set; }
        public string DifficultyStr
        {
            get
            {
                if (Difficulty == null)
                    Difficulty = SettlersConfiguration.DefaultDifficulty;

                return Difficulty.Name;
            }

            set
            {
                if (value != null && !GameDifficulty.GameDifficulties.ContainsKey(value))
                    Difficulty = SettlersConfiguration.DefaultDifficulty;
                else
                    Difficulty = GameDifficulty.GameDifficulties[value];
            }
        }
        public int MaxPerSpawn
        {
            get
            {
                var max = SettlerManager.MIN_PERSPAWN;

                if (ColonyRef != null && ColonyRef.FollowerCount >= SettlerManager.MAX_BUYABLE)
                    max += Pipliz.Random.Next((int)ColonyRef.TemporaryData.GetAsOrDefault(PandaResearch.GetResearchKey(PandaResearch.MinSettlers), 0f),
                                                SettlerManager.ABSOLUTE_MAX_PERSPAWN + (int)ColonyRef.TemporaryData.GetAsOrDefault(PandaResearch.GetResearchKey(PandaResearch.MaxSettlers), 0f));

                return max;
            }
        }

        public ColonyState(Colony c)
        {
            ColonyRef = c;
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnUpdate, GameLoader.NAMESPACE + ".Entities.ColonyState.OnUpdate")]
        public static void OnUpdate()
        {
            if (ServerManager.ColonyTracker != null)
            foreach (var c in ServerManager.ColonyTracker.ColoniesByID.Values)
            {
                if (c.OwnerIsOnline())
                {
                    try
                    {
                        var ps = GetColonyState(c);

                        if (ps.NextColonistBuyTime != 0 && TimeCycle.TotalTime.Value.Hours > ps.NextColonistBuyTime)
                        {
                            PandaChat.Send(c, "The compounding cost of buying colonists has been reset.", ChatColor.orange);
                            ps.NextColonistBuyTime = 0;
                            ps.ColonistsBought = 0;
                        }
                    }
                    catch (Exception ex)
                    {
                        PandaLogger.LogError(ex);
                    }
                }
            }
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnLoadingColony, GameLoader.NAMESPACE + ".Entities.ColonyState.OnLoadingColony")]
        public static void OnLoadingColony(Colony c, JSONNode n)
        {
            if (!_colonyStates.ContainsKey(c))
                _colonyStates.Add(c, new ColonyState(c));

            if (n.TryGetChild(GameLoader.NAMESPACE + ".ColonyState", out var stateNode))
            {
                if (stateNode.TryGetAs("Difficulty", out string diff))
                    _colonyStates[c].DifficultyStr = diff;

                if (stateNode.TryGetAs(nameof(BossesEnabled), out bool bosses))
                    _colonyStates[c].BossesEnabled = bosses;

                if (stateNode.TryGetAs(nameof(NotifySettlers), out bool settlers))
                    _colonyStates[c].NotifySettlers = settlers;

                if (stateNode.TryGetAs(nameof(SettlersEnabled), out bool notify))
                    _colonyStates[c].SettlersEnabled = notify;

                if (stateNode.TryGetAs(nameof(HighestColonistCount), out int hsc))
                    _colonyStates[c].HighestColonistCount = hsc;

                if (stateNode.TryGetAs(nameof(NeedsABed), out int nb))
                    _colonyStates[c].NeedsABed = nb;

                if (stateNode.TryGetAs(nameof(SettlersToggledTimes), out int stt))
                    _colonyStates[c].SettlersToggledTimes = stt;
            }
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnSavingColony, GameLoader.NAMESPACE + ".Entities.ColonyState.OnSavingColony")]
        public static void OnSavingColony(Colony c, JSONNode n)
        {
            if (_colonyStates.ContainsKey(c))
            {
                var node = new JSONNode();

                node.SetAs("Difficulty", _colonyStates[c].DifficultyStr);
                node.SetAs(nameof(BossesEnabled), _colonyStates[c].BossesEnabled);
                node.SetAs(nameof(SettlersEnabled), _colonyStates[c].SettlersEnabled);
                node.SetAs(nameof(HighestColonistCount), _colonyStates[c].HighestColonistCount);
                node.SetAs(nameof(SettlersToggledTimes), _colonyStates[c].SettlersToggledTimes);
                node.SetAs(nameof(NotifySettlers), _colonyStates[c].NotifySettlers);

                n.SetAs(GameLoader.NAMESPACE + ".ColonyState", node);
            }
        }

        public static ColonyState GetColonyState(Colony c)
        {
            if (c != null)
            {
                if (!_colonyStates.ContainsKey(c))
                    _colonyStates.Add(c, new ColonyState(c));

                return _colonyStates[c];
            }

            return null;
        }
    }
}
