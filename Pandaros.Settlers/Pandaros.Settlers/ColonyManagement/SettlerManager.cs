using NetworkUI;
using NetworkUI.Items;
using NPC;
using Pandaros.API;
using Pandaros.API.ColonyManagement;
using Pandaros.API.Entities;
using Pandaros.API.Extender;
using Pandaros.API.localization;
using Pandaros.API.Models;
using Pandaros.Settlers.AI;
using Pandaros.Settlers.Items.Healing;
using Pipliz.JSON;
using Shared;
using System;
using System.Collections.Generic;
using Math = System.Math;
using Random = Pipliz.Random;
using Time = Pipliz.Time;

namespace Pandaros.Settlers.ColonyManagement
{
    [ModLoader.ModManager]
    public class SettlerManagerUIPromopt : IOnConstructInventoryManageColonyUI
    {
        static readonly LocalizationHelper _localizationHelper = new LocalizationHelper(GameLoader.NAMESPACE, "colonytool");

        public void OnConstructInventoryManageColonyUI(Players.Player player, NetworkMenu networkMenu, (Table, Table) table)
        {
            if (player.ActiveColony != null)
                networkMenu.Items.Add(new ButtonCallback(GameLoader.NAMESPACE + ".UnemployedLength", new LabelData(_localizationHelper.GetLocalizationKey("UnemployedLength"), UnityEngine.Color.black), 200));
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnPlayerPushedNetworkUIButton, GameLoader.NAMESPACE + ".ColonyManagement.SettlerManagerUIPromopt.PressButton")]
        public static void PressButton(ButtonPressCallbackData data)
        {
            if (data.ButtonIdentifier == GameLoader.NAMESPACE + ".UnemployedLength" && data.Player.ActiveColony != null)
            {
                NetworkMenu menu = new NetworkMenu();
                menu.LocalStorage.SetAs("header", _localizationHelper.LocalizeOrDefault("UnemployedLength", data.Player));
                menu.Width = 800;
                menu.Height = 600;
                menu.ForceClosePopups = true;

                List<ValueTuple<IItem, int>> headerItems = new List<ValueTuple<IItem, int>>();

                headerItems.Add((new Label(new LabelData(_localizationHelper.GetLocalizationKey("Colonist"), UnityEngine.Color.black)), 600));
                headerItems.Add((new Label(new LabelData(_localizationHelper.GetLocalizationKey("TimeToLeave"), UnityEngine.Color.black, UnityEngine.TextAnchor.MiddleCenter)), 200));

                menu.Items.Add(new HorizontalRow(headerItems));

                for (var i = 0; i < data.Player.ActiveColony.LaborerCount; i++)
                {
                    var npc = data.Player.ActiveColony.FindLaborer(i);
                    var npcState = ColonistInventory.Get(npc);

                    List<ValueTuple<IItem, int>> items = new List<ValueTuple<IItem, int>>();
                    items.Add((new Label(new LabelData(npcState.ColonistsName, UnityEngine.Color.black)), 400));
                    items.Add((new Label(new LabelData(Pipliz.Math.RoundToInt(npcState.UnemployedLeaveTime - TimeCycle.TotalHours) + "h", UnityEngine.Color.black, UnityEngine.TextAnchor.MiddleCenter)), 200));
                    menu.Items.Add(new HorizontalRow(items));
                }

                NetworkMenuManager.SendServerPopup(data.Player, menu);
            }
        }
    }

    [ModLoader.ModManager]
    public static class SettlerManager
    {
        public const int MAX_BUYABLE = 10;
        public const int MIN_PERSPAWN = 1;
        public const int ABSOLUTE_MAX_PERSPAWN = 5;
        public const string LAST_KNOWN_JOB_TIME_KEY = "lastKnownTime";
        public const string LEAVETIME_BED = "LeaveTime_BED";
        public const string ISSETTLER = "isSettler";
        public const string KNOWN_ITTERATIONS = "SKILLED_ITTERATIONS";

        public const int _NUMBEROFCRAFTSPERPERCENT = 200;
        public const int _UPDATE_TIME = 10;
        public static double BED_LEAVE_HOURS = 5;
        public static double COLD_LEAVE_HOURS = 5;
        public static double HOT_LEAVE_HOURS = 6;
        public static double _updateTime;

        public static List<HealingOverTimeNPC> HealingSpells { get; } = new List<HealingOverTimeNPC>();
        private static LocalizationHelper _localizationHelper = new LocalizationHelper(GameLoader.NAMESPACE, "SettlerManager");
        private static float _settlersBuffer = 0;

        public static int MaxPerSpawn(Colony ColonyRef)
        {
            var max = MIN_PERSPAWN;

            if (ColonyRef != null && ColonyRef.FollowerCount >= SettlerManager.MAX_BUYABLE)
                max += Pipliz.Random.Next((int)ColonyRef.TemporaryData.GetAsOrDefault(GameLoader.NAMESPACE + ".MinSettlers", 0f),
                                          ABSOLUTE_MAX_PERSPAWN + (int)ColonyRef.TemporaryData.GetAsOrDefault(GameLoader.NAMESPACE + ".MaxSettlers", 0f));

            return max;
        }

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
                    var cs = ColonyState.GetColonyState(colony);
                    EvaluateSettlers(cs);

                    if (cs.Difficulty.Name != GameDifficulty.Normal.Name)
                    {
                        EvaluateLaborers(cs);
                        EvaluateBeds(cs);
                        UpdateFoodUse(cs);
                    }

                    UpdateMagicItemms(cs);

                    colony.SendCommonData();

                    if (_updateTime < Time.SecondsLastFrame && colony.OwnerIsOnline())
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

                    if (cs.ColonyRef.FollowerCount > 10)
                    {
                        float foodConsumedPerSecondPerColonist = (cs.FoodPerHour * TimeCycle.GameTimeScale) / 3600;
                        float secondsTick = (float)Time.SecondsLastFrame;
                        float foodToConsume = foodConsumedPerSecondPerColonist * colony.FollowerCount * secondsTick;
                        float diff = Math.Min(foodToConsume, _settlersBuffer);
                        foodToConsume -= diff;
                        _settlersBuffer -= diff;

                        if (foodToConsume > 0.0001f)
                        {
                            colony.Stockpile.TryRemoveFood(ref _settlersBuffer, foodToConsume);
                        }
                    }
                }


            if (_updateTime < Time.SecondsLastFrame && TimeCycle.IsDay)
                _updateTime = Time.SecondsLastFrame + _UPDATE_TIME;
        }

        private static void UpdateMagicItemms(ColonyState state)
        {
            try
            {
                if (state.HealingUpdateTime < Time.SecondsSinceStartDouble)
                {
                    var colony = state.ColonyRef;

                    foreach (var follower in colony.Followers)
                    {
                        var inv = ColonistInventory.Get(follower);

                        if (inv.HealingItemUpdateTime < Time.SecondsSinceStartDouble)
                        {
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


                            inv.HealingItemUpdateTime = Time.SecondsSinceStartDouble + Random.Next(3, 5);
                        }
                    }

                    state.HealingUpdateTime = Time.SecondsSinceStartDouble + 5;
                }
            }
            catch (Exception ex)
            {
                SettlersLogger.LogError(ex);
            }
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterWorldLoad, GameLoader.NAMESPACE + ".SettlerManager.AfterWorldLoad")]
        public static void AfterWorldLoad()
        {
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

                if (cs.SettlersEnabled != SettlersState.Disabled && SettlersConfiguration.GetorDefault("ColonistsRecruitment", true))
                    PandaChat.Send(p, _localizationHelper, "BuyingColonists", ChatColor.orange, MAX_BUYABLE.ToString(), cs.Difficulty.GetorDefault("UnhappyColonistsBought", 1).ToString());

                if (cs.SettlersToggledTimes < SettlersConfiguration.GetorDefault("MaxSettlersToggle", 4))
                    PandaChat.Send(p, _localizationHelper, "SettlersEnabled", ChatColor.orange, cs.SettlersEnabled.ToString());
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
                var npcInv = ColonistInventory.Get(npc);
                if (npc.CustomData == null)
                    npc.CustomData = new JSONNode();

                if (npc.NPCType.IsLaborer)
                    npcInv.UnemployedLeaveTime = TimeCycle.TotalHours + 48;

                if (npc.CustomData.TryGetAs(ISSETTLER, out bool settler) && settler)
                    return;

                var ps = ColonyState.GetColonyState(npc.Colony);

                npc.FoodHoursCarried = ServerManager.ServerSettings.NPCs.InitialFoodCarriedHours;

                if (ps.SettlersEnabled != API.Models.SettlersState.Disabled)
                {
                    if (SettlersConfiguration.GetorDefault("ColonistsRecruitment", true))
                    {
                        if (npc.Colony.FollowerCount > MAX_BUYABLE)
                        {
                            if (!ColonistsBought.BoughtCount.ContainsKey(npc.Colony))
                                ColonistsBought.BoughtCount.Add(npc.Colony, new List<double>());

                            ColonistsBought.BoughtCount[npc.Colony].Add(TimeCycle.TotalHours + 24);
                        }

                        ColonistInventory.Get(npc);
                        UpdateFoodUse(ps);
                    }
                    else
                    {
                        PandaChat.Send(npc.Colony, _localizationHelper, "AdminDisabled", ChatColor.red);
                        npc.health = 0;
                        npc.Update();
                    }
                }
            }
            catch (Exception ex)
            {
                SettlersLogger.LogError(ex);
            }
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnNPCDied, GameLoader.NAMESPACE + ".SettlerManager.OnNPCDied")]
        public static void OnNPCDied(NPCBase npc)
        {
            ColonistInventory.Get(npc);
            UpdateFoodUse(ColonyState.GetColonyState(npc.Colony));
        }

        public static void UpdateFoodUse(ColonyState state)
        {
            //if (ServerManager.TerrainGenerator != null && state.Difficulty.Name != GameDifficulty.Normal.Name)
            //{
            //    if (state.Difficulty != GameDifficulty.Normal && state.ColonyRef.FollowerCount > 10)
            //    {
            //        float multiplier = .01f - state.ColonyRef.TemporaryData.GetAsOrDefault(GameLoader.NAMESPACE + ".ReducedWaste", 0f);
            //        multiplier += (multiplier * (float)state.Difficulty.GetorDefault("FoodMultiplier", .4));
            //        state.FoodPerHour = multiplier;
            //    }
            //}
        }

        public static bool EvaluateSettlers(ColonyState state)
        {
            var update = false;

            if (state.SettlersEnabled != SettlersState.Disabled && state.ColonyRef.OwnerIsOnline())
            {
                if (state.NextGenTime == 0)
                    state.NextGenTime = TimeCycle.TotalHours + Random.Next(8, 16);

                if (TimeCycle.TotalHours > state.NextGenTime && state.ColonyRef.FollowerCount >= MAX_BUYABLE)
                {
                    var chance =
                        state.ColonyRef.TemporaryData.GetAsOrDefault(GameLoader.NAMESPACE + ".SettlerChance", 0f) +
                        state.Difficulty.GetorDefault("AdditionalChance", 0);

                    chance += SettlerEvaluation.SpawnChance(state);

                    var rand = Random.NextFloat();

                    if (chance > rand)
                    {
                        var addCount = Math.Floor(SettlerManager.MaxPerSpawn(state.ColonyRef) * chance);

                        // if we lost alot of colonists add extra to help build back up.
                        if (state.ColonyRef.FollowerCount < state.HighestColonistCount)
                        {
                            var diff = state.HighestColonistCount - state.ColonyRef.FollowerCount;
                            addCount += Math.Floor(diff * .25);
                        }

                        try
                        {
                            var skillChance = state.ColonyRef.TemporaryData.GetAsOrDefault(GameLoader.NAMESPACE + ".SkilledLaborer", 0f) + SkilledSettlerChance.GetSkilledSettlerChance(state.ColonyRef);
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
                                SettlersLogger.Log("NumberSkilledLaborer");
                                SettlersLogger.LogError(ex);
                            }


                            if (addCount > 0)
                            {
                                if (addCount > 30)
                                    addCount = 30;

                                if (state.SettlersEnabled == SettlersState.AlwaysAccept)
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

                                            menu.Items.Add(new ButtonCallback(GameLoader.NAMESPACE + ".NewSettlers." + state.ColonyRef.ColonyID + ".Accept." + addCount + "." + numbSkilled,
                                                                              new LabelData(_localizationHelper.GetLocalizationKey("Accept"),
                                                                              UnityEngine.Color.black,
                                                                              UnityEngine.TextAnchor.MiddleCenter)));

                                            menu.Items.Add(new ButtonCallback(GameLoader.NAMESPACE + ".NewSettlers." + state.ColonyRef.ColonyID + ".Decline",
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
                            SettlersLogger.Log("SkilledLaborer");
                            SettlersLogger.LogError(ex);
                        }

                        if (state.ColonyRef.FollowerCount > state.HighestColonistCount)
                            state.HighestColonistCount = state.ColonyRef.FollowerCount;
                    }


                    state.NextGenTime = TimeCycle.TotalHours + Random.Next(8, 16);

                    state.ColonyRef.SendCommonData();
                }
            }

            return update;
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnPlayerPushedNetworkUIButton, GameLoader.NAMESPACE + ".ColonyManagement.SettlerManager.PressButton")]
        public static void PressButton(ButtonPressCallbackData data)
        {
            if (!data.ButtonIdentifier.Contains(GameLoader.NAMESPACE + ".NewSettlers") &&
                !data.ButtonIdentifier.Contains("Decline"))
                return;

            var replaceOne = data.ButtonIdentifier.Replace(GameLoader.NAMESPACE + ".NewSettlers.", "");
            var val = replaceOne.Substring(0, replaceOne.IndexOf('.'));

            if (int.TryParse(val, out int colonyId) && ServerManager.ColonyTracker.ColoniesByID.TryGetValue(colonyId, out var colony))
            {
                foreach (var p in colony.Owners)
                    if (p.IsConnected())
                        NetworkMenuManager.CloseServerPopup(p);

                if (data.ButtonIdentifier.Contains(".Decline"))
                    return;

                var recruitmentInfoStr = replaceOne.Substring(val.Length).Replace(".Accept.", "");
                var unparsedString = recruitmentInfoStr.Split('.');
                var addCount = int.Parse(unparsedString[0]);
                var numbSkilled = int.Parse(unparsedString[1]);
                var state = ColonyState.GetColonyState(colony);

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

            PandaChat.Send(state.ColonyRef, _localizationHelper, reason, ChatColor.green);

            for (var i = 0; i < addCount; i++)
            {
                var newGuy = new NPCBase(state.ColonyRef, state.ColonyRef.GetRandomBanner().Position);

                NPCTracker.Add(newGuy);
                state.ColonyRef.RegisterNPC(newGuy);
                ColonistInventory.Get(newGuy);
                newGuy.CustomData.SetAs(ISSETTLER, true);

                if (i <= numbSkilled)
                    newGuy.CustomData.SetAs(GameLoader.ALL_SKILLS, Random.Next(5, 10) * 0.005f);

                ModLoader.Callbacks.OnNPCRecruited.Invoke(newGuy);
            }
        }

        private static bool EvaluateLaborers(ColonyState state)
        {
            var update = false;

            if (TimeCycle.TotalHours > state.NextLaborerTime)
            {
                var unTrack = new List<NPCBase>();
                var left    = 0;
                var leaving = 0;
                List<NPCBase> leavingNPCs = new List<NPCBase>();

                foreach (var npc in state.ColonyRef.Followers)
                    if (npc.NPCType.IsLaborer)
                    {
                        leaving++;

                        if (leaving > 10)
                        {
                            var inv = ColonistInventory.Get(npc);

                            if (inv.UnemployedLeaveTime == 0)
                            {
                                inv.UnemployedLeaveTime = TimeCycle.TotalHours + 48;
                            }
                            else if (inv.UnemployedLeaveTime < TimeCycle.TotalHours)
                            {
                                left++;
                                leavingNPCs.Add(npc);
                            }
                        }
                    }

                foreach (var npc in leavingNPCs)
                    NPCLeaving(npc);

                if (left > 0)
                    PandaChat.Send(state.ColonyRef, _localizationHelper, "ColonistsLeft", ChatColor.red);

                update = unTrack.Count != 0;
                state.ColonyRef.SendCommonData();

                state.NextLaborerTime = TimeCycle.TotalHours + 1;
            }

            return update;
        }

        private static void NPCLeaving(NPCBase npc)
        {
            if (Random.NextFloat() > .49f)
            {
                float cost = ColonistManager.PenalizeFood(npc.Colony, 0.05f);

                PandaChat.Send(npc.Colony, _localizationHelper, "TakenFood", ChatColor.red);
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
                        
                        PandaChat.Send(npc.Colony, _localizationHelper, "LeavingTakingItems", ChatColor.red, leaveTax.ToString(), ItemId.GetItemId(item.Type).Name);
                    }
                }

                PandaChat.Send(npc.Colony, _localizationHelper, "LeavingNumberOfItems", ChatColor.red, numberOfItems.ToString());
            }

            npc.health = 0;
            npc.OnDeath();
        }

        private static bool EvaluateBeds(ColonyState state)
        {
            var update = false;

            try
            {
                if (!TimeCycle.IsDay && TimeCycle.TotalHours > state.NextBedTime)
                {
                    var remainingBeds = state.ColonyRef.BedTracker.CalculateTotalBedCount() - state.ColonyRef.FollowerCount;
                    var left          = 0;

                    if (remainingBeds >= 0)
                    {
                        state.NeedsABed = 0;
                    }
                    else
                    {
                        if (state.NeedsABed == 0)
                        {
                            state.NeedsABed = TimeCycle.TotalHours + 24;
                            PandaChat.Send(state.ColonyRef, _localizationHelper, SettlerReasoning.GetNeedBed(), ChatColor.grey);
                        }
                        else if (state.NeedsABed <= TimeCycle.TotalHours)
                        {
                            List<NPCBase> leaving = new List<NPCBase>();

                            foreach (var follower in state.ColonyRef.Followers)
                                if (follower.UsedBed == null)
                                {
                                    left++;
                                    leaving.Add(follower);
                                }

                            state.NeedsABed = 0;

                            foreach (var npc in leaving)
                                NPCLeaving(npc);
                        }

                        if (left > 0)
                        {
                            PandaChat.Send(state.ColonyRef, _localizationHelper, string.Concat(SettlerReasoning.GetNoBed(), string.Format(" {0} colonists have left your colony.", left)), ChatColor.red);
                            update = true;
                        }


                        state.ColonyRef.SendCommonData();
                    }

                    state.NextBedTime = TimeCycle.TotalHours + Random.Next(5, 8);
                }
            }
            catch (Exception ex)
            {
                SettlersLogger.LogError(ex, "EvaluateBeds");
            }

            return update;
        }
    }
}