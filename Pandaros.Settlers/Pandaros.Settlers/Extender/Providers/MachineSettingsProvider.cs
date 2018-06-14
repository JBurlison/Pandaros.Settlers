using Pandaros.Settlers.Items.Machines;
using Pandaros.Settlers.Managers;
using Pandaros.Settlers.Monsters.Bosses;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pandaros.Settlers.Extender.Providers
{
    public class MachineSettingsProvider : ISettlersExtension
    {
        public List<Type> LoadedAssembalies { get; } = new List<Type>();

        public string InterfaceName => nameof(IMachineSettings);

        public void AfterAddingBaseTypes(Dictionary<string, ItemTypesServer.ItemTypeRaw> itemTypes)
        {
            
        }

        public void AfterItemTypesDefined()
        {
          
        }

        public void AfterSelectedWorld()
        {
            
        }

        public void AfterWorldLoad()
        {
            StringBuilder sb = new StringBuilder();
            PandaLogger.Log(ChatColor.lime, "-------------------Machines Loaded----------------------");

            foreach (var s in LoadedAssembalies)
            {
                if (Activator.CreateInstance(s) is IMachineSettings machineSettings &&
                    !string.IsNullOrEmpty(machineSettings.Name))
                {
                    sb.Append($"{machineSettings.Name}, ");
                    MachineManager.RegisterMachineType(machineSettings);
                }
            }

            PandaLogger.Log(ChatColor.lime, sb.ToString());
            PandaLogger.Log(ChatColor.lime, "---------------------------------------------------------");

            
        }
    }
}
