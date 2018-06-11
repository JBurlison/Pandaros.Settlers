﻿using System;
using System.Collections.Generic;
using System.Linq;
using Pandaros.Settlers.Items;
using Pandaros.Settlers.Items.Machines;
using Pandaros.Settlers.Managers;
using Pandaros.Settlers.Monsters.Bosses;
using Pandaros.Settlers.Seasons;

namespace Pandaros.Settlers.Extender
{
    [ModLoader.ModManager]
    public static class SettlersExtender
    {
        private static List<ISettlersExtension> _settlersExtensions = new List<ISettlersExtension>();

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterWorldLoad,   GameLoader.NAMESPACE + ".Extender.SettlersExtender.AfterWorldLoad")]
        [ModLoader.ModCallbackProvidesFor(GameLoader.NAMESPACE + ".Managers.MonsterManager.AfterWorldLoad")]
        public static void AfterWorldLoad()
        {
            foreach (var extension in _settlersExtensions)
                extension.ActiveAssemblies();
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterModsLoaded, GameLoader.NAMESPACE + ".Extender.SettlersExtender.AfterModsLoaded")]
        public static void AfterModsLoaded(List<ModLoader.ModDescription> list)
        {
            LoadExtenstions(list);
            LoadImplementation(list);
        }

        private static void LoadImplementation(List<ModLoader.ModDescription> list)
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
                            foreach (var e in _settlersExtensions)
                                if (e.InterfaceName == iface.Name)
                                    e.LoadedAssembalies.Add(type);
                    }
                }
                catch (Exception)
                {
                    // Do not log it is not the correct type.
                }
        }

        private static void LoadExtenstions(List<ModLoader.ModDescription> list)
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
                                if (iface.Name == nameof(ISettlersExtension) &&
                                    Activator.CreateInstance(type) is ISettlersExtension extension)
                                {
                                    _settlersExtensions.Add(extension);
                                }
                            }
                            catch (Exception ex)
                            {
                                PandaLogger.LogError(ex, $"Error loading interface {iface.Name} on type {type.Name}");
                            }
                    }
                }
                catch (Exception)
                {
                    // Do not log it is not the correct type.
                }
        }
    }
}