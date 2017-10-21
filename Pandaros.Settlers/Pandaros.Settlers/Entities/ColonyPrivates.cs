using NPC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Pandaros.Settlers.Entities
{
    public class PlayerColonyInterface
    {
        public PlayerColonyInterface(Colony c)
        {
            Colony = c;
        }

        public Colony Colony { get; private set; }

    }
}
