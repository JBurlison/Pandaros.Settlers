using System.Collections.Generic;
using NPC;
using Pandaros.Settlers.Extender;

namespace Pandaros.Settlers.Items
{
    public interface INpcMagicItem : ICSType, IMagicEffect
    {
        NPCBase Owner { get; set; }
    }
}