using Pandaros.Settlers.Items;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandaros.Settlers.Energy
{
    public class ManaTank : CSType
    {
        public override string name { get; set; }
    }
    /*
     Mana Machines
        * Increased Guard damage (10%)
        * Monster AoE Damage 10 per 5/sec, 4 block each direction (advanced)
        * Invisible wall (5x2) rotatable (normal blocks, just invisible)
        * +2% Luck 4 blocks each direction (Luck in settlers is used for crit chance and bonus item procs (bonus items are rare items sometimes crafted with settlers items)
        *  Happiness Generator (Colonists are hypnotized by its beauty) +10 happiness while running. (Advanced)

    All machines have to be maintained by a Artificer, a roaming job similar to machinist .  Machines need to be connected by mana pipes and consume 1 mana every in game hour.

    These mana pipes need a mana pump attached to a mana tank (also maintained and filled by a Artificer)
    */
}
