using BlockTypes;
using NPC;
using Pandaros.Settlers.Items;
using Pipliz.APIProvider.Jobs;
using Pipliz.JSON;
using Recipes;
using System.Collections.Generic;
using UnityEngine;

namespace Pandaros.Settlers.Jobs
{
    [ModLoader.ModManager]
    public static class ApothecaryRegister
    {
        public static string JOB_NAME = GameLoader.NAMESPACE + ".Apothecary";
        public static string JOB_ITEM_KEY = GameLoader.NAMESPACE + ".ApothecaryTable";
        public static string JOB_RECIPE = JOB_ITEM_KEY + ".recipe";

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterItemTypesDefined, GameLoader.NAMESPACE + ".Jobs.ApothecaryRegister.RegisterJobs")]
        [ModLoader.ModCallbackProvidesFor("pipliz.apiprovider.jobs.resolvetypes")]
        public static void RegisterJobs()
        {
            NPCType.AddSettings(new NPCTypeStandardSettings
            {
                keyName = JOB_NAME,
                printName = "Apothecary",
                maskColor1 = new Color32(101, 121, 123, 255),
                type = NPCTypeID.GetNextID()
            });

            ServerManager.BlockEntityCallbacks.RegisterEntityManager(new BlockJobManager<CraftingJobInstance>(new CraftingJobSettings(JOB_RECIPE, JOB_NAME)));
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterSelectedWorld,
            GameLoader.NAMESPACE + ".Jobs.ApothecaryRegister.AddTextures")]
        [ModLoader.ModCallbackProvidesFor("pipliz.server.registertexturemappingtextures")]
        public static void AddTextures()
        {
            var textureMapping = new ItemTypesServer.TextureMapping(new JSONNode());
            textureMapping.AlbedoPath = GameLoader.BLOCKS_ALBEDO_PATH + "ApothecaryTable.png";
            textureMapping.HeightPath = GameLoader.BLOCKS_HEIGHT_PATH + "ApothecaryTable.png";
            textureMapping.NormalPath = GameLoader.BLOCKS_NORMAL_PATH + "ApothecaryTable.png";

            ItemTypesServer.SetTextureMapping(GameLoader.NAMESPACE + "ApothecaryTable", textureMapping);
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterAddingBaseTypes,
            GameLoader.NAMESPACE + ".Jobs.ApothecaryRegister.AfterAddingBaseTypes")]
        public static void AfterAddingBaseTypes(Dictionary<string, ItemTypesServer.ItemTypeRaw> itemTypes)
        {
            var item = new JSONNode()
                      .SetAs("icon", GameLoader.ICON_PATH + "ApothecaryTable.png")
                      .SetAs("onPlaceAudio", "woodPlace")
                      .SetAs("onRemoveAudio", "woodDeleteLight")
                      .SetAs("sideall", GameLoader.NAMESPACE + "ApothecaryTable")
                      .SetAs("npcLimit", 0);

            var categories = new JSONNode(NodeType.Array);
            categories.AddToArray(new JSONNode("job"));
            item.SetAs("categories", categories);

            itemTypes.Add(JOB_ITEM_KEY, new ItemTypesServer.ItemTypeRaw(JOB_ITEM_KEY, item));
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterWorldLoad,
            GameLoader.NAMESPACE + ".Jobs.ApothecaryRegister.AfterWorldLoad")]
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