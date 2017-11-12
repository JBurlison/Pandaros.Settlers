using NPC;
using Pandaros.Settlers.AI;
using Pandaros.Settlers.Entities;
using Pandaros.Settlers.Research;
using Pipliz;
using Pipliz.APIProvider.Jobs;
using Pipliz.BlockNPCs.Implementations;
using Pipliz.Chatting;
using Pipliz.JSON;
using Server.NPCs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Pandaros.Settlers.Managers
{
    [ModLoader.ModManager]
    public class SettlerManager
    {
        private static System.Random _r = new System.Random();
        private static float _baseFoodPerHour;
        private static Thread _foodThread = new Thread(() => UpdateFoodUse());


        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterWorldLoad, GameLoader.NAMESPACE + ".SettlerManager.AfterWorldLoad")]
        public static void AfterWorldLoad()
        {
            _baseFoodPerHour = ServerManager.ServerVariables.NPCFoodUsePerHour;
            _foodThread.IsBackground = true;
            _foodThread.Start();
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnPlayerConnectedLate, GameLoader.NAMESPACE + ".SettlerManager.OnPlayerConnectedLate")]
        public static void OnPlayerConnectedLate(Players.Player p)
        {
            if (p.IsConnected)
            {
                Colony colony = Colony.Get(p);
                PlayerState state = PlayerState.GetPlayerState(p);
                GameDifficultyChatCommand.PossibleCommands(p, ChatColor.grey);
            }
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnNPCRecruited, GameLoader.NAMESPACE + ".SettlerManager.OnNPCRecruited")]
        public static void OnNPCRecruited(NPC.NPCBase npc)
        {
            SettlerInventory.GetSettlerInventory(npc);
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnNPCJobChanged, GameLoader.NAMESPACE + ".SettlerManager.OnNPCJobChanged")]
        public static void OnNPCJobChanged(NPC.NPCBase npc)
        {
            var tmpVals = npc.GetTempValues();

            if (npc.Job != null && !npc.Job.NPCType.IsLaborer)
            {
                var skilled = tmpVals.GetOrDefault(GameLoader.ALL_SKILLS, 0f);
                var inv = SettlerInventory.GetSettlerInventory(npc);
                var jobName = npc.Job.NPCType.ToString();

                if (!inv.JobSkills.ContainsKey(jobName))
                    inv.JobSkills[jobName] = skilled;
            }
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnNPCLoaded, GameLoader.NAMESPACE + ".SettlerManager.OnNPCLoaded")]
        public static void OnNPCLoaded(NPC.NPCBase npc, JSONNode node)
        {
            if (node.TryGetAs<JSONNode>(GameLoader.SETTLER_INV, out var invNode))
                npc.GetTempValues(true).Set(GameLoader.SETTLER_INV, new SettlerInventory(invNode));
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnNPCSaved, GameLoader.NAMESPACE + ".SettlerManager.OnNPCSaved")]
        public static void OnNPCSaved(NPC.NPCBase npc, JSONNode node)
        {
            node.SetAs(GameLoader.SETTLER_INV, SettlerInventory.GetSettlerInventory(npc).ToJsonNode());
        }

        public static void UpdateFoodUse()
        {
            while (GameLoader.WorldLoaded)
            {
                Players.PlayerDatabase.ForeachValue(p =>
                {
                    if (p.IsConnected && p.ID.type != NetworkID.IDType.Server)
                    {
                        Colony colony = Colony.Get(p);
                        PlayerState ps = PlayerState.GetPlayerState(p);

                        var food = _baseFoodPerHour;

                        if (ps.Difficulty != GameDifficulty.Normal && colony.FollowerCount > 10)
                        {
                            var foodDivider = _r.NextDouble(colony.FollowerCount * .3, colony.FollowerCount * .55);
                            var multiplier = (foodDivider / colony.FollowerCount) - p.GetTempValues(true).GetOrDefault(PandaResearch.GetResearchKey(PandaResearch.ReducedWaste), 0f);

                            food += (float)(_baseFoodPerHour * multiplier);
                            food = food * ps.Difficulty.FoodMultiplier;
                        }

                        if (colony.InSiegeMode)
                            food = food * ServerManager.ServerVariables.NPCfoodUseMultiplierSiegeMode;

                        if (food < _baseFoodPerHour)
                            food = _baseFoodPerHour;

                        colony.FoodUsePerHour = food;
                    }
                });

                Thread.Sleep(10000);
            }
        }
    }
}
