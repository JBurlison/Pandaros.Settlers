using Pipliz.JSON;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandaros.Settlers
{
    public static class JSONExtentionMethods
    {
        public static List<T> GetSimpleListFromFile<T>(string file) where T : IJsonDeserializable, new()
        {
            var retval = new List<T>();

            if (!Directory.Exists(file.Substring(0, file.LastIndexOf('/'))))
                Directory.CreateDirectory(file.Substring(0, file.LastIndexOf('/')));

            if (File.Exists(file))
            {
                var jsonFile = JSON.Deserialize(file);

                if (jsonFile.NodeType == NodeType.Array && jsonFile.ChildCount > 0)
                    foreach (var pos in jsonFile.LoopArray())
                    {
                        var noteType = new T();
                        noteType.JsonDeerialize(pos);
                        retval.Add(noteType);
                    }
            }

                return retval;
        }

        public static void SaveSimpleListToJson<T>(string file, List<T> list) where T : IJsonSerializable
        {
            if (!Directory.Exists(file.Substring(0, file.LastIndexOf('/'))))
                Directory.CreateDirectory(file.Substring(0, file.LastIndexOf('/')));

            JSONNode saveNode = new JSONNode(NodeType.Array);

            foreach (var item in list)
                saveNode.AddToArray(item.JsonSerialize());

            JSON.Serialize(file, saveNode);
        }
    }
}
