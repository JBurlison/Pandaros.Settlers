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

        [ModLoader.ModCallbackAttribute(ModLoader.EModCallbackType.AfterWorldLoad,   GameLoader.NAMESPACE + ".Gameloader.SettlersExtender.AfterWorldLoad")]
        [ModLoader.ModCallbackProvidesForAttribute(GameLoader.NAMESPACE + ".Managers.MonsterManager.AfterWorldLoad")]
        public static void AfterWorldLoad()
        {
            foreach (var monster in _monsters)
            {
                var pandaBoss = (IPandaBoss) Activator.CreateInstance(monster);

                if (pandaBoss != null && !string.IsNullOrEmpty(pandaBoss.Name))
                {
                    PandaLogger.Log($"Boss {pandaBoss.Name} Loaded!");
                    MonsterManager.AddBoss(pandaBoss);
                }
            }
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
                                        var magicItem = (IMagicItem) Activator.CreateInstance(type);
                                        PandaLogger.Log($"Magic Item {magicItem.Name} Loaded!");
                                        break;

                                    case nameof(IPandaBoss):
                                        _monsters.Add(type);
                                        break;

                                    case nameof(ISeason):
                                        var season = (ISeason) Activator.CreateInstance(type);
                                        PandaLogger.Log($"Season {season.Name} Loaded.");
                                        SeasonsFactory.AddSeason(season);
                                        break;

                                    case nameof(IMachineSettings):
                                        var machineSettings = (IMachineSettings) Activator.CreateInstance(type);

                                        if (machineSettings != null && !string.IsNullOrEmpty(machineSettings.Name))
                                        {
                                            PandaLogger.Log($"Machine {machineSettings.Name} Loaded!");
                                            MachineManager.RegisterMachineType(machineSettings.Name, machineSettings);
                                        }

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

            SeasonsFactory.ResortSeasons();
        }
    }
}