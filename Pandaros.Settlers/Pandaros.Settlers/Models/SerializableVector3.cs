using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Pandaros.Settlers.Items
{
    public class SerializableVector3
    {
        public SerializableVector3() { }
        public SerializableVector3(Vector3 vector3)
        {
            x = vector3.x;
            y = vector3.y;
            z = vector3.z;
        }

        public float x { get; set; }
        public float y { get; set; }
        public float z { get; set; }
    }
}
