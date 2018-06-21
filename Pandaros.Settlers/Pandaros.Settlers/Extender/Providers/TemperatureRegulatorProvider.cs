using Pandaros.Settlers.Items;
using Pandaros.Settlers.Items.Temperature;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pandaros.Settlers.Extender.Providers
{
    public class TemperatureRegulatorProvider : ISettlersExtension
    {
        public List<Type> LoadedAssembalies { get; } = new List<Type>();

        public string InterfaceName => nameof(ITemperatureRegulator);
        public Type ClassType => null;

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
            PandaLogger.Log(ChatColor.lime, "-------------------Temperature Regulators Loaded----------------------");

            foreach (var item in LoadedAssembalies)
            {
                if (Activator.CreateInstance(item) is ITemperatureRegulator tempRegulator &&
                    !string.IsNullOrEmpty(tempRegulator.Name))
                {
                    sb.Append($"{tempRegulator.Name}, ");
                }
            }

            PandaLogger.Log(ChatColor.lime, sb.ToString());
            PandaLogger.Log(ChatColor.lime, "---------------------------------------------------------");
        }

        public void OnAddResearchables()
        {
            
        }
    }
}
