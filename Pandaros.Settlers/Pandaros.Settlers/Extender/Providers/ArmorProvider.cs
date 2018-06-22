using Pandaros.Settlers.Items;
using Pandaros.Settlers.Items.Armor;
using System;
using System.Collections.Generic;
using System.Text;
using static Pandaros.Settlers.Items.ArmorFactory;

namespace Pandaros.Settlers.Extender.Providers
{
    public class ArmorProvider : ISettlersExtension
    {
        public List<Type> LoadedAssembalies { get; } = new List<Type>();

        public string InterfaceName => nameof(IArmor);
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
            PandaLogger.Log(ChatColor.lime, "-------------------Armor Loaded----------------------");

            foreach (var item in LoadedAssembalies)
            {
                if (Activator.CreateInstance(item) is IArmor armor &&
                    !string.IsNullOrEmpty(armor.Name))
                {
                    if (ItemTypes.IndexLookup.TryGetIndex(armor.Name, out var index))
                    {
                        ArmorFactory.ArmorLookup[index] = armor;
                        sb.Append($"{armor.Name}, ");
                    }
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
