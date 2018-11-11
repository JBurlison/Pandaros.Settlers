using Pandaros.Settlers.Items;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pandaros.Settlers.Extender.Providers
{
    public class ItemsProvider : IAfterWorldLoad, IAddItemTypes
    {
        StringBuilder _sb = new StringBuilder();

        public List<Type> LoadedAssembalies { get; } = new List<Type>();

        public string InterfaceName => nameof(ICSType);
        public Type ClassType => null;

        public void AddItemTypes(Dictionary<string, ItemTypesServer.ItemTypeRaw> itemTypes)
        {
            foreach (var item in LoadedAssembalies)
            {
                try
                {
                    if (Activator.CreateInstance(item) is ICSType itemType &&
                        !string.IsNullOrEmpty(itemType.Name))
                    {
                        var rawItem = new ItemTypesServer.ItemTypeRaw(itemType.Name, itemType.JsonSerialize());
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

        public void AfterWorldLoad()
        {
            PandaLogger.Log(ChatColor.lime, "-------------------Items Loaded----------------------");
            PandaLogger.Log(ChatColor.lime, _sb.ToString());
            PandaLogger.Log(ChatColor.lime, "------------------------------------------------------"); 
        }
    }
}
