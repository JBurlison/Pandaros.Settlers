using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Pipliz.JSON;
using UnityEngine;

namespace Pandaros.Settlers.Extender
{
    public class Colliders : IJsonConvertable
    {
        public class Boxes
        {
            public Vector3 min { get; private set; }
            public Vector3 max { get; private set; }

            public Boxes(Vector3 minCollide, Vector3 maxCollide)
            {
                min = minCollide;
                max = maxCollide;
            }
        }

        public bool collidePlayer { get; private set; }
        public bool collideSelection { get; private set; }
        public Boxes boxes { get; private set; }

        public Colliders (bool colideplayer, bool collideselection, Boxes collider)
        {
            collidePlayer = colideplayer;
            collideSelection = collideselection;
            boxes = collider;
        }

        public JSONNode ToJsonNode()
        {
            var node = new JSONNode();

            return node;
        }
    }
}
