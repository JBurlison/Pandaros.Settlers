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
        static System.Random _rand = new System.Random();

        public static float MAX_DURABILITY { get; set; } = 100;
        public static int MAX_FUEL { get; set; } = 10;

        public Vector3Int Position { get; private set; }
        public float Durability { get; set; } = MAX_DURABILITY;
        public int Fuel { get; set; } = MAX_FUEL;
        public string MachineType { get; private set; }
        public Players.Player Owner { get; private set; }

        private MachineManager.MachineCallback _machineCallback;

        public MachineState(Vector3Int pos, Players.Player owner, string machineType)
        {
            Position = pos;
            MachineType = machineType;
            Owner = owner;

            _machineCallback = MachineManager.GetCallback(machineType);
        }

        public MachineState(JSONNode baseNode, Players.Player owner)
        {

        }

        public virtual JSONNode ToJsonNode()
        {
            var baseNode = new JSONNode();

            return baseNode;
        }

        public void Repair(ref NPCBase.NPCState state)
        {
            _machineCallback.Repair(state, Owner, this);
        }
        
        public void Refuel(ref NPCBase.NPCState state)
        {
            _machineCallback.Refuel(state, Owner, this);
        }

        public void DoWork()
        {
            _machineCallback.DoWork(Owner, this);
        }
    }
}
