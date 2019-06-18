using MeshedObjects;
using Pipliz.Helpers;
using Shared;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Pandaros.Settlers.Server
{
    public static class AnimationManager
    {
        public const string SLINGBULLET = "slingbullet";
        public const string ARROW = "arrow";
        public const string CROSSBOWBOLT = "crossbowbolt";
        public const string LEADBULLET = "leadbullet";

        static AnimationManager()
        {
            AnimatedObjects[SLINGBULLET] = new AnimatedObject(SLINGBULLET, GameLoader.MESH_PATH + "slingbullet.ply");
            AnimatedObjects[ARROW] = new AnimatedObject(ARROW, GameLoader.MESH_PATH + "arrow.ply");
            AnimatedObjects[CROSSBOWBOLT] = new AnimatedObject(CROSSBOWBOLT, GameLoader.MESH_PATH + "crossbowbolt.ply");
            AnimatedObjects[LEADBULLET] = new AnimatedObject(LEADBULLET, GameLoader.MESH_PATH + "leadbullet.ply");
        }

        public static Dictionary<string, AnimatedObject> AnimatedObjects { get; } = new Dictionary<string, AnimatedObject>(StringComparer.OrdinalIgnoreCase);

        public static AnimatedObject RegisterNewAnimatedObject(string key, string meshPath, string textureMapping)
        {
            AnimatedObjects[key] = new AnimatedObject(key, meshPath, textureMapping);
            return AnimatedObjects[key];
        }


        public class AnimatedObject
        {
            public AnimatedObject(string key, string meshPath, string textureMapping = "neutral")
            {
                string mesh = IOHelper.SimplifyRelativeDirectory(meshPath, "");
                ObjSettings = new MeshedObjectTypeSettings(key, mesh, textureMapping);
                ObjType     = MeshedObjectType.Register(ObjSettings);
                FileTable.Register(mesh, ECachedFileType.Mesh);
            }

            public string Name { get; private set; }
            public MeshedObjectType ObjType { get; }
            public MeshedObjectTypeSettings ObjSettings { get; }

            public ClientMeshedObject SendMoveToInterpolated(Vector3 start, Vector3 end, float deltaTime = .2f)
            {
                var obj = new ClientMeshedObject(ObjType);
                obj.SendMoveToInterpolated(start, Quaternion.identity, deltaTime, ObjSettings);
                obj.SendMoveToInterpolated(end, Quaternion.identity, deltaTime, ObjSettings);

                Task.Run(() =>
                {
                    System.Threading.Thread.Sleep((int)(deltaTime * 1000) + 500);
                    obj.SendRemoval(end, ObjSettings);
                });

                return obj;
            }
        }
    }
}