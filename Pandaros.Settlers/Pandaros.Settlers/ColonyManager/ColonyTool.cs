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


    [ModLoader.ModManager]
    public class ColonyTool
    {
        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnPlayerClicked, GameLoader.NAMESPACE + ".ColonyManager.ColonyTool.OpenMenu")]
        public static void OpenMenu(Players.Player player, Box<PlayerClickedData> boxedData)
        {
            //Only launch on RIGHT click
            if (player == null || boxedData.item1.clickType != PlayerClickedData.ClickType.Right || player.ActiveColony == null)
                return;

            if (ItemTypes.IndexLookup.TryGetIndex(GameLoader.NAMESPACE + ".ColonyManagementTool", out var toolItem) &&
                boxedData.item1.typeSelected == toolItem)
            {
                Dictionary<string, int> jobCounts = new Dictionary<string, int>();
                var jobs = player?.ActiveColony?.JobFinder?.JobsData?.OpenJobs;

                if (jobs != null)
                    foreach (var job in jobs)
                    {
                        if (NPCType.NPCTypes.TryGetValue(job.NPCType, out var nPCTypeSettings))
                        {
                            if (!jobCounts.ContainsKey(nPCTypeSettings.PrintName))
                                jobCounts.Add(nPCTypeSettings.PrintName, 0);

                            jobCounts[nPCTypeSettings.PrintName]++;
                        }
                    }

                NetworkMenu menu = new NetworkMenu();
                menu.LocalStorage.SetAs("header", "Colony Management");

                foreach (var jobKvp in jobCounts)
                {
                    menu.Items.Add(new HorizontalSplit(new Label(new LabelData(jobKvp.Key, UnityEngine.Color.black)),
                                                       new Label(new LabelData("Open Jobs: " + jobKvp.Value, UnityEngine.Color.black))));
                }

                NetworkMenuManager.SendServerPopup(player, menu);
            }
        }
    }
}

