using BlockTypes.Builtin;
using Pipliz.JSON;
using Pipliz.Mods.APIProvider.Jobs;
using Server.NPCs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
            BlockJobManagerTracker.Register<ApothecaryJob>(JOB_ITEM_KEY);
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterSelectedWorld, GameLoader.NAMESPACE + ".Jobs.ApothecaryRegister.AddTextures"), ModLoader.ModCallbackProvidesFor("pipliz.server.registertexturemappingtextures")]
        public static void AddTextures()
        {
            var textureMapping = new ItemTypesServer.TextureMapping(new JSONNode());
            textureMapping.AlbedoPath = GameLoader.TEXTURE_FOLDER_PANDA + "/albedo/ApothecaryTable.png";
            textureMapping.HeightPath = GameLoader.TEXTURE_FOLDER_PANDA + "/height/ApothecaryTable.png";
            textureMapping.NormalPath = GameLoader.TEXTURE_FOLDER_PANDA + "/normal/ApothecaryTable.png";

            ItemTypesServer.SetTextureMapping(GameLoader.NAMESPACE + "ApothecaryTable", textureMapping);
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterAddingBaseTypes, GameLoader.NAMESPACE + ".Jobs.ApothecaryRegister.AfterAddingBaseTypes")]
        public static void AfterAddingBaseTypes(Dictionary<string, ItemTypesServer.ItemTypeRaw> itemTypes)
        {
            itemTypes.Add(JOB_ITEM_KEY, new ItemTypesServer.ItemTypeRaw(JOB_ITEM_KEY, new JSONNode()
              .SetAs("icon", System.IO.Path.Combine(GameLoader.ICON_FOLDER_PANDA, "ApothecaryTable.png"))
              .SetAs("onPlaceAudio", "woodPlace")
              .SetAs("onRemoveAudio", "woodDeleteLight")
              .SetAs("sideall", GameLoader.NAMESPACE + "ApothecaryTable")
              .SetAs("npcLimit", 0)
            ));
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterWorldLoad, GameLoader.NAMESPACE + ".Jobs.ApothecaryRegister.AfterWorldLoad")]
        public static void AfterWorldLoad()
        {
            var iron = new InventoryItem(BuiltinBlocks.BronzeIngot, 2);
            var tools = new InventoryItem(BuiltinBlocks.CopperTools, 1);
            var planks = new InventoryItem(BuiltinBlocks.Planks, 4);

            var recipe = new Recipe(JOB_RECIPE,
                    new List<InventoryItem>() { iron, tools, planks },
                    new InventoryItem(JOB_ITEM_KEY, 1), 2);

            //ItemTypesServer.LoadSortOrder(JOB_ITEM_KEY, GameLoader.GetNextItemSortIndex());
            RecipePlayer.AddOptionalRecipe(recipe);
            RecipeStorage.AddOptionalLimitTypeRecipe(Items.ItemFactory.JOB_CRAFTER, recipe);
        }
    }

    public class ApothecaryJob : CraftingJobBase, IBlockJobBase, INPCTypeDefiner
    {
        public static float StaticCraftingCooldown = 15f;

        public override string NPCTypeKey
        {
            get
            {
                return ApothecaryRegister.JOB_NAME;
            }
        }

        public override int MaxRecipeCraftsPerHaul
        {
            get
            {
                return 1;
            }
        }

        public override float CraftingCooldown
        {
            get
            {
                return StaticCraftingCooldown;
            }
            set
            {
                StaticCraftingCooldown = value;
            }
        }

        NPCTypeStandardSettings INPCTypeDefiner.GetNPCTypeDefinition()
        {
            return new NPCTypeStandardSettings
            {
                keyName = this.NPCTypeKey,
                printName = "Apothecary",
                maskColor1 = new Color32(101, 121, 123, 255),
                type = NPCTypeID.GetNextID()
            };
        }

        protected override void OnRecipeCrafted()
        {
            base.OnRecipeCrafted();
            ServerManager.SendAudio(this.position.Vector, "crafting");
        }

        protected override string GetRecipeLocation()
        {
            return System.IO.Path.Combine(GameLoader.MOD_FOLDER, "AutoLoad/Apothecary.json");
        }
    }
}
