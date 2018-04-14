using Pandaros.Settlers.Monsters.Bosses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Pandaros.Settlers
{
    [ModLoader.ModManager]
    class SettlersExtender
    {
        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterModsLoaded, GameLoader.NAMESPACE + ".Gameloader.AfterModsLoaded")]
        public static void AfterModsLoaded(List<ModLoader.ModDescription> list)
        {
            foreach (var mod in list.Where(m => m.HasAssembly))
            {
                try
                {
                    // Get all Types available in the assembly in an array
                    Type[] typeArray = mod.LoadedAssembly.GetTypes();

                    // Walk through each Type and list their Information
                    foreach (Type type in typeArray)
                    {
                        Type[] ifaces = type.GetInterfaces();

                        foreach (Type iface in ifaces)
                        {
                            switch (iface.Name)
                            {
                                case nameof(IPandaBoss):

                                    break;
                            }

                            //if (iface.Name == FILTER_INTERFACE)
                            //{
                            //    IUiAlertModule module = (IUiAlertModule)Activator.CreateInstance(type);

                            //    if (module != null)
                            //    {
                            //        modules.Add(module);
                            //        Logger.LogInformation("Module Loaded: " + file);
                            //    }
                            //}
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
}
