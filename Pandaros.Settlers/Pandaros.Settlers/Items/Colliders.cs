using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Pipliz.JSON;
using UnityEngine;

namespace Pandaros.Settlers.Items
{
    public class Colliders : IJsonConvertable
    {
        public class Boxes : IJsonConvertable
        {
            public Vector3 min { get; private set; }
            public Vector3 max { get; private set; }

            public Boxes(Vector3 minCollide, Vector3 maxCollide)
            {
                min = minCollide;
                max = maxCollide;
            }

            public JSONNode ToJsonNode()
            {
                var node = new JSONNode();
                var nodemin = new JSONNode(NodeType.Array);
                var nodemax = new JSONNode(NodeType.Array);

                nodemin.AddToArray(new JSONNode(min.x));
                nodemin.AddToArray(new JSONNode(min.y));
                nodemin.AddToArray(new JSONNode(min.z));

                nodemax.AddToArray(new JSONNode(max.x));
                nodemax.AddToArray(new JSONNode(max.y));
                nodemax.AddToArray(new JSONNode(max.z));

                node.SetAs(nameof(min), nodemin);
                node.SetAs(nameof(max), nodemax);

                return node;
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

            node.SetAs(nameof(collidePlayer), collidePlayer);
            node.SetAs(nameof(collideSelection), collideSelection);
            node.SetAs(nameof(boxes), boxes.ToJsonNode());

            return node;
        }
    }
}
