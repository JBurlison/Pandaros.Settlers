using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Pipliz.JSON;

namespace Pandaros.Settlers.Items
{
    public class OnRemove
    {
        public int amount { get; private set; }
        public float chance { get; private set; }
        public string type { get; private set; }

        public OnRemove() { }

        public OnRemove(int dropAmount, float dropChance, string csType)
        {
            amount = dropAmount;
            chance = dropChance;
            type = csType;
        }
    }
}
