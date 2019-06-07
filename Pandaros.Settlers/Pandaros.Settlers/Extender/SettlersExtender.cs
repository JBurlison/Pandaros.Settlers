using Pipliz.JSON;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Pandaros.Settlers.Extender
{
    [ModLoader.ModManager]
    public static class SettlersExtender
    {
        private static List<ISettlersExtension> _settlersExtensions = new List<ISettlersExtension>();
        private static List<IOnTimedUpdate> _timedUpdate = new List<IOnTimedUpdate>();

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnUpdate, GameLoader.NAMESPACE + ".Extender.SettlersExtender.OnUpdate")]
        public static void OnUpdate()
        {
            foreach (var extension in _timedUpdate)
                try
                {
                    if (extension.NextUpdateTime < TimeCycle.TotalTime.Value.TotalSeconds)
                    {
                        extension.OnTimedUpdate();
                        extension.NextUpdateTime = TimeCycle.TotalTime.Value.TotalSeconds + Pipliz.Random.NextDouble(extension.NextUpdateTimeMin, extension.NextUpdateTimeMax);
                    }
                }
                catch (Exception ex)
                {
                    PandaLogger.LogError(ex);
                }
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnAddResearchables, GameLoader.NAMESPACE + ".Extender.SettlersExtender.OnAddResearchables")]
        [ModLoader.ModCallbackDependsOn(GameLoader.NAMESPACE + ".Research.PandaResearch.OnAddResearchables")]
        public static void Register()
        {
            foreach (var extension in _settlersExtensions.Where(s => s as IOnAddResearchables != null).Select(ex => ex as IOnAddResearchables))
                try
                {
                    extension.OnAddResearchables();
                }
                catch (Exception ex)
                {
                    PandaLogger.LogError(ex);
                }
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnLoadingColony, GameLoader.NAMESPACE + ".Extender.SettlersExtender.OnLoadingColony")]
        public static void OnLoadingColony(Colony c, JSONNode n)
        {
            foreach (var extension in _settlersExtensions.Where(s => s as IOnLoadingColony != null).Select(ex => ex as IOnLoadingColony))
                try
                {
                    extension.OnLoadingColony(c, n);
                }
                catch (Exception ex)
                {
                    PandaLogger.LogError(ex);
                }
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterWorldLoad,   GameLoader.NAMESPACE + ".Extender.SettlersExtender.AfterWorldLoad")]
        [ModLoader.ModCallbackProvidesFor(GameLoader.NAMESPACE + ".Managers.MonsterManager.AfterWorldLoad")]
        public static void AfterWorldLoad()
        {
            foreach (var extension in _settlersExtensions.Where(s => s as IAfterWorldLoad != null).Select(ex => ex as IAfterWorldLoad))
                try
                {
                    extension.AfterWorldLoad();
                }
                catch (Exception ex)
                {
                    PandaLogger.LogError(ex);
                }
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterModsLoaded, GameLoader.NAMESPACE + ".Extender.SettlersExtender.AfterModsLoaded")]
        public static void AfterModsLoaded(List<ModLoader.ModDescription> list)
        {
            LoadExtenstions(list);
            LoadImplementation(list);
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterItemTypesDefined, GameLoader.NAMESPACE + ".Extender.SettlersExtender.AfterItemTypesDefined")]
        public static void AfterItemTypesDefined()
        {
            foreach (var extension in _settlersExtensions.Where(s => s as IAfterItemTypesDefined != null).Select(ex => ex as IAfterItemTypesDefined))
                try
                {
                    extension.AfterItemTypesDefined();
                }
                catch (Exception ex)
                {
                    PandaLogger.LogError(ex);
                }
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterSelectedWorld, GameLoader.NAMESPACE + ".Extender.SettlersExtender.AfterSelectedWorld")]
        [ModLoader.ModCallbackProvidesFor("pipliz.server.registertexturemappingtextures")]
        public static void AfterSelectedWorld()
        {
            foreach (var extension in _settlersExtensions.Where(s => s as IAfterSelectedWorld != null).Select(ex => ex as IAfterSelectedWorld))
                try
                {
                    extension.AfterSelectedWorld();
                }
                catch (Exception ex)
                {
                    PandaLogger.LogError(ex);
                }
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnCreatedColony, GameLoader.NAMESPACE + ".Extender.SettlersExtender.OnCreatedColony")]
        public static void OnCreatedColony(Colony c)
        {
            foreach (var extension in _settlersExtensions.Where(s => s as IOnColonyCreated != null).Select(ex => ex as IOnColonyCreated))
                try
                {
                    extension.ColonyCreated(c);
                }
                catch (Exception ex)
                {
                    PandaLogger.LogError(ex);
                }
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AddItemTypes, GameLoader.NAMESPACE + ".Extender.SettlersExtender.AddItemTypes")]
        [ModLoader.ModCallbackDependsOn("pipliz.server.applymoditempatches")]
        public static void AddItemTypes(Dictionary<string, ItemTypesServer.ItemTypeRaw> itemTypes)
        {
            foreach (var extension in _settlersExtensions.Where(s => s as IAddItemTypes != null).Select(ex => ex as IAddItemTypes))
                try
                {
                    extension.AddItemTypes(itemTypes);
                }
                catch (Exception ex)
                {
                    PandaLogger.LogError(ex);
                }
        }

        private static void LoadImplementation(List<ModLoader.ModDescription> list)
        {
            foreach (var mod in list.Where(m => m.HasAssembly && !string.IsNullOrEmpty(m.assemblyPath) && !m.assemblyPath.Contains("Pipliz\\modInfo.json")))
                try
                {
                    // Get all Types available in the assembly in an array
                    var typeArray = mod.LoadedAssembly.GetTypes();

                    // Walk through each Type and list their Information
                    foreach (var type in typeArray)
                    {
                        var ifaces = type.GetInterfaces();

                        foreach (var iface in ifaces)
                        {
                            foreach (var e in _settlersExtensions)
                                if (!string.IsNullOrEmpty(e.InterfaceName) && e.InterfaceName == iface.Name && !type.IsInterface)
                                {
                                    var constructor = type.GetConstructor(Type.EmptyTypes);

                                    if (constructor != null)
                                        e.LoadedAssembalies.Add(type);
                                }

                            if (!string.IsNullOrEmpty(iface.Name) && nameof(IOnTimedUpdate) == iface.Name && !type.IsInterface)
                            {
                                var constructor = type.GetConstructor(Type.EmptyTypes);

                                if (constructor != null && Activator.CreateInstance(type) is IOnTimedUpdate onUpdateCallback)
                                {
                                    PandaLogger.Log(ChatColor.lime, "OnTimedUpdateLoaded: {0}", onUpdateCallback.GetType().Name);
                                    _timedUpdate.Add(onUpdateCallback);
                                }
                            }
                        }

                        foreach (var e in _settlersExtensions)
                            if (e.ClassType != null && type.Equals(e.ClassType))
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
                            catch (MissingMethodException)
                            {
                                // do nothing, we tried to load a interface.
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