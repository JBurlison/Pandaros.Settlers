using System.Collections.Generic;
using NPC;
using Pandaros.Settlers.Entities;
using Pandaros.Settlers.Jobs;
using Pipliz;
using Pipliz.JSON;
using Recipes;
using Shared;

namespace Pandaros.Settlers.Items.Healing
{
    [ModLoader.ModManager]
    public static class TreatedBandage
    {
        public const long COOLDOWN = 5000;
        public const float INITIALHEAL = 50f;
        public const float TOTALHOT = 70f;
        private static readonly Dictionary<Players.Player, long> _coolDown = new Dictionary<Players.Player, long>();
        public static ItemTypesServer.ItemTypeRaw Item { get; private set; }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterItemTypesDefined, GameLoader.NAMESPACE + ".Items.Healing.TreatedBandage.Register")]
        [ModLoader.ModCallbackProvidesFor("pipliz.server.loadresearchables")]
        public static void Register()
        {
            var bandage     = new InventoryItem(Bandage.Item.ItemIndex, 1);
            var antibiotics = new InventoryItem(Anitbiotic.Item.ItemIndex, 1);

            var recipe = new Recipe(Item.name,
                                    new List<InventoryItem> {antibiotics, bandage},
                                    new RecipeResult(Item.ItemIndex, 1),
                                    50);

            ServerManager.RecipeStorage.AddLimitTypeRecipe(ApothecaryRegister.JOB_NAME, recipe);
            ServerManager.RecipeStorage.AddScienceRequirement(recipe);
        }


        [ModLoader.ModCallback(ModLoader.EModCallbackType.AddItemTypes, GameLoader.NAMESPACE + ".Items.Healing.TreatedBandage.Add")]
        [ModLoader.ModCallbackDependsOn("pipliz.server.applymoditempatches")]
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

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnPlayerClicked, GameLoader.NAMESPACE + ".Items.Healing.TreatedBandage.Click")]
        public static void Click(Players.Player player, PlayerClickedData playerClickData)
        {
            var healed = false;

            if (!_coolDown.ContainsKey(player))
                _coolDown.Add(player, 0);

            if (playerClickData.ClickType == PlayerClickedData.EClickType.Right &&
                playerClickData.TypeSelected == Item.ItemIndex)
            {
                if (Time.MillisecondsSinceStart > _coolDown[player])
                {
                    var healing = new HealingOverTimePC(player, INITIALHEAL, TOTALHOT, 5);
                    healed = true;
                }
            }
            else if (playerClickData.ClickType == PlayerClickedData.EClickType.Left &&
                     playerClickData.TypeSelected == Item.ItemIndex &&
                     playerClickData.HitType == PlayerClickedData.EHitType.NPC)
            {
                if (NPCTracker.TryGetNPC(playerClickData.GetNPCHit().NPCID, out var npc))
                    if (Time.MillisecondsSinceStart > _coolDown[player])
                    {
                        var heal = new HealingOverTimeNPC(npc, INITIALHEAL, TOTALHOT, 5, Item.ItemIndex);
                        healed = true;
                    }
            }

            if (healed)
            {
                _coolDown[player]            = Time.MillisecondsSinceStart + COOLDOWN;
                playerClickData.ConsumedType = PlayerClickedData.EConsumedType.UsedByMod;
                AudioManager.SendAudio(player.Position, GameLoader.NAMESPACE + ".Bandage");
                player.Inventory.TryRemove(Item.ItemIndex);
            }
        }
    }
}