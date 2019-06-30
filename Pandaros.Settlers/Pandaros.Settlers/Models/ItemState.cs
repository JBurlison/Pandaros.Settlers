using Pipliz.JSON;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pandaros.Settlers.Models
{
    public class ItemStateChangedEventArgs: EventArgs
    {
        public ItemStateChangedEventArgs(ushort value, ushort oldValue)
        {
            Value = value;
            ReplacedValue = oldValue;
        }

        public ushort Value { get; private set; }

        public ushort ReplacedValue { get; private set; }
    }

    [Serializable]
    public class ItemState
    {
        ushort _id = default(ushort);

        public event EventHandler<ItemStateChangedEventArgs> IdChanged;

        public ItemState()
        {
        }

        public ItemState(JSONNode node)
        {
            if (node.TryGetAs(nameof(Id), out ushort id))
                Id = id;

            if (node.TryGetAs(nameof(Durability), out int durablility))
                Durability = durablility;
        }

        public ushort Id
        {
            get
            {
                return _id;
            }
            set
            {
                if (_id != value)
                {
                    var oldVal = _id;
                    _id = value;

                    if (IdChanged != null && IdChanged.GetInvocationList().Length != 0)
                        IdChanged(this, new ItemStateChangedEventArgs(_id, oldVal));
                }
            }
        }
        public int Durability { get; set; }

        public bool IsEmpty()
        {
            return Id == default(ushort);
        }

        public void FromJsonNode(string nodeName, JSONNode node)
        {
            if (node.TryGetAs(nodeName, out JSONNode stateNode))
            {
                if (stateNode.TryGetAs(nameof(Id), out ushort id))
                    Id = id;

                if (stateNode.TryGetAs(nameof(Durability), out int durablility))
                    Durability = durablility;
            }
        }

        public JSONNode ToJsonNode()
        {
            var baseNode = new JSONNode();

            baseNode[nameof(Id)] = new JSONNode(Id);
            baseNode[nameof(Durability)] = new JSONNode(Durability);

            return baseNode;
        }
    }
}
