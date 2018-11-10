using Pandaros.Settlers.Items;
using Pandaros.Settlers.Items.Temperature;
using Pandaros.Settlers.Items.Weapons;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pandaros.Settlers.Extender.Providers
{
    public class WeaponProvider : ISettlersExtension
    {
        public List<Type> LoadedAssembalies { get; } = new List<Type>();

        public string InterfaceName => nameof(IWeapon);
        public Type ClassType => null;

        public void AddItemTypes(Dictionary<string, ItemTypesServer.ItemTypeRaw> itemTypes)
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
            PandaLogger.Log(ChatColor.lime, "-------------------Weapons Loaded----------------------");

            foreach (var item in LoadedAssembalies)
            {
                if (Activator.CreateInstance(item) is IWeapon weapon &&
                    !string.IsNullOrEmpty(weapon.Name))
                {
                    if (ItemTypes.IndexLookup.TryGetIndex(weapon.Name, out var index))
                    {
                        WeaponFactory.WeaponLookup[index] = weapon;
                        sb.Append($"{weapon.Name}, ");
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
