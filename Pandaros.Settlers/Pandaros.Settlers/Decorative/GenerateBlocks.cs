using System.Collections.Generic;
using static ItemTypesServer;
using Pipliz;
using System.IO;
using UnityEngine;
using Pandaros.API.Models;
using Recipes;
using Pandaros.API;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Pandaros.API.Extender;
using Pipliz.JSON;
using System.Reflection;
using System;

namespace Pandaros.Settlers.Decorative
{
    [ModLoader.ModManager]
    public class GenerateBlocks
    {
        static Dictionary<string, List<Tuple<string, string, string>>> _loadedItems = new Dictionary<string, List<Tuple<string, string, string>>>();

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AddItemTypes, GameLoader.NAMESPACE + ".Pandaros.Settlers.Decorative.GenerateBlocks", float.MaxValue)]
        public static void generateTypes(Dictionary<string, ItemTypeRaw> types)
        {
            Dictionary<string, ItemTypeRaw> newItemsDic = new Dictionary<string, ItemTypeRaw>();
            var mi = typeof(ItemTypesServer.BlockRotator).GetMethod("CreateAndRegisterRotatedBlocks", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);

            var generatedIconDir = GameLoader.ICON_PATH + "gen/";

            if (!Directory.Exists(generatedIconDir))
                Directory.CreateDirectory(generatedIconDir);

            if (mi != null)
                foreach (var itemType in types.Values)
                {
                    if (itemType.Categories == null)
                        continue;

                    if (!itemType.Categories.Contains("decorative"))
                        continue;

                    if (itemType.Mesh != null)
                        continue;

                    if (itemType.Mesh != null && !string.IsNullOrEmpty(itemType.Mesh.MeshPath))
                        continue;

                    if (!string.IsNullOrEmpty(itemType.ParentType))
                        continue;

                    if (!string.IsNullOrEmpty(itemType.RotatedXMinus) ||
                        !string.IsNullOrEmpty(itemType.RotatedXPlus) ||
                        !string.IsNullOrEmpty(itemType.RotatedZMinus) ||
                        !string.IsNullOrEmpty(itemType.RotatedZPlus))
                        continue;

                    if (!itemType.IsPlaceable)
                        continue;

                    if (string.IsNullOrEmpty(itemType.SideAll))
                        continue;

                    try
                    {
                        List<Tuple<string, string, string>> blockTypes = new List<Tuple<string, string, string>>();

                        foreach (var blockType in CollederStorage.Colliders_Dict)
                        {
                            var overlayIcon = GameLoader.ICON_PATH + blockType.Key + GameLoader.ICONTYPE;
                            var newIcon = generatedIconDir + itemType.name + "." + blockType.Key + GameLoader.ICONTYPE;
                            var newType = new DecorTypeBase();
                            var typeName = itemType.name + " " + blockType.Key;

                            blockTypes.Add(Tuple.Create(typeName, itemType.name, blockType.Key));
                            

                            if (!File.Exists(newIcon))
                            {
                                var back = new Texture2D(64, 64);

                                if (File.Exists(itemType.Icon))
                                    back.LoadImage(File.ReadAllBytes(itemType.Icon));
                                else
                                    SettlersLogger.Log(ChatColor.red, "Icon not found:" + itemType.Icon);

                                var overlay = new Texture2D(64, 64);

                                if (File.Exists(overlayIcon))
                                    overlay.LoadImage(File.ReadAllBytes(overlayIcon));
                                else
                                    SettlersLogger.Log(ChatColor.red, "Icon not found:" + overlayIcon);

                                var tex = AddWatermark(back, overlay);
                                SaveTextureAsPNG(tex, newIcon);
                            }

                            newType.icon = newIcon;
                            newType.name = typeName;
                            newType.categories.Add(itemType.name);
                            newType.categories.Add(blockType.Key);
                            newType.categories.Add("decorative");
                            newType.categories.Add(GameLoader.NAMESPACE);
                            newType.sideall = itemType.SideAll;
                            newType.mesh = GameLoader.MESH_PATH + blockType.Key + GameLoader.MESHTYPE;
                            newType.colliders.boxes = blockType.Value;

                            if (itemType.CustomDataNode != null)
                                newType.customData = JObject.Parse(itemType.CustomDataNode.ToString());
                            else
                                newType.customData = new JObject();

                            newType.customData["useNormalMap"] = true;
                            newType.customData["useHeightMap"] = true;
                            
                            newType.color = ColorUtility.ToHtmlStringRGBA(itemType.Color);

                            var itemJson = JsonConvert.SerializeObject(newType, Formatting.None, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });
                            var rawItem = new ItemTypeRaw(typeName, JSON.DeserializeString(itemJson));
                            mi.Invoke(null, new object[] { newItemsDic, new BlockRotator.RotatorSettings(rawItem, null, null, null, null), null });
                        }

                        _loadedItems.Add(itemType.name, blockTypes);

                    }
                    catch (Exception ex)
                    {
                        SettlersLogger.LogError(ex);
                    }
                }

            foreach (var newItem in newItemsDic)
            {
                types[newItem.Key] = newItem.Value;
            }

            SettlersLogger.Log(newItemsDic.Count + " new items added.");
        }

        public static void SaveTextureAsPNG(Texture2D _texture, string _fullPath)
        {
            byte[] _bytes = _texture.EncodeToPNG();
            System.IO.File.WriteAllBytes(_fullPath, _bytes);
        }

        public static Texture2D AddWatermark(Texture2D background, Texture2D watermark)
        {

            int startX = 0;
            int startY = background.height - watermark.height;

            for (int x = startX; x < background.width; x++)
            {

                for (int y = startY; y < background.height; y++)
                {
                    Color bgColor = background.GetPixel(x, y);
                    Color wmColor = watermark.GetPixel(x - startX, y - startY);

                    Color final_color = Color.Lerp(bgColor, wmColor, wmColor.a / 1.0f);

                    background.SetPixel(x, y, final_color);
                }
            }

            background.Apply();
            return background;
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterItemTypesDefined, GameLoader.NAMESPACE + ".Pandaros.Settlers.Decorative.GenerateRecipes")]
        public static void GenerateRecipes()
        {
            List<string> recipesAdded = new List<string>();

            foreach (var item in _loadedItems)
            {
                var baseType = item.Key;
                var typeName = GameLoader.TYPEPREFIX + baseType + ".Blocks";
                var typeNameRecipe = typeName + ".Recipe";

                if (!recipesAdded.Contains(typeNameRecipe))
                {
                    var recipe = new TypeRecipeBase();
                    recipe.name = typeNameRecipe;
                    recipe.requires.Add(new RecipeItem(baseType, 5));
                    recipe.Job = Jobs.DecorBuilderRegister.JOB_NAME;

                    foreach (var i in item.Value)
                        recipe.results.Add(new RecipeResult(i.Item1));

                    var requirements = new List<InventoryItem>();
                    var results = new List<RecipeResult>();
                    recipe.JsonSerialize();

                    foreach (var ri in recipe.requires)
                    {
                        if (ItemTypes.IndexLookup.TryGetIndex(ri.type, out var itemIndex))
                        {
                            requirements.Add(new InventoryItem(itemIndex, ri.amount));
                        }
                        else
                        {
                            SettlersLogger.LogToFile("\"" + typeNameRecipe + "\" bad requirement \"" + ri.type + "\"");
                        }
                    }
                    foreach (var ri in recipe.results)
                        results.Add(ri);

                    var newRecipe = new Recipe(recipe.name, requirements, results, 0, 0, (int)recipe.defaultPriority);

                    ServerManager.RecipeStorage.AddLimitTypeRecipe(recipe.Job, newRecipe);
                }

            }

            
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterWorldLoad, GameLoader.NAMESPACE + ".Pandaros.Settlers.Decorative.AfterWorldLoad")]
        [ModLoader.ModCallbackProvidesFor("pipliz.server.localization.convert")]
        public static void AfterWorldLoad()
        {
            var patch = new Localization.LocalePatch();
            patch.Locale = "en-US";
            patch.Data = new JSONNode();
            patch.Data["types"] = new JSONNode();

            foreach (var item in _loadedItems)
                foreach (var newItem in item.Value)
                {
                    
                    if (Localization.TryGetType("en-US", newItem.Item1, out string readableString))
                        patch.Data["types"][newItem.Item1] = new JSONNode(readableString + " " + newItem.Item3);
                    else
                        patch.Data["types"][newItem.Item1] = new JSONNode(newItem.Item2 + " " + newItem.Item3);
                }

            Localization.QueueLocalePatch(patch);
            _loadedItems.Clear();
        }

    }

}
