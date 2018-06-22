using System.Collections.Generic;
using NPC;
using Pandaros.Settlers.Extender;

namespace Pandaros.Settlers.Items
{
    public interface IMagicItem : ICSType
    {
        NPCBase Owner { get; set; }
        IMagicEffect Effect { get; }
        void Update();
    }
}