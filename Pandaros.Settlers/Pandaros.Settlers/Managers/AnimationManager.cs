using System;
using System.Collections.Generic;
using Server.MeshedObjects;
using UnityEngine;

namespace Pandaros.Settlers.Managers
{
    public static class AnimationManager
    {
        public const string SLINGBULLET = "slingbullet";
        public const string ARROW = "arrow";
        public const string CROSSBOWBOLT = "crossbowbolt";
        public const string LEADBULLET = "leadbullet";

        static AnimationManager()
        {
            AnimatedObjects[SLINGBULLET] = new AnimatedObject(SLINGBULLET, "gamedata/meshes/slingbullet.obj", "projectile");
            AnimatedObjects[ARROW] = new AnimatedObject(ARROW, "gamedata/meshes/arrow.obj", "projectile");
            AnimatedObjects[CROSSBOWBOLT] = new AnimatedObject(CROSSBOWBOLT, "gamedata/meshes/crossbowbolt.obj", "projectile");
            AnimatedObjects[LEADBULLET] = new AnimatedObject(LEADBULLET, "gamedata/meshes/leadbullet.obj", "projectile");
        }

        public static Dictionary<string, AnimatedObject> AnimatedObjects { get; } = new Dictionary<string, AnimatedObject>(StringComparer.OrdinalIgnoreCase);

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

            public void SendMoveToInterpolatedOnce(Vector3 start, Vector3 end, float deltaTime = 1f)
            {
                ClientMeshedObject.SendMoveOnceInterpolatedPositionAutoRotation(start, end, deltaTime, ObjSettings);
            }

            public ClientMeshedObject SendMoveToInterpolated(Vector3 start, Vector3 end, float deltaTime = 1f)
            {
                var obj = new ClientMeshedObject(ObjType);
                obj.SendMoveToInterpolated(start, end, deltaTime);
                return obj;
            }
        }
    }
}