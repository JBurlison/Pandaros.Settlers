using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MeshedObjects;
using Pipliz;
using Pipliz.Collections;
using Shared;
using Transport;
using UnityEngine;

namespace Pandaros.Settlers.Transportation
{
    public class TrainMovement : TransportManager.ITransportMovement, CollisionChecker.ICollisionSource
    {
        private static Type[] RigidBodyList = new Type[1]
          {
            typeof (Rigidbody)
          };
        private Transform CoreTransform;
        private Rigidbody CoreRigidBody;
        private TransportManager.ITransportVehicle ParentTransport;

        public TrainMovement(Vector3 startPosition, Quaternion startRotation, Players.Player playerInside)
        {
            LastInputPlayer = playerInside;

            if (TransportManager.TransportRootTransform == null)
            {
                TransportManager.TransportRootTransform = new GameObject("transport_root").transform;
            }
        }

        public Players.Player LastInputPlayer { get; private set; }

        public Vector3 Position { get; private set; }

        public Quaternion Rotation { get; private set; }

        public BoundsPip PossibleCollisionBounds { get; private set; }

        public void SetParent(TransportManager.ITransportVehicle vehicle)
        {
            ParentTransport = vehicle;
        }

        public int GetDelayMillisecondsToNextUpdate()
        {
            return 200;
        }

        public void OnColliderChangeNearby()
        {
            if (CoreRigidBody == null || !CoreRigidBody.IsSleeping())
                return;

            CoreRigidBody.WakeUp();
        }

        public void OnCollidersSpawnedResult(bool success)
        {
            if (success)
            {
                CoreRigidBody.isKinematic = false;
                OnColliderChangeNearby();
            }
            else
                CoreRigidBody.isKinematic = true;
        }

        public CollisionChecker.OnFixedUpdateResult OnFixedUpdate()
        {
            if (CoreTransform == null)
                return new CollisionChecker.OnFixedUpdateResult()
                {
                    IsValidSource = false
                };

            bool flag = false;
            MeshedVehicleDescription description;

            if (LastInputPlayer != null && MeshedObjectManager.TryGetVehicle(LastInputPlayer, out description) && 
                ParentTransport.MatchesMeshID(description.Object.ObjectID.ID))
            {
                flag = true;
                if (LastInputPlayer.ConnectionState != Players.EConnectionState.Connected)
                {
                    CoreRigidBody.isKinematic = true;
                    return new CollisionChecker.OnFixedUpdateResult()
                    {
                        IsValidSource = true
                    };
                }
                CoreRigidBody.isKinematic = false;
            }

            return new CollisionChecker.OnFixedUpdateResult()
            {
                ForceCheckColliders = true,
                IsValidSource = true,
                EstimatedDeltaPosition = CoreRigidBody.velocity * UnityEngine.Time.fixedDeltaTime
            };
        }

        public void OnRemove()
        {
            UnityEngine.Object.Destroy(CoreTransform.gameObject);
            CoreTransform = null;
            CoreRigidBody = null;
            LastInputPlayer = null;
            ParentTransport = null;
        }

        public void ProcessInputs(Players.Player player, Pipliz.Collections.SortedList<EInputKey, float> keyTimes, float deltaTime)
        {
            
        }

        public TransportManager.ETransportUpdateResult UpdateTransport()
        {
            return CoreTransform == null ? TransportManager.ETransportUpdateResult.Remove : TransportManager.ETransportUpdateResult.KeepUpdating;
        }
    }
}
