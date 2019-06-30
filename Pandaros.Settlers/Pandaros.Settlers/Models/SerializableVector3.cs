using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Pandaros.Settlers.Models
{
    public class SerializableVector3
    {
        public SerializableVector3() { }

        public SerializableVector3(float X, float Y, float Z)
        {
            x = X;
            y = Y;
            z = Z;
        }

        public SerializableVector3(Vector3 vector3)
        {
            x = vector3.x;
            y = vector3.y;
            z = vector3.z;
        }

        public SerializableVector3(Pipliz.Vector3Int vector3)
        {
            x = vector3.x;
            y = vector3.y;
            z = vector3.z;
        }

        public SerializableVector3(Quaternion quant)
        {
            x = quant.x;
            y = quant.y;
            z = quant.z;
        }

        public float x { get; set; }
        public float y { get; set; }
        public float z { get; set; }

        public static implicit operator Vector3(SerializableVector3 serializableVector3)
        {
            return new Vector3(serializableVector3.x, serializableVector3.y, serializableVector3.z);
        }

        public static implicit operator Pipliz.Vector3Int(SerializableVector3 serializableVector3)
        {
            return new Pipliz.Vector3Int(serializableVector3.x, serializableVector3.y, serializableVector3.z);
        }

        public static implicit operator Quaternion(SerializableVector3 serializableVector3)
        {
            return Quaternion.Euler(serializableVector3.x, serializableVector3.y, serializableVector3.z);
        }
    }
}
