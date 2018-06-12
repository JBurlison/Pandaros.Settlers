using Pipliz.JSON;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Pandaros.Settlers.Extender
{
    public abstract class CSType : ICSType
    {
        public virtual bool isDestructible { get; } = true;
        public virtual bool isRotatable { get; } = false;
        public virtual bool isSolid { get; } = true;
        public virtual bool isFertile { get; } = false;
        public virtual bool isPlaceable { get; } = false;
        public virtual bool needsBase { get; } = false;
        public virtual int maxStackSize { get; } = 50;
        public virtual float nutritionalValue { get; } = 0f;
        public virtual string mesh { get; }
        public virtual string icon { get; }
        public virtual string onRemoveAudio { get; }
        public virtual string onPlaceAudio { get; }
        public virtual int destructionTime { get; } = 400;
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
        public virtual Color color { get; } = Color.white;
        public virtual string onRemoveType { get; }
        public virtual string onRemoveAmount { get; }
        public virtual string onRemoveChance { get; }
        public virtual ReadOnlyCollection<OnRemove> onRemove { get; } = new ReadOnlyCollection<OnRemove>(new List<OnRemove>());
        public virtual bool blocksPathing => isSolid;
        public virtual ReadOnlyCollection<Colliders> colliders { get; } = new ReadOnlyCollection<Colliders>(new List<Colliders>());
        public virtual ReadOnlyCollection<string> categories { get; } = new ReadOnlyCollection<string>(new List<string>());

        public virtual JSONNode ToJsonNode()
        {
            var node = new JSONNode();
            
            return node;    
        }
    }
}
