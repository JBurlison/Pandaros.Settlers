using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Pipliz.JSON;
using UnityEngine;

namespace Pandaros.Settlers.Items
{
    [JSON.HintAutoObject]
    public class Colliders : IJsonSerializable, IJsonDeserializable
    {
        [JSON.HintAutoObject]
        public class Boxes : IJsonSerializable, IJsonDeserializable
        {
            public Vector3 min { get; private set; }
            public Vector3 max { get; private set; }

            public Boxes() { }

            public Boxes(Vector3 minCollide, Vector3 maxCollide)
            {
                min = minCollide;
                max = maxCollide;
            }

            public void JsonDeerialize(JSONNode node)
            {
                JSON.LoadFields(this, node);
            }

            public virtual JSONNode JsonSerialize()
            {
                return JSON.SaveField(this);
            }
        }

        public bool collidePlayer { get; private set; }
        public bool collideSelection { get; private set; }
        public Boxes boxes { get; private set; }

        public Colliders() { }

        public Colliders (bool colideplayer, bool collideselection, Boxes collider)
        {
            collidePlayer = colideplayer;
            collideSelection = collideselection;
            boxes = collider;
        }

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
