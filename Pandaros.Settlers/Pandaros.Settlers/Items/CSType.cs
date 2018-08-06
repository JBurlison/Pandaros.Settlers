using Pipliz.JSON;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Pandaros.Settlers.Items
{
    public abstract class CSType : ICSType
    {
        public virtual string Name { get; }
        public virtual bool? isDestructible { get; }
        public virtual bool? isRotatable { get; }
        public virtual bool? isSolid { get; } 
        public virtual bool? isFertile { get; }
        public virtual bool? isPlaceable { get; }
        public virtual bool? needsBase { get; }
        public virtual int? maxStackSize { get; } 
        public virtual float? nutritionalValue { get; }
        public virtual string mesh { get; }
        public virtual string icon { get; }
        public virtual string onRemoveAudio { get; }
        public virtual string onPlaceAudio { get; }
        public virtual int? destructionTime { get; }
        public virtual JSONNode customData { get; }
        public virtual string parentType { get; }
        public virtual string rotatablexp { get; }
        public virtual string rotatablexn { get; }
        public virtual string rotatablezp { get; }
        public virtual string rotatablezn { get; }
        public virtual string sideall { get; }
        public virtual string sidexp { get; }
        public virtual string sidexn { get; }
        public virtual string sideyp { get; }
        public virtual string sideyn { get; }
        public virtual string sidezp { get; }
        public virtual string sidezn { get; }
        public virtual Color color { get; } 
        public virtual string onRemoveType { get; }
        public virtual string onRemoveAmount { get; }
        public virtual string onRemoveChance { get; }
        public virtual ReadOnlyCollection<OnRemove> onRemove { get; } = new ReadOnlyCollection<OnRemove>(new List<OnRemove>());
        public virtual bool? blocksPathing => isSolid;
        public virtual ReadOnlyCollection<Colliders> colliders { get; } = new ReadOnlyCollection<Colliders>(new List<Colliders>());
        public virtual ReadOnlyCollection<string> categories { get; } = new ReadOnlyCollection<string>(new List<string>());
        public virtual ItemRarity Rarity { get; } = ItemRarity.Artifact;

        public virtual JSONNode ToJsonNode()
        {
            var node = new JSONNode();

            if (isDestructible != null)
                node.SetAs(nameof(isDestructible), isDestructible);

            if (isRotatable != null)
                node.SetAs(nameof(isRotatable), isRotatable);

            if (isSolid != null)
                node.SetAs(nameof(isSolid), isSolid);

            if (isFertile != null)
                node.SetAs(nameof(isFertile), isFertile);

            if (isPlaceable != null)
                node.SetAs(nameof(isPlaceable), isPlaceable);

            if (needsBase != null)
                node.SetAs(nameof(needsBase), needsBase);

            if (maxStackSize != null)
                node.SetAs(nameof(maxStackSize), maxStackSize);

            if (nutritionalValue != null)
                node.SetAs(nameof(nutritionalValue), nutritionalValue);

            if (mesh != null)
                node.SetAs(nameof(mesh), mesh);

            if (icon != null)
                node.SetAs(nameof(icon), icon);

            if (onRemoveAudio != null)
                node.SetAs(nameof(onRemoveAudio), onRemoveAudio);

            if (destructionTime != null)
                node.SetAs(nameof(destructionTime), destructionTime);

            if (customData != null)
                node.SetAs(nameof(customData), customData);

            if (parentType != null)
                node.SetAs(nameof(parentType), parentType);

            if (rotatablexp != null)
                node.SetAs("rotatablex+", rotatablexp);

            if (rotatablexn != null)
                node.SetAs("rotatablex-", rotatablexn);

            if (rotatablezp != null)
                node.SetAs("rotatablez+", rotatablezp);

            if (rotatablezn != null)
                node.SetAs("rotatablez-", rotatablezn);

            if (sideall != null)
                node.SetAs(nameof(sideall), sideall);

            if (sidexp != null)
                node.SetAs("sidex+", sidexp);

            if (sidexn != null)
                node.SetAs("sidex-", sidexn);

            if (sideyp != null)
                node.SetAs("sidey+", sideyp);

            if (sideyn != null)
                node.SetAs("sidey-", sideyn);

            if (sidezp != null)
                node.SetAs("sidez+", sidezp);

            if (sidezn != null)
                node.SetAs("sidez-", sidezn);

            if (color != null && !color.Equals(Color.clear))
                node.SetAs(nameof(color), color.ToRGBHex());

            if (onRemoveType != null)
                node.SetAs(nameof(onRemoveType), onRemoveType);

            if (onRemoveAmount != null)
                node.SetAs(nameof(onRemoveAmount), onRemoveAmount);

            if (onRemoveChance != null)
                node.SetAs(nameof(onRemoveChance), onRemoveChance);

            if (onRemove.Count != 0)
                node.SetAs(nameof(onRemove), onRemove.ToJsonNode());

            if (blocksPathing != null)
                node.SetAs(nameof(blocksPathing), blocksPathing);

            if (colliders.Count != 0)
                node.SetAs(nameof(colliders), colliders.ToJsonNode());

            if (categories.Count != 0)
                node.SetAs(nameof(categories), categories.ToJsonNode());

            node.SetAs(nameof(Rarity), Rarity);

#if DEBUG
            PandaLogger.Log(node.ToString());
#endif
            return node;    
        }
    }
}
