using Pandaros.Settlers.Items.Machines;
using Pandaros.Settlers.Managers;
using Pandaros.Settlers.Monsters.Bosses;
using System;
using System.Collections.Generic;

namespace Pandaros.Settlers.Extender.Providers
{
    public class MachineSettingsProvider : ISettlersExtension
    {
        public List<Type> LoadedAssembalies { get; } = new List<Type>();

        public string InterfaceName => nameof(IMachineSettings);

        public void ActiveAssemblies()
        {
            foreach (var s in LoadedAssembalies)
            {
                if (Activator.CreateInstance(s) is IMachineSettings machineSettings &&
                    !string.IsNullOrEmpty(machineSettings.Name))
                {
                    PandaLogger.Log($"Machine {machineSettings.Name} Loaded!");
                    MachineManager.RegisterMachineType(machineSettings);
                }
            }
        }
    }
}
