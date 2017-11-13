using BlockTypes.Builtin;
using Pipliz.JSON;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pandaros.Settlers.Cosmetics
{
    public static class Register
    {
        public const string TALIOR_JOB = "pipliz.tailor";

        public static void AddCarpetTextures(string key)
        {
            var textureMapping = new ItemTypesServer.TextureMapping(new JSONNode());

            textureMapping.AlbedoPath = GameLoader.TEXTURE_FOLDER_PANDA + "/albedo/" + key + ".png";
            textureMapping.NormalPath = "gamedata/textures/materials/blocks/normal/carpet.png";
            textureMapping.EmissivePath = "gamedata/textures/materials/blocks/emissiveMaskAlpha/neutral.png";
            textureMapping.HeightPath = "gamedata/textures/materials/blocks/heightSmoothnessSpecularity/carpet.png";

            ItemTypesServer.SetTextureMapping(GameLoader.NAMESPACE + "." + key, textureMapping);
        }

        public static ItemTypesServer.ItemTypeRaw AddCarpetTypeTypes(Dictionary<string, ItemTypesServer.ItemTypeRaw> itemTypes, string key)
        {
            var item = new ItemTypesServer.ItemTypeRaw(GameLoader.NAMESPACE + "." + key, new JSONNode()
              .SetAs("icon", GameLoader.ICON_FOLDER_PANDA + "/" + key + ".png")
              .SetAs("onPlaceAudio", "dirtPlace")
              .SetAs("onRemoveAudio", "grassDelete")
              .SetAs("sideall", GameLoader.NAMESPACE + "." + key)
              .SetAs("maxStackSize", 200)
            );

            itemTypes.Add(GameLoader.NAMESPACE + "." + key, item);

            return item;
        }
    }
}
