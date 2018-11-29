using NetworkUI;
using NetworkUI.Items;
using NPC;
using Pandaros.Settlers.Items;
using Pipliz;
using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Pandaros.Settlers.Items.StaticItems;

namespace Pandaros.Settlers.ColonyManager
{
    public class ColonyManagementTool : CSType
    {
        public static string NAME = GameLoader.NAMESPACE + ".ColonyManagementTool";
        public override string Name => NAME;
        public override string icon => GameLoader.ICON_PATH + "ColonyManager.png";
        public override bool? isPlaceable => false;
        public override int? maxStackSize => 1;
        public override StaticItem StaticItemSettings => new StaticItem() { Name = GameLoader.NAMESPACE + ".ColonyManagementTool" };
    }

    public class JobCounts
    {
        public string Name { get; set; }
        public int Free { get; set; }
        public int Working { get; set; }
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

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnPlayerClicked, GameLoader.NAMESPACE + ".ColonyManager.ColonyTool.OpenMenu")]
        public static void OpenMenu(Players.Player player, Box<PlayerClickedData> boxedData)
        {
            //Only launch on RIGHT click
            if (player == null || boxedData.item1.clickType != PlayerClickedData.ClickType.Right || player.ActiveColony == null)
                return;

            if (ItemTypes.IndexLookup.TryGetIndex(GameLoader.NAMESPACE + ".ColonyManagementTool", out var toolItem) &&
                boxedData.item1.typeSelected == toolItem)
            {
                Dictionary<string, JobCounts> jobCounts = GetJobCounts(player.ActiveColony);

                NetworkMenu menu = new NetworkMenu();
                menu.LocalStorage.SetAs("header", "Colony Management");
                menu.Width = 1000;
                menu.Height = 600;

                List<IItem> header = new List<IItem>();

                header.Add(new Label(new LabelData("Job", UnityEngine.Color.black)));
                header.Add(new Label(new LabelData("Working", UnityEngine.Color.black)));
                header.Add(new Label(new LabelData("Not Working", UnityEngine.Color.black)));
                header.Add(new Label(new LabelData("", UnityEngine.Color.black)));
                header.Add(new Label(new LabelData("", UnityEngine.Color.black)));

                menu.Items.Add(new HorizontalGrid(header, 200));

                foreach (var jobKvp in jobCounts)
                {
                    List<IItem> items = new List<IItem>();

                    items.Add(new Label(new LabelData(jobKvp.Key, UnityEngine.Color.black)));
                    items.Add(new Label(new LabelData(jobKvp.Value.Working.ToString(), UnityEngine.Color.black)));
                    items.Add(new Label(new LabelData(jobKvp.Value.Free.ToString(), UnityEngine.Color.black)));
                    items.Add(new DropDown(new LabelData("Amount", UnityEngine.Color.black), jobKvp.Key + ".Recruit", _recruitCount));
                    items.Add(new HorizontalSplit(new ButtonCallback(jobKvp.Key + ".RecruitButton", new LabelData("Recruit", UnityEngine.Color.black, UnityEngine.TextAnchor.MiddleLeft)),
                                                  new ButtonCallback(jobKvp.Key + ".FireButton", new LabelData("Fire!", UnityEngine.Color.black, UnityEngine.TextAnchor.MiddleLeft))));
                    menu.LocalStorage.SetAs(jobKvp.Key + ".Recruit", 0);

                    menu.Items.Add(new HorizontalGrid(items, 200));
                }

                NetworkMenuManager.SendServerPopup(player, menu);
            }
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

                        jobCounts[nPCTypeSettings.PrintName].Free++;
                    }
                }


            if (npcs != null)
                foreach (var npc in npcs)
                {
                    if (npc.Job != null && npc.Job.IsValid && NPCType.NPCTypes.TryGetValue(npc.Job.NPCType, out var nPCTypeSettings))
                    {
                        if (!jobCounts.ContainsKey(nPCTypeSettings.PrintName))
                            jobCounts.Add(nPCTypeSettings.PrintName, new JobCounts() { Name = nPCTypeSettings.PrintName });

                        jobCounts[nPCTypeSettings.PrintName].Working++;
                    }
                }

            return jobCounts;
        }
    }
}

