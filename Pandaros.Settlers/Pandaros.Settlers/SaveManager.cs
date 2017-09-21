using Pandaros.Settlers.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace Pandaros.Settlers
{
    public static class SaveManager
    {
        const string CONFIG_PATH = "gamedata/mods/Pandaros/Settlers/State.xml";
        public static Type CollectionType = typeof(SerializableDictionary<string, ColonyState>);

        public static void SaveState(SerializableDictionary<string, ColonyState> states)
        {
            try
            {
                var stringWriter = new StringWriter();

                XmlSerializer xmlserializer = new XmlSerializer(CollectionType);
                using (var writer = XmlWriter.Create(stringWriter))
                {
                    xmlserializer.Serialize(writer, states);
                    File.WriteAllText(CONFIG_PATH, stringWriter.ToString());
                }
            }
            catch (Exception ex)
            {
                PandaLogger.LogError("SaveState", ex);
            }
        }

        public static SerializableDictionary<string, ColonyState> LoadState()
        {
            var retVal = new SerializableDictionary<string, ColonyState>();

            try
            {
                PandaLogger.Log("Loading from State file from " + CONFIG_PATH);

                if (File.Exists(CONFIG_PATH))
                {
                    XmlSerializer xmlserializer = new XmlSerializer(CollectionType);
                    using (StreamReader reader = new StreamReader(CONFIG_PATH))
                        retVal = (SerializableDictionary<string, ColonyState>)xmlserializer.Deserialize(reader);
                }
                else
                    PandaLogger.Log("Unable to find existing config file. Creating new file.");

                if (retVal == null)
                {
                    PandaLogger.Log("Failed to load stock from config file.");
                    retVal = new SerializableDictionary<string, ColonyState>();
                }
                else
                {
                    PandaLogger.Log("Stock Loaded from Config file.");

                    foreach (var world in retVal)
                    {
                        PandaLogger.Log("World: " + world.Key);

                        var players = string.Empty;

                        foreach (var p in world.Value.PlayerStates)
                            players += " " + p.Key;

                        PandaLogger.Log("Indexed Players: " + players);
                    }
                }
            }
            catch (Exception ex)
            {
                PandaLogger.LogError("LoadState", ex);
            }

            return retVal;
        }
    }
}
