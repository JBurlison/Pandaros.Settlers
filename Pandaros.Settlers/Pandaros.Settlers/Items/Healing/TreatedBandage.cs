using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BlockTypes.Builtin;
using Pipliz.JSON;

namespace Pandaros.Settlers.Items.Healing
{
    [ModLoader.ModManager]
    public static class TreatedBandage
    {
        public static ItemTypesServer.ItemTypeRaw Item { get; private set; }
        public const long COOLDOWN = 5000;
        public const float INITIALHEAL = 50f;
        public const float TOTALHOT = 70f;
        private static Dictionary<Players.Player, long> _coolDown = new Dictionary<Players.Player, long>();

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterItemTypesDefined, GameLoader.NAMESPACE + ".Items.Healing.TreatedBandage.Register")]
        public static void Register()
        {
            var bandage = new InventoryItem(Bandage.Item.ItemIndex, 1);
            var antibiotics = new InventoryItem(Anitbiotic.Item.ItemIndex, 1);

            var recipe = new Recipe(Item.name,
                                    new List<InventoryItem>() { antibiotics, bandage },
                                    new InventoryItem(Item.ItemIndex, 1),
                                    50);
            
            RecipeStorage.AddOptionalLimitTypeRecipe(Jobs.ApothecaryRegister.JOB_NAME, recipe);
        }


        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterAddingBaseTypes, GameLoader.NAMESPACE + ".Items.Healing.TreatedBandage.Add"), ModLoader.ModCallbackDependsOn("pipliz.blocknpcs.addlittypes")]
        public static void Add(Dictionary<string, ItemTypesServer.ItemTypeRaw> items)
        {
            var name = GameLoader.NAMESPACE + ".TreatedBandage";
            var node = new JSONNode();
            node["icon"] = new JSONNode(GameLoader.ICON_FOLDER_PANDA + "/TreatedBandage.png");
            node["isPlaceable"] = new JSONNode(false);

            JSONNode categories = new JSONNode(NodeType.Array);
            categories.AddToArray(new JSONNode("medicine"));
            node.SetAs("categories", categories);

            Item = new ItemTypesServer.ItemTypeRaw(name, node);
            items.Add(name, Item);
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnPlayerClicked, GameLoader.NAMESPACE + ".Items.Healing.TreatedBandage.Click")]
        public static void Click(Players.Player player, Pipliz.Box<Shared.PlayerClickedData> boxedData)
        {
            bool healed = false;

            if (!_coolDown.ContainsKey(player))
                _coolDown.Add(player, 0);

            if (boxedData.item1.clickType == Shared.PlayerClickedData.ClickType.Right &&
                boxedData.item1.typeSelected == Item.ItemIndex)
            {
                if (Pipliz.Time.MillisecondsSinceStart > _coolDown[player])
                {
                    var healing = new Entities.HealingOverTimePC(player, INITIALHEAL, TOTALHOT, 5);
                    healed = true;
                }
            }
            else if (boxedData.item1.clickType == Shared.PlayerClickedData.ClickType.Left &&
                boxedData.item1.typeSelected == Item.ItemIndex &&
                boxedData.item1.rayCastHit.rayHitType == Shared.RayHitType.NPC)
            {
                if (NPC.NPCTracker.TryGetNPC(boxedData.item1.rayCastHit.hitNPCID, out var npc))
                {
                    if (Pipliz.Time.MillisecondsSinceStart > _coolDown[player])
                    {
                        var heal = new Entities.HealingOverTimeNPC(npc, INITIALHEAL, TOTALHOT, 5, Item.ItemIndex);
                        healed = true;
                    }
                }
            }

            if (healed)
            {
                _coolDown[player] = Pipliz.Time.MillisecondsSinceStart + COOLDOWN;
                boxedData.item1.consumedType = Shared.PlayerClickedData.ConsumedType.UsedByMod;
                ServerManager.SendAudio(player.Position, GameLoader.NAMESPACE + ".Bandage");

                if (Inventory.TryGetInventory(player, out var inv))
                    inv.TryRemove(Item.ItemIndex);
            }
        }
    }
}
