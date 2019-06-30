using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Pipliz.JSON;

namespace Pandaros.Settlers.Models
{
    public class OnRemove
    {
        public int amount { get; set; }
        public float chance { get; set; }
        public string type { get; set; }

        public OnRemove() { }

        public OnRemove(int dropAmount, float dropChance, string csType)
        {
            amount = dropAmount;
            chance = dropChance;
            type = csType;
        }
    }
}
