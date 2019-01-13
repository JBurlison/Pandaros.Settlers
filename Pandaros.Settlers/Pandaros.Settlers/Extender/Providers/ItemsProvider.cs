using Newtonsoft.Json;
using Pandaros.Settlers.Items;
using Pipliz.JSON;
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
            List<ICSType> loadedItems = new List<ICSType>();

            foreach (var item in LoadedAssembalies)
            {
                try
                {
                    if (Activator.CreateInstance(item) is ICSType itemType &&
                        !string.IsNullOrEmpty(itemType.Name))
                    {
                        loadedItems.Add(itemType);            
                    }
                }
                catch (Exception ex)
                {
                    PandaLogger.LogError(ex);
                }
            }

            foreach (var modInfoKvP in GameLoader.AllModInfos)
            {
                foreach (var info in GameLoader.AllModInfos)
                    if (info.Value.TryGetAs(GameLoader.NAMESPACE + ".jsonFiles", out JSONNode jsonFilles))
                    {
                        foreach (var jsonNode in jsonFilles.LoopArray())
                        {
                            if (jsonNode.TryGetAs("fileType", out string jsonFileType) &&
                                jsonFileType == GameLoader.NAMESPACE + ".CSItems" &&
                                jsonNode.TryGetAs("relativePath", out string itemsPath))
                            {
                                try
                                {
                                    loadedItems.AddRange(JSONExtentionMethods.GetSimpleListFromFile<CSType>(itemsPath));
                                }
                                catch (Exception ex)
                                {
                                    PandaLogger.LogError(ex);
                                }
                            }
                        }
                    }
            }

            foreach (var itemType in loadedItems)
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

        public void AfterWorldLoad()
        {
            PandaLogger.Log(ChatColor.lime, "-------------------Items Loaded----------------------");
            PandaLogger.Log(ChatColor.lime, _sb.ToString());
            PandaLogger.Log(ChatColor.lime, "------------------------------------------------------"); 
        }
    }
}
