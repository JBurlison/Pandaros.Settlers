using Pandaros.Settlers.Items;
using Pandaros.Settlers.Jobs.Roaming;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pandaros.Settlers.Extender.Providers
{
    public class AfterModsLoadedProvider : IAfterModsLoadedExtention
    {
        public List<Type> LoadedAssembalies { get; } = new List<Type>();

        public string InterfaceName => nameof(IAfterModsLoaded);

        public Type ClassType => null;

        public void AfterModsLoaded(List<ModLoader.ModDescription> list)
        {
            foreach (var s in LoadedAssembalies)
            {
                if (Activator.CreateInstance(s) is IAfterModsLoaded modsLoaded)
                {
                    modsLoaded.AfterModsLoaded(list);
                }
            }
        }
    }
}
