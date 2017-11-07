using NPC;
using Pipliz;
using Pipliz.JSON;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pandaros.Settlers.Items.Machines
{
    public class MachineState
    {
        const double MINETIME = 4;
        static System.Random _rand = new System.Random();

        public static float MAX_DURABILITY { get; set; } = 100;
        public static int MAX_FUEL { get; set; } = 10;

        public Vector3Int Position { get; private set; }
        public float Durability { get; set; } = MAX_DURABILITY;
        public int Fuel { get; set; } = MAX_FUEL;

        public MachineState(Vector3Int pos)
        {
            Position = pos;
        }

        public MachineState(JSONNode baseNode)
        {

        }

        public virtual JSONNode ToJsonNode()
        {
            var baseNode = new JSONNode();

            return baseNode;
        }

        public virtual bool Repair(ref NPCBase.NPCState state, Players.Player player)
        {
            return false;
        }
        
        public virtual bool Refuel(ref NPCBase.NPCState state, Players.Player player)
        {
            return false;
        }
    }
}
