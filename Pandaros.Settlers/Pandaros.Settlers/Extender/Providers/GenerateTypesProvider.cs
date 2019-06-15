using Newtonsoft.Json;
using Pandaros.Settlers.Items;
using Pipliz.JSON;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandaros.Settlers.Extender.Providers
{
    public class GenerateTypesProvider : IAfterSelectedWorld
    {
        public List<Type> LoadedAssembalies { get; } = new List<Type>();

        public string InterfaceName => nameof(ICSGenerateType);
        public Type ClassType => null;

        public void AfterSelectedWorld()
        {
            StringBuilder sb = new StringBuilder();
            PandaLogger.Log(ChatColor.lime, "-------------------Generate Type Loaded----------------------");
            var i = 0;
            List<ICSGenerateType> json = new List<ICSGenerateType>();

            foreach (var item in LoadedAssembalies)
            {
                if (Activator.CreateInstance(item) is ICSGenerateType generateType &&
                    !string.IsNullOrEmpty(generateType.typeName))
                {
                    json.Add(generateType);
                    
                    sb.Append($"{generateType.typeName}, ");
                    i++;

                    if (i > 5)
                    {
                        sb.Append("</color>");
                        i = 0;
                        sb.AppendLine();
                        sb.Append("<color=lime>");
                    }
                }
            }

            if (json.Count != 0)
            {
                var strValue = JsonConvert.SerializeObject(json, Formatting.None, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });
                PandaLogger.Log(strValue);
                ItemTypesServer.BlockRotator.Patches.AddPatch(new ItemTypesServer.BlockRotator.BlockGeneratePatch(GameLoader.MOD_FOLDER, -99999, JSON.DeserializeString(strValue)));
            }

            PandaLogger.Log(ChatColor.lime, sb.ToString());
            PandaLogger.Log(ChatColor.lime, "---------------------------------------------------------");
        }
    }
}
