using Server.MeshedObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Pandaros.Settlers.Managers
{
    public static class AnimationManager
    {
        public const string SLINGBULLET = "slingbullet";
        public const string ARROW = "arrow";
        public const string CROSSBOWBOLT = "crossbowbolt";
        public const string LEADBULLET = "leadbullet";

        public class AnimatedObject
        {
            public string Name { get; private set; }
            
            public MeshedObjectType ObjType { get; private set; }
            public MeshedObjectTypeSettings ObjSettings { get; private set; }

            public AnimatedObject(string key, string meshPath, string textureMapping)
            {
                ObjSettings = new MeshedObjectTypeSettings(key, meshPath, textureMapping);
                ObjType = MeshedObjectType.Register(ObjSettings);
            }

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

        public static Dictionary<string, AnimatedObject> AnimatedObjects { get; private set; } = new Dictionary<string, AnimatedObject>(StringComparer.OrdinalIgnoreCase);

        static AnimationManager()
        {
            AnimatedObjects[SLINGBULLET] = new AnimatedObject(SLINGBULLET, "gameobject/meshes/slingbullet.obj", "projectile");
            AnimatedObjects[ARROW] = new AnimatedObject(ARROW, "gameobject/meshes/arrow.obj", "projectile");
            AnimatedObjects[CROSSBOWBOLT] = new AnimatedObject(CROSSBOWBOLT, "gameobject/meshes/crossbowbolt.obj", "projectile");
            AnimatedObjects[LEADBULLET] = new AnimatedObject(LEADBULLET, "gameobject/meshes/leadbullet.obj", "projectile");
        }
    }
}
