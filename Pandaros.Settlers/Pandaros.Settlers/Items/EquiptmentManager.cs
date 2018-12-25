using BlockTypes;
using NetworkUI;
using NetworkUI.Items;
using Pandaros.Settlers.ColonyManager;
using Pandaros.Settlers.Entities;
using Pandaros.Settlers.Managers;
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
            if (data.ButtonIdentifier.Contains(GameLoader.NAMESPACE + ".PlayerDetails"))
            {
                BuildPlayerDetailsMenu(data);
                return;
            }
            else if (data.ButtonIdentifier.Contains(".AddPlayerEquiptmentButton"))
            {
                NetworkMenu menu = new NetworkMenu();
                menu.LocalStorage.SetAs("header", "Player Equiptment");
                menu.Width = 1000;
                menu.Height = 600;

                foreach (var kvp in data.Player.ActiveColony.Stockpile.Items)
                {
                    if (kvp.Value > 0 && Armor.ArmorFactory.ArmorLookup.TryGetValue(kvp.Key, out var armItem) && data.ButtonIdentifier.Contains(armItem.Slot + "."))
                    {
                        List<IItem> items = new List<IItem>();
                        items.Add(new ItemIcon(kvp.Key));
                        items.Add(new Label(new LabelData(armItem.Name, UnityEngine.Color.black, UnityEngine.TextAnchor.MiddleLeft, 18, LabelData.ELocalizationType.Type)));
                        items.Add(new Label(new LabelData("Stockpile: " + kvp.Value.ToString(), UnityEngine.Color.black)));
                        items.Add(new ButtonCallback(kvp.Key + ".AddPlayerSelectedEquiptmentButton", new LabelData("Select", UnityEngine.Color.black, UnityEngine.TextAnchor.MiddleCenter)));
                        menu.Items.Add(new HorizontalGrid(items, 250));
                    }
                }

                NetworkMenuManager.SendServerPopup(data.Player, menu);
            }
            else if (data.ButtonIdentifier.Contains(".AddPlayerSelectedEquiptmentButton"))
            {

                foreach (var kvp in data.Player.ActiveColony.Stockpile.Items)
                {
                    if (Armor.ArmorFactory.ArmorLookup.TryGetValue(kvp.Key, out var armItem) &&
                        data.ButtonIdentifier.Contains(kvp.Key + ".") &&
                        data.Player.ActiveColony.Stockpile.TryRemove(kvp.Key))
                    {
                        var ps = PlayerState.GetPlayerState(data.Player);

                        if (ps.Armor[armItem.Slot].Id != default(ushort))
                            data.Player.ActiveColony.Stockpile.Add(ps.Armor[armItem.Slot].Id);

                        ps.Armor[armItem.Slot].Id = kvp.Key;
                        ps.Armor[armItem.Slot].Durability = armItem.Durability;
                        BuildPlayerDetailsMenu(data);
                        return;
                    }
                }
            }
            else if (data.ButtonIdentifier.Contains(".RemovePlayerEquiptmentButton"))
            {
                var ps = PlayerState.GetPlayerState(data.Player);

                foreach (var armor in ps.Armor)
                {
                    if (data.ButtonIdentifier.Contains(armor.Key + "."))
                    {
                        if (armor.Value.Id != default(ushort))
                            data.Player.ActiveColony.Stockpile.Add(armor.Value.Id);

                        armor.Value.Id = default(ushort);
                        armor.Value.Durability = default(int);
                        break;
                    }
                }

                BuildPlayerDetailsMenu(data);
            }
            else if (data.ButtonIdentifier.Contains(".JobDetailsButton"))
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

                            items.Add(new ButtonCallback(jobKvp.Key + "." + job.NPC.ID + ".EquiptmentButton", new LabelData("Details", UnityEngine.Color.black, UnityEngine.TextAnchor.MiddleCenter)));
                            menu.Items.Add(new HorizontalGrid(items, 100));
                        }

                        menu.Items.Add(new Line());
                        menu.Items.Add(new ButtonCallback(GameLoader.NAMESPACE + ".ColonyToolMainMenu", new LabelData("Back", UnityEngine.Color.black, UnityEngine.TextAnchor.MiddleCenter)));

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
                            if (data.ButtonIdentifier.Contains("." + job.NPC.ID.ToString() + "."))
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
                                            items.Add(new Label(new LabelData(wepItem.Name, UnityEngine.Color.black, UnityEngine.TextAnchor.MiddleLeft, 18, LabelData.ELocalizationType.Type)));
                                            items.Add(new Label(new LabelData("Stockpile: " + kvp.Value.ToString(), UnityEngine.Color.black)));
                                            items.Add(new ButtonCallback(newButtonID + "." + kvp.Key + ".AddSelectedEquiptmentButton", new LabelData("Select", UnityEngine.Color.black, UnityEngine.TextAnchor.MiddleCenter)));
                                            menu.Items.Add(new HorizontalGrid(items, 250));
                                        }
                                    }
                                }
                                else if (data.ButtonIdentifier.Contains(".arm."))
                                {
                                    foreach (var kvp in data.Player.ActiveColony.Stockpile.Items)
                                    {
                                        if (kvp.Value > 0 && Armor.ArmorFactory.ArmorLookup.TryGetValue(kvp.Key, out var armItem) && data.ButtonIdentifier.Contains("." + armItem.Slot + "."))
                                        {
                                            List<IItem> items = new List<IItem>();
                                            items.Add(new ItemIcon(kvp.Key));
                                            items.Add(new Label(new LabelData(armItem.Name, UnityEngine.Color.black, UnityEngine.TextAnchor.MiddleLeft, 18, LabelData.ELocalizationType.Type)));
                                            items.Add(new Label(new LabelData("Stockpile: " + kvp.Value.ToString(), UnityEngine.Color.black)));
                                            items.Add(new ButtonCallback(newButtonID + "." + kvp.Key + ".AddSelectedEquiptmentButton", new LabelData("Select", UnityEngine.Color.black, UnityEngine.TextAnchor.MiddleCenter)));
                                            menu.Items.Add(new HorizontalGrid(items, 250));
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
                                        if (Weapons.WeaponFactory.WeaponLookup.TryGetValue(kvp.Key, out var wepItem) &&
                                            data.ButtonIdentifier.Contains("." + kvp.Key + ".") &&
                                            job.NPC.Colony.Stockpile.TryRemove(kvp.Key))
                                        {
                                            var inv = Entities.SettlerInventory.GetSettlerInventory(job.NPC);

                                            if (inv.Weapon.Id != default(ushort))
                                                job.NPC.Colony.Stockpile.Add(inv.Weapon.Id);

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
                                        if (Armor.ArmorFactory.ArmorLookup.TryGetValue(kvp.Key, out var armItem) &&
                                            data.ButtonIdentifier.Contains("." + kvp.Key + ".") &&
                                            job.NPC.Colony.Stockpile.TryRemove(kvp.Key))
                                        {
                                            var inv = Entities.SettlerInventory.GetSettlerInventory(job.NPC);

                                            if (inv.Armor[armItem.Slot].Id != default(ushort))
                                                job.NPC.Colony.Stockpile.Add(inv.Armor[armItem.Slot].Id);

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

        private static void BuildPlayerDetailsMenu(ButtonPressCallbackData data)
        {
            NetworkMenu menu = new NetworkMenu();
            menu.LocalStorage.SetAs("header", "Player Details");
            menu.Width = 1000;
            menu.Height = 600;

            var ps = PlayerState.GetPlayerState(data.Player);

            menu.Items.Add(new Label(new LabelData("Stats", UnityEngine.Color.black, UnityEngine.TextAnchor.MiddleLeft, 24)));
            menu.Items.Add(new HorizontalSplit(new Label(new LabelData("Join Date:", UnityEngine.Color.black)),
                                            new Label(new LabelData(ps.JoinDate.ToString(), UnityEngine.Color.black))));

            menu.Items.Add(new HorizontalSplit(new Label(new LabelData("Blocks Placed:", UnityEngine.Color.black)),
                                            new Label(new LabelData(ps.ItemsPlaced.Sum(kvp => kvp.Value).ToString(), UnityEngine.Color.black))));
            menu.Items.Add(new HorizontalSplit(new Label(new LabelData("Blocks Removed:", UnityEngine.Color.black)),
                                            new Label(new LabelData(ps.ItemsRemoved.Sum(kvp => kvp.Value).ToString(), UnityEngine.Color.black))));

            foreach (var statItem in ps.Stats)
            {
                if (ItemTypes.IndexLookup.TryGetIndex(statItem.Key, out var itemIndex))
                    menu.Items.Add(new HorizontalSplit(new Label(new LabelData(statItem.Key, UnityEngine.Color.black, UnityEngine.TextAnchor.MiddleLeft, 18, LabelData.ELocalizationType.Type)),
                                                new Label(new LabelData(statItem.Value.ToString(), UnityEngine.Color.black))));
                else
                    menu.Items.Add(new HorizontalSplit(new Label(new LabelData(statItem.Key, UnityEngine.Color.black)),
                                                new Label(new LabelData(statItem.Value.ToString(), UnityEngine.Color.black))));
            }

            var totalArmor = 0f;

            foreach (var a in ps.Armor)
            {
                if (Armor.ArmorFactory.ArmorLookup.TryGetValue(a.Value.Id, out var armorItem))
                    totalArmor += armorItem.ArmorRating;
            }

            menu.Items.Add(new HorizontalSplit(new Label(new LabelData("Damage Reduction:", UnityEngine.Color.black)),
                                            new Label(new LabelData((totalArmor * 100) + "%", UnityEngine.Color.black))));

            menu.Items.Add(new Line(UnityEngine.Color.black));
            menu.Items.Add(new Label(new LabelData("Equiptment", UnityEngine.Color.black, UnityEngine.TextAnchor.MiddleLeft, 24)));

            foreach (var armor in ps.Armor)
            {
                List<IItem> items = new List<IItem>();
                items.Add(new Label(new LabelData(armor.Key.ToString(), UnityEngine.Color.black)));
                items.Add(new ItemIcon(armor.Value.Id));

                if (Armor.ArmorFactory.ArmorLookup.TryGetValue(armor.Value.Id, out var arm))
                {
                    items.Add(new Label(new LabelData(arm.Name, UnityEngine.Color.black, UnityEngine.TextAnchor.MiddleLeft, 18, LabelData.ELocalizationType.Type)));
                    items.Add(new ButtonCallback(armor.Key + ".AddPlayerEquiptmentButton", new LabelData("Swap", UnityEngine.Color.black, UnityEngine.TextAnchor.MiddleCenter)));
                    items.Add(new ButtonCallback(armor.Key + ".RemovePlayerEquiptmentButton", new LabelData("Remove", UnityEngine.Color.black, UnityEngine.TextAnchor.MiddleCenter)));
                }
                else
                {
                    items.Add(new Label(new LabelData("", UnityEngine.Color.black)));
                    items.Add(new ButtonCallback(armor.Key + ".AddPlayerEquiptmentButton", new LabelData("Add", UnityEngine.Color.black, UnityEngine.TextAnchor.MiddleCenter)));
                }

                menu.Items.Add(new HorizontalGrid(items, 200));
            }

            NetworkMenuManager.SendServerPopup(data.Player, menu);
        }

        public static void BuildSettlerDetailMenu(ButtonPressCallbackData data, KeyValuePair<string, JobCounts> jobKvp, global::Jobs.IJob job)
        {
            var inv = Entities.SettlerInventory.GetSettlerInventory(job.NPC);
            NetworkMenu menu = new NetworkMenu();
            menu.LocalStorage.SetAs("header", "Colonist " + inv.SettlerName + " Details");
            menu.Width = 1000;
            menu.Height = 600;

            menu.Items.Add(new Label(new LabelData("Stats", UnityEngine.Color.black, UnityEngine.TextAnchor.MiddleLeft, 24)));
            menu.Items.Add(new HorizontalSplit(new Label(new LabelData("Skill Proc Chance:", UnityEngine.Color.black)),
                                                new Label(new LabelData((inv.GetSkillModifier() * 100) + "%", UnityEngine.Color.black))));
            menu.Items.Add(new Label(new LabelData("Skill proc chance is the % to do double damage and proc bonus items.", UnityEngine.Color.black, UnityEngine.TextAnchor.MiddleLeft, 13)));
            SettlerManager.GetSkillInformation(job, out var nextLevel, out var itt, out var allSkill);
            menu.Items.Add(new HorizontalSplit(new Label(new LabelData("Number of Attacks/Crafts to next skill up:", UnityEngine.Color.black)),
                                                new Label(new LabelData((nextLevel - itt).ToString(), UnityEngine.Color.black))));

            foreach (var statItem in inv.Stats)
            {
                if (ItemTypes.IndexLookup.TryGetIndex(statItem.Key, out var itemIndex))
                    menu.Items.Add(new HorizontalSplit(new Label(new LabelData(statItem.Key, UnityEngine.Color.black, UnityEngine.TextAnchor.MiddleLeft, 18, LabelData.ELocalizationType.Type)),
                                                new Label(new LabelData(statItem.Value.ToString(), UnityEngine.Color.black))));
                else
                    menu.Items.Add(new HorizontalSplit(new Label(new LabelData(statItem.Key, UnityEngine.Color.black)),
                                                new Label(new LabelData(statItem.Value.ToString(), UnityEngine.Color.black))));
            }

            menu.Items.Add(new Line(UnityEngine.Color.black));
            menu.Items.Add(new Label(new LabelData("Bonus Procs", UnityEngine.Color.black, UnityEngine.TextAnchor.MiddleLeft, 20)));

            var procItemsHeader = new List<IItem>();
            procItemsHeader.Add(new Label(new LabelData("Item", UnityEngine.Color.black)));
            procItemsHeader.Add(new Label(new LabelData("", UnityEngine.Color.black)));
            procItemsHeader.Add(new Label(new LabelData("Count", UnityEngine.Color.black)));
            menu.Items.Add(new HorizontalGrid(procItemsHeader, 150));

            if (inv.BonusProcs.Count == 0)
            {
                menu.Items.Add(new Label(new LabelData("None Yet!", UnityEngine.Color.black, UnityEngine.TextAnchor.MiddleLeft, 20)));
            }

            foreach (var proc in inv.BonusProcs)
            {
                var procItems = new List<IItem>();
                procItems.Add(new ItemIcon(proc.Key));
                procItems.Add(new Label(new LabelData(ItemTypes.GetType(proc.Key).Name, UnityEngine.Color.black, UnityEngine.TextAnchor.MiddleLeft, 18, LabelData.ELocalizationType.Type)));
                procItems.Add(new Label(new LabelData("x " +proc.Value, UnityEngine.Color.black)));
                menu.Items.Add(new HorizontalGrid(procItems, 150));
            }

            menu.Items.Add(new Line(UnityEngine.Color.black));
            menu.Items.Add(new Label(new LabelData("Equiptment", UnityEngine.Color.black, UnityEngine.TextAnchor.MiddleLeft, 24)));
            List<IItem> wep = new List<IItem>();
            wep.Add(new Label(new LabelData("Weapon", UnityEngine.Color.black)));
            wep.Add(new ItemIcon(inv.Weapon.Id));

            if (Weapons.WeaponFactory.WeaponLookup.TryGetValue(inv.Weapon.Id, out var wepItem))
            {
                wep.Add(new Label(new LabelData(wepItem.Name, UnityEngine.Color.black, UnityEngine.TextAnchor.MiddleLeft, 18, LabelData.ELocalizationType.Type)));
                wep.Add(new ButtonCallback(jobKvp.Key + ".wep." + job.NPC.ID + ".AddEquiptmentButton", new LabelData("Swap", UnityEngine.Color.black, UnityEngine.TextAnchor.MiddleCenter)));
                wep.Add(new ButtonCallback(jobKvp.Key + ".wep." + job.NPC.ID + ".RemoveEquiptmentButton", new LabelData("Remove", UnityEngine.Color.black, UnityEngine.TextAnchor.MiddleCenter)));
            }
            else
            {
                wep.Add(new Label(new LabelData("", UnityEngine.Color.black)));
                wep.Add(new ButtonCallback(jobKvp.Key + ".wep." + job.NPC.ID + ".AddEquiptmentButton", new LabelData("Add", UnityEngine.Color.black, UnityEngine.TextAnchor.MiddleCenter)));
            }

            menu.Items.Add(new HorizontalGrid(wep, 200));

            foreach (var armor in inv.Armor)
            {
                List<IItem> items = new List<IItem>();
                items.Add(new Label(new LabelData(armor.Key.ToString(), UnityEngine.Color.black)));
                items.Add(new ItemIcon(armor.Value.Id));

                if (Armor.ArmorFactory.ArmorLookup.TryGetValue(armor.Value.Id, out var arm))
                {
                    items.Add(new Label(new LabelData(arm.Name, UnityEngine.Color.black, UnityEngine.TextAnchor.MiddleLeft, 18, LabelData.ELocalizationType.Type)));
                    items.Add(new ButtonCallback(jobKvp.Key + "." + armor.Key + ".arm." + job.NPC.ID + ".AddEquiptmentButton", new LabelData("Swap", UnityEngine.Color.black, UnityEngine.TextAnchor.MiddleCenter)));
                    items.Add(new ButtonCallback(jobKvp.Key + "." + armor.Key + ".arm." + job.NPC.ID + ".RemoveEquiptmentButton", new LabelData("Remove", UnityEngine.Color.black, UnityEngine.TextAnchor.MiddleCenter)));
                }
                else
                {
                    items.Add(new Label(new LabelData("", UnityEngine.Color.black)));
                    items.Add(new ButtonCallback(jobKvp.Key + "." + armor.Key + ".arm." + job.NPC.ID + ".AddEquiptmentButton", new LabelData("Add", UnityEngine.Color.black, UnityEngine.TextAnchor.MiddleCenter)));
                }

                menu.Items.Add(new HorizontalGrid(items, 200));
            }

            menu.Items.Add(new Line(UnityEngine.Color.black));
            menu.Items.Add(new ButtonCallback(jobKvp.Key + ".JobDetailsButton", new LabelData("Back", UnityEngine.Color.black, UnityEngine.TextAnchor.MiddleCenter)));
            NetworkMenuManager.SendServerPopup(data.Player, menu);
        }
    }
}
