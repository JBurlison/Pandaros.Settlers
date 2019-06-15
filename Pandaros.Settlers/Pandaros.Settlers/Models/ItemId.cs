using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandaros.Settlers.Models
{
    public class ItemId : IEquatable<ItemId>
    {
        static Dictionary<string, ItemId> _cacheString = new Dictionary<string, ItemId>();
        static Dictionary<ushort, ItemId> _cacheUshort = new Dictionary<ushort, ItemId>();

        public static ItemId GetItemId(string name)
        {
            ItemId item = null;

            lock (_cacheString)
                if (!_cacheString.TryGetValue(name, out item))
                {
                    item = new ItemId(name);
                    _cacheString.Add(item.Name, item);
                }

            return item;
        }

        public static ItemId GetItemId(ushort id)
        {
            ItemId item = null;

            lock (_cacheUshort)
                if (!_cacheUshort.TryGetValue(id, out item))
                {
                    item = new ItemId(id);
                    _cacheUshort.Add(item.Id, item);
                }

            return item;
        }

        ushort _id;
        string _name;

        public string Name
        {
            get
            {
                if (string.IsNullOrEmpty(_name))
                {
                    if (ItemTypes.IndexLookup.IndexLookupTable.TryGetValue(_id, out string name))
                        _name = name;
                    else
                        throw new ArgumentException($"Id {_id} is not registered as an item type yet. Unable to create ItemId object.");
                }

                return _name;
            }
            private set
            {
                _name = value;
            }
        }

        public ushort Id
        {
            get
            {
                if (_id == default(ushort))
                {
                    if (ItemTypes.IndexLookup.StringLookupTable.TryGetValue(_name, out var id))
                        _id = id;
                    else
                        throw new ArgumentException($"name {_name} is not registered as an item type yet. Unable to create ItemId object.");
                }

                return _id;
            }
            private set
            {
                _id = value;
            }
        }

        private ItemId(ushort id)
        {
            _id = id;
        }

        private ItemId(string name)
        {
            _name = name;
        }

        public override int GetHashCode()
        {
            return Id;
        }

        public bool Equals(ItemId other)
        {
            return Name == other.Name;
        }

        public static implicit operator string(ItemId itemId)
        {
            return itemId.Name;
        }

        public static implicit operator ushort(ItemId itemId)
        {
            return itemId.Id;
        }

        public static implicit operator ItemId(ushort id)
        {
            return GetItemId(id);
        }

        public static implicit operator ItemId(string name)
        {
            return GetItemId(name);
        }

        public override string ToString()
        {
            return Name;
        }

        public override bool Equals(object obj)
        {
            if (obj.GetType() == typeof(string))
                return obj as string == Name;

            if (obj.GetType() == typeof(ushort))
                return (ushort)obj == Id;

            return base.Equals(obj);
        }
    }
}
