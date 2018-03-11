using NPC;
using Pandaros.Settlers.AI;
using Pandaros.Settlers.Entities;
using Pandaros.Settlers.Research;
using Pipliz;
using Pipliz.Chatting;
using Pipliz.JSON;
using Server.NPCs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using UnityEngine;

namespace Pandaros.Settlers.Managers
{
    [ModLoader.ModManager]
    public static class SettlerManager
    {
        public const int MAX_BUYABLE = 10;
        public const int MIN_PERSPAWN = 5;
        public const int ABSOLUTE_MAX_PERSPAWN = 20;
        public const double BED_LEAVE_HOURS = 5;
        private const string LAST_KNOWN_JOB_TIME_KEY = "lastKnownTime";
        public static readonly double LOABOROR_LEAVE_HOURS = TimeSpan.FromDays(7).TotalHours;

        private static float _baseFoodPerHour;
        private static double _updateTime;
        private static double _nextSettlerCheck;
        private static int _idNext = 1;

        public static List<HealingOverTimeNPC> HealingSpells { get; private set; } = new List<HealingOverTimeNPC>();

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

            HealingOverTimeNPC.NewInstance += HealingOverTimeNPC_NewInstance;
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnPlayerClicked, GameLoader.NAMESPACE + ".SettlerManager.OnPlayerClicked")]
        public static void OnPlayerClicked(Players.Player player, Pipliz.Box<Shared.PlayerClickedData> boxedData)
        {
            if (boxedData.item1.clickType == Shared.PlayerClickedData.ClickType.Right &&
                boxedData.item1.rayCastHit.rayHitType == Shared.RayHitType.Block &&
                World.TryGetTypeAt(boxedData.item1.rayCastHit.voxelHit, out var blockHit) &&
                blockHit == BlockTypes.Builtin.BuiltinBlocks.BerryBush)
            {
                var inv = Inventory.GetInventory(player);
                inv.TryAdd(BlockTypes.Builtin.BuiltinBlocks.Berry, 2);
            }
        }

        private static void HealingOverTimeNPC_NewInstance(object sender, EventArgs e)
        {
            var healing = sender as HealingOverTimeNPC;

            lock (HealingSpells)
                HealingSpells.Add(healing);

            healing.Complete += Healing_Complete;
        }

        private static void Healing_Complete(object sender, EventArgs e)
        {
            var healing = sender as HealingOverTimeNPC;

            lock (HealingSpells)
                HealingSpells.Remove(healing);

            healing.Complete -= Healing_Complete;
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnUpdate, GameLoader.NAMESPACE + ".SettlerManager.OnUpdate")]
        public static void OnUpdate()
        {
            if (_updateTime < Pipliz.Time.SecondsSinceStartDouble && TimeCycle.IsDay)
            {
                Players.PlayerDatabase.ForeachValue(p =>
                {
                    if (p.IsConnected)
                    {
                        var colony = Colony.Get(p);
                        NPCBase lastNPC = null;

                        foreach (var follower in colony.Followers)
                        {
                            if (lastNPC == null || (Vector3.Distance(lastNPC.Position.Vector, follower.Position.Vector) > 15 && Pipliz.Random.NextBool()))
                            {
                                lastNPC = follower;
                                ServerManager.SendAudio(follower.Position.Vector, GameLoader.NAMESPACE + "TalkingAudio");
                            }
                        }

                        if ( EvaluateSettlers(p))
                            UpdateFoodUse(p);
                    }
                });

                _updateTime = Pipliz.Time.SecondsSinceStartDouble + 10;
            }
            else
            {
                Players.PlayerDatabase.ForeachValue(p =>
                {
                    var stockpile = Stockpile.GetStockPile(p);
                    var colony = Colony.Get(p);
                    var hasBandages = stockpile.Contains(Items.Healing.TreatedBandage.Item.ItemIndex) ||
                            stockpile.Contains(Items.Healing.Bandage.Item.ItemIndex);

                    if (hasBandages)
                        foreach (var follower in colony.Followers)
                        {
                            if (follower.health < NPCBase.MaxHealth &&
                                !HealingOverTimeNPC.NPCIsBeingHealed(follower))
                            {
                                bool healing = false;

                                if (NPCBase.MaxHealth - follower.health > Items.Healing.TreatedBandage.INITIALHEAL)
                                {
                                    stockpile.TryRemove(Items.Healing.TreatedBandage.Item.ItemIndex);
                                    healing = true;
                                    ServerManager.SendAudio(follower.Position.Vector, GameLoader.NAMESPACE + ".Bandage");
                                    var heal = new Entities.HealingOverTimeNPC(follower, Items.Healing.TreatedBandage.INITIALHEAL, Items.Healing.TreatedBandage.TOTALHOT, 5, Items.Healing.TreatedBandage.Item.ItemIndex);
                                }

                                if (!healing)
                                {
                                    stockpile.TryRemove(Items.Healing.Bandage.Item.ItemIndex);
                                    healing = true;
                                    ServerManager.SendAudio(follower.Position.Vector, GameLoader.NAMESPACE + ".Bandage");
                                    var heal = new Entities.HealingOverTimeNPC(follower, Items.Healing.Bandage.INITIALHEAL, Items.Healing.Bandage.TOTALHOT, 5, Items.Healing.Bandage.Item.ItemIndex);
                                }
                            }
                        }
                });
            }
        }


        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterWorldLoad, GameLoader.NAMESPACE + ".SettlerManager.AfterWorldLoad")]
        public static void AfterWorldLoad()
        {
            _baseFoodPerHour = ServerManager.ServerVariables.NPCFoodUsePerHour;
            Players.PlayerDatabase.ForeachValue(p => UpdateFoodUse(p));
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnPlayerConnectedEarly, GameLoader.NAMESPACE + ".SettlerManager.OnPlayerConnectedEarly")]
        public static void OnPlayerConnectedEarly(Players.Player p)
        {
            if (p.IsConnected && !Configuration.OfflineColonies)
            {
                var jf = JobTracker.GetOrCreateJobFinder(p) as JobTracker.JobFinder;
                string file = $"{GameLoader.GAMEDATA_FOLDER}/savegames/{ServerManager.WorldName}/players/NPCArchive/{p.ID.steamID.ToString()}.json";

                if (File.Exists(file) && JSON.Deserialize(file, out var followersNode, false))
                {
                    PandaLogger.Log(ChatColor.cyan, $"Player {p.ID.steamID} is reconnected. Restoring Colony.");

                    foreach (var node in followersNode.LoopArray())
                    {
                        try
                        {
                            node.SetAs("id", GetAIID());

                            var npc = new NPCBase(p, node);
                            ModLoader.TriggerCallbacks<NPCBase, JSONNode>(ModLoader.EModCallbackType.OnNPCLoaded, npc, node);

                            foreach (var job in jf.openJobs)
                                if (node.TryGetAs("JobPoS", out JSONNode pos) && job.KeyLocation == (Vector3Int)pos)
                                {
                                    if (job.IsValid && job.NeedsNPC)
                                    {
                                        npc.TakeJob(job);
                                        job.NPC = npc;
                                        JobTracker.Remove(p, job.KeyLocation);
                                    }
                                    break;
                                }
                        }
                        catch (Exception ex)
                        {
                            PandaLogger.LogError(ex);
                        }
                    }
                    
                    JSON.Serialize(file, new JSONNode(NodeType.Array));
                    jf.Update();
                }
            }
        }

        private static int GetAIID()
        {
            while (true)
            {
                if (_idNext == 1000000000)
                {
                    _idNext = 1;
                }
                if (!NPCTracker.Contains(_idNext))
                {
                    break;
                }
               _idNext++;
            }

            return _idNext++;
        }


        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnPlayerConnectedLate, GameLoader.NAMESPACE + ".SettlerManager.OnPlayerConnectedLate")]
        public static void OnPlayerConnectedLate(Players.Player p)
        {
            if (Configuration.DifficutlyCanBeChanged)
                GameDifficultyChatCommand.PossibleCommands(p, ChatColor.grey);

            var ps = PlayerState.GetPlayerState(p);

            if (ps.SettlersEnabled)
                PandaChat.Send(p, string.Format("Recruiting over {0} colonists will cost {1}% of your food. If you build it... they will come.", MAX_BUYABLE, Configuration.GetorDefault("RecruitmentCostPercentOfFood", .10f) * 100), ChatColor.orange);

            if (ps.SettlersToggledTimes < Configuration.GetorDefault("MaxSettlersToggle", 3))
            {
                var settlers = ps.SettlersEnabled ? "on" : "off";

                if (Configuration.GetorDefault("MaxSettlersToggle", 3) > 0)
                    PandaChat.Send(p, $"To disable/enable gaining random settlers type '/settlers off' Note: this can only be used {Configuration.GetorDefault("MaxSettlersToggle", 3)} times.", ChatColor.orange);
                else
                    PandaChat.Send(p, $"To disable/enable gaining random settlers type '/settlers off'", ChatColor.orange);

                PandaChat.Send(p, $"Random Settlers are currently {settlers}.", ChatColor.orange);
            }

            UpdateFoodUse(p);

            Colony.Get(p).SendUpdate();
            Colony.SendColonistCount(p);
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnPlayerDisconnected, GameLoader.NAMESPACE + ".SettlerManager.OnPlayerDisconnected")]
        public static void OnPlayerDisconnected(Players.Player p)
        {
            SaveOffline(p);
        }

        public static void SaveOffline(Players.Player p)
        {
            if (p.ID == null || p.ID.steamID == null)
                return;

            try
            {
                string folder = $"{GameLoader.GAMEDATA_FOLDER}/savegames/{ServerManager.WorldName}/players/NPCArchive/";

                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);

                string file = $"{folder}{p.ID.steamID.ToString()}.json";

                if (!Configuration.OfflineColonies)
                {
                    if (!JSON.Deserialize(file, out var followers, false))
                        followers = new JSONNode(NodeType.Array);

                    followers.ClearChildren();
                    PandaLogger.Log(ChatColor.cyan, $"Player {p.ID.steamID} is disconnected. Clearing colony until reconnect.");
                    Colony colony = Colony.Get(p);
                    List<NPCBase> copyOfFollowers = new List<NPCBase>();

                    foreach (var follower in colony.Followers)
                    {

                        JSONNode jobloc = null;
                        if (follower.IsValid)
                        {
                            var job = follower.Job;

                            if (job != null && job.KeyLocation != Vector3Int.invalidPos)
                            {
                                jobloc = (JSONNode)job.KeyLocation;
                                follower.ClearJob();
                            }
                        }

                        if (follower.TryGetJSON(out var node))
                        {
                            if (jobloc != null)
                                node.SetAs("JobPoS", jobloc);

                            ModLoader.TriggerCallbacks<NPCBase, JSONNode>(ModLoader.EModCallbackType.OnNPCSaved, follower, node);
                            followers.AddToArray(node);
                            copyOfFollowers.Add(follower);
                        }
                    }


                    JSON.Serialize(file, followers);

                    foreach (var deadMan in copyOfFollowers)
                        deadMan.OnDeath();

                    Server.Monsters.MonsterTracker.KillAllZombies(p);
                }
            }
            catch (Exception ex)
            {
                PandaLogger.LogError(ex);
            }
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnNPCRecruited, GameLoader.NAMESPACE + ".SettlerManager.OnNPCRecruited")]
        public static void OnNPCRecruited(NPC.NPCBase npc)
        {
            var ps = PlayerState.GetPlayerState(npc.Colony.Owner);

            if (ps.SettlersEnabled && npc.Colony.FollowerCount > MAX_BUYABLE)
            {
                var cost =  npc.Colony.UsedStockpile.TotalFood * Configuration.GetorDefault("RecruitmentCostPercentOfFood", .20f);
                float num = 0f;

                if (cost < 1)
                    cost = 1;

                if (npc.Colony.UsedStockpile.TotalFood < cost || !npc.Colony.UsedStockpile.TryRemoveFood(ref num, cost))
                {
                    Chat.Send(npc.Colony.Owner, $"<color=red>Could not recruit a new colonist; not enough food in stockpile. {cost + ServerManager.ServerVariables.LaborerCost} food required.</color>", ChatSenderType.Server);
                    npc.Colony.UsedStockpile.Add(BlockTypes.Builtin.BuiltinBlocks.Bed, (int)System.Math.Floor(ServerManager.ServerVariables.LaborerCost / 3));
                    npc.health = 0;
                    npc.Update();
                    return;
                }
            }

            SettlerInventory.GetSettlerInventory(npc);
            UpdateFoodUse(npc.Colony.Owner);
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnNPCDied, GameLoader.NAMESPACE + ".SettlerManager.OnNPCRecruited")]
        public static void OnNPCDied(NPC.NPCBase npc)
        {
            SettlerInventory.GetSettlerInventory(npc);
            UpdateFoodUse(npc.Colony.Owner);
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
            if (Server.TerrainGeneration.TerrainGenerator.UsedGenerator != null &&
                Server.AI.AIManager.NPCPathFinder != null &&
                p.ID.type != NetworkID.IDType.Server)
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

        public static bool EvaluateSettlers(Players.Player p)
        {
            bool update = false;

            if (p.IsConnected)
            {
                Colony colony = Colony.Get(p);
                PlayerState state = PlayerState.GetPlayerState(p);

                if (state.NextGenTime == 0)
                    state.NextGenTime = TimeCycle.TotalTime + Pipliz.Random.Next(4, 14 - p.GetTempValues(true).GetOrDefault(PandaResearch.GetResearchKey(PandaResearch.TimeBetween), 0));

                if (TimeCycle.TotalTime > state.NextGenTime && colony.FollowerCount >= MAX_BUYABLE)
                {
                    float chance = p.GetTempValues(true).GetOrDefault(PandaResearch.GetResearchKey(PandaResearch.SettlerChance), 0f) + state.Difficulty.AdditionalChance;
                    chance += SettlerEvaluation.SpawnChance(p, colony, state);

                    var rand = Pipliz.Random.NextFloat();

                    if (chance > 0 && chance > rand)
                    {
                        var addCount = System.Math.Floor(state.MaxPerSpawn * chance);

                        // if we lost alot of colonists add extra to help build back up.
                        if (colony.FollowerCount < state.HighestColonistCount)
                        {
                            var diff = state.HighestColonistCount - colony.FollowerCount;
                            addCount += System.Math.Floor(diff * .25);
                        }

                        var skillChance = p.GetTempValues(true).GetOrDefault(PandaResearch.GetResearchKey(PandaResearch.SkilledLaborer), 0f);
                        int numbSkilled = 0;

                        rand = Pipliz.Random.NextFloat();

                        if (skillChance > rand)
                            numbSkilled = state.Rand.Next(1, 2 + p.GetTempValues(true).GetOrDefault(PandaResearch.GetResearchKey(PandaResearch.NumberSkilledLaborer), 0));

                        var reason = string.Format(SettlerReasoning.GetSettleReason(), addCount);

                        if (numbSkilled > 0)
                            if (numbSkilled == 1)
                                reason += string.Format(" {0} of them is skilled!", numbSkilled);
                            else
                                reason += string.Format(" {0} of them are skilled!", numbSkilled);

                        PandaChat.Send(p, reason, ChatColor.magenta);
                        var playerPos = new Vector3Int(p.Position);

                        for (int i = 0; i < addCount; i++)
                        {
                            NPCBase newGuy = new NPCBase(NPCType.GetByKeyNameOrDefault("pipliz.laborer"), BannerTracker.GetClosest(p, playerPos).KeyLocation.Vector, colony);
                            SettlerInventory.GetSettlerInventory(newGuy);

                            if (i <= numbSkilled)
                            {
                                var npcTemp = newGuy.GetTempValues(true);
                                npcTemp.Set(GameLoader.ALL_SKILLS, state.Rand.Next(1, 10) * 0.01f);
                            }

                            update = true;
                            colony.RegisterNPC(newGuy);
                            ModLoader.TriggerCallbacks(ModLoader.EModCallbackType.OnNPCRecruited, newGuy);
                        }

                        if (colony.FollowerCount > state.HighestColonistCount)
                            state.HighestColonistCount = colony.FollowerCount;
                    }

                    state.NextGenTime = TimeCycle.TotalTime + Pipliz.Random.Next(4, 14 - p.GetTempValues(true).GetOrDefault(PandaResearch.GetResearchKey(PandaResearch.TimeBetween), 0));
                }
            }

            return update;
        }
    }
}
