using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandaros.Settlers.Models
{
    public class ItemId : IEquatable<ItemId>
    {
        static List<ItemId> _cache = new List<ItemId>();

        public static ItemId GetItemId(string name)
        {
            var item = _cache.FirstOrDefault(i =>i.Name == name);

            if (item == null)
            {
                item = new ItemId(name);
                _cache.Add(item);
            }

            return item;
        }

        public static ItemId GetItemId(ushort id)
        {
            var item = _cache.FirstOrDefault(i => i.Id == id);

            if (item == null)
            {
                item = new ItemId(id);
                _cache.Add(item);
            }

            return item;
        }

        ushort _id;
        string _name;

        public ushort Id
        {
            get
            {
                if (_id == default(ushort))
                {
                    if (ItemTypes.IndexLookup.IndexLookupTable.TryGetItem(_name, out var item))
                        _id = item.ItemIndex;
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
            return Id == other.Id;
        }

        public static implicit operator string(ItemId itemId)
        {
            return itemId.Name;
        }

        public static implicit operator ushort(ItemId itemId)
        {
            return itemId.Id;
        }
    }
}
