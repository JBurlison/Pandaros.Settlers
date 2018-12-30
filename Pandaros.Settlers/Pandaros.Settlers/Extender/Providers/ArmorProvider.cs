using Pandaros.Settlers.Items;
using Pandaros.Settlers.Items.Armor;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pandaros.Settlers.Extender.Providers
{
    public class ArmorProvider : IAfterWorldLoad
    {
        public List<Type> LoadedAssembalies { get; } = new List<Type>();

        public string InterfaceName => nameof(IArmor);
        public Type ClassType => null;

        public void AfterWorldLoad()
        {
            StringBuilder sb = new StringBuilder();
            PandaLogger.Log(ChatColor.lime, "-------------------Armor Loaded----------------------");
            var i = 0;

            foreach (var item in LoadedAssembalies)
            {
                if (Activator.CreateInstance(item) is IArmor armor &&
                    !string.IsNullOrEmpty(armor.Name))
                {
                    if (ItemTypes.IndexLookup.TryGetIndex(armor.Name, out var index))
                    {
                        ArmorFactory.ArmorLookup[index] = armor;
                        sb.Append($"{armor.Name}, ");
                        i++;
                        
                        if (i > 5)
                        {
                            sb.Append("</color>");
                            i = 0;
                            sb.AppendLine();
                            sb.Append("<color=lime>");
                        }
                    }
                }
            }

            PandaLogger.Log(ChatColor.lime, sb.ToString());
            PandaLogger.Log(ChatColor.lime, "---------------------------------------------------------");
        }
    }
}
