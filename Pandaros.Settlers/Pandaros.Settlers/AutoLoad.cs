using System;
using System.IO;
using System.Collections.Generic;
using Pipliz;
using Pipliz.JSON;
using Pipliz.Threading;
using NPC;
using System.Text;

namespace Pandaros.Settlers
{
    [ModLoader.ModManager]
    public static class AutoLoad
    {
        private static string MOD_PREFIX = GameLoader.NAMESPACE + ".AutoLoad.";
        private static string VANILLA_PREFIX = "vanilla.";
        private static List<string> crateTypeKeys = new List<string>();


        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterSelectedWorld, GameLoader.NAMESPACE + ".AutoLoad.registertexturemappings")]
        [ModLoader.ModCallbackProvidesFor("pipliz.server.registertexturemappingtextures")]
        public static void AfterSelectedWorld()
        {
            PandaLogger.Log("Loading texture mappings...");

            if (JSON.Deserialize(MultiPath.Combine(GameLoader.AUTOLOAD_FOLDER_PANDA, "texturemapping.json"), out JSONNode jsonTextureMapping, false))
            {
                if (jsonTextureMapping.NodeType == NodeType.Object)
                {
                    foreach (KeyValuePair<string, JSONNode> textureEntry in jsonTextureMapping.LoopObject())
                    {
                        try
                        {
                            string albedoPath = null;
                            string normalPath = null;
                            string emissivePath = null;
                            string heightPath = null;

                            foreach (string textureType in new string[] { "albedo", "normal", "emissive", "height" })
                            {
                                string textureTypeValue = textureEntry.Value.GetAs<string>(textureType);
                                string realTextureTypeValue = textureTypeValue;

                                if (!textureTypeValue.Equals("neutral"))
                                {
                                    if (textureTypeValue.StartsWith(VANILLA_PREFIX))
                                    {
                                        realTextureTypeValue = realTextureTypeValue.Substring(VANILLA_PREFIX.Length);
                                    }
                                    else
                                    {
                                        realTextureTypeValue = MultiPath.Combine(GameLoader.TEXTURE_FOLDER_PANDA, textureType, textureTypeValue + ".png");

                                        switch(textureType.ToLowerInvariant())
                                        {
                                            case "albedo":
                                                albedoPath = realTextureTypeValue;
                                                break;
                                            case "normal":
                                                normalPath = realTextureTypeValue;
                                                break;
                                            case "emissive":
                                                emissivePath = realTextureTypeValue;
                                                break;
                                            case "height":
                                                heightPath = realTextureTypeValue;
                                                break;
                                        }
                                    }
                                }
                                textureEntry.Value.SetAs(textureType, realTextureTypeValue);
                            }

                            var textureMapping = new ItemTypesServer.TextureMapping(textureEntry.Value);

                            if (albedoPath != null)
                                textureMapping.AlbedoPath = albedoPath;

                            if (normalPath != null)
                                textureMapping.NormalPath = normalPath;

                            if (emissivePath != null)
                                textureMapping.EmissivePath = emissivePath;

                            if (heightPath != null)
                                textureMapping.HeightPath = heightPath;

                            string realkey = MOD_PREFIX + textureEntry.Key;
                            
                            ItemTypesServer.SetTextureMapping(realkey, textureMapping);
                        }
                        catch (Exception exception)
                        {
                            PandaLogger.LogError(exception, string.Format("Exception while loading from {0}; {1}", "texturemapping.json", exception.Message));
                        }
                    }
                }
                else
                {
                    PandaLogger.Log(ChatColor.red, string.Format("Expected json object in {0}, but got {1} instead", "texturemapping.json", jsonTextureMapping.NodeType));
                }
            }
            
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterAddingBaseTypes, GameLoader.NAMESPACE + ".AutoLoad.addrawtypes")]
        public static void AfterAddingBaseTypes(Dictionary<string, ItemTypesServer.ItemTypeRaw> itemTypes)
        {
            PandaLogger.Log("Loading types...");

            if (JSON.Deserialize(MultiPath.Combine(GameLoader.AUTOLOAD_FOLDER_PANDA, "types.json"), out JSONNode jsonTypes, false))
            {
                if (jsonTypes.NodeType == NodeType.Object)
                {
                    foreach (KeyValuePair<string, JSONNode> typeEntry in jsonTypes.LoopObject())
                    {
                        try
                        {
                            if (typeEntry.Value.TryGetAs("icon", out string icon))
                            {
                                string realicon;

                                if (icon.StartsWith(VANILLA_PREFIX))
                                    realicon = MultiPath.Combine("gamedata", "textures", "icons", icon.Substring(VANILLA_PREFIX.Length));
                                else
                                    realicon = MultiPath.Combine(GameLoader.ICON_FOLDER_PANDA, icon);
                                
                                typeEntry.Value.SetAs("icon", realicon);
                            }
                            
                            if (typeEntry.Value.TryGetAs("mesh", out string mesh))
                            {
                                string realmesh;

                                if (mesh.StartsWith(VANILLA_PREFIX))
                                    realmesh = mesh.Substring(VANILLA_PREFIX.Length);
                                else
                                    realmesh = MultiPath.Combine(GameLoader.MESH_FOLDER_PANDA, mesh);
                                
                                typeEntry.Value.SetAs("mesh", realmesh);
                            }


                            if (typeEntry.Value.TryGetAs("parentType", out string parentType))
                            {
                                string realParentType;

                                if (parentType.StartsWith(VANILLA_PREFIX))
                                    realParentType = parentType.Substring(VANILLA_PREFIX.Length);
                                else
                                    realParentType = MOD_PREFIX + parentType;
                                
                                typeEntry.Value.SetAs("parentType", realParentType);
                            }

                            foreach (string rotatable in new string[] { "rotatablex+", "rotatablex-", "rotatablez+", "rotatablez-" })
                            {
                                if (typeEntry.Value.TryGetAs(rotatable, out string key))
                                {
                                    string rotatablekey;

                                    if (key.StartsWith(VANILLA_PREFIX))
                                        rotatablekey = key.Substring(VANILLA_PREFIX.Length);
                                    else
                                        rotatablekey = MOD_PREFIX + key;
                
                                    typeEntry.Value.SetAs(rotatable, rotatablekey);
                                }
                            }

                            foreach (string side in new string[] { "sideall", "sidex+", "sidex-", "sidey+", "sidey-", "sidez+", "sidez-" })
                            {
                                if (typeEntry.Value.TryGetAs(side, out string key))
                                {
                                    if (!key.Equals("SELF"))
                                    {
                                        string sidekey;

                                        if (key.StartsWith(VANILLA_PREFIX))
                                            sidekey = key.Substring(VANILLA_PREFIX.Length);
                                        else
                                            sidekey = MOD_PREFIX + key;
                                        
                                        typeEntry.Value.SetAs(side, sidekey);
                                    }
                                }
                            }

                            if (typeEntry.Value.TryGetAs("onRemoveType", out string onRemoveType))
                            {
                                string realOnRemoveType;

                                if (onRemoveType.StartsWith(VANILLA_PREFIX))
                                    realOnRemoveType = onRemoveType.Substring(VANILLA_PREFIX.Length);
                                else
                                    realOnRemoveType = MOD_PREFIX + onRemoveType;
                                
                                typeEntry.Value.SetAs("onRemoveType", realOnRemoveType);
                            }

                            if (typeEntry.Value.TryGetAs("onPlaceAudio", out string onPlaceAudio))
                            {
                                string realOnPlaceAudio;

                                if (onPlaceAudio.StartsWith(VANILLA_PREFIX))
                                    realOnPlaceAudio = onPlaceAudio.Substring(VANILLA_PREFIX.Length);
                                else
                                    realOnPlaceAudio = MOD_PREFIX + onPlaceAudio;
                                
                                typeEntry.Value.SetAs("onPlaceAudio", realOnPlaceAudio);
                            }

                            if (typeEntry.Value.TryGetAs("onRemoveAudio", out string onRemoveAudio))
                            {
                                string realOnRemoveAudio;

                                if (onRemoveAudio.StartsWith(VANILLA_PREFIX))
                                    realOnRemoveAudio = onRemoveAudio.Substring(VANILLA_PREFIX.Length);
                                else
                                    realOnRemoveAudio = MOD_PREFIX + onRemoveAudio;
                                
                                typeEntry.Value.SetAs("onRemoveAudio", realOnRemoveAudio);
                            }

                            string realkey = MOD_PREFIX + typeEntry.Key;

                            if (typeEntry.Value.TryGetAs("isCrate", out bool isCrate) && isCrate)
                                crateTypeKeys.Add(realkey);

                            itemTypes.Add(realkey, new ItemTypesServer.ItemTypeRaw(realkey, typeEntry.Value));
                        }
                        catch (Exception exception)
                        {
                            PandaLogger.LogError(exception, string.Format("Exception while loading block type {0}; {1}", typeEntry.Key, exception.Message));
                        }
                    }
                }
                else
                {
                    PandaLogger.Log(ChatColor.red, string.Format("Expected json object in {0}, but got {1} instead", "types.json", jsonTypes.NodeType));
                }
            }
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterItemTypesDefined, GameLoader.NAMESPACE + ".AutoLoad.registertypes")]
        public static void AfterItemTypesDefined()
        {
            foreach (string typekey in crateTypeKeys)
            {
                ItemTypesServer.RegisterOnAdd(typekey, StockpileBlockTracker.Add);
                ItemTypesServer.RegisterOnRemove(typekey, StockpileBlockTracker.Remove);
            }
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnTryChangeBlock, GameLoader.NAMESPACE + ".AutoLoad.trychangeblock")]
        public static void OnTryChangeBlockUser(ModLoader.OnTryChangeBlockData userData)
        {
            if (userData.CallbackState == ModLoader.OnTryChangeBlockData.ECallbackState.Cancelled)
                return;

            if (userData.CallbackOrigin == ModLoader.OnTryChangeBlockData.ECallbackOrigin.ClientPlayerManual)
            {
                VoxelSide side = userData.PlayerClickedData.VoxelSideHit;
                ushort newType = userData.TypeNew;
                string suffix = string.Empty;

                switch (side)
                {
                    case VoxelSide.xPlus:
                        suffix = "right";
                        break;

                    case VoxelSide.xMin:
                        suffix = "left";
                        break;

                    case VoxelSide.yPlus:
                        suffix = "bottom";
                        break;

                    case VoxelSide.yMin:
                        suffix = "top";
                        break;

                    case VoxelSide.zPlus:
                        suffix = "front";
                        break;

                    case VoxelSide.zMin:
                        suffix = "back";
                        break;
                }

                if (newType != userData.TypeOld && ItemTypes.IndexLookup.TryGetName(newType, out string typename))
                {
                    string otherTypename = typename + suffix;

                    if (ItemTypes.IndexLookup.TryGetIndex(otherTypename, out ushort otherIndex))
                    {
                        Vector3Int position = userData.Position;
                        ThreadManager.InvokeOnMainThread(delegate () {
                            ServerManager.TryChangeBlock(position, otherIndex);
                        }, 0.1f);
                    }
                }
            }
        }
    }

    public static class MultiPath
    {
        public static string Combine(params string[] pathParts)
        {
            StringBuilder result = new StringBuilder();
            foreach (string part in pathParts)
            {
                result.Append(part.TrimEnd('/', '\\')).Append(Path.DirectorySeparatorChar);
            }
            return result.ToString().TrimEnd(Path.DirectorySeparatorChar);
        }
    }

    public static class TypeHelper
    {
        public static string RotatableToBasetype(string typename)
        {
            if (typename.EndsWith("x+") || typename.EndsWith("x-") || typename.EndsWith("z+") || typename.EndsWith("z-"))
            {
                return typename.Substring(0, typename.Length - 2);
            }
            else
            {
                return typename;
            }
        }

        public static string GetXZFromTypename(string typename)
        {
            if (typename.EndsWith("x+") || typename.EndsWith("x-") || typename.EndsWith("z+") || typename.EndsWith("z-"))
            {
                return typename.Substring(typename.Length - 2);
            }
            else
            {
                return "";
            }
        }

        public static Vector3Int RotatableToVector(string typename)
        {
            string xz = GetXZFromTypename(typename);
            if (xz.Equals("x+"))
            {
                return new Vector3Int(1, 0, 0);
            }
            else if (xz.Equals("x-"))
            {
                return new Vector3Int(-1, 0, 0);
            }
            else if (xz.Equals("y+"))
            {
                return new Vector3Int(0, 1, 0);
            }
            else if (xz.Equals("y-"))
            {
                return new Vector3Int(0, -1, 0);
            }
            else if (xz.Equals("z+"))
            {
                return new Vector3Int(0, 0, 1);
            }
            else if (xz.Equals("z-"))
            {
                return new Vector3Int(0, 0, -1);
            }
            else
            {
                return new Vector3Int(0, 0, 0);
            }
        }

        public static string VectorToXZ(Vector3Int vec)
        {
            if (vec.x == 1)
            {
                return "x+";
            }
            else if (vec.x == -1)
            {
                return "x-";
            }
            else if (vec.y == 1)
            {
                return "y+";
            }
            else if (vec.y == -1)
            {
                return "y-";
            }
            else if (vec.z == 1)
            {
                return "z+";
            }
            else if (vec.z == -1)
            {
                return "z-";
            }
            else
            {
                PandaLogger.Log(ChatColor.red, string.Format("Malformed vector {0}", vec));
                return "x+";
            }
        }
    }
}
