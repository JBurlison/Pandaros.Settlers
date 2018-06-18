using Pandaros.Settlers.Items;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pandaros.Settlers.Extender.Providers
{
    public class MagicItemsProvider : ISettlersExtension
    {
        public List<Type> LoadedAssembalies { get; } = new List<Type>();

        public string InterfaceName => nameof(IMagicItem);
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
            PandaLogger.Log(ChatColor.lime, "-------------------Magic Items Loaded----------------------");

            foreach (var item in LoadedAssembalies)
            {
                if (Activator.CreateInstance(item) is IMagicItem magicItem &&
                    !string.IsNullOrEmpty(magicItem.Name))
                {
                    sb.Append($"{magicItem.Name}, ");
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
