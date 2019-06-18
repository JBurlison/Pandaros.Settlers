using MeshedObjects;
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
            AnimatedObjects[SLINGBULLET] = new AnimatedObject(SLINGBULLET, GameLoader.MESH_PATH + "slingbullet.ply", "projectile");
            AnimatedObjects[ARROW] = new AnimatedObject(ARROW, GameLoader.MESH_PATH + "arrow.ply", "projectile");
            AnimatedObjects[CROSSBOWBOLT] = new AnimatedObject(CROSSBOWBOLT, GameLoader.MESH_PATH + "crossbowbolt.ply", "projectile");
            AnimatedObjects[LEADBULLET] = new AnimatedObject(LEADBULLET, GameLoader.MESH_PATH + "leadbullet.ply", "projectile");
        }

        public static Dictionary<string, AnimatedObject> AnimatedObjects { get; } = new Dictionary<string, AnimatedObject>(StringComparer.OrdinalIgnoreCase);

        public static AnimatedObject RegisterNewAnimatedObject(string key, string meshPath, string textureMapping)
        {
            AnimatedObjects[key] = new AnimatedObject(key, meshPath, textureMapping);
            return AnimatedObjects[key];
        }


        public class AnimatedObject
        {
            public AnimatedObject(string key, string meshPath, string textureMapping)
            {
                ObjSettings = new MeshedObjectTypeSettings(key, meshPath, textureMapping);
                ObjType     = MeshedObjectType.Register(ObjSettings);
            }

            public string Name { get; private set; }
            public MeshedObjectType ObjType { get; }
            public MeshedObjectTypeSettings ObjSettings { get; }

            public ClientMeshedObject SendMoveToInterpolated(Vector3 start, Vector3 end, float deltaTime = 1f)
            {
                var obj = new ClientMeshedObject(ObjType);
                obj.SendMoveToInterpolated(start, new Quaternion(end.x, end.y, end.z, 0), deltaTime, ObjSettings);
                
                Task.Run(() =>
                {
                    System.Threading.Thread.Sleep((int)(deltaTime * 1001));
                    obj.SendRemoval(end, ObjSettings);
                });

                return obj;
            }
        }
    }
}