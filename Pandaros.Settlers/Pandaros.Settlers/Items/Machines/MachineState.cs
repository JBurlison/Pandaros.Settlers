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

        public static float MAX_DURABILITY { get; set; } = 1f;
        public static float MAX_FUEL { get; set; } = 1f;
        public static float MAX_LOAD { get; set; } = 1f;

        public Vector3Int Position { get; private set; }
        public float Durability { get; set; } = MAX_DURABILITY;
        public float Fuel { get; set; } = MAX_FUEL;
        public float Load { get; set; } = MAX_LOAD;

        public string MachineType { get; private set; }
        public Players.Player Owner { get; private set; }
        public double NextTimeForWork { get; set; } = Time.SecondsSinceStartDouble + _rand.NextDouble(0, 5);

        public MachineManager.MachineSettings MachineSettings { get; private set; }

        public MachineState(Vector3Int pos, Players.Player owner, string machineType)
        {
            Position = pos;
            MachineType = machineType;
            Owner = owner;

            MachineSettings = MachineManager.GetCallbacks(machineType);
        }

        public MachineState(JSONNode baseNode, Players.Player owner)
        {
            Position = (Vector3Int)baseNode[nameof(Position)];
            Durability = baseNode.GetAs<float>(nameof(Durability));
            Fuel = baseNode.GetAs<float>(nameof(Fuel));
            MachineType = baseNode.GetAs<string>(nameof(MachineType));

            if (baseNode.TryGetAs<float>(nameof(Load), out var load))
                Load = load;

            MachineSettings = MachineManager.GetCallbacks(MachineType);
        }

        public virtual JSONNode ToJsonNode()
        {
            var baseNode = new JSONNode();

            baseNode.SetAs(nameof(Position), (JSONNode)Position);
            baseNode.SetAs(nameof(Durability), Durability);
            baseNode.SetAs(nameof(Fuel), Fuel);
            baseNode.SetAs(nameof(Load), Load);
            baseNode.SetAs(nameof(MachineType), MachineType);

            return baseNode;
        }
    }
}
