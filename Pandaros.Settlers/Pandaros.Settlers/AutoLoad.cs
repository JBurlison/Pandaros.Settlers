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

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterItemTypesDefined, GameLoader.NAMESPACE + ".AutoLoad.loadrecipes")]
        [ModLoader.ModCallbackProvidesFor("pipliz.apiprovider.registerrecipes")]
        public static void LoadRecipes()
        {
            //if (JSON.Deserialize(MultiPath.Combine(GameLoader.AUTOLOAD_FOLDER_PANDA, "types.json"), out JSONNode jsonTypes, false))
            //    if (jsonTypes.NodeType == NodeType.Object)
            //        foreach (KeyValuePair<string, JSONNode> typeEntry in jsonTypes.LoopObject())
            //            if (!typeEntry.Key.EndsWith("top") &&
            //                !typeEntry.Key.EndsWith("right") &&
            //                !typeEntry.Key.EndsWith("left") &&
            //                !typeEntry.Key.EndsWith("front") &&
            //                !typeEntry.Key.EndsWith("bottom") &&
            //                !typeEntry.Key.EndsWith("back") &&
            //                !typeEntry.Key.EndsWith("walkz") &&
            //                !typeEntry.Key.EndsWith("walkx") &&
            //                !typeEntry.Key.EndsWith("z+") &&
            //                !typeEntry.Key.EndsWith("z-") &&
            //                !typeEntry.Key.EndsWith("y+") &&
            //                !typeEntry.Key.EndsWith("y-") &&
            //                !typeEntry.Key.EndsWith("x+") &&
            //                !typeEntry.Key.EndsWith("x-"))
            //            ItemTypesServer.LoadSortOrder(MOD_PREFIX + typeEntry.Key, GameLoader.GetNextItemSortIndex());

            PandaLogger.Log("Loading recipes...");

            foreach (string[] jobAndFilename in new string[][] {
                            new string[] { "pipliz.crafter", "crafting.json"},
                            new string[] { "pipliz.tailor", "tailoring.json" },
                            new string[] { "pipliz.grinder", "grinding.json" },
                            new string[] { "pipliz.minter", "minting.json" },
                            new string[] { "pipliz.merchant", "shopping.json" },
                            new string[] { "pipliz.technologist", "technologist.json" },
                            new string[] { "pipliz.smelter", "smelting.json" },
                            new string[] { Jobs.AdvancedCrafterRegister.JOB_NAME, "AdvancedCrafter.json" },
                            new string[] { Jobs.ApothecaryRegister.JOB_NAME, "Apothecary.json" },
                            new string[] { "pipliz.baker", "baking.json" },
                            new string[] { "pipliz.stonemason", "stonemasonry.json" },
                            new string[] { "pipliz.metalsmithjob", "metalsmithing.json" },
                            new string[] { "pipliz.kilnjob", "kiln.json" },
                            new string[] { "pipliz.gunsmithjob", "gunsmith.json" },
                            new string[] { "pipliz.fineryforgejob", "fineryforge.json" },
                            new string[] { "pipliz.dyer", "dyeing.json" },
                            })
            {
                Recipe craftingRecipe = null;
                try
                {
                    if (JSON.Deserialize(MultiPath.Combine(GameLoader.AUTOLOAD_FOLDER_PANDA, jobAndFilename[1]), out JSONNode jsonRecipes, false))
                    {
                        if (jsonRecipes.NodeType == NodeType.Array)
                        {
                            foreach (JSONNode craftingEntry in jsonRecipes.LoopArray())
                            {
                                if (craftingEntry.TryGetAs("name", out string name))
                                {
                                    if (name.StartsWith(VANILLA_PREFIX))
                                        name = name.Substring(VANILLA_PREFIX.Length);
                                    else
                                        name = MOD_PREFIX + name;

                                    craftingEntry.SetAs("name", name);

                                    foreach (string recipePart in new string[] { "results", "requires" })
                                    {
                                        JSONNode jsonRecipeParts = craftingEntry.GetAs<JSONNode>(recipePart);

                                        foreach (JSONNode jsonRecipePart in jsonRecipeParts.LoopArray())
                                        {
                                            string type = jsonRecipePart.GetAs<string>("type");
                                            string realtype;

                                            if (type.StartsWith(VANILLA_PREFIX))
                                                realtype = type.Substring(VANILLA_PREFIX.Length);
                                            else
                                                realtype = MOD_PREFIX + type;
                                            
                                            jsonRecipePart.SetAs("type", realtype);
                                        }
                                    }
                                }

                                craftingRecipe = new Recipe(craftingEntry);
                                RecipeStorage.AddDefaultLimitTypeRecipe(jobAndFilename[0], craftingRecipe);
                            }
                        }
                        else
                        {
                            PandaLogger.Log(ChatColor.red, string.Format("Expected json array in {0}, but got {1} instead", jobAndFilename[1], jsonRecipes.NodeType));
                        }
                    }
                }
                catch (Exception exception)
                {
                    if (craftingRecipe != null)
                        PandaLogger.LogError(exception, "Exception while loading recipes from {0}: {1}. {2}", jobAndFilename[0], jobAndFilename[1], craftingRecipe.ToString());
                    else
                        PandaLogger.LogError(exception, "Exception while loading recipes from {0}: {1}.", jobAndFilename[0], jobAndFilename[1]);

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

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnTryChangeBlockUser, GameLoader.NAMESPACE + ".AutoLoad.trychangeblock")]
        public static bool OnTryChangeBlockUser(ModLoader.OnTryChangeBlockUserData userData)
        {
            if (!userData.isPrimaryAction)
            {
                VoxelSide side = userData.voxelHitSide;
                ushort newType = userData.typeToBuild;

                string suffix;

                if (side == VoxelSide.xPlus)
                {
                    suffix = "right";
                }
                else if (side == VoxelSide.xMin)
                {
                    suffix = "left";
                }
                else if (side == VoxelSide.yPlus)
                {
                    suffix = "bottom";
                }
                else if (side == VoxelSide.yMin)
                {
                    suffix = "top";
                }
                else if (side == VoxelSide.zPlus)
                {
                    suffix = "front";
                }
                else if (side == VoxelSide.zMin)
                {
                    suffix = "back";
                }
                else
                {
                    return true;
                }

                if (newType != userData.typeTillNow && ItemTypes.IndexLookup.TryGetName(newType, out string typename))
                {
                    string otherTypename = typename + suffix;

                    if (ItemTypes.IndexLookup.TryGetIndex(otherTypename, out ushort otherIndex))
                    {
                        Vector3Int position = userData.VoxelToChange;
                        ThreadManager.InvokeOnMainThread(delegate () {
                            ServerManager.TryChangeBlock(position, otherIndex, ServerManager.SetBlockFlags.DefaultAudio);
                        }, 0.1f);
                    }
                }
            }

            return true;
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
