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
        public static string ICON_FOLDER_PANDA = @"gamedata\mods\Pandaros\settlers\icons";
        public static string LOCALIZATION_FOLDER_PANDA = @"gamedata\mods\Pandaros\settlers\localization";
        public static string MOD_FOLDER = @"gamedata\mods\Pandaros\settlers";

        public const string NAMESPACE = "Pandaros.Settlers";

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnAssemblyLoaded, NAMESPACE + ".OnAssemblyLoaded")]
        public static void OnAssemblyLoaded(string path)
        {
            MOD_FOLDER = Path.GetDirectoryName(path);
            PandaLogger.Log("Found mod in {0}", MOD_FOLDER);
            LOCALIZATION_FOLDER_PANDA = Path.Combine(MOD_FOLDER, "localization");
            ICON_FOLDER_PANDA = Path.Combine(MOD_FOLDER, "icons");
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterWorldLoad, NAMESPACE + ".Localize")]
        [ModLoader.ModCallbackDependsOn("pipliz.server.localization.waitforloading")]
        [ModLoader.ModCallbackProvidesFor("pipliz.server.localization.convert")]
        public static void Localize()
        {
            PandaLogger.Log("Localization directory: {0}", LOCALIZATION_FOLDER_PANDA);
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
                                PandaLogger.Log("Found mod localization file for '{0}' localization", name);
                                localize(name, text, jsonFromMod);
                            }
                        }
                        catch (Exception ex)
                        {
                            PandaLogger.Log("Exception reading localization from {0}; {1}", text2, ex.Message);
                        }
                    }
                }
            }
            catch (DirectoryNotFoundException)
            {
                PandaLogger.Log("Localization directory not found at {0}", LOCALIZATION_FOLDER_PANDA);
            }
        }

        public static void localize(string locName, string locFilename, JSONNode jsonFromMod)
        {
            try
            {
                foreach (KeyValuePair<string, JSONNode> modNode in jsonFromMod.LoopObject())
                {
                    PandaLogger.Log("Adding localization for '{0}' from '{1}'.", modNode.Key, Path.Combine(locName, locFilename));
                    JSONNode jsn;

                    if (Server.Localization.Localization.LoadedTranslation == null)
                    {
                        PandaLogger.Log("Unable to localize. Server.Localization.Localization.LoadedTranslation is null.");
                    }
                    else
                    {
                        if (Server.Localization.Localization.LoadedTranslation.TryGetValue(locName, out jsn))
                        {
                            if (jsn != null)
                            {
                                jsn["sentences"][modNode.Key] = modNode.Value;
                            }
                            else
                                PandaLogger.Log("Unable to localize. Localization '{0}' not found and is null.", locName);
                        }
                        else
                            PandaLogger.Log("Localization '{0}' not supported", locName);
                    }
                }

                PandaLogger.Log("Patched mod localization file '{0}/{1}'", locName, locFilename);
                
            }
            catch (Exception ex)
            {
                PandaLogger.LogError(ex, "Exception while localizing {0}", Path.Combine(locName, locFilename));
            }
        }
    }
}
