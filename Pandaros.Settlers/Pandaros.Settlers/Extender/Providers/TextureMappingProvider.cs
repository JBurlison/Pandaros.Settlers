using Pandaros.Settlers.Items;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pandaros.Settlers.Extender.Providers
{
    public class TextureMappingProvider : IAfterSelectedWorld
    {
        public List<Type> LoadedAssembalies { get; } = new List<Type>();

        public string InterfaceName => nameof(ICSTextureMapping);
        public Type ClassType => null;

        public void AfterSelectedWorld()
        {
            StringBuilder sb = new StringBuilder();
            PandaLogger.Log(ChatColor.lime, "-------------------Texture Mapping Loaded----------------------");
            var i = 0;

            foreach (var item in LoadedAssembalies)
            {
                if (Activator.CreateInstance(item) is ICSTextureMapping texture &&
                    !string.IsNullOrEmpty(texture.Name))
                {
                    ItemTypesServer.SetTextureMapping(texture.Name, new ItemTypesServer.TextureMapping(texture.JsonSerialize()));
                    sb.Append($"{texture.Name}, ");
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

            PandaLogger.Log(ChatColor.lime, sb.ToString());
            PandaLogger.Log(ChatColor.lime, "---------------------------------------------------------");
        }
    }
}
