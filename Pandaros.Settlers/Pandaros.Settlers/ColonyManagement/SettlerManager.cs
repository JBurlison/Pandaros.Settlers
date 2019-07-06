using AI;
using BlockTypes;
using Jobs;
using Monsters;
using NetworkUI;
using NetworkUI.Items;
using NPC;
using Pandaros.Settlers.AI;
using Pandaros.Settlers.Entities;
using Pandaros.Settlers.Items.Armor;
using Pandaros.Settlers.Items.Healing;
using Pandaros.Settlers.Research;
using Pipliz;
using Pipliz.JSON;
using Recipes;
using Shared;
using System;
using System.Collections.Generic;
using System.IO;
using TerrainGeneration;
using static ItemTypes;
using Math = System.Math;
using Random = Pipliz.Random;
using Time = Pipliz.Time;

namespace Pandaros.Settlers.ColonyManagement
{
    [ModLoader.ModManager]
    public static class SettlerManager
    {
        public const int MAX_BUYABLE = 10;
        public const int MIN_PERSPAWN = 1;
        public const int ABSOLUTE_MAX_PERSPAWN = 5;
        public const string LAST_KNOWN_JOB_TIME_KEY = "lastKnownTime";
        public const string LEAVETIME_JOB = "LeaveTime_JOB";
        public const string LEAVETIME_BED = "LeaveTime_BED";
        public const string ISSETTLER = "isSettler";
        public const string KNOWN_ITTERATIONS = "SKILLED_ITTERATIONS";

        public const int _NUMBEROFCRAFTSPERPERCENT = 200;
        public const int _UPDATE_TIME = 10;
        public static double IN_GAME_HOUR_IN_SECONDS = 3600 / TimeCycle.Settings.GameTimeScale;
        public static double BED_LEAVE_HOURS = IN_GAME_HOUR_IN_SECONDS * 5;
        public static double LOABOROR_LEAVE_HOURS = TimeSpan.FromDays(7).TotalHours * IN_GAME_HOUR_IN_SECONDS;
        public static double COLD_LEAVE_HOURS = IN_GAME_HOUR_IN_SECONDS * 5;
        public static double HOT_LEAVE_HOURS = IN_GAME_HOUR_IN_SECONDS * 6;
        public static float _baseFoodPerHour;
        public static double _updateTime;
        public static double _magicUpdateTime = Time.SecondsSinceStartDouble + Random.Next(2, 5);
        public static double _nextLaborerTime = Time.SecondsSinceStartDouble + Random.Next(2, 6);
        public static double _nextbedTime = Time.SecondsSinceStartDouble + Random.Next(1, 2);

        public static List<HealingOverTimeNPC> HealingSpells { get; } = new List<HealingOverTimeNPC>();
        private static localization.LocalizationHelper _localizationHelper = new localization.LocalizationHelper("SettlerManager");

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterSelectedWorld, GameLoader.NAMESPACE + ".Managers.SettlerManager.AfterSelectedWorld.Healing")]
        public static void Healing()
        {
            HealingOverTimeNPC.NewInstance += HealingOverTimeNPC_NewInstance;
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnPlayerClicked,  GameLoader.NAMESPACE + ".SettlerManager.OnPlayerClicked")]
        public static void OnPlayerClicked(Players.Player player, PlayerClickedData playerClickData)
        {
            if (playerClickData.ClickType == PlayerClickedData.EClickType.Right &&
                playerClickData.HitType == PlayerClickedData.EHitType.Block &&
                World.TryGetTypeAt(playerClickData.GetVoxelHit().BlockHit, out ushort blockHit) &&
                blockHit == ColonyBuiltIn.ItemTypes.BERRYBUSH)
            {
                var inv = player.Inventory;
                inv.TryAdd(ColonyBuiltIn.ItemTypes.BERRY, 1);
            }
        }

        private static void HealingOverTimeNPC_NewInstance(object sender, EventArgs e)
        {
            var healing = sender as HealingOverTimeNPC;

            lock (HealingSpells)
            {
                HealingSpells.Add(healing);
            }

            healing.Complete += Healing_Complete;
        }

        private static void Healing_Complete(object sender, EventArgs e)
        {
            var healing = sender as HealingOverTimeNPC;

            lock (HealingSpells)
            {
                HealingSpells.Remove(healing);
            }

            healing.Complete -= Healing_Complete;
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnUpdate, GameLoader.NAMESPACE + ".SettlerManager.OnUpdate")]
        public static void OnUpdate()
        {
            if (ServerManager.ColonyTracker != null)
                foreach (var colony in ServerManager.ColonyTracker.ColoniesByID.Values)
                {
                    if (_magicUpdateTime < Time.SecondsSinceStartDouble)
                    {
                        foreach (var follower in colony.Followers)
                        {
                            var inv = SettlerInventory.GetSettlerInventory(follower);

                            if (inv.MagicItemUpdateTime < Time.SecondsSinceStartDouble)
                            {
                                foreach (var item in inv.Armor)
                                    if (item.Value.Id != 0 && ArmorFactory.ArmorLookup.TryGetValue(item.Value.Id, out var armor))
                                    {
                                        armor.Update();

                                        if (armor.HPTickRegen != 0)
                                            follower.Heal(armor.HPTickRegen);
                                    }

                                if (Items.Weapons.WeaponFactory.WeaponLookup.TryGetValue(inv.Weapon.Id, out var wep))
                                {
                                    wep.Update();

                                    if (wep.HPTickRegen != 0)
                                        follower.Heal(wep.HPTickRegen);
                                }

                                var hasBandages = colony.Stockpile.Contains(TreatedBandage.Item.ItemIndex) ||
                                      colony.Stockpile.Contains(Bandage.Item.ItemIndex);

                                if (hasBandages &&
                                    follower.health < follower.Colony.NPCHealthMax &&
                                    !HealingOverTimeNPC.NPCIsBeingHealed(follower))
                                {
                                    var healing = false;

                                    if (follower.Colony.NPCHealthMax - follower.health > TreatedBandage.INITIALHEAL)
                                    {
                                        colony.Stockpile.TryRemove(TreatedBandage.Item.ItemIndex);
                                        healing = true;
                                        AudioManager.SendAudio(follower.Position.Vector, GameLoader.NAMESPACE + ".Bandage");

                                        var heal = new HealingOverTimeNPC(follower, TreatedBandage.INITIALHEAL,
                                                                          TreatedBandage.TOTALHOT, 5,
                                                                          TreatedBandage.Item.ItemIndex);
                                    }

                                    if (!healing)
                                    {
                                        colony.Stockpile.TryRemove(Bandage.Item.ItemIndex);
                                        healing = true;
                                        AudioManager.SendAudio(follower.Position.Vector, GameLoader.NAMESPACE + ".Bandage");

                                        var heal = new HealingOverTimeNPC(follower, Bandage.INITIALHEAL, Bandage.TOTALHOT, 5,
                                                                          Bandage.Item.ItemIndex);
                                    }
                                }


                                inv.MagicItemUpdateTime += 5000;
                            }
                        }
                    }


                    if (_updateTime < Time.SecondsSinceStartDouble && colony.OwnerIsOnline())
                    {
                        NPCBase lastNPC = null;

                        foreach (var follower in colony.Followers)
                        {
                            if (TimeCycle.IsDay)
                                if (lastNPC == null ||
                                UnityEngine.Vector3.Distance(lastNPC.Position.Vector, follower.Position.Vector) > 15 &&
                                Random.NextBool())
                                {
                                    lastNPC = follower;
                                    AudioManager.SendAudio(follower.Position.Vector, GameLoader.NAMESPACE + ".TalkingAudio");
                                }
                        }
                    }

                    var cs = ColonyState.GetColonyState(colony);

                    if (cs.SettlersEnabled != Models.SettlersState.Disabled)
                        if (EvaluateSettlers(cs) ||
                            EvaluateLaborers(cs) ||
                            EvaluateBeds(cs))
                            colony.SendCommonData();

                    UpdateFoodUse(cs);
                }

            if (_magicUpdateTime < Time.SecondsSinceStartDouble)
                _magicUpdateTime = Time.SecondsSinceStartDouble + 1;

            if (_updateTime < Time.SecondsSinceStartDouble && TimeCycle.IsDay)
                _updateTime = Time.SecondsSinceStartDouble + _UPDATE_TIME;
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterWorldLoad, GameLoader.NAMESPACE + ".SettlerManager.AfterWorldLoad")]
        public static void AfterWorldLoad()
        {
            _baseFoodPerHour = 1;
            IN_GAME_HOUR_IN_SECONDS = 3600 / TimeCycle.Settings.GameTimeScale;
            BED_LEAVE_HOURS = IN_GAME_HOUR_IN_SECONDS * 5;
            LOABOROR_LEAVE_HOURS = TimeSpan.FromDays(7).TotalHours * IN_GAME_HOUR_IN_SECONDS;
            COLD_LEAVE_HOURS = IN_GAME_HOUR_IN_SECONDS * 5;
            HOT_LEAVE_HOURS = IN_GAME_HOUR_IN_SECONDS * 6;

            foreach (var p in ServerManager.ColonyTracker.ColoniesByID.Values)
                UpdateFoodUse(ColonyState.GetColonyState(p));
        }



        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnPlayerConnectedLate, GameLoader.NAMESPACE + ".SettlerManager.OnPlayerConnectedLate")]
        public static void OnPlayerConnectedLate(Players.Player p)
        {
            if (p == null || p.Colonies == null || p.Colonies.Length == 0)
                return;

            if (SettlersConfiguration.GetorDefault("SettlersEnabled", true) &&
                SettlersConfiguration.GetorDefault("MaxSettlersToggle", 4) > 0 &&
                p.ActiveColony != null)
            {
                var cs = ColonyState.GetColonyState(p.ActiveColony);

                if (cs.SettlersEnabled != Models.SettlersState.Disabled && SettlersConfiguration.GetorDefault("ColonistsRecruitment", true))
                    PandaChat.Send(p,
                                   string
                                      .Format("Recruiting over {0} colonists will cost the base food cost plus a compounding {1} food. This compounding value resets once per in game day. If you build it... they will come.",
                                              MAX_BUYABLE,
                                              SettlersConfiguration.GetorDefault("CompoundingFoodRecruitmentCost", 2)),
                                   ChatColor.orange);

                if (cs.SettlersToggledTimes < SettlersConfiguration.GetorDefault("MaxSettlersToggle", 4))
                {
                    var settlers = cs.SettlersEnabled.ToString();

                    if (SettlersConfiguration.GetorDefault("MaxSettlersToggle", 4) > 0)
                        PandaChat.Send(p,
                                       $"To disable/enable gaining random settlers type '/settlers off' Note: this can only be used {SettlersConfiguration.GetorDefault("MaxSettlersToggle", 4)} times.",
                                       ChatColor.orange);
                    else
                        PandaChat.Send(p, $"To disable/enable gaining random settlers type '/settlers off'",
                                       ChatColor.orange);

                    PandaChat.Send(p, $"Random Settlers are currently {settlers}!", ChatColor.orange);
                }
            }

            foreach (Colony c in p.Colonies)
            {
                UpdateFoodUse(ColonyState.GetColonyState(c));
                c.SendCommonData();
            }
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnNPCRecruited, GameLoader.NAMESPACE + ".SettlerManager.OnNPCRecruited")]
        public static void OnNPCRecruited(NPCBase npc)
        {
            try
            {
                if (npc.CustomData == null)
                    npc.CustomData = new JSONNode();

                if (npc.CustomData.TryGetAs(ISSETTLER, out bool settler) && settler)
                    return;

                var ps = ColonyState.GetColonyState(npc.Colony);

                npc.FoodHoursCarried = ServerManager.ServerSettings.NPCs.InitialFoodCarriedHours;

                if (ps.SettlersEnabled != Models.SettlersState.Disabled)
                {
                    if (SettlersConfiguration.GetorDefault("ColonistsRecruitment", true))
                    {
                        if (npc.Colony.FollowerCount > MAX_BUYABLE)
                        {
                            ps.ColonistsBought++;
                            ps.NextColonistBuyTime = TimeCycle.TotalTime.Value.Hours + 24;
                        }

                        SettlerInventory.GetSettlerInventory(npc);
                        UpdateFoodUse(ps);
                    }
                    else
                    {
                        PandaChat.Send(npc.Colony, "The server administrator has disabled recruitment of colonists while settlers are enabled.");
                        npc.health = 0;
                        npc.Update();
                    }
                }
            }
            catch (Exception ex)
            {
                PandaLogger.LogError(ex);
            }
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnNPCDied, GameLoader.NAMESPACE + ".SettlerManager.OnNPCDied")]
        public static void OnNPCDied(NPCBase npc)
        {
            SettlerInventory.GetSettlerInventory(npc);
            UpdateFoodUse(ColonyState.GetColonyState(npc.Colony));
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnNPCLoaded, GameLoader.NAMESPACE + ".SettlerManager.OnNPCLoaded")]
        public static void OnNPCLoaded(NPCBase npc, JSONNode node)
        {
            if (npc.CustomData == null)
                npc.CustomData = new JSONNode();

            if (node.TryGetAs<JSONNode>(GameLoader.SETTLER_INV, out var invNode))
                npc.CustomData.SetAs(GameLoader.SETTLER_INV, new SettlerInventory(invNode, npc));

            if (node.TryGetAs<double>(LEAVETIME_JOB, out var leaveTime))
                npc.CustomData.SetAs(LEAVETIME_JOB, leaveTime);

            if (node.TryGetAs<float>(GameLoader.ALL_SKILLS, out var skills))
                npc.CustomData.SetAs(GameLoader.ALL_SKILLS, skills);

            if (node.TryGetAs<int>(KNOWN_ITTERATIONS, out var jobItterations))
                npc.CustomData.SetAs(KNOWN_ITTERATIONS, jobItterations);
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnNPCSaved, GameLoader.NAMESPACE + ".SettlerManager.OnNPCSaved")]
        public static void OnNPCSaved(NPCBase npc, JSONNode node)
        {
            node.SetAs(GameLoader.SETTLER_INV, SettlerInventory.GetSettlerInventory(npc).ToJsonNode());

            if (npc.NPCType.IsLaborer && npc.CustomData.TryGetAs(LEAVETIME_JOB, out double leave))
                node.SetAs(LEAVETIME_JOB, leave);

            if (npc.CustomData.TryGetAs(GameLoader.ALL_SKILLS, out float allSkill))
                node.SetAs(GameLoader.ALL_SKILLS, allSkill);

            if (npc.CustomData.TryGetAs(KNOWN_ITTERATIONS, out int itt))
                node.SetAs(KNOWN_ITTERATIONS, itt);
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnNPCGathered, GameLoader.NAMESPACE + ".SettlerManager.OnNPCGathered")]
        public static void OnNPCGathered(IJob job, Vector3Int location, List<ItemTypeDrops> results)
        {
            if (job != null && job.NPC != null && results != null && results.Count > 0)
            {
                IncrimentSkill(job.NPC);
                var inv = SettlerInventory.GetSettlerInventory(job.NPC);
               
                foreach (var item in results)
                {
                    if (ItemTypes.TryGetType(item.Type, out var itemType))
                        inv.IncrimentStat(itemType.Name, item.Amount);
                }
            }
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnNPCCraftedRecipe, GameLoader.NAMESPACE + ".SettlerManager.OnNPCCraftedRecipe")]
        public static void OnNPCCraftedRecipe(IJob job, Recipe recipe, List<RecipeResult> results)
        {
            IncrimentSkill(job.NPC);

            var inv = SettlerInventory.GetSettlerInventory(job.NPC);
            inv.IncrimentStat("Number of Crafts");

            double weightSum = 0;
            double roll = Random.Next() + inv.GetSkillModifier();
            List<RecipeResult> bonusItems = new List<RecipeResult>();

            foreach (var item in results)
            {
                weightSum += 1;

                if (roll > weightSum)
                    bonusItems.Add(new RecipeResult(item.Type, item.Amount));

                inv.IncrimentStat(ItemTypes.GetType(item.Type).Name, item.Amount);
            }

            results.AddRange(bonusItems);

        }

        public static void IncrimentSkill(NPCBase npc)
        {
            GetSkillInformation(npc, out var nextLevel, out var itt, out var allSkill);

            if (itt >= nextLevel)
            {
                var nextFloat = allSkill + 0.005f;

                if (nextFloat > 0.25f)
                    nextFloat = 0.25f;

                npc.CustomData.SetAs(KNOWN_ITTERATIONS, 1);
                npc.CustomData.SetAs(GameLoader.ALL_SKILLS, nextFloat);
            }
        }

        public static void GetSkillInformation(NPCBase npc, out int nextLevel, out int itt, out float allSkill)
        {
            if (!npc.CustomData.TryGetAs(KNOWN_ITTERATIONS, out itt))
            {
                npc.CustomData.SetAs(KNOWN_ITTERATIONS, 1);
                itt = 1;
            }
            else
                npc.CustomData.SetAs(KNOWN_ITTERATIONS, itt + 1);

            if (!npc.CustomData.TryGetAs(GameLoader.ALL_SKILLS, out allSkill))
            {
                npc.CustomData.SetAs(GameLoader.ALL_SKILLS, 0.005f);
                allSkill = 0.005f;
            }

            nextLevel = Pipliz.Math.RoundToInt(allSkill * 1000) * _NUMBEROFCRAFTSPERPERCENT;
        }

        public static void UpdateFoodUse(ColonyState state)
        {
            //if (ServerManager.TerrainGenerator != null)
            //{
            //    var food = _baseFoodPerHour;

            //    if (state.Difficulty != GameDifficulty.Normal && state.ColonyRef.FollowerCount > 10)
            //    {
            //        var multiplier = .4 - state.ColonyRef.TemporaryData.GetAsOrDefault(GameLoader.NAMESPACE + ".ReducedWaste", 0f);
            //        multiplier = (multiplier + state.Difficulty.FoodMultiplier);

            //        food -= (float)((_baseFoodPerHour * multiplier));
            //    }

            //    if (state.ColonyRef.InSiegeMode)
            //        food -= food * .4f;


            //    state.FoodPerHour = food;

            //    foreach (var npc in state.ColonyRef.Followers)
            //        npc.FoodHoursCarried = food;

            //    state.ColonyRef.SendCommonData();
            //}
        }

        public static bool EvaluateSettlers(ColonyState state)
        {
            var update = false;

            if (state.ColonyRef.OwnerIsOnline())
            {
                if (state.NextGenTime == 0)
                    state.NextGenTime = Time.SecondsSinceStartDouble + Random.Next(8, 16) * IN_GAME_HOUR_IN_SECONDS;

                if (Time.SecondsSinceStartDouble > state.NextGenTime && state.ColonyRef.FollowerCount >= MAX_BUYABLE)
                {
                    var chance =
                        state.ColonyRef.TemporaryData.GetAsOrDefault(GameLoader.NAMESPACE + ".SettlerChance", 0f) +
                        state.Difficulty.AdditionalChance;

                    chance += SettlerEvaluation.SpawnChance(state);

                    var rand = Random.NextFloat();

                    if (chance > rand)
                    {
                        var addCount = Math.Floor(state.MaxPerSpawn * chance);

                        // if we lost alot of colonists add extra to help build back up.
                        if (state.ColonyRef.FollowerCount < state.HighestColonistCount)
                        {
                            var diff = state.HighestColonistCount - state.ColonyRef.FollowerCount;
                            addCount += Math.Floor(diff * .25);
                        }

                        try
                        {
                            var skillChance = state.ColonyRef.TemporaryData.GetAsOrDefault(GameLoader.NAMESPACE + ".SkilledLaborer", 0f);
                            var numbSkilled = 0;
                            rand = Random.NextFloat();

                            try
                            {
                                if (skillChance > rand)
                                    numbSkilled = Pipliz.Random.Next(1,
                                                        2 + Pipliz.Math.RoundToInt(state.ColonyRef.TemporaryData.GetAsOrDefault(GameLoader.NAMESPACE + ".NumberSkilledLaborer", 0f)));
                            }
                            catch (Exception ex)
                            {
                                PandaLogger.Log("NumberSkilledLaborer");
                                PandaLogger.LogError(ex);
                            }


                            if (addCount > 0)
                            {
                                if (addCount > 30)
                                    addCount = 30;

                                if (state.SettlersEnabled == Models.SettlersState.AlwaysAccept)
                                    AddNewSettlers(addCount, numbSkilled, state);
                                else
                                    foreach (var p in state.ColonyRef.Owners)
                                    {
                                        if (p.IsConnected())
                                        {
                                            NetworkMenu menu = new NetworkMenu();
                                            menu.LocalStorage.SetAs("header", state.ColonyRef.Name + ": " + addCount + _localizationHelper.LocalizeOrDefault("NewSettlers", p));
                                            menu.Width = 600;
                                            menu.Height = 300;

                                            menu.Items.Add(new ButtonCallback(GameLoader.NAMESPACE + ".NewSettlers.Accept." + addCount + "." + numbSkilled,
                                                                              new LabelData(_localizationHelper.GetLocalizationKey("Accept"),
                                                                              UnityEngine.Color.black,
                                                                              UnityEngine.TextAnchor.MiddleCenter)));

                                            menu.Items.Add(new ButtonCallback(GameLoader.NAMESPACE + ".NewSettlers.Decline",
                                                                              new LabelData(_localizationHelper.GetLocalizationKey("Decline"),
                                                                              UnityEngine.Color.black,
                                                                              UnityEngine.TextAnchor.MiddleCenter)));

                                            NetworkMenuManager.SendServerPopup(p, menu);
                                        }
                                    }
                            }
                        }
                        catch (Exception ex)
                        {
                            PandaLogger.Log("SkilledLaborer");
                            PandaLogger.LogError(ex);
                        }

                        if (state.ColonyRef.FollowerCount > state.HighestColonistCount)
                            state.HighestColonistCount = state.ColonyRef.FollowerCount;
                    }


                    state.NextGenTime = Time.SecondsSinceStartDouble + Random.Next(8, 16) * IN_GAME_HOUR_IN_SECONDS;

                    state.ColonyRef.SendCommonData();
                }
            }

            return update;
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnPlayerPushedNetworkUIButton, GameLoader.NAMESPACE + ".ColonyManager.ColonyTool.PressButton")]
        public static void PressButton(ButtonPressCallbackData data)
        {
            if (!data.ButtonIdentifier.Contains(GameLoader.NAMESPACE + ".NewSettlers.Accept.") &&
                !data.ButtonIdentifier.Contains(GameLoader.NAMESPACE + ".NewSettlers.Decline"))
                return;

            foreach (var p in data.Player.ActiveColony.Owners)
                if (p.IsConnected())
                    NetworkMenuManager.CloseServerPopup(p);

            if (data.ButtonIdentifier.Contains(GameLoader.NAMESPACE + ".NewSettlers.Accept."))
            {
                var recruitmentInfoStr = data.ButtonIdentifier.Replace(GameLoader.NAMESPACE + ".NewSettlers.Accept.", "");
                var unparsedString = recruitmentInfoStr.Split('.');
                var addCount = int.Parse(unparsedString[0]);
                var numbSkilled = int.Parse(unparsedString[1]);
                var state = ColonyState.GetColonyState(data.Player.ActiveColony);

                AddNewSettlers(addCount, numbSkilled, state);
            }
        }

        private static void AddNewSettlers(double addCount, int numbSkilled, ColonyState state)
        {
            var reason = string.Format(SettlerReasoning.GetSettleReason(), addCount);

            if (numbSkilled > 0)
                if (numbSkilled == 1)
                    reason += string.Format(" {0} of them is skilled!", numbSkilled);
                else
                    reason += string.Format(" {0} of them are skilled!", numbSkilled);

            PandaChat.Send(state.ColonyRef, reason, ChatColor.magenta);

            for (var i = 0; i < addCount; i++)
            {
                var newGuy = new NPCBase(state.ColonyRef, state.ColonyRef.GetRandomBanner().Position);

                NPCTracker.Add(newGuy);
                state.ColonyRef.RegisterNPC(newGuy);
                SettlerInventory.GetSettlerInventory(newGuy);
                newGuy.CustomData.SetAs(ISSETTLER, true);

                if (i <= numbSkilled)
                    newGuy.CustomData.SetAs(GameLoader.ALL_SKILLS, Random.Next(1, 10) * 0.002f);

                ModLoader.Callbacks.OnNPCRecruited.Invoke(newGuy);
            }
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnNPCJobChanged, GameLoader.NAMESPACE + ".SettlerManager.OnNPCJobChanged")]
        public static void OnNPCJobChanged(ValueTuple<NPCBase, IJob, IJob> data)
        {
            try
            {
                if (data.Item1 != null && !data.Item1.NPCType.IsLaborer)
                    data.Item1.CustomData.SetAs(LEAVETIME_JOB, 0);

                if (data.Item3 is GuardJobInstance guardJob)
                {
                    var settings = (GuardJobSettings)guardJob.Settings;

                    if (settings != null)
                        guardJob.Settings = new GuardJobSettings()
                        {
                            BlockTypes = settings.BlockTypes,
                            CooldownMissingItem = settings.CooldownMissingItem,
                            CooldownSearchingTarget = settings.CooldownSearchingTarget,
                            CooldownShot = settings.CooldownShot,
                            Damage = settings.Damage,
                            NPCType = settings.NPCType,
                            NPCTypeKey = settings.NPCTypeKey,
                            OnHitAudio = settings.OnHitAudio,
                            OnShootAudio = settings.OnShootAudio,
                            OnShootResultItem = settings.OnShootResultItem,
                            Range = settings.Range,
                            RecruitmentItem = settings.RecruitmentItem,
                            ShootItem = settings.ShootItem,
                            SleepType = settings.SleepType
                        };
                }
                else if (data.Item3 is CraftingJobInstance craftingJob)
                {
                    var settings = (CraftingJobSettings)craftingJob.Settings;

                    if (settings != null)
                        craftingJob.Settings = new CraftingJobSettings()
                        {
                            BlockTypes = settings.BlockTypes,
                            CraftingCooldown = settings.CraftingCooldown,
                            MaxCraftsPerHaul = settings.MaxCraftsPerHaul,
                            NPCType = settings.NPCType,
                            NPCTypeKey = settings.NPCTypeKey,
                            OnCraftedAudio = settings.OnCraftedAudio,
                            RecruitmentItem = settings.RecruitmentItem
                        };
                }
                
                data.Item1?.ApplyJobResearch();
            }
            catch (Exception ex)
            {
                PandaLogger.LogError(ex);
            }
        }

        public static void ApplyJobCooldownsToNPCs(Colony c)
        {
            foreach (var npc in c.Followers)
                npc.ApplyJobResearch();
        }

        private static bool EvaluateLaborers(ColonyState state)
        {
            var update = false;

            if (TimeCycle.IsDay && Time.SecondsSinceStartDouble > _nextLaborerTime)
            {
                if (state.ColonyRef.OwnerIsOnline())
                {
                    var unTrack = new List<NPCBase>();
                    var left    = 0;

                    for (var i = 0; i < state.ColonyRef.LaborerCount; i++)
                    {
                        var npc     = state.ColonyRef.FindLaborer(i);

                        if (!npc.CustomData.TryGetAs(LEAVETIME_JOB, out double leaveTime))
                        {
                            npc.CustomData.SetAs(LEAVETIME_JOB, Time.SecondsSinceStartDouble + LOABOROR_LEAVE_HOURS);
                        }
                        else if (leaveTime < Time.SecondsSinceStartDouble)
                        {
                            left++;
                            NPCLeaving(npc);
                        }
                    }

                    if (left > 0)
                        PandaChat.Send(state.ColonyRef,
                                       string.Concat(SettlerReasoning.GetNoJobReason(),
                                                     string.Format(" {0} colonists have left your colony.", left)),
                                       ChatColor.red);

                    update = unTrack.Count != 0;
                    state.ColonyRef.SendCommonData();
                }

                _nextLaborerTime = Time.SecondsSinceStartDouble + Random.Next(4, 6) * IN_GAME_HOUR_IN_SECONDS * 24;
            }

            return update;
        }

        private static void NPCLeaving(NPCBase npc)
        {
            if (Random.NextFloat() > .49f)
            {
                float cost = PenalizeFood(npc.Colony, 0.05f);
                PandaChat.Send(npc.Colony, $"A colonist has left your colony taking {cost} food.", ChatColor.red);
            }
            else
            {
                var numberOfItems = Random.Next(1, 10);

                for (var i = 0; i < numberOfItems; i++)
                {
                    var randItem = Random.Next(npc.Colony.Stockpile.ItemCount);
                    var item     = npc.Colony.Stockpile.GetByIndex(randItem);

                    if (item.Type != ColonyBuiltIn.ItemTypes.AIR.Id && item.Amount != 0)
                    {
                        var leaveTax = Pipliz.Math.RoundToInt(item.Amount * .10);
                        npc.Colony.Stockpile.TryRemove(item.Type, leaveTax);
                        
                        PandaChat.Send(npc.Colony, $"A leaving colonist has taken {leaveTax} {Models.ItemId.GetItemId(item.Type).Name}", ChatColor.red);
                    }
                }

                PandaChat.Send(npc.Colony, $"A colonist has left your colony taking 10% of {numberOfItems} items from your stockpile.", ChatColor.red);
            }

            npc.health = 0;
            npc.OnDeath();
        }

        public static float PenalizeFood(Colony c, float percent)
        {
            var cost = (float)Math.Ceiling(c.Stockpile.TotalFood * percent);
            var num = 0f;

            if (cost < 1)
                cost = 1;

            c.Stockpile.TryRemoveFood(ref num, cost);
            return cost;
        }

        private static bool EvaluateBeds(ColonyState state)
        {
            var update = false;

            try
            {
                if (!TimeCycle.IsDay && Time.SecondsSinceStartDouble > _nextbedTime)
                {
                    if (state.ColonyRef.OwnerIsOnline())
                    {
                        // TODO Fix bed count
                        var remainingBeds = ServerManager.BlockEntityTracker.BedTracker.CalculateBedCount(state.ColonyRef) - state.ColonyRef.FollowerCount;
                        var left          = 0;

                        if (remainingBeds >= 0)
                        {
                            state.NeedsABed = 0;
                        }
                        else
                        {
                            if (state.NeedsABed == 0)
                            {
                                state.NeedsABed = Time.SecondsSinceStartDouble + LOABOROR_LEAVE_HOURS;
                                PandaChat.Send(state.ColonyRef, SettlerReasoning.GetNeedBed(), ChatColor.grey);
                            }

                            if (state.NeedsABed != 0 && state.NeedsABed < Time.SecondsSinceStartDouble)
                            {
                                foreach (var follower in state.ColonyRef.Followers)
                                    if (follower.UsedBed == null)
                                    {
                                        left++;
                                        NPCLeaving(follower);
                                    }

                                state.NeedsABed = 0;
                            }

                            if (left > 0)
                            {
                                PandaChat.Send(state.ColonyRef, string.Concat(SettlerReasoning.GetNoBed(), string.Format(" {0} colonists have left your colony.", left)), ChatColor.red);
                                update = true;
                            }
                        }

                        state.ColonyRef.SendCommonData();
                    }

                    _nextbedTime = Time.SecondsSinceStartDouble + Random.Next(5, 8) * IN_GAME_HOUR_IN_SECONDS * 24;
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