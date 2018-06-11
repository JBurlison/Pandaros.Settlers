using Pandaros.Settlers.Items;
using System;
using System.Collections.Generic;

namespace Pandaros.Settlers.Extender.Providers
{
    public class MagicItemsProvider : ISettlersExtension
    {
        public List<Type> LoadedAssembalies { get; } = new List<Type>();

        public string InterfaceName => nameof(IMagicItem);

        public void ActiveAssemblies()
        {
            foreach (var item in LoadedAssembalies)
            {
                if (Activator.CreateInstance(item) is IMagicItem magicItem &&
                    !string.IsNullOrEmpty(magicItem.Name))
                {
                    PandaLogger.Log($"Magic Item {magicItem.Name} Loaded!");
                }
            }
        }
    }
}
