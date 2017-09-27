using Pipliz.JSON;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Pandaros.Settlers
{
    [ModLoader.ModManager]
    public static class GameLoader
    {
        public static string ICON_FOLDER_PANDA_REL = @"gamedata\mods\Pandaros\settlers\icons";
        public static string LOCALIZATION_FOLDER_PANDA = @"gamedata\mods\Pandaros\settlers\localization";
        public static string MOD_FOLDER = @"gamedata\mods\Pandaros\settlers";

        public const string NAMESPACE = "Pandaros.Settlers";

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnAssemblyLoaded, "Pandaros.Settlers.OnAssemblyLoaded")]
        public static void OnAssemblyLoaded(string path)
        {
            MOD_FOLDER = Path.GetDirectoryName(path);
            PandaLogger.Log(string.Format("Found mod in {0}", MOD_FOLDER));
            LOCALIZATION_FOLDER_PANDA = Path.Combine(MOD_FOLDER, "localization");
            ICON_FOLDER_PANDA_REL = Path.Combine(MOD_FOLDER, "icons");
            Localize();
        }

        public static void Localize()
        {
            PandaLogger.Log(string.Format("Localization directory: {0}", LOCALIZATION_FOLDER_PANDA));
            try
            {
                string[] array = new string[]
                {
                    "translation.json"
                };
                for (int i = 0; i < array.Length; i++)
                {
                    string text = array[i];
                    string[] files = Directory.GetFiles(LOCALIZATION_FOLDER_PANDA, text, SearchOption.AllDirectories);
                    string[] array2 = files;
                    for (int j = 0; j < array2.Length; j++)
                    {
                        string text2 = array2[j];
                        try
                        {
                            JSONNode jsonFromMod;
                            if (JSON.Deserialize(text2, out jsonFromMod, false))
                            {
                                string name = Directory.GetParent(text2).Name;
                                PandaLogger.Log(string.Format("Found mod localization file for '{0}' localization", name));
                                localize(name, text, jsonFromMod);
                            }
                        }
                        catch (Exception ex)
                        {
                            PandaLogger.Log(string.Format("Exception reading localization from {0}; {1}", text2, ex.Message));
                        }
                    }
                }
            }
            catch (DirectoryNotFoundException)
            {
                PandaLogger.Log(string.Format("Localization directory not found at {0}", LOCALIZATION_FOLDER_PANDA));
            }
        }

        public static void localize(string locName, string locFilename, JSONNode jsonFromMod)
        {
            try
            {
                string text = CombinePaths(new string[]
                {
                    "gamedata",
                    "localization",
                    locName,
                    locFilename
                });

                JSONNode jSONNode;

                if (JSON.Deserialize(text, out jSONNode, false))
                {
                    foreach (KeyValuePair<string, JSONNode> modNode in jsonFromMod.LoopObject())
                    {
                        PandaLogger.Log(string.Format("localization '{0}' added to '{1}'. This will apply AFTER next restart!!!", modNode.Key, Path.Combine(locName, locFilename)));
                        jSONNode["sentences"][modNode.Key] = modNode.Value;
                    }

                    JSON.Serialize(text, jSONNode, 2);
                    PandaLogger.Log(string.Format("Patched mod localization file '{0}/{1}' into '{2}'", locName, locFilename, text));
                }
                else
                {
                    PandaLogger.Log(string.Format("Could not deserialize json from '{0}'", text));
                }
            }
            catch (Exception ex)
            {
                PandaLogger.LogError(string.Format("Exception while localizing {0}", Path.Combine(locName, locFilename)), ex);
            }
        }

        public static string CombinePaths(params string[] pathParts)
        {
            StringBuilder stringBuilder = new StringBuilder();
            for (int i = 0; i < pathParts.Length; i++)
            {
                string text = pathParts[i];
                stringBuilder.Append(text.TrimEnd(new char[]
                {
                    '/',
                    '\\'
                })).Append(Path.DirectorySeparatorChar);
            }
            return stringBuilder.ToString().TrimEnd(new char[]
            {
                Path.DirectorySeparatorChar
            });
        }
    }
}
