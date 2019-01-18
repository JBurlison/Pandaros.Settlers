using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Pipliz.JSON;
using UnityEngine;

namespace Pandaros.Settlers.Items
{
    public class Colliders
    {
        public class Boxes
        {
            public Vector3 min { get; private set; }
            public Vector3 max { get; private set; }

            public Boxes() { }

            public Boxes(Vector3 minCollide, Vector3 maxCollide)
            {
                min = minCollide;
                max = maxCollide;
            }
        }

        public bool collidePlayer { get; private set; }
        public bool collideSelection { get; private set; }
        public List<Boxes> boxes { get; private set; } = new List<Boxes>();

        public Colliders() { }

        public Colliders (bool colideplayer, bool collideselection, List<Boxes> collider)
        {
            collidePlayer = colideplayer;
            collideSelection = collideselection;
            boxes = collider;
        }
    }
}
