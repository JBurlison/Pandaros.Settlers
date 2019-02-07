using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Pipliz.JSON;

namespace Pandaros.Settlers.Items
{
    public class CSTextureMapping : ICSTextureMapping
    {
        public virtual string name => null;

        public virtual string emissive => null;

        public virtual string albedo => null;

        public virtual string normal => null;

        public virtual string height => null;

        public JSONNode JsonSerialize()
        {
            var textureNode = new JSONNode();

            if (!string.IsNullOrEmpty(emissive))
                textureNode.SetAs(nameof(emissive), emissive);

            if (!string.IsNullOrEmpty(albedo))
                textureNode.SetAs(nameof(albedo), albedo);

            if (!string.IsNullOrEmpty(normal))
                textureNode.SetAs(nameof(normal), normal);

            if (!string.IsNullOrEmpty(height))
                textureNode.SetAs(nameof(height), height);

            return textureNode; 
        }
    }
}
