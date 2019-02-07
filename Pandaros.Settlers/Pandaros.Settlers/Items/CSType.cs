using Newtonsoft.Json;
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
    public class CSType : ICSType
    {
        public virtual string name { get; set; }
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
        public virtual dynamic customData { get; set; }
        public virtual string parentType { get; set; }
        [JsonProperty("rotatablex+")]
        public virtual string rotatablexp { get; set; }
        [JsonProperty("rotatablex-")]
        public virtual string rotatablexn { get; set; }
        [JsonProperty("rotatablez+")]
        public virtual string rotatablezp { get; set; }
        [JsonProperty("rotatablez-")]
        public virtual string rotatablezn { get; set; }
        public virtual string sideall { get; set; }
        [JsonProperty("sidex+")]
        public virtual string sidexp { get; set; }
        [JsonProperty("sidex-")]
        public virtual string sidexn { get; set; }
        [JsonProperty("sidey+")]
        public virtual string sideyp { get; set; }
        [JsonProperty("sidey-")]
        public virtual string sideyn { get; set; }
        [JsonProperty("sidez+")]
        public virtual string sidezp { get; set; }
        [JsonProperty("sidez-")]
        public virtual string sidezn { get; set; }
        public virtual string color { get; set; } 
        public virtual string onRemoveType { get; set; }
        public virtual string onRemoveAmount { get; set; }
        public virtual string onRemoveChance { get; set; }
        public virtual List<OnRemove> onRemove { get; set; } = new List<OnRemove>();
        public virtual bool? blocksPathing => isSolid;
        public virtual Colliders colliders { get; set; }
        public virtual List<string> categories { get; set; } = new List<string>();
        public virtual ItemRarity Rarity { get; set; } = ItemRarity.Common;
        public virtual StaticItem StaticItemSettings { get; set; }
    }
}
