using System.Collections.Generic;
using NPC;
using Pandaros.Settlers.Entities;
using Pandaros.Settlers.Jobs;
using Pipliz;
using Pipliz.JSON;
using Shared;

namespace Pandaros.Settlers.Items.Healing
{
    [ModLoader.ModManagerAttribute]
    public static class TreatedBandage
    {
        public const long COOLDOWN = 5000;
        public const float INITIALHEAL = 50f;
        public const float TOTALHOT = 70f;
        private static readonly Dictionary<Players.Player, long> _coolDown = new Dictionary<Players.Player, long>();
        public static ItemTypesServer.ItemTypeRaw Item { get; private set; }

        [ModLoader.ModCallbackAttribute(ModLoader.EModCallbackType.AfterItemTypesDefined,
            GameLoader.NAMESPACE + ".Items.Healing.TreatedBandage.Register")]
        public static void Register()
        {
            var bandage     = new InventoryItem(Bandage.Item.ItemIndex, 1);
            var antibiotics = new InventoryItem(Anitbiotic.Item.ItemIndex, 1);

            var recipe = new Recipe(Item.name,
                                    new List<InventoryItem> {antibiotics, bandage},
                                    new InventoryItem(Item.ItemIndex, 1),
                                    50);

            RecipeStorage.AddOptionalLimitTypeRecipe(ApothecaryRegister.JOB_NAME, recipe);
        }


        [ModLoader.ModCallbackAttribute(ModLoader.EModCallbackType.AfterAddingBaseTypes,
            GameLoader.NAMESPACE + ".Items.Healing.TreatedBandage.Add")]
        [ModLoader.ModCallbackDependsOnAttribute("pipliz.blocknpcs.addlittypes")]
        public static void Add(Dictionary<string, ItemTypesServer.ItemTypeRaw> items)
        {
            var name = GameLoader.NAMESPACE + ".TreatedBandage";
            var node = new JSONNode();
            node["icon"]        = new JSONNode(GameLoader.ICON_PATH + "TreatedBandage.png");
            node["isPlaceable"] = new JSONNode(false);

            var categories = new JSONNode(NodeType.Array);
            categories.AddToArray(new JSONNode("medicine"));
            node.SetAs("categories", categories);

            Item = new ItemTypesServer.ItemTypeRaw(name, node);
            items.Add(name, Item);
        }

        [ModLoader.ModCallbackAttribute(ModLoader.EModCallbackType.OnPlayerClicked,
            GameLoader.NAMESPACE + ".Items.Healing.TreatedBandage.Click")]
        public static void Click(Players.Player player, Box<PlayerClickedData> boxedData)
        {
            var healed = false;

            if (!_coolDown.ContainsKey(player))
                _coolDown.Add(player, 0);

            if (boxedData.item1.clickType == PlayerClickedData.ClickType.Right &&
                boxedData.item1.typeSelected == Item.ItemIndex)
            {
                if (Time.MillisecondsSinceStart > _coolDown[player])
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
                    if (Time.MillisecondsSinceStart > _coolDown[player])
                    {
                        var heal = new HealingOverTimeNPC(npc, INITIALHEAL, TOTALHOT, 5, Item.ItemIndex);
                        healed = true;
                    }
            }

            if (healed)
            {
                _coolDown[player]            = Time.MillisecondsSinceStart + COOLDOWN;
                boxedData.item1.consumedType = PlayerClickedData.ConsumedType.UsedByMod;
                ServerManager.SendAudio(player.Position, GameLoader.NAMESPACE + ".Bandage");

                if (Inventory.TryGetInventory(player, out var inv))
                    inv.TryRemove(Item.ItemIndex);
            }
        }
    }
}