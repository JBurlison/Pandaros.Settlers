using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Pipliz.JSON;

namespace Pandaros.Settlers.Extender
{
    public class OnRemove : IJsonConvertable
    {
        public int amount { get; private set; }
        public float chance { get; private set; }
        public string type { get; private set; }

        public OnRemove(int dropAmount, float dropChance, string csType)
        {
            amount = dropAmount;
            chance = dropChance;
            type = csType;
        }

        public JSONNode ToJsonNode()
        {
            var node = new JSONNode();

            node.SetAs(nameof(amount), amount);
            node.SetAs(nameof(chance), chance);
            node.SetAs(nameof(type), type);

            return node;
        }
    }
}
