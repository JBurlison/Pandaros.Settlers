using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandaros.Settlers.Models
{
    public class OpenMenuSettings
    {
        public string UIUrl { get; set; }
        public Shared.PlayerClickedData.EClickType ActivateClickType { get; set; } = Shared.PlayerClickedData.EClickType.Right;
        public string ItemName { get; set; }
    }
}
