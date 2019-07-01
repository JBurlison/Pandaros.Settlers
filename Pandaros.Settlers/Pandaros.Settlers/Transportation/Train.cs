using MeshedObjects;
using Newtonsoft.Json.Linq;
using Pandaros.Settlers.Items.Transportation;
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
using Newtonsoft.Json;

namespace Pandaros.Settlers.Transportation
{
    [ModLoader.ModManager]
    public static class Train
    {
        public static Dictionary<ItemId, ICSType> TrainTypes { get; set; } = new Dictionary<ItemId, ICSType>();
        public static Dictionary<ItemId, AnimationManager.AnimatedObject> TrainAnimations { get; set; } = new Dictionary<ItemId, AnimationManager.AnimatedObject>();

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterItemTypesDefined, GameLoader.NAMESPACE + ".Transportation.Train.Initialize", 5000)]
        private static void Initialize()
        {
            foreach (var train in TrainTypes.Values)
            {
                var animatedObject = AnimationManager.RegisterNewAnimatedObject(train.name, train.mesh, train.sideall);
                animatedObject.ObjSettings.colliders = new List<RotatedBounds>() { new RotatedBounds(Vector3.zero, train.TrainConfiguration.TrainBounds, Quaternion.identity) };
                animatedObject.ObjSettings.InterpolationLooseness = 1.5f;
                animatedObject.ObjSettings.sendUpdateRadius = 500;

                TrainAnimations[train.name] = animatedObject;
            }

            var worldFile = Path.Combine(GameLoader.SAVE_LOC, "world.json");

            if (File.Exists(worldFile))
            {
                JObject rootObj = JsonConvert.DeserializeObject<JObject>(File.ReadAllText(worldFile));

                if (rootObj.TryGetValue("transports", out JToken transports))
                {
                    if (transports.Type != JTokenType.Array)
                        return;

                    List<TransportSave> trainSaves = transports.ToObject<List<TransportSave>>();

                    foreach (var save in trainSaves)
                    {
                        if (TrainTransport.TryCreateFromSave(save, out var trainTransport))
                            TransportManager.RegisterTransport(trainTransport);
                    }
                }
            }
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnPlayerClicked, GameLoader.NAMESPACE + ".Transportation.Train.OnPlayerClicked")]
        private static void OnClicked(Players.Player sender, PlayerClickedData data)
        {
            if (data.ConsumedType != PlayerClickedData.EConsumedType.Not ||
                data.IsHoldingButton ||
                (data.ClickType != PlayerClickedData.EClickType.Right || data.OnBuildCooldown) ||
                (data.HitType != PlayerClickedData.EHitType.Block ||
                !TrainTypes.TryGetValue(data.TypeSelected, out var cSType) || !sender.Inventory.TryRemove(data.TypeSelected, 1, -1, true)))
                return;

            data.ConsumedType = PlayerClickedData.EConsumedType.UsedAsTool;
            CreateTrain(cSType, data.GetExactHitPositionWorld());
        }

        public static TrainTransport CreateTrain(ICSType cSType, Vector3 spawnPosition)
        {
            TrainTransport trainTransport = new TrainTransport(spawnPosition, TrainAnimations[cSType.name], cSType);
            TransportManager.RegisterTransport(trainTransport);
            return trainTransport;
        }
    }
}
