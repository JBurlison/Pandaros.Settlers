using BlockTypes;
using Pandaros.Settlers.Jobs;
using Pandaros.Settlers.Research;
using Pipliz.JSON;
using Recipes;
using Science;
using System.Collections.Generic;

namespace Pandaros.Settlers.Items
{
    [ModLoader.ModManager]
    public static class Elementium
    {
        public static ItemTypesServer.ItemTypeRaw Item { get; private set; }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterItemTypesDefined, GameLoader.NAMESPACE + ".Items.Elementium.Register")]
        [ModLoader.ModCallbackProvidesFor("pipliz.server.loadresearchables")]
        public static void Register()
        {
            var aether = new InventoryItem(Aether.Item.ItemIndex, 1);
            var copper = new InventoryItem(ColonyBuiltIn.ItemTypes.COPPER.Name, 40);
            var iron   = new InventoryItem(ColonyBuiltIn.ItemTypes.IRONORE.Name, 20);
            var tin    = new InventoryItem(ColonyBuiltIn.ItemTypes.TIN.Name, 40);
            var gold   = new InventoryItem(ColonyBuiltIn.ItemTypes.GOLDORE.Name, 10);
            var silver = new InventoryItem(ColonyBuiltIn.ItemTypes.GALENASILVER.Name, 20);
            var lead   = new InventoryItem(ColonyBuiltIn.ItemTypes.GALENALEAD.Name, 20);

            var recipe = new Recipe(Item.name,
                                    new List<InventoryItem> {aether, copper, iron, tin, gold, silver, lead},
                                    new RecipeResult(Item.ItemIndex, 1),
                                    10);

            recipe.Results.Add(new RecipeResult(Void.Item.ItemIndex, 1, 0.03f));

            ServerManager.RecipeStorage.AddLimitTypeRecipe(ApothecaryRegister.JOB_NAME, recipe);
            ServerManager.RecipeStorage.AddScienceRequirement(recipe);
        }


        [ModLoader.ModCallback(ModLoader.EModCallbackType.AddItemTypes, GameLoader.NAMESPACE + ".Items.Elementium.Add")]
        [ModLoader.ModCallbackDependsOn("pipliz.server.applymoditempatches")]
        public static void Add(Dictionary<string, ItemTypesServer.ItemTypeRaw> items)
        {
            var name = GameLoader.NAMESPACE + ".Elementium";
            var node = new JSONNode();
            node["icon"]        = new JSONNode(GameLoader.ICON_PATH + "Elementium.png");
            node["isPlaceable"] = new JSONNode(false);

            var categories = new JSONNode(NodeType.Array);
            categories.AddToArray(new JSONNode("ingredient"));
            categories.AddToArray(new JSONNode("magic"));
            node.SetAs("categories", categories);

            Item = new ItemTypesServer.ItemTypeRaw(name, node);
            items.Add(name, Item);
        }
    }

    public class ElementiumResearch : PandaResearch
    {
        public override string IconDirectory => GameLoader.ICON_PATH;
        public override string name => GameLoader.NAMESPACE + ".Elementium";

        public override Dictionary<int, List<InventoryItem>> RequiredItems => new Dictionary<int, List<InventoryItem>>()
        {
            {
                0,
                new List<InventoryItem>()
                {
                    new InventoryItem(ColonyBuiltIn.ItemTypes.COPPER.Id, 20),
                    new InventoryItem(ColonyBuiltIn.ItemTypes.IRONORE.Id, 20),
                    new InventoryItem(ColonyBuiltIn.ItemTypes.TIN.Id, 20),
                    new InventoryItem(ColonyBuiltIn.ItemTypes.GOLDORE.Id, 20),
                    new InventoryItem(ColonyBuiltIn.ItemTypes.GALENASILVER.Id, 20),
                    new InventoryItem(ColonyBuiltIn.ItemTypes.GALENALEAD.Id, 20),
                    new InventoryItem(SettlersBuiltIn.ItemTypes.AETHER.Id, 1)
                }
            }
        };

        public override Dictionary<int, List<IResearchableCondition>> Conditions => new Dictionary<int, List<IResearchableCondition>>()
        {
            {
                0,
                new List<IResearchableCondition>()
                {
                    new ColonistCountCondition() { Threshold = 150 },
                    new HappinessCondition() { Threshold = 50 }
                }
            }
        };

        public override Dictionary<int, List<string>> Dependancies => new Dictionary<int, List<string>>()
        {
            {
                0,
                new List<string>()
                {
                    SettlersBuiltIn.Research.MANA1
                }
            }
        };

        public override Dictionary<int, List<RecipeUnlock>> Unlocks => new Dictionary<int, List<RecipeUnlock>>()
        {
            {
                1,
                new List<RecipeUnlock>()
                {
                    new RecipeUnlock(SettlersBuiltIn.ItemTypes.ELEMENTIUM, ERecipeUnlockType.Recipe),
                    new RecipeUnlock(SettlersBuiltIn.ItemTypes.EARTHSTONE, ERecipeUnlockType.Recipe),
                    new RecipeUnlock(SettlersBuiltIn.ItemTypes.FIRESTONE, ERecipeUnlockType.Recipe),
                    new RecipeUnlock(SettlersBuiltIn.ItemTypes.WATERSTONE, ERecipeUnlockType.Recipe),
                    new RecipeUnlock(SettlersBuiltIn.ItemTypes.AIRSTONE, ERecipeUnlockType.Recipe),
                    new RecipeUnlock(SettlersBuiltIn.ItemTypes.AIRTURRET, ERecipeUnlockType.Recipe),
                    new RecipeUnlock(SettlersBuiltIn.ItemTypes.FIRETURRET, ERecipeUnlockType.Recipe),
                    new RecipeUnlock(SettlersBuiltIn.ItemTypes.EARTHTURRET, ERecipeUnlockType.Recipe),
                    new RecipeUnlock(SettlersBuiltIn.ItemTypes.WATERTURRET, ERecipeUnlockType.Recipe),
                    new RecipeUnlock(SettlersBuiltIn.ItemTypes.VOIDTURRET, ERecipeUnlockType.Recipe)
                }
            }
        };

        public override int NumberOfLevels => 1;

        public override int BaseIterationCount => 50;

    }
}