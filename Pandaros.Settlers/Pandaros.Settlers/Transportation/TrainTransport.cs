using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MeshedObjects;
using Newtonsoft.Json.Linq;
using Pandaros.Settlers.Models;
using Pandaros.Settlers.Server;
using Pipliz.Collections;
using Shared;
using Transport;
using UnityEngine;

namespace Pandaros.Settlers.Transportation
{
    public class TrainTransport : TransportManager.ITransportVehicle
    {
        MeshedVehicleDescription _meshedVehicleDescription;
        AnimationManager.AnimatedObject _animatedObject;
        ItemId _trainId = ItemId.GetItemId(GameLoader.NAMESPACE + ".PropulsionPlatform");

        public TrainTransport(MeshedVehicleDescription vehicle, AnimationManager.AnimatedObject animatedObject)
        {
            _meshedVehicleDescription = vehicle;
            _animatedObject = animatedObject;
        }

        public int GetDelayMillisecondsToNextUpdate()
        {
            return 200;
        }

        public bool MatchesMeshID(int id)
        {
            return _meshedVehicleDescription.Object.ObjectID.ID == id;
        }

        public void OnClicked(Players.Player sender, PlayerClickedData click)
        {
            
        }

        public void ProcessInputs(Players.Player player, Pipliz.Collections.SortedList<EInputKey, float> keyTimes, float deltaTime)
        {
            throw new NotImplementedException();
        }

        public JObject Save()
        {
            throw new NotImplementedException();
        }

        public TransportManager.ETransportUpdateResult Update()
        {
            throw new NotImplementedException();
        }
    }
}
