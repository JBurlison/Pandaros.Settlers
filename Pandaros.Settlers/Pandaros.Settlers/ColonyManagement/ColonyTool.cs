using Jobs;
using NetworkUI;
using NetworkUI.Items;
using NPC;
using Pandaros.Settlers.Entities;
using Pandaros.Settlers.Items;
using Pandaros.Settlers.Models;
using Pipliz;
using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Pandaros.Settlers.Items.StaticItems;

namespace Pandaros.Settlers.ColonyManagement
{
    public class ColonyManagementTool : CSType
    {
        public static string NAME = GameLoader.NAMESPACE + ".ColonyManagementTool";
        public override string name => NAME;
        public override string icon => GameLoader.ICON_PATH + "ColonyManager.png";
        public override bool? isPlaceable => false;
        public override int? maxStackSize => 1;
        public override List<string> categories { get; set; } = new List<string>()
        {
            "essential",
            "aaa"
        };
        public override StaticItem StaticItemSettings => new StaticItem() { Name = GameLoader.NAMESPACE + ".ColonyManagementTool" };
    }

    public class JobCounts
    {
        public string Name { get; set; }
        public int AvailableCount { get; set; }
        public int TakenCount { get; set; }
        public List<IJob> AvailableJobs { get; set; } = new List<IJob>();
        public List<IJob> TakenJobs { get; set; } = new List<IJob>();
    }


    [ModLoader.ModManager]
    public class ColonyTool
    {
        public static List<string> _recruitCount = new List<string>()
        {
            "1",
            "5",
            "10",
            "Max"
        };

        static readonly Pandaros.Settlers.localization.LocalizationHelper _localizationHelper = new localization.LocalizationHelper("colonytool");

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnPlayerClicked, GameLoader.NAMESPACE + ".ColonyManager.ColonyTool.OpenMenu")]
        public static void OpenMenu(Players.Player player, PlayerClickedData playerClickData)
        {
            //Only launch on RIGHT click
            if (player == null || playerClickData.ClickType != PlayerClickedData.EClickType.Right || player.ActiveColony == null)
                return;
           
            if (ItemTypes.IndexLookup.TryGetIndex(GameLoader.NAMESPACE + ".ColonyManagementTool", out var toolItem) &&
                playerClickData.TypeSelected == toolItem)
            {
                Dictionary<string, JobCounts> jobCounts = GetJobCounts(player.ActiveColony);
                NetworkMenuManager.SendServerPopup(player, BuildMenu(player, jobCounts, false, string.Empty, 0));
            }
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnPlayerPushedNetworkUIButton, GameLoader.NAMESPACE + ".ColonyManager.ColonyTool.PressButton")]
        public static void PressButton(ButtonPressCallbackData data)
        {
            if ((!data.ButtonIdentifier.Contains(".RecruitButton") &&
                !data.ButtonIdentifier.Contains(".FireButton") &&
                !data.ButtonIdentifier.Contains(".MoveFired") &&
                !data.ButtonIdentifier.Contains(".ColonyToolMainMenu") &&
                !data.ButtonIdentifier.Contains(".KillFired") &&
                !data.ButtonIdentifier.Contains(".CallToArms")) || data.Player.ActiveColony == null)
                return;

            Dictionary<string, JobCounts> jobCounts = GetJobCounts(data.Player.ActiveColony);

            if (data.ButtonIdentifier.Contains(".ColonyToolMainMenu"))
            {
                NetworkMenuManager.SendServerPopup(data.Player, BuildMenu(data.Player, jobCounts, false, string.Empty, 0));
            }
            else if (data.ButtonIdentifier.Contains(".FireButton"))
            {
                foreach (var job in jobCounts)
                    if (data.ButtonIdentifier.Contains(job.Key))
                    {
                        var recruit = data.Storage.GetAs<int>(job.Key + ".Recruit");
                        var count = GetCountValue(recruit);
                        var menu = BuildMenu(data.Player, jobCounts, true, job.Key, count);

                        menu.LocalStorage.SetAs(GameLoader.NAMESPACE + ".FiredJobName", job.Key);
                        menu.LocalStorage.SetAs(GameLoader.NAMESPACE + ".FiredJobCount", count);

                        NetworkMenuManager.SendServerPopup(data.Player, menu);
                        break;
                    }
            }
            else if (data.ButtonIdentifier.Contains(".KillFired"))
            {
                var firedJob = data.Storage.GetAs<string>(GameLoader.NAMESPACE + ".FiredJobName");
                var count = data.Storage.GetAs<int>(GameLoader.NAMESPACE + ".FiredJobCount");

                foreach (var job in jobCounts)
                {
                    if (job.Key == firedJob)
                    {
                        if (count > job.Value.TakenCount)
                            count = job.Value.TakenCount;

                        for (int i = 0; i < count; i++)
                        {
                            var npc = job.Value.TakenJobs[i].NPC;
                            npc.ClearJob();
                            npc.OnDeath();
                        }

                        break;
                    }
                }

                data.Player.ActiveColony.SendCommonData();
                jobCounts = GetJobCounts(data.Player.ActiveColony);
                NetworkMenuManager.SendServerPopup(data.Player, BuildMenu(data.Player, jobCounts, false, string.Empty, 0));
            }
            else if (data.ButtonIdentifier.Contains(".MoveFired"))
            {
                var firedJob = data.Storage.GetAs<string>(GameLoader.NAMESPACE + ".FiredJobName");
                var count = data.Storage.GetAs<int>(GameLoader.NAMESPACE + ".FiredJobCount");

                foreach (var job in jobCounts)
                    if (data.ButtonIdentifier.Contains(job.Key))
                    {
                        if (count > job.Value.AvailableCount)
                            count = job.Value.AvailableCount;

                        if (jobCounts.TryGetValue(firedJob, out var firedJobCounts))
                        {
                            for (int i = 0; i < count; i++)
                            {
                                if (firedJobCounts.TakenCount > i)
                                {
                                    var npc = firedJobCounts.TakenJobs[i].NPC;
                                    npc.ClearJob();
                                    npc.TakeJob(job.Value.AvailableJobs[i]);
                                    data.Player.ActiveColony.JobFinder.Remove(job.Value.AvailableJobs[i]);
                                }
                                else
                                    break;
                            }
                        }

                        data.Player.ActiveColony.SendCommonData();
                        break;
                    }

                jobCounts = GetJobCounts(data.Player.ActiveColony);
                NetworkMenuManager.SendServerPopup(data.Player, BuildMenu(data.Player, jobCounts, false, string.Empty, 0));
            }
            else if (data.ButtonIdentifier.Contains(".RecruitButton"))
            {
                foreach (var job in jobCounts)
                    if (data.ButtonIdentifier.Contains(job.Key))
                    {
                        var recruit = data.Storage.GetAs<int>(job.Key + ".Recruit");
                        var count = GetCountValue(recruit);

                        if (count > job.Value.AvailableCount)
                            count = job.Value.AvailableCount;

                        for (int i = 0; i < count; i++)
                        {
                            var num = 0f;
                            data.Player.ActiveColony.HappinessData.RecruitmentCostCalculator.GetCost(data.Player.ActiveColony.HappinessData.CachedHappiness, data.Player.ActiveColony, out float foodCost);
                            if (data.Player.ActiveColony.Stockpile.TotalFood < foodCost ||
                                !data.Player.ActiveColony.Stockpile.TryRemoveFood(ref num, foodCost))
                            {
                                PandaChat.Send(data.Player, _localizationHelper.LocalizeOrDefault("Notenoughfood", data.Player), ChatColor.red);
                                break;
                            }
                            else
                            {
                                var newGuy = new NPCBase(data.Player.ActiveColony, data.Player.ActiveColony.GetClosestBanner(new Vector3Int(data.Player.Position)).Position);
                                newGuy.FoodHoursCarried = ServerManager.ServerSettings.NPCs.InitialFoodCarriedHours;
                                data.Player.ActiveColony.RegisterNPC(newGuy);
                                SettlerInventory.GetSettlerInventory(newGuy);
                                NPCTracker.Add(newGuy);
                                ModLoader.Callbacks.OnNPCRecruited.Invoke(newGuy);

                                if (newGuy.IsValid)
                                {
                                    newGuy.TakeJob(job.Value.AvailableJobs[i]);
                                    data.Player.ActiveColony.JobFinder.Remove(job.Value.AvailableJobs[i]);
                                }
                            }
                        }


                        data.Player.ActiveColony.SendCommonData();

                        jobCounts = GetJobCounts(data.Player.ActiveColony);
                        NetworkMenuManager.SendServerPopup(data.Player, BuildMenu(data.Player, jobCounts, false, string.Empty, 0));
                    }
            }
            else if (data.ButtonIdentifier.Contains(".CallToArms"))
            {
                AI.CalltoArms.ProcesssCallToArms(data.Player, data.Player.ActiveColony);
                NetworkMenuManager.SendServerPopup(data.Player, BuildMenu(data.Player, jobCounts, false, string.Empty, 0));
            }
        }

        public static int GetCountValue(int countIndex)
        {
            var value = _recruitCount[countIndex];
            int retval = int.MaxValue;

            if (int.TryParse(value, out int count))
                retval = count;

            return retval;
        }

        public static NetworkMenu BuildMenu(Players.Player player, Dictionary<string, JobCounts> jobCounts, bool fired, string firedName, int firedCount)
        {
            NetworkMenu menu = new NetworkMenu();
            menu.LocalStorage.SetAs("header", _localizationHelper.LocalizeOrDefault("ColonyManagement", player));
            menu.Width = 1000;
            menu.Height = 600;

            if (fired)
            {
                var count = firedCount.ToString();

                if (firedCount == int.MaxValue)
                    count = "all";

                menu.Items.Add(new ButtonCallback(GameLoader.NAMESPACE + ".KillFired", new LabelData($"{_localizationHelper.LocalizeOrDefault("Kill", player)} {count} {_localizationHelper.LocalizeOrDefault("Fired", player)} {firedName}", UnityEngine.Color.black, UnityEngine.TextAnchor.MiddleCenter)));
            }
            else
                menu.Items.Add(new ButtonCallback(GameLoader.NAMESPACE + ".PlayerDetails", new LabelData(_localizationHelper.GetLocalizationKey("PlayerDetails"), UnityEngine.Color.black, UnityEngine.TextAnchor.MiddleCenter)));

            menu.Items.Add(new Line());

            if (!fired)
            {
                ColonyState ps = ColonyState.GetColonyState(player.ActiveColony);

                if (SettlersConfiguration.GetorDefault("ColonistsRecruitment", true))
                {                  
                    player.ActiveColony.HappinessData.RecruitmentCostCalculator.GetCost(player.ActiveColony.HappinessData.CachedHappiness, player.ActiveColony, out float num);
                    var cost = SettlersConfiguration.GetorDefault("CompoundingFoodRecruitmentCost", 2) * ps.ColonistsBought;

                    if (cost < 1)
                        cost = 1;

                    menu.Items.Add(new HorizontalSplit(new Label(new LabelData(_localizationHelper.GetLocalizationKey("RecruitmentCost"), UnityEngine.Color.black)),
                                                       new Label(new LabelData((cost + num).ToString(), UnityEngine.Color.black))));
                }

                if(ps.CallToArmsEnabled)
                    menu.Items.Add(new ButtonCallback(GameLoader.NAMESPACE + ".CallToArms", new LabelData(_localizationHelper.GetLocalizationKey("DeactivateCallToArms"), UnityEngine.Color.black, UnityEngine.TextAnchor.MiddleCenter)));
                else
                    menu.Items.Add(new ButtonCallback(GameLoader.NAMESPACE + ".CallToArms", new LabelData(_localizationHelper.GetLocalizationKey("ActivateCallToArms"), UnityEngine.Color.black, UnityEngine.TextAnchor.MiddleCenter)));
            }


            List<ValueTuple<IItem, int>> header = new List<ValueTuple<IItem, int>>();

            header.Add(ValueTuple.Create<IItem, int>(new Label(new LabelData(_localizationHelper.GetLocalizationKey("Job"), UnityEngine.Color.black)), 140));

            if (!fired)
                header.Add(ValueTuple.Create<IItem, int>(new Label(new LabelData("", UnityEngine.Color.black)), 140));

            header.Add(ValueTuple.Create<IItem, int>(new Label(new LabelData(_localizationHelper.GetLocalizationKey("Working"), UnityEngine.Color.black)), 140));
            header.Add(ValueTuple.Create<IItem, int>(new Label(new LabelData(_localizationHelper.GetLocalizationKey("NotWorking"), UnityEngine.Color.black)), 140));
            header.Add(ValueTuple.Create<IItem, int>(new Label(new LabelData("", UnityEngine.Color.black)), 140));
            header.Add(ValueTuple.Create<IItem, int>(new Label(new LabelData("", UnityEngine.Color.black)), 140));

            menu.Items.Add(new HorizontalRow(header));
            int jobCount = 0;

            foreach (var jobKvp in jobCounts)
            {
                if (fired && jobKvp.Value.AvailableCount == 0)
                    continue;

                jobCount++;
                List<ValueTuple<IItem, int>> items = new List<ValueTuple<IItem, int>>();

                items.Add(ValueTuple.Create<IItem, int>(new Label(new LabelData(_localizationHelper.LocalizeOrDefault(jobKvp.Key.Replace(" ", ""), player), UnityEngine.Color.black)), 140));

                if (!fired)
                    items.Add(ValueTuple.Create<IItem, int>(new ButtonCallback(jobKvp.Key + ".JobDetailsButton", new LabelData(_localizationHelper.GetLocalizationKey("Details"), UnityEngine.Color.black, UnityEngine.TextAnchor.MiddleCenter)), 140));

                items.Add(ValueTuple.Create<IItem, int>(new Label(new LabelData(jobKvp.Value.TakenCount.ToString(), UnityEngine.Color.black)), 140));
                items.Add(ValueTuple.Create<IItem, int>(new Label(new LabelData(jobKvp.Value.AvailableCount.ToString(), UnityEngine.Color.black)), 140));

                if (fired)
                {
                    items.Add(ValueTuple.Create<IItem, int>(new ButtonCallback(jobKvp.Key + ".MoveFired", new LabelData(_localizationHelper.GetLocalizationKey("MoveFired"), UnityEngine.Color.black, UnityEngine.TextAnchor.MiddleLeft)), 140));
                }
                else
                {
                    items.Add(ValueTuple.Create<IItem, int>(new DropDown(new LabelData(_localizationHelper.GetLocalizationKey("Amount"), UnityEngine.Color.black), jobKvp.Key + ".Recruit", _recruitCount), 140));
                    items.Add(ValueTuple.Create<IItem, int>(new ButtonCallback(jobKvp.Key + ".RecruitButton", new LabelData(_localizationHelper.GetLocalizationKey("Recruit"), UnityEngine.Color.black, UnityEngine.TextAnchor.MiddleCenter)), 140));
                    items.Add(ValueTuple.Create<IItem, int>(new ButtonCallback(jobKvp.Key + ".FireButton", new LabelData(_localizationHelper.GetLocalizationKey("Fire"), UnityEngine.Color.black, UnityEngine.TextAnchor.MiddleCenter)), 140));
                    
                }

                menu.LocalStorage.SetAs(jobKvp.Key + ".Recruit", 0);

                menu.Items.Add(new HorizontalRow(items));
            }

            if (jobCount == 0)
                menu.Items.Add(new Label(new LabelData(_localizationHelper.LocalizeOrDefault("NoJobs", player), UnityEngine.Color.black)));

            return menu;
        }

        public static Dictionary<string, JobCounts> GetJobCounts(Colony colony)
        {
            Dictionary<string, JobCounts> jobCounts = new Dictionary<string, JobCounts>();
            var jobs = colony?.JobFinder?.JobsData?.OpenJobs;
            var npcs = colony?.Followers;

            if (jobs != null)
                foreach (var job in jobs)
                {
                    if (NPCType.NPCTypes.TryGetValue(job.NPCType, out var nPCTypeSettings))
                    {
                        if (!jobCounts.ContainsKey(nPCTypeSettings.PrintName))
                            jobCounts.Add(nPCTypeSettings.PrintName, new JobCounts() { Name = nPCTypeSettings.PrintName });

                        jobCounts[nPCTypeSettings.PrintName].AvailableCount++;
                        jobCounts[nPCTypeSettings.PrintName].AvailableJobs.Add(job);
                    }
                }


            if (npcs != null)
                foreach (var npc in npcs)
                {
                    if (npc.Job != null && npc.Job.IsValid && NPCType.NPCTypes.TryGetValue(npc.Job.NPCType, out var nPCTypeSettings))
                    {
                        if (!jobCounts.ContainsKey(nPCTypeSettings.PrintName))
                            jobCounts.Add(nPCTypeSettings.PrintName, new JobCounts() { Name = nPCTypeSettings.PrintName });

                        jobCounts[nPCTypeSettings.PrintName].TakenCount++;
                        jobCounts[nPCTypeSettings.PrintName].TakenJobs.Add(npc.Job);
                    }
                }

            return jobCounts;
        }
    }
}

