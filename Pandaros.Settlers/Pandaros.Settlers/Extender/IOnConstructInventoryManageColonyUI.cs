using NetworkUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandaros.Settlers.Extender
{
    public interface IOnConstructInventoryManageColonyUI
    {
        void OnConstructInventoryManageColonyUI(Players.Player player, NetworkMenu networkMenu);
    }
}
