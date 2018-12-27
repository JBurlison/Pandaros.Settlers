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
            var i = 0;

            foreach (var item in LoadedAssembalies)
            {
                try
                {
                    if (Activator.CreateInstance(item) is ICSType itemType &&
                        !string.IsNullOrEmpty(itemType.Name))
                    {
                        var rawItem = new ItemTypesServer.ItemTypeRaw(itemType.Name, itemType.JsonSerialize());
                        itemTypes.Add(itemType.Name, rawItem);

                        if (itemType.StaticItemSettings != null && !string.IsNullOrWhiteSpace(itemType.StaticItemSettings.Name))
                            StaticItems.List.Add(itemType.StaticItemSettings);

                        if (itemType is IPlayerMagicItem pmi)
                            MagicItemsCache.PlayerMagicItems[pmi.Name] = pmi;

                        _sb.Append($"{itemType.Name}, ");
                        i++;

                        if (i > 5)
                        {
                            _sb.Append("</color>");
                            i = 0;
                            _sb.AppendLine();
                            _sb.Append("<color=lime>");
                        }
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
