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
        Vector3 _position;

        public TrainTransport(Vector3 position, MeshedVehicleDescription vehicle, AnimationManager.AnimatedObject animatedObject)
        {
            _meshedVehicleDescription = vehicle;
            _animatedObject = animatedObject;
            _position = position;
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
            int countConnected = Players.CountConnected;
            Players.Player player = null;
            for (int index = 0; index < countConnected; ++index)
            {
                Players.Player connectedByIndex = Players.GetConnectedByIndex(index);
                MeshedVehicleDescription description;
                if (connectedByIndex != null && 
                    MeshedObjectManager.TryGetVehicle(connectedByIndex, out description) && 
                    description.Object.ObjectID.ID == _meshedVehicleDescription.Object.ObjectID.ID)
                    player = connectedByIndex;
            }
            if (click.ClickType == PlayerClickedData.EClickType.Right)
            {
                if (player == null)
                    MeshedObjectManager.Attach(sender, _meshedVehicleDescription);
                else
                    MeshedObjectManager.Detach(player);
            }
            else
            {
                if (click.ClickType != PlayerClickedData.EClickType.Left || player != null || 
                    !sender.Inventory.TryAdd(_trainId, 1, -1, true))
                    return;

                _meshedVehicleDescription.Object.SendRemoval(_position, null);
            }
        }

        public void ProcessInputs(Players.Player player, Pipliz.Collections.SortedList<EInputKey, float> keyTimes, float deltaTime)
        {
            
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
