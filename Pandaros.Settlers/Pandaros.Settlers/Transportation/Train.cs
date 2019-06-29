using MeshedObjects;
using Newtonsoft.Json.Linq;
using Pandaros.Settlers.Models;
using Pandaros.Settlers.Server;
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
        private static AnimationManager.AnimatedObject AnimatedObject;

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterSelectedWorld, GameLoader.NAMESPACE + ".Transportation.Train.Initialize")]
        private static void Initialize()
        {
            AnimatedObject = AnimationManager.RegisterNewAnimatedObject(GameLoader.NAMESPACE + ".PropulsionPlatform", Path.Combine(GameLoader.MESH_PATH, "PropulsionPlatform.obj"), GameLoader.NAMESPACE + ".PropulsionPlatform");
            AnimatedObject.ObjSettings.colliders = new List<RotatedBounds>() { new RotatedBounds(Vector3.zero, new Vector3(3, 2, 3), Quaternion.identity) };
            AnimatedObject.ObjSettings.InterpolationLooseness = 1.5f;
            AnimatedObject.ObjSettings.sendUpdateRadius = 500;
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnPlayerClicked, GameLoader.NAMESPACE + ".Transportation.Train.OnPlayerClicked")]
        private static void OnClicked(Players.Player sender, PlayerClickedData data)
        {
            if (data.ConsumedType != PlayerClickedData.EConsumedType.Not ||
                data.IsHoldingButton ||
                (data.ClickType != PlayerClickedData.EClickType.Right || data.OnBuildCooldown) ||
                (data.HitType != PlayerClickedData.EHitType.Block ||
                data.TypeSelected != ItemId.GetItemId(GameLoader.NAMESPACE + ".PropulsionPlatform").Id || !sender.Inventory.TryRemove(data.TypeSelected, 1, -1, true)))
                return;

            data.ConsumedType = PlayerClickedData.EConsumedType.UsedAsTool;
            CreateTrain(data.GetExactHitPositionWorld(), new MeshedVehicleDescription(new ClientMeshedObject(AnimatedObject.ObjType), new Vector3(0.0f, 1.25f, 0.0f), false));
        }

        public static TrainTransport CreateTrain(Vector3 spawnPosition, MeshedVehicleDescription vehicle)
        {
            TrainTransport trainTransport = new TrainTransport(vehicle, AnimatedObject);
            TransportManager.RegisterTransport(trainTransport);
            return trainTransport;
        }
    }
}
