using Pandaros.Settlers.Items;
using Pandaros.Settlers.Items.Weapons;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pandaros.Settlers.Extender.Providers
{
    public class WeaponProvider : IAfterWorldLoad
    {
        public List<Type> LoadedAssembalies { get; } = new List<Type>();

        public string InterfaceName => nameof(IWeapon);
        public Type ClassType => null;

        public void AfterWorldLoad()
        {
            StringBuilder sb = new StringBuilder();
            PandaLogger.Log(ChatColor.lime, "-------------------Weapons Loaded----------------------");
            var i = 0;

            foreach (var item in LoadedAssembalies)
            {
                if (Activator.CreateInstance(item) is IWeapon weapon &&
                    !string.IsNullOrEmpty(weapon.Name))
                {
                    if (ItemTypes.IndexLookup.TryGetIndex(weapon.Name, out var index))
                    {
                        WeaponFactory.WeaponLookup[index] = weapon;
                        sb.Append($"{weapon.Name}, ");
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
