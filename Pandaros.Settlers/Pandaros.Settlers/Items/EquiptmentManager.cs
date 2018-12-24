using NetworkUI;
using NetworkUI.Items;
using Pandaros.Settlers.ColonyManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandaros.Settlers.Items
{
    [ModLoader.ModManager]
    public class EquiptmentManager
    {
        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnPlayerPushedNetworkUIButton, GameLoader.NAMESPACE + ".Items.EquiptmentManager.PressButton")]
        public static void PressButton(ButtonPressCallbackData data)
        {
            if (data.ButtonIdentifier.Contains(".JobDetailsButton"))
            {
                Dictionary<string, JobCounts> jobCounts = ColonyTool.GetJobCounts(data.Player.ActiveColony);

                foreach (var jobKvp in jobCounts)
                    if (data.ButtonIdentifier.Contains(jobKvp.Key))
                    {
                        NetworkMenu menu = new NetworkMenu();
                        menu.LocalStorage.SetAs("header", "Job Details");
                        menu.Width = 1000;
                        menu.Height = 600;
                        var firstGuy = jobKvp.Value.TakenJobs.FirstOrDefault();
                        var firstInv = Entities.SettlerInventory.GetSettlerInventory(firstGuy.NPC);
                        List<IItem> headerItems = new List<IItem>();

                        headerItems.Add(new Label(new LabelData("Name", UnityEngine.Color.black)));
                        headerItems.Add(new Label(new LabelData("Weapon", UnityEngine.Color.black)));

                        foreach (var a in firstInv.Armor)
                            headerItems.Add(new Label(new LabelData(a.Key.ToString(), UnityEngine.Color.black)));

                        menu.Items.Add(new HorizontalGrid(headerItems, 100));

                        foreach (var job in jobKvp.Value.TakenJobs)
                        {
                            var inv = Entities.SettlerInventory.GetSettlerInventory(job.NPC);
                            List<IItem> items = new List<IItem>();
                            items.Add(new Label(new LabelData(inv.SettlerName, UnityEngine.Color.black)));
                            items.Add(new ItemIcon(inv.Weapon.Id));

                            foreach (var armor in inv.Armor)
                                items.Add(new ItemIcon(armor.Value.Id));

                            items.Add(new ButtonCallback(jobKvp.Key + "." + job.NPC.ID + ".EquiptmentButton", new LabelData("Details", UnityEngine.Color.black)));
                            menu.Items.Add(new HorizontalGrid(items, 100));
                        }

                        NetworkMenuManager.SendServerPopup(data.Player, menu);
                        break;
                    }
            }
            else if (data.ButtonIdentifier.Contains(".EquiptmentButton"))
            {
                Dictionary<string, JobCounts> jobCounts = ColonyTool.GetJobCounts(data.Player.ActiveColony);

                foreach (var jobKvp in jobCounts)
                    if (data.ButtonIdentifier.Contains(jobKvp.Key))
                    {
                        foreach (var job in jobKvp.Value.TakenJobs)
                            if (data.ButtonIdentifier.Contains("." + job.NPC.ID.ToString() + ".EquiptmentButton"))
                            {

                            }
                    }
            }
            else if (data.ButtonIdentifier.Contains(".ColonyToolMainMenu"))
            {
                Dictionary<string, JobCounts> jobCounts = ColonyTool.GetJobCounts(data.Player.ActiveColony);
                NetworkMenuManager.SendServerPopup(data.Player, ColonyTool.BuildMenu(data.Player, jobCounts, false, string.Empty, 0));
            }
        }
    }
}
