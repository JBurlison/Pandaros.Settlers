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
            public List<float> min { get; set; }
            public List<float> max { get; set; }

            public Boxes() { }

            public Boxes(List<float> minCollide, List<float> maxCollide)
            {
                min = minCollide;
                max = maxCollide;
            }
        }

        public bool collidePlayer { get; set; }
        public bool collideSelection { get; set; }
        public List<Boxes> boxes { get; set; }

        public Colliders() { }

        public Colliders (bool colideplayer, bool collideselection, List<Boxes> collider)
        {
            collidePlayer = colideplayer;
            collideSelection = collideselection;
            boxes = collider;
        }
    }
}
