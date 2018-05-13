using System;
using System.Collections.Generic;
using System.Linq;
using Pandaros.Settlers.Items;
using Pandaros.Settlers.Items.Machines;
using Pandaros.Settlers.Managers;
using Pandaros.Settlers.Monsters.Bosses;
using Pandaros.Settlers.Seasons;

namespace Pandaros.Settlers
{
    [ModLoader.ModManagerAttribute]
    public static class SettlersExtender
    {
        private static readonly List<Type> _monsters = new List<Type>();
        private static readonly List<Type> _magicITems = new List<Type>();
        private static readonly List<Type> _season = new List<Type>();
        private static readonly List<Type> _machineSettings = new List<Type>();

        [ModLoader.ModCallbackAttribute(ModLoader.EModCallbackType.AfterWorldLoad,   GameLoader.NAMESPACE + ".Gameloader.SettlersExtender.AfterWorldLoad")]
        [ModLoader.ModCallbackProvidesForAttribute(GameLoader.NAMESPACE + ".Managers.MonsterManager.AfterWorldLoad")]
        public static void AfterWorldLoad()
        {
            ActivateBosses();
            ActivateMachine();
            ActivateMagicItems();
            ActivateSeasons();
        }

        [ModLoader.ModCallbackAttribute(ModLoader.EModCallbackType.AfterModsLoaded,
            GameLoader.NAMESPACE + ".Gameloader.SettlersExtender.AfterModsLoaded")]
        public static void AfterModsLoaded(List<ModLoader.ModDescription> list)
        {
            foreach (var mod in list.Where(m => m.HasAssembly))
                try
                {
                    // Get all Types available in the assembly in an array
                    var typeArray = mod.LoadedAssembly.GetTypes();

                    // Walk through each Type and list their Information
                    foreach (var type in typeArray)
                    {
                        var ifaces = type.GetInterfaces();

                        foreach (var iface in ifaces)
                            try
                            {
                                switch (iface.Name)
                                {
                                    case nameof(IMagicItem):
                                        _magicITems.Add(type);
                                        break;

                                    case nameof(IPandaBoss):
                                        _monsters.Add(type);
                                        break;

                                    case nameof(ISeason):
                                        _season.Add(type);
                                        break;

                                    case nameof(IMachineSettings):
                                        _machineSettings.Add(type);
                                        break;
                                }
                            }
                            catch (Exception ex)
                            {
                                PandaLogger.LogError(ex, $"Error loading interface {iface.Name}");
                            }
                    }
                }
                catch (Exception)
                {
                    // Do not log it is not the correct type.
                }
        }

        private static void ActivateBosses()
        {
            foreach (var monster in _monsters)
            {
                if (Activator.CreateInstance(monster) is IPandaBoss pandaBoss &&
                    !string.IsNullOrEmpty(pandaBoss.Name))
                {
                    PandaLogger.Log($"Boss {pandaBoss.Name} Loaded!");
                    MonsterManager.AddBoss(pandaBoss);
                }
            }
        }

        private static void ActivateMagicItems()
        {
            foreach (var item in _magicITems)
            {
                if (Activator.CreateInstance(item) is IMagicItem magicItem &&
                    !string.IsNullOrEmpty(magicItem.Name))
                {
                    PandaLogger.Log($"Magic Item {magicItem.Name} Loaded!");
                }
            }
        }

        private static void ActivateSeasons()
        {
            foreach (var s in _season)
            {
                if (Activator.CreateInstance(s) is ISeason season &&
                    !string.IsNullOrEmpty(season.Name))
                {
                    PandaLogger.Log($"Season {season.Name} Loaded.");
                    SeasonsFactory.AddSeason(season);
                }
            }

            SeasonsFactory.ResortSeasons();
        }

        private static void ActivateMachine()
        {
            foreach (var s in _machineSettings)
            {
                if (Activator.CreateInstance(s) is IMachineSettings machineSettings &&
                    !string.IsNullOrEmpty(machineSettings.Name))
                {
                    PandaLogger.Log($"Machine {machineSettings.Name} Loaded!");
                    MachineManager.RegisterMachineType(machineSettings.Name, machineSettings);
                }
            }
        }
    }
}