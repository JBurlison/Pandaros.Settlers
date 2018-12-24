using BlockTypes;
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
                        menu.LocalStorage.SetAs("header", jobKvp.Key + " Job Details");
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
                        return;
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
                                BuildSettlerDetailMenu(data, jobKvp, job);
                                return;
                            }
                    }
            }
            else if (data.ButtonIdentifier.Contains(".AddEquiptmentButton"))
            {
                Dictionary<string, JobCounts> jobCounts = ColonyTool.GetJobCounts(data.Player.ActiveColony);

                foreach (var jobKvp in jobCounts)
                    if (data.ButtonIdentifier.Contains(jobKvp.Key))
                    {
                        foreach (var job in jobKvp.Value.TakenJobs)
                            if (data.ButtonIdentifier.Contains("." + job.NPC.ID.ToString() + ".EquiptmentButton"))
                            {
                                var inv = Entities.SettlerInventory.GetSettlerInventory(job.NPC);
                                NetworkMenu menu = new NetworkMenu();
                                menu.LocalStorage.SetAs("header", inv.SettlerName + " Equiptment");
                                menu.Width = 1000;
                                menu.Height = 600;
                                var newButtonID = data.ButtonIdentifier.Replace(".AddEquiptmentButton", "");

                                if (data.ButtonIdentifier.Contains(".wep."))
                                {
                                    foreach (var kvp in data.Player.ActiveColony.Stockpile.Items)
                                    {
                                        if (kvp.Value > 0 && Weapons.WeaponFactory.WeaponLookup.TryGetValue(kvp.Key, out var wepItem))
                                        {
                                            List<IItem> items = new List<IItem>();
                                            items.Add(new ItemIcon(kvp.Key));
                                            items.Add(new Label(new LabelData(wepItem.Name, UnityEngine.Color.black)));
                                            items.Add(new ButtonCallback(newButtonID + "." + kvp.Key + ".AddSelectedEquiptmentButton", new LabelData("Select", UnityEngine.Color.black)));
                                            menu.Items.Add(new HorizontalGrid(items, 330));
                                        }
                                    }
                                }
                                else if (data.ButtonIdentifier.Contains(".arm."))
                                {
                                    foreach (var kvp in data.Player.ActiveColony.Stockpile.Items)
                                    {
                                        if (kvp.Value > 0 && Armor.ArmorFactory.ArmorLookup.TryGetValue(kvp.Key, out var armItem))
                                        {
                                            List<IItem> items = new List<IItem>();
                                            items.Add(new ItemIcon(kvp.Key));
                                            items.Add(new Label(new LabelData(armItem.Name, UnityEngine.Color.black)));
                                            items.Add(new ButtonCallback(newButtonID + "." + kvp.Key + ".AddSelectedEquiptmentButton", new LabelData("Select", UnityEngine.Color.black)));
                                            menu.Items.Add(new HorizontalGrid(items, 330));
                                        }
                                    }
                                }

                                NetworkMenuManager.SendServerPopup(data.Player, menu);
                                return;
                            }
                    }
            }
            else if (data.ButtonIdentifier.Contains(".AddSelectedEquiptmentButton"))
            {
                Dictionary<string, JobCounts> jobCounts = ColonyTool.GetJobCounts(data.Player.ActiveColony);

                foreach (var jobKvp in jobCounts)
                    if (data.ButtonIdentifier.Contains(jobKvp.Key))
                    {
                        foreach (var job in jobKvp.Value.TakenJobs)
                            if (data.ButtonIdentifier.Contains("." + job.NPC.ID.ToString() + "."))
                            {
                                if (data.ButtonIdentifier.Contains(".wep."))
                                {
                                    foreach (var kvp in data.Player.ActiveColony.Stockpile.Items)
                                    {
                                        if (Weapons.WeaponFactory.WeaponLookup.TryGetValue(kvp.Key, out var wepItem) && data.ButtonIdentifier.Contains("." + kvp.Key + "."))
                                        {
                                            var inv = Entities.SettlerInventory.GetSettlerInventory(job.NPC);
                                            inv.Weapon.Id = kvp.Key;
                                            inv.Weapon.Durability = wepItem.Durability;
                                            BuildSettlerDetailMenu(data, jobKvp, job);
                                            return;
                                        }
                                    }
                                }
                                else if (data.ButtonIdentifier.Contains(".arm."))
                                {
                                    foreach (var kvp in data.Player.ActiveColony.Stockpile.Items)
                                    {
                                        if (Armor.ArmorFactory.ArmorLookup.TryGetValue(kvp.Key, out var armItem) && data.ButtonIdentifier.Contains("." + kvp.Key + ".") && data.Player.ActiveColony.Stockpile.TryRemove(kvp.Key))
                                        {
                                            var inv = Entities.SettlerInventory.GetSettlerInventory(job.NPC);
                                            inv.Armor[armItem.Slot].Id = kvp.Key;
                                            inv.Armor[armItem.Slot].Durability = armItem.Durability;
                                            BuildSettlerDetailMenu(data, jobKvp, job);
                                            return;
                                        }
                                    }
                                }
                            }
                    }
            }
            else if (data.ButtonIdentifier.Contains(".RemoveEquiptmentButton"))
            {
                Dictionary<string, JobCounts> jobCounts = ColonyTool.GetJobCounts(data.Player.ActiveColony);

                foreach (var jobKvp in jobCounts)
                    if (data.ButtonIdentifier.Contains(jobKvp.Key))
                    {
                        foreach (var job in jobKvp.Value.TakenJobs)
                            if (data.ButtonIdentifier.Contains("." + job.NPC.ID.ToString() + ".EquiptmentButton"))
                            {
                                var inv = Entities.SettlerInventory.GetSettlerInventory(job.NPC);

                                if (data.ButtonIdentifier.Contains(".wep."))
                                {
                                    if (inv.Weapon.Id != default(ushort))
                                        job.NPC.Colony.Stockpile.Add(inv.Weapon.Id);

                                    inv.Weapon.Id = default(ushort);
                                    inv.Weapon.Durability = default(int);
                                }
                                else if (data.ButtonIdentifier.Contains(".arm."))
                                {
                                    foreach (var armor in inv.Armor)
                                    {
                                        if (data.ButtonIdentifier.Contains("." + armor.Key + "."))
                                        {
                                            if (armor.Value.Id != default(ushort))
                                                job.NPC.Colony.Stockpile.Add(armor.Value.Id);

                                            armor.Value.Id = default(ushort);
                                            armor.Value.Durability = default(int);
                                            break;
                                        }
                                    }
                                }

                                BuildSettlerDetailMenu(data, jobKvp, job);
                                return;
                            }
                    }
            }
            else if (data.ButtonIdentifier.Contains(".ColonyToolMainMenu"))
            {
                Dictionary<string, JobCounts> jobCounts = ColonyTool.GetJobCounts(data.Player.ActiveColony);
                NetworkMenuManager.SendServerPopup(data.Player, ColonyTool.BuildMenu(data.Player, jobCounts, false, string.Empty, 0));
            }
        }

        public static void BuildSettlerDetailMenu(ButtonPressCallbackData data, KeyValuePair<string, JobCounts> jobKvp, global::Jobs.IJob job)
        {
            var inv = Entities.SettlerInventory.GetSettlerInventory(job.NPC);
            NetworkMenu menu = new NetworkMenu();
            menu.LocalStorage.SetAs("header", inv.SettlerName);
            menu.Width = 1000;
            menu.Height = 600;

            List<IItem> wep = new List<IItem>();
            wep.Add(new Label(new LabelData("Weapon", UnityEngine.Color.black)));
            wep.Add(new ItemIcon(inv.Weapon.Id));

            if (Weapons.WeaponFactory.WeaponLookup.TryGetValue(inv.Weapon.Id, out var wepItem))
            {
                wep.Add(new Label(new LabelData(wepItem.Name, UnityEngine.Color.black, UnityEngine.TextAnchor.MiddleLeft, 18, LabelData.ELocalizationType.Type)));
                wep.Add(new ButtonCallback(jobKvp.Key + ".wep." + job.NPC.ID + ".AddEquiptmentButton", new LabelData("Swap", UnityEngine.Color.black)));
                wep.Add(new ButtonCallback(jobKvp.Key + ".wep." + job.NPC.ID + ".RemoveEquiptmentButton", new LabelData("Remove", UnityEngine.Color.black)));
            }
            else
            {
                wep.Add(new Label(new LabelData("", UnityEngine.Color.black)));

                if (inv.Weapon.Id != default(ushort))
                    wep.Add(new ButtonCallback(jobKvp.Key + ".wep." + job.NPC.ID + ".AddEquiptmentButton", new LabelData("Swap", UnityEngine.Color.black)));
            }

            menu.Items.Add(new HorizontalGrid(wep, 200));

            foreach (var armor in inv.Armor)
            {
                List<IItem> items = new List<IItem>();
                items.Add(new Label(new LabelData(armor.Key.ToString(), UnityEngine.Color.black)));
                items.Add(new ItemIcon(armor.Value.Id));

                if (Armor.ArmorFactory.ArmorLookup.TryGetValue(armor.Value.Id, out var arm))
                {
                    PandaLogger.Log("adding item {0}", armor.Value.Id);
                    items.Add(new Label(new LabelData(arm.Name, UnityEngine.Color.black, UnityEngine.TextAnchor.MiddleLeft, 18, LabelData.ELocalizationType.Type)));
                    items.Add(new ButtonCallback(jobKvp.Key + "." + armor.Key + ".arm." + job.NPC.ID + ".AddEquiptmentButton", new LabelData("Swap", UnityEngine.Color.black)));
                    items.Add(new ButtonCallback(jobKvp.Key + "." + armor.Key + ".arm." + job.NPC.ID + ".RemoveEquiptmentButton", new LabelData("Remove", UnityEngine.Color.black)));
                }
                else
                {
                    items.Add(new Label(new LabelData("", UnityEngine.Color.black)));

                    if (armor.Value.Id != default(ushort))
                        items.Add(new ButtonCallback(jobKvp.Key + "." + armor.Key + ".arm." + job.NPC.ID + ".AddEquiptmentButton", new LabelData("Swap", UnityEngine.Color.black)));
                }

                menu.Items.Add(new HorizontalGrid(items, 200));
            }

            NetworkMenuManager.SendServerPopup(data.Player, menu);
        }
    }
}
