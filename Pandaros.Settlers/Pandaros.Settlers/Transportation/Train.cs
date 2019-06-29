using MeshedObjects;
using Newtonsoft.Json.Linq;
using Pandaros.Settlers.Models;
using Pipliz;
using Pipliz.Collections;
using Pipliz.Helpers;
using Shared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Transport;
using UnityEngine;

namespace Pandaros.Settlers.Transportation
{
    [ModLoader.ModManager]
    public static class Train
    {
        private static MeshedObjectType TrainMeshType;

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterSelectedWorld, GameLoader.NAMESPACE + ".Transportation.Train.Initialize")]
        private static void Initialize()
        {
            string str = IOHelper.SimplifyRelativeDirectory(Path.Combine(GameLoader.MESH_PATH, "PropulsionPlatform.obj"), "");
            FileTable.Register(str, ECachedFileType.Mesh);
            TrainMeshType = MeshedObjectType.Register(new MeshedObjectTypeSettings(GameLoader.NAMESPACE + ".PropulsionPlatform", str, GameLoader.NAMESPACE + ".PropulsionPlatform")
            {
                colliders = new List<RotatedBounds>() { new RotatedBounds(Vector3.zero, new Vector3(3, 2, 3), Quaternion.identity) },
                InterpolationLooseness = 1.5f,
                sendUpdateRadius = 500
            });
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnPlayerClicked, "clicked_glider")]
        [ModLoader.ModCallbackProvidesFor("clicked_transport")]
        private static void OnClicked(Players.Player sender, PlayerClickedData data)
        {
            if (data.ConsumedType != PlayerClickedData.EConsumedType.Not ||
                data.IsHoldingButton ||
                (data.ClickType != PlayerClickedData.EClickType.Right || data.OnBuildCooldown) ||
                (data.HitType != PlayerClickedData.EHitType.Block ||
                data.TypeSelected != ItemId.GetItemId(GameLoader.NAMESPACE + ".PropulsionPlatform").Id || !sender.Inventory.TryRemove(data.TypeSelected, 1, -1, true)))
                return;
            data.ConsumedType = PlayerClickedData.EConsumedType.UsedAsTool;
            CreateTrain(data.GetExactHitPositionWorld(), Quaternion.identity, Glider.CreateVehicleDescription(MeshedObjectID.GetNew()), null);
        }

        public static TrainTransport CreateTrain(Vector3 spawnPosition, Quaternion rotation, MeshedVehicleDescription vehicle, Players.Player playerInside)
        {
            TrainMovement trainMovement = new TrainMovement(spawnPosition, rotation, playerInside);
            TrainTransport trainTransport = new TrainTransport(trainMovement, vehicle, new InventoryItem(ItemId.GetItemId(GameLoader.NAMESPACE + ".PropulsionPlatform").Id));
            trainMovement.SetParent(trainTransport);
            CollisionChecker.RegisterSource(trainMovement);
            TransportManager.RegisterTransport(trainTransport);
            return trainTransport;
        }

        private static Transform SpawnRootBox(TransportManager.RigidBodySettings settings)
        {
            Transform transform = new GameObject(GameLoader.NAMESPACE + ".PropulsionPlatform", Glider.GliderMovement.RigidBodyList).transform;
            transform.SetParent(TransportManager.TransportRootTransform);
            return transform;
        }
    }
}
