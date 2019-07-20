using Pandaros.API;
using Pandaros.Settlers.Jobs;
using Pipliz.JSON;
using Recipes;
using System.Collections.Generic;

namespace Pandaros.Settlers.Items.Healing
{
    [ModLoader.ModManager]
    public static class Anitbiotic
    {
        public static ItemTypesServer.ItemTypeRaw Item { get; private set; }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterItemTypesDefined, GameLoader.NAMESPACE + ".Items.Healing.Anitbiotic.Register")]
        [ModLoader.ModCallbackProvidesFor("pipliz.server.loadresearchables")]
        public static void Register()
        {
            var herbs  = new InventoryItem(ColonyBuiltIn.ItemTypes.HOLLYHOCK.Name, 2);
            var herbs2 = new InventoryItem(ColonyBuiltIn.ItemTypes.ALKANET.Name, 2);
            var oil    = new InventoryItem(ColonyBuiltIn.ItemTypes.OLIVEOIL.Name, 1);
            var flour  = new InventoryItem(ColonyBuiltIn.ItemTypes.FLOUR.Name, 2);

            var recipe = new Recipe(Item.name,
                                    new List<InventoryItem> {flour, oil, herbs, herbs2},
                                    new RecipeResult(Item.ItemIndex, 1),
                                    50);

            ServerManager.RecipeStorage.AddLimitTypeRecipe(ApothecaryRegister.JOB_NAME, recipe);
            ServerManager.RecipeStorage.AddScienceRequirement(recipe);
        }


        [ModLoader.ModCallback(ModLoader.EModCallbackType.AddItemTypes, GameLoader.NAMESPACE + ".Items.Healing.Anitbiotic.Add")]
        [ModLoader.ModCallbackDependsOn("pipliz.server.applymoditempatches")]
        public static void Add(Dictionary<string, ItemTypesServer.ItemTypeRaw> items)
        {
            var name = GameLoader.NAMESPACE + ".Anitbiotic";
            var node = new JSONNode();
            node["icon"]        = new JSONNode(GameLoader.ICON_PATH + "Anitbiotic.png");
            node["isPlaceable"] = new JSONNode(false);

            var categories = new JSONNode(NodeType.Array);
            categories.AddToArray(new JSONNode("medicine"));
            node.SetAs("categories", categories);

            Item = new ItemTypesServer.ItemTypeRaw(name, node);
            items.Add(name, Item);
        }
    }
}