using Jobs;
using NPC;
using Pandaros.API;
using Pandaros.API.Models;
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
    }

    public class AdvancedCraftingTableRecipe : ICSRecipe
    {
        public List<RecipeItem> requires => new List<RecipeItem>()
        {
            new RecipeItem(ColonyBuiltIn.ItemTypes.BRONZEINGOT.Id, 2),
            new RecipeItem(ColonyBuiltIn.ItemTypes.COPPERTOOLS.Id, 1),
            new RecipeItem(ColonyBuiltIn.ItemTypes.PLANKS.Id, 4)
        };

        public List<RecipeResult> results => new List<RecipeResult>()
        {
            new RecipeResult(SettlersBuiltIn.ItemTypes.ADVANCEDCRAFTINGTABLE.Id)
        };

        public CraftPriority defaultPriority => CraftPriority.Medium;

        public bool isOptional => true;

        public int defaultLimit => 1;

        public string Job => ColonyBuiltIn.NpcTypes.CRAFTER;

        public string name => SettlersBuiltIn.ItemTypes.ADVANCEDCRAFTINGTABLE;
        public List<string> JobBlock => new List<string>()
        {
            ColonyBuiltIn.ItemTypes.WORKBENCH
        };
    }
}