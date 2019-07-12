using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetworkUI;

namespace Pandaros.Settlers.Extender.Providers
{
    public class OnConstructInventoryManageColonyUIProvider : IOnConstructInventoryManageColonyUIExtender
    {
        public List<Type> LoadedAssembalies { get; } = new List<Type>();

        public string InterfaceName => nameof(IOnConstructInventoryManageColonyUI);

        public Type ClassType => null;

        List<IOnConstructInventoryManageColonyUI> _onConstructInventoryManageColonyUIs = new List<IOnConstructInventoryManageColonyUI>();

        public void OnConstructInventoryManageColonyUI(Players.Player player, NetworkMenu networkMenu)
        {
            if (_onConstructInventoryManageColonyUIs.Count == 0)
                foreach(var type in LoadedAssembalies)
                    _onConstructInventoryManageColonyUIs.Add((IOnConstructInventoryManageColonyUI)Activator.CreateInstance(type));

            foreach (var ui in _onConstructInventoryManageColonyUIs)
                try
                {
                    ui.OnConstructInventoryManageColonyUI(player, networkMenu);
                }
                catch  (Exception ex)
                {
                    PandaLogger.LogError(ex);
                }
        }
    }
}
