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
        public virtual bool? isDestructible { get; set; }
        public virtual bool? isRotatable { get; set; }
        public virtual bool? isSolid { get; set; } 
        public virtual bool? isFertile { get; set; }
        public virtual bool? isPlaceable { get; set; }
        public virtual bool? needsBase { get; set; }
        public virtual int? maxStackSize { get; set; } 
        public virtual float? nutritionalValue { get; set; }
        public virtual string mesh { get; set; }
        public virtual string icon { get; set; }
        public virtual string onRemoveAudio { get; set; }
        public virtual string onPlaceAudio { get; set; }
        public virtual int? destructionTime { get; set; }
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
            return JSON.SaveField(this);    
        }
    }
}
