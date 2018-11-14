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

        public ushort Id { get; private set; }
        public string Name { get; private set; }

        private ItemId(ushort id)
        {
            Id = id;

            if (ItemTypes.IndexLookup.IndexLookupTable.TryGetValue(id, out string name))
                Name = name;
            else
                throw new ArgumentException($"Id {id} is not registered as an item type yet. Unable to create ItemId object.");
        }

        private ItemId(string name)
        {
            Name = name;

            if (ItemTypes.IndexLookup.IndexLookupTable.TryGetItem(name, out var item))
                Id = item.ItemIndex;
            else
                throw new ArgumentException($"name {name} is not registered as an item type yet. Unable to create ItemId object.");
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
