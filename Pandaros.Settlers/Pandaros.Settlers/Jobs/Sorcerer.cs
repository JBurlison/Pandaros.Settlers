using BlockTypes;
using Jobs;
using NPC;
using Pandaros.Settlers.Items;
using Pipliz.JSON;
using Recipes;
using System.Collections.Generic;
using UnityEngine;

namespace Pandaros.Settlers.Jobs
{
    [ModLoader.ModManager]
    public static class SorcererRegister
    {
        public static string JOB_NAME = GameLoader.NAMESPACE + ".Sorcerer";
        public static string JOB_ITEM_KEY = GameLoader.NAMESPACE + ".SorcererTable";
        public static string JOB_RECIPE = JOB_ITEM_KEY + ".recipe";

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterItemTypesDefined, GameLoader.NAMESPACE + ".SorcererRegister.RegisterJobs")]
        [ModLoader.ModCallbackProvidesFor("create_savemanager")]
        public static void RegisterJobs()
        {
            NPCType.AddSettings(new NPCTypeStandardSettings
            {
                keyName = JOB_NAME,
                printName = "Sorcerer",
                maskColor1 = new Color32(9, 0, 115, 255),
                type = NPCTypeID.GetNextID()
            });

            ServerManager.BlockEntityCallbacks.RegisterEntityManager(new BlockJobManager<CraftingJobInstance>(new CraftingJobSettings(JOB_ITEM_KEY, JOB_NAME)));
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterSelectedWorld,
            GameLoader.NAMESPACE + ".SorcererRegister.AddTextures")]
        [ModLoader.ModCallbackProvidesFor("pipliz.server.registertexturemappingtextures")]
        public static void AddTextures()
        {
            var textureMapping = new ItemTypesServer.TextureMapping(new JSONNode());
            textureMapping.AlbedoPath = GameLoader.BLOCKS_ALBEDO_PATH + "SorcererTableTop.png";
            textureMapping.NormalPath = GameLoader.BLOCKS_NORMAL_PATH + "SorcererTableTop.png";
            textureMapping.HeightPath = GameLoader.BLOCKS_HEIGHT_PATH + "SorcererTableTop.png";

            ItemTypesServer.SetTextureMapping(GameLoader.NAMESPACE + "SorcererTableTop", textureMapping);
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AddItemTypes,
            GameLoader.NAMESPACE + ".SorcererRegister.AddItemTypes")]
        public static void AddItemTypes(Dictionary<string, ItemTypesServer.ItemTypeRaw> itemTypes)
        {
            var item = new JSONNode()
                      .SetAs("icon", GameLoader.ICON_PATH + "SorcererTable.png")
                      .SetAs("onPlaceAudio", "woodPlace")
                      .SetAs("onRemoveAudio", "woodDeleteLight")
                      .SetAs("sideall", "coatedplanks")
                      .SetAs("sidey+", GameLoader.NAMESPACE + "SorcererTableTop")
                      .SetAs("npcLimit", 0);

            var categories = new JSONNode(NodeType.Array);
            categories.AddToArray(new JSONNode("job"));
            item.SetAs("categories", categories);

            itemTypes.Add(JOB_ITEM_KEY, new ItemTypesServer.ItemTypeRaw(JOB_ITEM_KEY, item));
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterWorldLoad,  GameLoader.NAMESPACE + ".SorcererRegister.AfterWorldLoad")]
        public static void AfterWorldLoad()
        {
            var iron   = new InventoryItem(BuiltinBlocks.BronzeIngot, 2);
            var tools  = new InventoryItem(BuiltinBlocks.CopperTools, 1);
            var planks = new InventoryItem(BuiltinBlocks.Planks, 4);

            var recipe = new Recipe(JOB_RECIPE,
                                    new List<InventoryItem> {iron, tools, planks},
                                    new InventoryItem(JOB_ITEM_KEY, 1), 2);

            ServerManager.RecipeStorage.AddPlayerOptionalRecipe(recipe);
            ServerManager.RecipeStorage.AddOptionalLimitTypeRecipe(ItemFactory.JOB_CRAFTER, recipe);
        }
    }
}