using MeshedObjects;
using System;
using System.Collections.Generic;
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
            AnimatedObjects[SLINGBULLET] = new AnimatedObject(SLINGBULLET, GameLoader.GAMEDATA_FOLDER + "meshes/slingbullet.ply", "projectile");
            AnimatedObjects[ARROW] = new AnimatedObject(ARROW, GameLoader.GAMEDATA_FOLDER + "meshes/arrow.ply", "projectile");
            AnimatedObjects[CROSSBOWBOLT] = new AnimatedObject(CROSSBOWBOLT, GameLoader.GAMEDATA_FOLDER + "meshes/crossbowbolt.ply", "projectile");
            AnimatedObjects[LEADBULLET] = new AnimatedObject(LEADBULLET, GameLoader.GAMEDATA_FOLDER + "meshes/leadbullet.ply", "projectile");
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