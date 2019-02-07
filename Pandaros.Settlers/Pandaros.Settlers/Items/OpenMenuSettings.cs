using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandaros.Settlers.Items
{
    public class OpenMenuSettings
    {
        public string UIUrl { get; set; }
        public Shared.PlayerClickedData.ClickType ActivateClickType { get; set; } = Shared.PlayerClickedData.ClickType.Right;
        public string ItemName { get; set; }
    }
}
