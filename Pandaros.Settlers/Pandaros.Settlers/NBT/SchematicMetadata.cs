using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pandaros.Settlers.Models;
using Pipliz.JSON;

namespace Pandaros.Settlers.NBT
{
    public class SchematicMetadata : IJsonDeserializable, IJsonSerializable
    {
        public string Name { get; set; }
        public int MaxX { get; set; }
        public int MaxY { get; set; }
        public int MaxZ { get; set; }

        public Dictionary<ushort, SchematicBlockMetadata> Blocks { get; set; } = new Dictionary<ushort, SchematicBlockMetadata>();

        public SchematicMetadata() { }

        public SchematicMetadata(JSONNode node)
        {
            JsonDeerialize(node);
        }


        public void JsonDeerialize(JSONNode node)
        {
            if (node.TryGetAs(nameof(Blocks), out JSONNode blocks))
                foreach (var b in blocks.LoopArray())
                {
                    var block = new SchematicBlockMetadata(b);
                    Blocks.Add(block.ItemId, block);
                }

            if (node.TryGetAs(nameof(Name), out string name))
                Name = name;

            if (node.TryGetAs(nameof(MaxX), out int maxx))
                MaxX = maxx;

            if (node.TryGetAs(nameof(MaxY), out int maxy))
                MaxY = maxy;

            if (node.TryGetAs(nameof(MaxZ), out int maxz))
                MaxZ = maxz;
        }

        public JSONNode JsonSerialize()
        {
            var json = new JSONNode();
            var blocks = new JSONNode(NodeType.Array);

            foreach (var block in Blocks)
                blocks.AddToArray(block.Value.JsonSerialize());

            json.SetAs(nameof(Blocks), blocks);
            json.SetAs(nameof(Name), Name);
            json.SetAs(nameof(MaxX), MaxX);
            json.SetAs(nameof(MaxY), MaxY);
            json.SetAs(nameof(MaxZ), MaxZ);

            return json;
        }
    }

    public class SchematicBlockMetadata : IJsonDeserializable, IJsonSerializable, IEquatable<SchematicBlockMetadata>
    {
        public ushort ItemId { get; set; }
        public long Count { get; set; }

        public SchematicBlockMetadata() { }

        public SchematicBlockMetadata(JSONNode node)
        {
            JsonDeerialize(node);
        }

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

        public void JsonDeerialize(JSONNode node)
        {
            if (node.TryGetAs(nameof(ItemId), out ushort id))
                ItemId = id;

            if (node.TryGetAs(nameof(Count), out long count))
                Count = count;
        }

        public JSONNode JsonSerialize()
        {
            JSONNode node = new JSONNode();
            node.SetAs(nameof(ItemId), ItemId);
            node.SetAs(nameof(Count), Count);
            return node;
        }
    }
}
