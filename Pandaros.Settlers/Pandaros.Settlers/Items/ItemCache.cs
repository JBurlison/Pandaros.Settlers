using Pandaros.Settlers.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandaros.Settlers.Items
{
    public static class ItemCache
    {
        public static EventedDictionary<string, ICSType> CSItems { get; private set; } = new EventedDictionary<string, ICSType>();
    }
}
