using System.Collections.Generic;
using NPC;
using Pandaros.Settlers.Extender;

namespace Pandaros.Settlers.Items
{
    public interface IMagicItem : ICSType, IMagicEffect
    {
        NPCBase Owner { get; set; }
    }
}