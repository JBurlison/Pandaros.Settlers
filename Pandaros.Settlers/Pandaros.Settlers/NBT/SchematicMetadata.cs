using System;
using System.Collections.Generic;

namespace Pandaros.Settlers.NBT
{
    public class SchematicMetadata
    {
        public string Name { get; set; }
        public int MaxX { get; set; }
        public int MaxY { get; set; }
        public int MaxZ { get; set; }

        public Dictionary<ushort, SchematicBlockMetadata> Blocks { get; set; } = new Dictionary<ushort, SchematicBlockMetadata>();
    }

    public class SchematicBlockMetadata : IEquatable<SchematicBlockMetadata>
    {
        public ushort ItemId { get; set; }
        public long Count { get; set; }

        public bool Equals(SchematicBlockMetadata other)
        {
            return other.ItemId == ItemId;
        }

        public override int GetHashCode()
        {
            return ItemId;
        }

        public override string ToString()
        {
            return $"{nameof(ItemId)}: {ItemId}, {nameof(Count)}: {Count}";
        }
    }
}
