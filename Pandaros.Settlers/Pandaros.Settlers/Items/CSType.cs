using Pipliz.JSON;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using UnityEngine;
using static Pandaros.Settlers.Items.StaticItems;

namespace Pandaros.Settlers.Items
{
    [JSON.HintAutoObject]
    public abstract class CSType : ICSType
    {
        public virtual string Name { get; set; }
        public virtual bool? isDestructible { get; }
        public virtual bool? isRotatable { get; }
        public virtual bool? isSolid { get; }
        public virtual bool? isFertile { get; }
        public virtual bool? isPlaceable { get; }
        public virtual bool? needsBase { get; }
        public virtual int? maxStackSize { get; }
        public virtual float? foodValue { get; }
        public virtual float? happiness { get; }
        public virtual float? dailyFoodFractionOptimal { get; }
        public virtual string mesh { get; set; }
        public virtual string icon { get; set; }
        public virtual string onRemoveAudio { get; set; }
        public virtual string onPlaceAudio { get; set; }
        public virtual int? destructionTime { get; }
        public virtual JSONNode customData { get; set; }
        public virtual string parentType { get; set; }
        public virtual string rotatablexp { get; set; }
        public virtual string rotatablexn { get; set; }
        public virtual string rotatablezp { get; set; }
        public virtual string rotatablezn { get; set; }
        public virtual string sideall { get; set; }
        public virtual string sidexp { get; set; }
        public virtual string sidexn { get; set; }
        public virtual string sideyp { get; set; }
        public virtual string sideyn { get; set; }
        public virtual string sidezp { get; set; }
        public virtual string sidezn { get; set; }
        public virtual Color color { get; set; } 
        public virtual string onRemoveType { get; set; }
        public virtual string onRemoveAmount { get; set; }
        public virtual string onRemoveChance { get; set; }
        public virtual List<OnRemove> onRemove { get; set; } = new List<OnRemove>();
        public virtual bool? blocksPathing => isSolid;
        public virtual List<Colliders> colliders { get; set; } = new List<Colliders>();
        public virtual List<string> categories { get; set; } = new List<string>();
        public virtual ItemRarity Rarity { get; set; } = ItemRarity.Common;
        public virtual StaticItem StaticItemSettings { get; set; }

        public void JsonDeerialize(JSONNode node)
        {
            JSON.LoadFields(this, node);
        }

        public virtual JSONNode JsonSerialize()
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

            if (foodValue != null)
                node.SetAs(nameof(foodValue), foodValue);

            if (happiness != null)
                node.SetAs(nameof(happiness), happiness);

            if (foodValue != null)
                node.SetAs(nameof(dailyFoodFractionOptimal), dailyFoodFractionOptimal);

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

            if (Rarity != ItemRarity.Common)
                node.SetAs(nameof(Rarity), Rarity);

            if (StaticItemSettings != null)
                node.SetAs(nameof(StaticItemSettings), StaticItemSettings);

            return node;
        }
    }
}
