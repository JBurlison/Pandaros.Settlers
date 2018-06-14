using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pandaros.Settlers.Extender.Providers
{
    public class ItemsProvider : ISettlersExtension
    {
        public List<Type> LoadedAssembalies { get; } = new List<Type>();

        public string InterfaceName => nameof(ICSType);

        public void AfterAddingBaseTypes(Dictionary<string, ItemTypesServer.ItemTypeRaw> itemTypes)
        {
            StringBuilder sb = new StringBuilder();

            PandaLogger.Log(ChatColor.lime, "-------------------Items Loaded----------------------");

            foreach (var item in LoadedAssembalies)
            {
                if (item.Name != nameof(CSType) &&
                    Activator.CreateInstance(item) is ICSType itemType &&
                    !string.IsNullOrEmpty(itemType.Name))
                {
                    var rawItem = new ItemTypesServer.ItemTypeRaw(itemType.Name, itemType.ToJsonNode());
                    itemTypes.Add(itemType.Name, rawItem);
                    sb.Append($"{itemType.Name}, ");
                }
            }

            PandaLogger.Log(ChatColor.lime, sb.ToString());
            PandaLogger.Log(ChatColor.lime, "------------------------------------------------------");
        }

        public void AfterItemTypesDefined()
        {
            
        }

        public void AfterSelectedWorld()
        {
            
        }

        public void AfterWorldLoad()
        {
            
        }
    }
}
