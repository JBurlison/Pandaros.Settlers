using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MeshedObjects;
using Newtonsoft.Json.Linq;
using Pandaros.Settlers.Items;
using Pandaros.Settlers.Items.ConnectedBlocks;
using Pandaros.Settlers.Items.Transportation;
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
        ICSType _cSType;
        Vector3 _position;
        ItemId _trainId;
        Pipliz.Vector3Int _trackPosition = Pipliz.Vector3Int.zero;
        int _delay = 1000;
        TrackCalculationType _trackCalculationType = new TrackCalculationType();
        int _idealHeightFromTrack = 3;

        public TrainTransport(Vector3 position, MeshedVehicleDescription vehicle, AnimationManager.AnimatedObject animatedObject, ICSType trainType)
        {
            _meshedVehicleDescription = vehicle;
            _animatedObject = animatedObject;
            _position = position;
            _cSType = trainType;
            _trainId = ItemId.GetItemId(trainType.name);
        }

        public int GetDelayMillisecondsToNextUpdate()
        {
            return _delay;
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
                if (click.ClickType != PlayerClickedData.EClickType.Left ||
                    player != null || 
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
            return null;
        }

        public TransportManager.ETransportUpdateResult Update()
        {
            var currentPositionInt = new Pipliz.Vector3Int(_position);
            var heightFromTrack = _idealHeightFromTrack;

            if (_trackPosition == Pipliz.Vector3Int.zero)
                for(int i = -1; i > _idealHeightFromTrack * -1; i--)
                {
                    var trackPos = currentPositionInt.Add(0, i, 0);
                    if (World.TryGetTypeAt(trackPos, out ItemTypes.ItemType possibleTrack) &&
                        ConnectedBlockSystem.BlockLookup.TryGetValue(possibleTrack.Name, out var track) &&
                        track.ConnectedBlock.CalculationType == "Track" && _cSType.ConnectedBlock.BlockType == track.ConnectedBlock.BlockType)
                    {
                        heightFromTrack = i * -1;
                        _trackPosition = trackPos;
                        break; 
                    }
                }

            if (heightFromTrack != _idealHeightFromTrack)
            {
                _position = currentPositionInt.Add(0, heightFromTrack, 0).Vector;
                _meshedVehicleDescription.Object.SendMoveToInterpolated(_position, Quaternion.identity, GetDelayMillisecondsToNextUpdate(), _animatedObject.ObjSettings);
            }
            else if (_trackPosition != Pipliz.Vector3Int.zero)
            {
                foreach (var side in _trackCalculationType.AvailableBlockSides)
                {
                    var searchSide = _trackPosition.GetBlockOffset(side);

                    if (World.TryGetTypeAt(searchSide, out ItemTypes.ItemType possibleTrack) &&
                        ConnectedBlockSystem.BlockLookup.TryGetValue(possibleTrack.Name, out var track) &&
                        track.ConnectedBlock.CalculationType == "Track" && _cSType.ConnectedBlock.BlockType == track.ConnectedBlock.BlockType)
                    {
                        _trackPosition = searchSide;
                        _position = currentPositionInt.GetBlockOffset(side).Vector;
                        _meshedVehicleDescription.Object.SendMoveToInterpolated(_position, Quaternion.identity, GetDelayMillisecondsToNextUpdate(), _animatedObject.ObjSettings);
                        break;
                    }
                }
            }
            else
                return TransportManager.ETransportUpdateResult.Remove;

            return TransportManager.ETransportUpdateResult.KeepUpdating;
        }
    }
}
