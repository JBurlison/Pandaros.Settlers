using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pandaros.Settlers.Extender.Providers
{
    public class ItemsProvider : ISettlersExtension
    {
        StringBuilder _sb = new StringBuilder();

        public List<Type> LoadedAssembalies { get; } = new List<Type>();

        public string InterfaceName => nameof(ICSType);
        public string ClassName => null;

        public void AfterAddingBaseTypes(Dictionary<string, ItemTypesServer.ItemTypeRaw> itemTypes)
        {
            foreach (var item in LoadedAssembalies)
            {
                try
                {
                    if (item.FullName != "Pandaros.Settlers.Extender.CSType" &&
                        Activator.CreateInstance(item) is ICSType itemType &&
                        !string.IsNullOrEmpty(itemType.Name))
                    {
                        var rawItem = new ItemTypesServer.ItemTypeRaw(itemType.Name, itemType.ToJsonNode());
                        itemTypes.Add(itemType.Name, rawItem);
                        _sb.Append($"{itemType.Name}, ");
                    }
                }
                catch (Exception ex)
                {
                    PandaLogger.LogError(ex);
                }
            }

        }

        public void AfterItemTypesDefined()
        {
            
        }

        public void AfterSelectedWorld()
        {
            
        }

        public void AfterWorldLoad()
        {
            PandaLogger.Log(ChatColor.lime, "-------------------Items Loaded----------------------");
            PandaLogger.Log(ChatColor.lime, _sb.ToString());
            PandaLogger.Log(ChatColor.lime, "------------------------------------------------------"); 
        }
    }
}
