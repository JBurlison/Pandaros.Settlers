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
using UnityEngine;

namespace Pandaros.Settlers.Managers
{
    [ModLoader.ModManager]
    public class SettlerManager
    {
        private static float _baseFoodPerHour;
        private static double _updateTime;

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterSelectedWorld, GameLoader.NAMESPACE + ".Managers.SettlerManager.RegisterAudio"),
            ModLoader.ModCallbackProvidesFor("pipliz.server.loadaudiofiles"), ModLoader.ModCallbackDependsOn("pipliz.server.registeraudiofiles")]
        public static void RegisterAudio()
        {
            GameLoader.AddSoundFile(GameLoader.NAMESPACE + "TalkingAudio", new List<string>()
            {
                GameLoader.AUDIO_FOLDER_PANDA + "/Talking1.ogg",
                GameLoader.AUDIO_FOLDER_PANDA + "/Talking2.ogg",
                GameLoader.AUDIO_FOLDER_PANDA + "/Talking3.ogg",
                GameLoader.AUDIO_FOLDER_PANDA + "/Talking4.ogg",
                GameLoader.AUDIO_FOLDER_PANDA + "/Talking5.ogg"
            });
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnUpdate, GameLoader.NAMESPACE + ".SettlerManager.OnUpdate")]
        public static void OnUpdate()
        {
            if (_updateTime < Pipliz.Time.SecondsSinceStartDouble)
            {
                Players.PlayerDatabase.ForeachValue(p =>
                {
                    var colony = Colony.Get(p);
                    NPCBase lastNPC = null;

                    foreach (var follower in colony.Followers)
                    {
                        if (lastNPC == null || Vector3.Distance(lastNPC.Position.Vector, follower.Position.Vector) > 10)
                        {
                            lastNPC = follower;
                            ServerManager.SendAudio(follower.Position.Vector, GameLoader.NAMESPACE + "TalkingAudio");
                        }
                    }
                });

                _updateTime = Pipliz.Time.SecondsSinceStartDouble + 10;
            }
        }


        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterWorldLoad, GameLoader.NAMESPACE + ".SettlerManager.AfterWorldLoad")]
        public static void AfterWorldLoad()
        {
            _baseFoodPerHour = ServerManager.ServerVariables.NPCFoodUsePerHour;
            Players.PlayerDatabase.ForeachValue(p => UpdateFoodUse(p));
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnPlayerConnectedLate, GameLoader.NAMESPACE + ".SettlerManager.OnPlayerConnectedLate")]
        public static void OnPlayerConnectedLate(Players.Player p)
        {
            if (p.IsConnected)
            {
                Colony colony = Colony.Get(p);
                PlayerState state = PlayerState.GetPlayerState(p);
                GameDifficultyChatCommand.PossibleCommands(p, ChatColor.grey);
                UpdateFoodUse(p);
            }
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnNPCRecruited, GameLoader.NAMESPACE + ".SettlerManager.OnNPCRecruited")]
        public static void OnNPCRecruited(NPC.NPCBase npc)
        {
            SettlerInventory.GetSettlerInventory(npc);
            UpdateFoodUse(npc.Colony.Owner);
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnNPCDied, GameLoader.NAMESPACE + ".SettlerManager.OnNPCRecruited")]
        public static void OnNPCDied(NPC.NPCBase npc)
        {
            SettlerInventory.GetSettlerInventory(npc);
            UpdateFoodUse(npc.Colony.Owner);
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

        public static void UpdateFoodUse(Players.Player p)
        {
            if (p.IsConnected && p.ID.type != NetworkID.IDType.Server)
            {
                Colony colony = Colony.Get(p);
                PlayerState ps = PlayerState.GetPlayerState(p);
                var food = _baseFoodPerHour;

                if (ps.Difficulty != GameDifficulty.Normal && colony.FollowerCount > 10)
                {
                    var multiplier = (.7 / colony.FollowerCount) - p.GetTempValues(true).GetOrDefault(PandaResearch.GetResearchKey(PandaResearch.ReducedWaste), 0f);
                    food += (float)(_baseFoodPerHour * multiplier);
                    food *= ps.Difficulty.FoodMultiplier;
                }

                if (colony.InSiegeMode)
                    food = food * ServerManager.ServerVariables.NPCfoodUseMultiplierSiegeMode;

                if (food < _baseFoodPerHour)
                    food = _baseFoodPerHour;

                colony.FoodUsePerHour = food;
                colony.SendUpdate();
            }
        }
    }
}
