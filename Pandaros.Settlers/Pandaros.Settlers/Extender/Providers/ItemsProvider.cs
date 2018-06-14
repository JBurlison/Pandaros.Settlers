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
            foreach (var item in LoadedAssembalies)
            {
                if (item.Name != nameof(CSType) &&
                    Activator.CreateInstance(item) is ICSType itemType &&
                    !string.IsNullOrEmpty(itemType.Name))
                {
                    PandaLogger.Log($"Item {itemType.Name} Loaded!");

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
            
        }
    }
}
