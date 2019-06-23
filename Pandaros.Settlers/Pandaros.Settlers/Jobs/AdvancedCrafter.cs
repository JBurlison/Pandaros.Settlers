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
    public static class AdvancedCrafterRegister
    {
        public static string JOB_NAME = GameLoader.NAMESPACE + ".AdvancedCrafter";
        public static string JOB_ITEM_KEY = GameLoader.NAMESPACE + ".AdvancedCraftingTable";
        public static string JOB_RECIPE = JOB_ITEM_KEY;

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterItemTypesDefined, GameLoader.NAMESPACE + ".AdvancedCrafterRegister.RegisterJobs")]
        [ModLoader.ModCallbackProvidesFor("create_savemanager")]
        [ModLoader.ModCallbackProvidesFor("pipliz.server.loadresearchables")]
        public static void RegisterJobs()
        {
            NPCType.AddSettings(new NPCTypeStandardSettings
            {
                keyName = JOB_NAME,
                printName = "Advanced Crafter",
                maskColor1 = new Color32(101, 121, 123, 255),
                type = NPCTypeID.GetNextID()
            });

            ServerManager.BlockEntityCallbacks.RegisterEntityManager(new BlockJobManager<CraftingJobInstance>(new CraftingJobSettings(JOB_ITEM_KEY, JOB_NAME)));
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterSelectedWorld, GameLoader.NAMESPACE + ".AdvancedCrafterRegister.AddTextures")]
        [ModLoader.ModCallbackProvidesFor("pipliz.server.registertexturemappingtextures")]
        public static void AddTextures()
        {
            var textureMapping = new ItemTypesServer.TextureMapping(new JSONNode());
            textureMapping.AlbedoPath = GameLoader.BLOCKS_ALBEDO_PATH + "AdvancedCraftingTableTop.png";
            textureMapping.NormalPath = GameLoader.BLOCKS_NORMAL_PATH + "AdvancedCraftingTableTop.png";
            textureMapping.HeightPath = GameLoader.BLOCKS_HEIGHT_PATH + "AdvancedCraftingTableTop.png";

            ItemTypesServer.SetTextureMapping(GameLoader.NAMESPACE + "AdvancedCraftingTableTop", textureMapping);
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AddItemTypes, GameLoader.NAMESPACE + ".AdvancedCrafterRegister.AddItemTypes")]
        public static void AddItemTypes(Dictionary<string, ItemTypesServer.ItemTypeRaw> itemTypes)
        {
            var item = new JSONNode()
                      .SetAs("icon", GameLoader.ICON_PATH + "AdvancedCraftingTable.png")
                      .SetAs("onPlaceAudio", "woodPlace")
                      .SetAs("onRemoveAudio", "woodDeleteLight")
                      .SetAs("sideall", "planks")
                      .SetAs("sidey+", GameLoader.NAMESPACE + "AdvancedCraftingTableTop")
                      .SetAs("npcLimit", 0);

            var categories = new JSONNode(NodeType.Array);
            categories.AddToArray(new JSONNode("job"));
            item.SetAs("categories", categories);

            itemTypes.Add(JOB_ITEM_KEY, new ItemTypesServer.ItemTypeRaw(JOB_ITEM_KEY, item));
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterWorldLoad,  GameLoader.NAMESPACE + ".AdvancedCrafterRegister.AfterWorldLoad")]
        public static void AfterWorldLoad()
        {
            var iron   = new InventoryItem(ColonyBuiltIn.ItemTypes.BRONZEINGOT.Name, 2);
            var tools  = new InventoryItem(ColonyBuiltIn.ItemTypes.COPPERTOOLS.Name, 1);
            var planks = new InventoryItem(ColonyBuiltIn.ItemTypes.PLANKS.Name, 4);

            var recipe = new Recipe(JOB_RECIPE,
                                    new List<InventoryItem> {iron, tools, planks},
                                    new RecipeResult(ItemTypes.IndexLookup.StringLookupTable[JOB_ITEM_KEY], 1), 2);

            ServerManager.RecipeStorage.AddLimitTypeRecipe(ColonyBuiltIn.NpcTypes.CRAFTER, recipe);
            ServerManager.RecipeStorage.AddScienceRequirement(recipe);
            
        }
    }
}