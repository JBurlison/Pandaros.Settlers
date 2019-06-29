using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MeshedObjects;
using Newtonsoft.Json.Linq;
using Transport;
using UnityEngine;

namespace Pandaros.Settlers.Transportation
{
    public class TrainTransport : TransportManager.GenericTransport
    {
        public TrainTransport(TransportManager.ITransportMovement mover, MeshedVehicleDescription description, InventoryItem refundItems) :
            base(mover, description, refundItems)
        {
        }

        public override JObject Save()
        {
            if (Mover == null)
                return null;

            Vector3 position = Mover.Position;
            Vector3 eulerAngles = Mover.Rotation.eulerAngles;
            JObject jobject = new JObject()
            {
              {
                "type",
                (JToken) GameLoader.NAMESPACE + ".PropulsionPlatform"
              },
              {
                "position",
                (JToken) new JObject()
                {
                  {
                    "x",
                    (JToken) position.x
                  },
                  {
                    "y",
                    (JToken) position.y
                  },
                  {
                    "z",
                    (JToken) position.z
                  }
                }
              },
              {
                "rotation",
                (JToken) new JObject()
                {
                  {
                    "x",
                    (JToken) eulerAngles.x
                  },
                  {
                    "y",
                    (JToken) eulerAngles.y
                  },
                  {
                    "z",
                    (JToken) eulerAngles.z
                  }
                }
              },
              {
                "meshid",
                (JToken) this.VehicleDescription.Object.ObjectID.ID
              }
            };

            TrainMovement mover = Mover as TrainMovement;
            MeshedVehicleDescription description;

            if (mover.LastInputPlayer != null && MeshedObjectManager.TryGetVehicle(mover.LastInputPlayer, out description) && 
                VehicleDescription.Object.ObjectID.ID == description.Object.ObjectID.ID)
                jobject["player"] = (JToken)mover.LastInputPlayer.ID.ToString();

            return jobject;
        }
    }
}
