using BlockTypes;
using NPC;
using Pandaros.Settlers.Entities;
using Pandaros.Settlers.Jobs;
using Pipliz;
using Pipliz.JSON;
using Recipes;
using Shared;
using System.Collections.Generic;

namespace Pandaros.Settlers.Items.Healing
{
    [ModLoader.ModManager]
    public static class Bandage
    {
        public const double COOLDOWN = 5;
        public const float INITIALHEAL = 20f;
        public const float TOTALHOT = 40f;
        private static readonly Dictionary<Players.Player, double> _coolDown = new Dictionary<Players.Player, double>();
        public static ItemTypesServer.ItemTypeRaw Item { get; private set; }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterItemTypesDefined, GameLoader.NAMESPACE + ".Items.Healing.Bandage.Register")]
        public static void Register()
        {
            var oil   = new InventoryItem(BuiltinBlocks.LinseedOil, 1);
            var linen = new InventoryItem(BuiltinBlocks.Linen, 1);

            var recipe = new Recipe(Item.name,
                                    new List<InventoryItem> {linen, oil},
                                    new InventoryItem(Item.ItemIndex, 1),
                                    50);

            ServerManager.RecipeStorage.AddDefaultLimitTypeRecipe(ApothecaryRegister.JOB_NAME, recipe);
        }


        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterAddingBaseTypes, GameLoader.NAMESPACE + ".Items.Healing.Bandage.Add")]
        [ModLoader.ModCallbackDependsOn("pipliz.blocknpcs.addlittypes")]
        public static void Add(Dictionary<string, ItemTypesServer.ItemTypeRaw> items)
        {
            var name = GameLoader.NAMESPACE + ".Bandage";
            var node = new JSONNode();
            node["icon"]        = new JSONNode(GameLoader.ICON_PATH + "Bandage.png");
            node["isPlaceable"] = new JSONNode(false);

            var categories = new JSONNode(NodeType.Array);
            categories.AddToArray(new JSONNode("medicine"));
            node.SetAs("categories", categories);

            Item = new ItemTypesServer.ItemTypeRaw(name, node);
            items.Add(name, Item);
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnPlayerClicked, GameLoader.NAMESPACE + ".Items.Healing.Bandage.Click")]
        public static void Click(Players.Player player, Box<PlayerClickedData> boxedData)
        {
            var healed = false;

            if (!_coolDown.ContainsKey(player))
                _coolDown.Add(player, 0);

            if (boxedData.item1.clickType == PlayerClickedData.ClickType.Right &&
                boxedData.item1.typeSelected == Item.ItemIndex)
            {
                if (Time.SecondsSinceStartDouble > _coolDown[player])
                {
                    var healing = new HealingOverTimePC(player, INITIALHEAL, TOTALHOT, 5);
                    healed = true;
                }
            }
            else if (boxedData.item1.clickType == PlayerClickedData.ClickType.Left &&
                     boxedData.item1.typeSelected == Item.ItemIndex &&
                     boxedData.item1.rayCastHit.rayHitType == RayHitType.NPC)
            {
                if (NPCTracker.TryGetNPC(boxedData.item1.rayCastHit.hitNPCID, out var npc))
                    if (Time.SecondsSinceStartDouble > _coolDown[player])
                    {
                        var heal = new HealingOverTimeNPC(npc, INITIALHEAL, TOTALHOT, 5, Item.ItemIndex);
                        healed = true;
                    }
            }

            if (healed)
            {
                _coolDown[player]            = Time.SecondsSinceStartDouble + COOLDOWN;
                boxedData.item1.consumedType = PlayerClickedData.ConsumedType.UsedByMod;
                ServerManager.SendAudio(player.Position, GameLoader.NAMESPACE + ".Bandage");

                if (Inventory.TryGetInventory(player, out var inv))
                    inv.TryRemove(Item.ItemIndex);
            }
        }
    }
}