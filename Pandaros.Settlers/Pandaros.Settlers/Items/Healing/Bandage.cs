using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BlockTypes.Builtin;
using Pipliz.JSON;

namespace Pandaros.Settlers.Items.Healing
{
    [ModLoader.ModManager]
    public static class Bandage
    {
        public static ItemTypesServer.ItemTypeRaw Item { get; private set; }

        public const double COOLDOWN = 5;

        private static Dictionary<Players.Player, double> _coolDown = new Dictionary<Players.Player, double>();

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterSelectedWorld, GameLoader.NAMESPACE + ".Items.Healing.Bandage.RegisterAudio"),
            ModLoader.ModCallbackProvidesFor("pipliz.server.loadaudiofiles"), ModLoader.ModCallbackDependsOn("pipliz.server.registeraudiofiles")]
        public static void RegisterAudio()
        {
            GameLoader.AddSoundFile(GameLoader.NAMESPACE + ".Bandage", new List<string>() { GameLoader.AUDIO_FOLDER_PANDA + "/Bandage.ogg" });
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterItemTypesDefined, GameLoader.NAMESPACE + ".Items.Healing.Bandage.Register")]
        public static void Register()
        {
            var oil = new InventoryItem(BuiltinBlocks.LinseedOil, 1);
            var linen = new InventoryItem(BuiltinBlocks.Linen, 1);

            var recipe = new Recipe(Item.name,
                                    new List<InventoryItem>() { linen, oil },
                                    new InventoryItem(Item.ItemIndex, 1),
                                    50);

            RecipeStorage.AddDefaultLimitTypeRecipe(Jobs.ApothecaryRegister.JOB_NAME, recipe);
        }


        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterAddingBaseTypes, GameLoader.NAMESPACE + ".Items.Healing.Bandage.Add"), ModLoader.ModCallbackDependsOn("pipliz.blocknpcs.addlittypes")]
        public static void Add(Dictionary<string, ItemTypesServer.ItemTypeRaw> items)
        {
            var name = GameLoader.NAMESPACE + ".Bandage";
            var node = new JSONNode();
            node["icon"] = new JSONNode(GameLoader.ICON_FOLDER_PANDA + "/Bandage.png");
            node["isPlaceable"] = new JSONNode(false);

            Item = new ItemTypesServer.ItemTypeRaw(name, node);
            items.Add(name, Item);
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnPlayerClicked, GameLoader.NAMESPACE + ".Items.Healing.Bandage.Click")]
        public static void Click(Players.Player player, Pipliz.Box<Shared.PlayerClickedData> boxedData)
        {
            bool healed = false;

            if (!_coolDown.ContainsKey(player))
                _coolDown.Add(player, 0);

            if (boxedData.item1.clickType == Shared.PlayerClickedData.ClickType.Right &&
                boxedData.item1.typeSelected == Item.ItemIndex)
            {
                //if (TimeCycle.TotalTime > _coolDown[player])
                {
                    var healing = new Entities.HealingOverTimePC(player, 20f, 40f, 5);
                    healed = true;
                }
            }
            else if (boxedData.item1.clickType == Shared.PlayerClickedData.ClickType.Left &&
                boxedData.item1.typeSelected == Item.ItemIndex &&
                boxedData.item1.rayCastHit.rayHitType == Shared.RayHitType.NPC)
            {
                if (NPC.NPCTracker.TryGetNPC(boxedData.item1.rayCastHit.hitNPCID, out var npc))
                {
                    //if (TimeCycle.TotalTime > _coolDown[player])
                    {
                        var heal = new Entities.HealingOverTimeNPC(npc, 20f, 40f, 5);
                        healed = true;
                    }
                }
            }

            if (healed)
            {
                _coolDown[player] = TimeCycle.TotalTime + COOLDOWN;
                boxedData.item1.consumedType = Shared.PlayerClickedData.ConsumedType.UsedByMod;
                ServerManager.SendAudio(player.Position, GameLoader.NAMESPACE + ".Bandage");
                if (Inventory.TryGetInventory(player, out var inv))
                    inv.TryRemove(Item.ItemIndex);
            }
        }
    }
}
